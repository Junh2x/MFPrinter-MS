"""
복합기 검색 및 감지 - Canon, Ricoh, Sindoh 공통

감지 전략 (3단 병렬):
  1순위: WS-Discovery 멀티캐스트 → 네트워크 복합기 자동 응답
  2순위: Windows Get-Printer API  → 이미 설치된 프린터 IP 수집
  3순위: 멀티포트 스캔 (9100, 80, 8000, 443, 631) → 놓친 기기 보완

브랜드/모델 식별:
  WS-Discovery Scopes → SNMP sysDescr → HTTP 페이지 (순차 폴백)

사용법:
    from common.discovery import discover_devices
    devices = discover_devices()                                           # 전체 자동
    devices = discover_devices(subnet="192.168.11")                        # 특정 서브넷
    devices = discover_devices(ips=["192.168.11.227", "192.168.11.185"])    # 특정 IP
"""
import re
import socket
import struct
import time
import uuid
import subprocess
import concurrent.futures
from datetime import datetime

import requests
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# ---------------------------------------------------------------------------
# 상수
# ---------------------------------------------------------------------------
UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
HTTP_PORTS = [80, 8000, 443, 8443]
PRINTER_PORTS = [9100, 631, 80, 8000, 443]
TIMEOUT_TCP = 1
TIMEOUT_HTTP = 3
TIMEOUT_SNMP = 2
WSD_TIMEOUT = 4          # WS-Discovery 대기 시간(초)
WSD_ADDR = "239.255.255.250"
WSD_PORT = 3702

# SNMP v1 sysDescr OID (1.3.6.1.2.1.1.1.0) — raw 바이트
_SNMP_OID_SYSDESCR = bytes([0x06, 0x08, 0x2b, 0x06, 0x01, 0x02, 0x01, 0x01, 0x01, 0x00])

# 브랜드 키워드
BRAND_KEYWORDS = {
    "canon":  ["canon", "imagerunner", "ir-adv", "iradv", "remote ui", "meap"],
    "ricoh":  ["ricoh", "im c", "imc", "web image monitor", "savin", "lanier", "gestetner"],
    "sindoh": ["sindoh", "신도", "d420", "d450", "d-color", "n410", "n610"],
}

# 모델명 추출 정규식
_MODEL_PATTERNS = {
    "canon":  r"((?:iR[ -]?ADV|imageRUNNER)\s*\S+(?:\s+\S+)?)",
    "ricoh":  r"((?:IM\s*C|MP\s*C?|SP)\s*\d+\S*)",
    "sindoh": r"([DNdn][ -]?\d{3,4}\S*)",
}


# ---------------------------------------------------------------------------
# 로깅
# ---------------------------------------------------------------------------
_debug = False

def _log(msg, level="INFO"):
    if _debug or level == "RESULT":
        ts = datetime.now().strftime("%H:%M:%S.%f")[:-3]
        prefix = {"INFO": " ", "FOUND": "+", "WARN": "!", "RESULT": "*", "DEBUG": " "}
        print(f"  [{ts}] [{prefix.get(level, ' ')}] {msg}")


# ---------------------------------------------------------------------------
# 내부 유틸
# ---------------------------------------------------------------------------
def _check_port(ip, port, timeout=TIMEOUT_TCP):
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(timeout)
        result = sock.connect_ex((ip, port))
        sock.close()
        return result == 0
    except Exception:
        return False


def _get_local_subnet():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(("8.8.8.8", 80))
        local_ip = s.getsockname()[0]
    finally:
        s.close()
    parts = local_ip.split(".")
    return ".".join(parts[:3]), local_ip


def _match_brand(text):
    if not text:
        return None
    lower = text.lower()
    for brand, keywords in BRAND_KEYWORDS.items():
        for kw in keywords:
            if kw in lower:
                return brand
    return None


def _extract_model(text, brand):
    if not text:
        return ""
    pat = _MODEL_PATTERNS.get(brand)
    if pat:
        m = re.search(pat, text, re.I)
        if m:
            return m.group(1).strip()
    return text.split("\n")[0][:60]


def _is_local_ip(ip):
    return ip.startswith("127.") or ip == "0.0.0.0" or ip == "::1"


# ===================================================================
# 1순위: WS-Discovery
# ===================================================================
def wsd_discover(timeout=WSD_TIMEOUT):
    """
    WS-Discovery 멀티캐스트 Probe를 보내고 응답한 장치 목록을 반환한다.

    Returns:
        list[dict]: [{
            "ip": str,
            "brand": str|None,
            "model": str,
            "source": "wsd",
            "types": str,
            "xaddrs": str,
            "scopes": str,
        }, ...]
    """
    probe_id = str(uuid.uuid4())
    probe_xml = (
        '<?xml version="1.0" encoding="utf-8"?>'
        '<soap:Envelope '
        'xmlns:soap="http://www.w3.org/2003/05/soap-envelope" '
        'xmlns:wsa="http://schemas.xmlsoap.org/ws/2004/08/addressing" '
        'xmlns:wsd="http://schemas.xmlsoap.org/ws/2005/04/discovery" '
        'xmlns:wsdp="http://schemas.xmlsoap.org/ws/2006/02/devprof">'
        '<soap:Header>'
        '<wsa:To>urn:schemas-xmlsoap-org:ws:2005:04:discovery</wsa:To>'
        '<wsa:Action>http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe</wsa:Action>'
        f'<wsa:MessageID>urn:uuid:{probe_id}</wsa:MessageID>'
        '</soap:Header>'
        '<soap:Body>'
        '<wsd:Probe/>'
        '</soap:Body>'
        '</soap:Envelope>'
    )

    _log("WS-Discovery 멀티캐스트 Probe 전송...")

    devices = {}
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, socket.IPPROTO_UDP)
        sock.setsockopt(socket.IPPROTO_IP, socket.IP_MULTICAST_TTL, 2)
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        sock.settimeout(1)

        # 여러 번 보내서 패킷 유실 방지
        for _ in range(3):
            sock.sendto(probe_xml.encode("utf-8"), (WSD_ADDR, WSD_PORT))
            time.sleep(0.1)

        start = time.time()
        while time.time() - start < timeout:
            try:
                data, addr = sock.recvfrom(65535)
                ip = addr[0]
                if _is_local_ip(ip) or ip in devices:
                    continue

                text = data.decode("utf-8", errors="replace")

                # 프린터 관련 타입 확인
                types_match = re.findall(r"<[^:]*:?Types>([^<]+)</[^:]*:?Types>", text)
                types_str = " ".join(types_match).lower()

                # XAddrs (접속 URL)
                xaddrs = re.findall(r"<[^:]*:?XAddrs>([^<]+)</[^:]*:?XAddrs>", text)
                xaddrs_str = " ".join(xaddrs)

                # Scopes (브랜드/모델 정보 포함)
                scopes = re.findall(r"<[^:]*:?Scopes>([^<]+)</[^:]*:?Scopes>", text)
                scopes_str = " ".join(scopes)

                # 브랜드/모델 식별 (Scopes에서)
                brand = _match_brand(scopes_str) or _match_brand(types_str) or _match_brand(xaddrs_str)
                model = _extract_model(scopes_str, brand) if brand else ""

                devices[ip] = {
                    "ip": ip,
                    "brand": brand,
                    "model": model,
                    "source": "wsd",
                    "types": types_str[:200],
                    "xaddrs": xaddrs_str[:200],
                    "scopes": scopes_str[:300],
                }
                _log(f"WSD 응답: {ip} brand={brand} model={model}", "FOUND")

            except socket.timeout:
                continue
            except Exception:
                continue

        sock.close()
    except Exception as e:
        _log(f"WSD 소켓 오류: {e}", "WARN")

    _log(f"WSD 완료: {len(devices)}개 장치 응답")
    return list(devices.values())


# ===================================================================
# 2순위: Windows Get-Printer API
# ===================================================================
def windows_printer_ips():
    """
    Windows에 등록된 프린터에서 네트워크 IP를 추출한다.

    Returns:
        list[dict]: [{"ip": str, "name": str, "driver": str, "source": "windows"}, ...]
    """
    _log("Windows 등록 프린터 조회 중...")
    results = []

    try:
        cmd = (
            'powershell -Command "'
            "Get-Printer | ForEach-Object {"
            "  $port = Get-PrinterPort -Name $_.PortName -ErrorAction SilentlyContinue;"
            "  if ($port -and $port.PrinterHostAddress) {"
            "    Write-Output ('{0}|{1}|{2}|{3}' -f $_.Name, $_.DriverName, $port.PrinterHostAddress, $port.PortNumber)"
            "  }"
            '}"'
        )
        proc = subprocess.run(cmd, capture_output=True, text=True, timeout=10, shell=True)

        for line in proc.stdout.strip().split("\n"):
            line = line.strip()
            if not line or "|" not in line:
                continue
            parts = line.split("|")
            if len(parts) < 3:
                continue

            name, driver, host_addr = parts[0], parts[1], parts[2]
            port_num = parts[3] if len(parts) > 3 else ""

            # 로컬 루프백 제외
            if _is_local_ip(host_addr):
                _log(f"  건너뜀 (로컬): {name} → {host_addr}")
                continue

            # IP 형식 검증
            try:
                socket.inet_aton(host_addr)
            except socket.error:
                # 호스트명일 수 있음 → DNS 해석 시도
                try:
                    host_addr = socket.gethostbyname(host_addr)
                except Exception:
                    _log(f"  건너뜀 (해석 불가): {name} → {host_addr}")
                    continue

            if _is_local_ip(host_addr):
                _log(f"  건너뜀 (루프백 해석): {name} → {host_addr}")
                continue

            brand = _match_brand(name) or _match_brand(driver)
            results.append({
                "ip": host_addr,
                "name": name,
                "driver": driver,
                "brand": brand,
                "source": "windows",
            })
            _log(f"  Windows 프린터: {name} → {host_addr} (brand={brand})", "FOUND")

    except Exception as e:
        _log(f"Windows 프린터 조회 실패: {e}", "WARN")

    _log(f"Windows 프린터 완료: {len(results)}개")
    return results


# ===================================================================
# 3순위: 멀티포트 서브넷 스캔
# ===================================================================
def scan_subnet(subnet_prefix=None, callback=None):
    """
    서브넷에서 프린터 포트(9100, 631, 80, 8000, 443) 열린 장치를 찾는다.

    Args:
        subnet_prefix: "192.168.11" 형태. None이면 자동 감지.
        callback:      fn(scanned_count, found_ip_or_None)

    Returns:
        list[str]: IP 목록
    """
    if subnet_prefix is None:
        subnet_prefix, local_ip = _get_local_subnet()
    else:
        local_ip = None

    _log(f"서브넷 스캔: {subnet_prefix}.0/24 (포트: {PRINTER_PORTS})")

    found = set()
    scanned = 0

    def _check(i):
        ip = f"{subnet_prefix}.{i}"
        for port in PRINTER_PORTS:
            if _check_port(ip, port, timeout=TIMEOUT_TCP):
                return ip, port
        return None, None

    with concurrent.futures.ThreadPoolExecutor(max_workers=50) as pool:
        futures = {pool.submit(_check, i): i for i in range(1, 255)}
        for future in concurrent.futures.as_completed(futures):
            scanned += 1
            ip, port = future.result()
            if ip and ip != local_ip:
                if ip not in found:
                    _log(f"포트 스캔 발견: {ip} (port {port})", "FOUND")
                found.add(ip)
            if callback:
                callback(scanned, ip)

    _log(f"서브넷 스캔 완료: {len(found)}개")
    return sorted(found, key=lambda x: int(x.split(".")[-1]))


# ===================================================================
# SNMP sysDescr (raw UDP — 외부 라이브러리 불필요)
# ===================================================================
def snmp_get_sysdescr(ip, community="public", timeout=TIMEOUT_SNMP):
    """
    SNMP v1 GET으로 sysDescr를 조회한다. (raw UDP)

    Returns:
        str|None: sysDescr 문자열, 실패 시 None
    """
    comm = community.encode()
    varbind = _SNMP_OID_SYSDESCR + b"\x05\x00"
    varbind = b"\x30" + bytes([len(varbind)]) + varbind
    varbind_list = b"\x30" + bytes([len(varbind)]) + varbind

    request_id = b"\x02\x01\x01"
    error_status = b"\x02\x01\x00"
    error_index = b"\x02\x01\x00"
    pdu_body = request_id + error_status + error_index + varbind_list
    pdu = b"\xa0" + bytes([len(pdu_body)]) + pdu_body

    version = b"\x02\x01\x00"
    community_tlv = bytes([0x04, len(comm)]) + comm
    msg_body = version + community_tlv + pdu
    message = b"\x30" + bytes([len(msg_body)]) + msg_body

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.settimeout(timeout)
    try:
        sock.sendto(message, (ip, 161))
        data, _ = sock.recvfrom(4096)
        idx = data.find(_SNMP_OID_SYSDESCR)
        if idx >= 0:
            idx += len(_SNMP_OID_SYSDESCR)
            if idx < len(data) and data[idx] == 0x04:
                length = data[idx + 1]
                # 긴 길이 처리 (length > 127)
                if length & 0x80:
                    num_bytes = length & 0x7F
                    length = int.from_bytes(data[idx + 2: idx + 2 + num_bytes], "big")
                    idx += num_bytes
                value = data[idx + 2: idx + 2 + length]
                return value.decode("utf-8", errors="replace")
    except Exception:
        pass
    finally:
        sock.close()
    return None


# ===================================================================
# HTTP 브랜드 식별 + 포트 확인
# ===================================================================
def http_identify(ip):
    """
    HTTP 접속으로 브랜드/모델을 식별하고 관리 URL을 찾는다.

    Returns:
        dict: {"brand": str|None, "model": str, "title": str, "port": int|None, "base_url": str}
    """
    result = {"brand": None, "model": "", "title": "", "port": None, "base_url": ""}

    for port in HTTP_PORTS:
        scheme = "https" if port in (443, 8443) else "http"
        url = f"{scheme}://{ip}:{port}/"
        try:
            r = requests.get(url, timeout=TIMEOUT_HTTP, verify=False,
                             allow_redirects=True, headers={"User-Agent": UA})
            if r.status_code >= 400:
                continue

            content = r.text.lower()
            server = r.headers.get("Server", "").lower()
            all_text = content + " " + server

            m = re.search(r"<title>([^<]+)</title>", r.text, re.I)
            title = m.group(1).strip() if m else ""

            brand = _match_brand(all_text)
            model = _extract_model(r.text, brand) if brand else ""

            if port in (80, 443):
                base_url = f"{scheme}://{ip}"
            else:
                base_url = f"{scheme}://{ip}:{port}"

            result = {"brand": brand, "model": model, "title": title, "port": port, "base_url": base_url}

            if brand:
                return result  # 브랜드 식별 성공 → 바로 반환

        except Exception:
            continue

    return result


# ===================================================================
# 단일 장치 상세 식별
# ===================================================================
def identify_device(ip, hints=None):
    """
    단일 IP에 대해 모든 수단으로 브랜드/모델을 식별한다.

    Args:
        ip:    대상 IP
        hints: 이전 단계에서 얻은 힌트 dict (brand, model, source 등)

    Returns:
        dict: {
            "ip", "brand", "model", "title", "port", "base_url",
            "sys_descr", "sources": list[str],
        }
    """
    info = {
        "ip": ip,
        "brand": "unknown",
        "model": "",
        "title": "",
        "port": None,
        "base_url": "",
        "sys_descr": "",
        "sources": [],
    }

    # 힌트 적용 (WSD / Windows에서 이미 브랜드를 알 수 있음)
    if hints:
        if hints.get("brand"):
            info["brand"] = hints["brand"]
            info["model"] = hints.get("model", "")
            info["sources"].append(hints.get("source", "hint"))

    # SNMP 시도
    _log(f"  [{ip}] SNMP 조회...")
    sys_descr = snmp_get_sysdescr(ip)
    if sys_descr:
        info["sys_descr"] = sys_descr
        info["sources"].append("snmp")
        snmp_brand = _match_brand(sys_descr)
        if snmp_brand:
            info["brand"] = snmp_brand
            info["model"] = _extract_model(sys_descr, snmp_brand)
            _log(f"  [{ip}] SNMP 성공: {snmp_brand} / {info['model']}", "FOUND")
    else:
        _log(f"  [{ip}] SNMP 무응답")

    # HTTP 시도
    _log(f"  [{ip}] HTTP 포트 탐색...")
    http_info = http_identify(ip)
    if http_info["port"]:
        info["port"] = http_info["port"]
        info["base_url"] = http_info["base_url"]
        info["title"] = http_info["title"]
        info["sources"].append("http")

        if info["brand"] == "unknown" and http_info["brand"]:
            info["brand"] = http_info["brand"]
            info["model"] = http_info["model"] or info["model"]
            _log(f"  [{ip}] HTTP 식별: {http_info['brand']} / {info['title']}", "FOUND")
        else:
            _log(f"  [{ip}] HTTP 접속 OK: {info['base_url']} ({info['title']})")
    else:
        _log(f"  [{ip}] HTTP 포트 없음")

    return info


# ===================================================================
# 통합 디스커버리
# ===================================================================
def discover_devices(subnet=None, ips=None, callback=None, debug=True):
    """
    네트워크에서 복합기를 검색하고 브랜드/모델을 식별한다.

    3단 병렬 감지:
      1순위: WS-Discovery 멀티캐스트 (네트워크 복합기 자동 응답)
      2순위: Windows Get-Printer API (이미 설치된 프린터 IP)
      3순위: 멀티포트 서브넷 스캔 (놓친 기기 보완)

    Args:
        subnet:   "192.168.11" 등. None이면 자동 감지.
        ips:      직접 지정할 IP 리스트. 지정 시 자동 검색 생략.
        callback: fn(phase, detail)
                  phase="wsd"      → detail={"count": int}
                  phase="windows"  → detail={"count": int}
                  phase="scan"     → detail={"scanned": int, "found": str|None}
                  phase="identify" → detail={"index": int, "total": int, "ip": str}
                  phase="done"     → detail={"devices": list}
        debug:    True면 상세 디버깅 로그 출력

    Returns:
        list[dict]: 감지된 복합기 정보 (brand != "unknown"만)
    """
    global _debug
    _debug = debug

    _log("=" * 60)
    _log("복합기 탐색 시작")
    _log("=" * 60)

    # IP가 직접 지정된 경우 → 바로 식별 단계로
    if ips:
        _log(f"지정 IP 모드: {ips}")
        return _identify_and_collect(ips, {}, callback)

    # ----------------------------------------------------------
    # Phase 1~2: WS-Discovery + Windows 프린터 (병렬)
    # ----------------------------------------------------------
    all_ips = {}  # ip → hints dict

    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
        wsd_future = pool.submit(wsd_discover)
        win_future = pool.submit(windows_printer_ips)

        # 서브넷 스캔은 시간이 오래 걸리므로 함께 시작
        scan_future = None
        if subnet is not None or True:  # 항상 스캔
            def _scan_cb(count, found_ip):
                if callback:
                    callback("scan", {"scanned": count, "found": found_ip})
            scan_future = pool.submit(scan_subnet, subnet, _scan_cb)

        # WSD 결과
        wsd_devices = wsd_future.result()
        if callback:
            callback("wsd", {"count": len(wsd_devices)})
        for d in wsd_devices:
            all_ips[d["ip"]] = d

        # Windows 결과
        win_devices = win_future.result()
        if callback:
            callback("windows", {"count": len(win_devices)})
        for d in win_devices:
            if d["ip"] not in all_ips:
                all_ips[d["ip"]] = d

        # 서브넷 스캔 결과
        if scan_future:
            scan_ips = scan_future.result()
            for ip in scan_ips:
                if ip not in all_ips:
                    all_ips[ip] = {"ip": ip, "source": "portscan"}

    _log(f"\n총 후보 IP: {len(all_ips)}개 "
         f"(WSD:{len(wsd_devices)} + WIN:{len(win_devices)} + SCAN:{len(all_ips) - len(wsd_devices) - len(win_devices)})")

    if not all_ips:
        _log("감지된 장치 없음", "WARN")
        if callback:
            callback("done", {"devices": []})
        return []

    # ----------------------------------------------------------
    # Phase 3: 상세 식별
    # ----------------------------------------------------------
    return _identify_and_collect(list(all_ips.keys()), all_ips, callback)


def _identify_and_collect(ip_list, hints_map, callback):
    """IP 리스트에 대해 상세 식별 후 결과 수집"""
    devices = []
    total = len(ip_list)

    _log(f"\n상세 식별 시작: {total}대")
    _log("-" * 40)

    with concurrent.futures.ThreadPoolExecutor(max_workers=5) as pool:
        future_map = {
            pool.submit(identify_device, ip, hints_map.get(ip)): ip
            for ip in ip_list
        }
        for i, future in enumerate(concurrent.futures.as_completed(future_map)):
            ip = future_map[future]
            if callback:
                callback("identify", {"index": i + 1, "total": total, "ip": ip})
            try:
                result = future.result()
                if result["brand"] != "unknown":
                    devices.append(result)
            except Exception as e:
                _log(f"  [{ip}] 식별 오류: {e}", "WARN")

    # 정렬: canon → ricoh → sindoh
    order = {"canon": 0, "ricoh": 1, "sindoh": 2}
    devices.sort(key=lambda d: (order.get(d["brand"], 9), d["ip"]))

    # ----------------------------------------------------------
    # 결과 출력
    # ----------------------------------------------------------
    _log("")
    _log("=" * 60)
    _log(f"  탐색 결과: 복합기 {len(devices)}대 감지", "RESULT")
    _log("=" * 60)
    for d in devices:
        _log(f"  [{d['brand'].upper():6s}]  {d['ip']}", "RESULT")
        _log(f"           모델:     {d['model'] or '(미확인)'}", "RESULT")
        _log(f"           웹 URL:   {d['base_url'] or '(없음)'}", "RESULT")
        _log(f"           웹 타이틀: {d['title'] or '(없음)'}", "RESULT")
        _log(f"           SNMP:     {d['sys_descr'][:70] or '(미응답)'}", "RESULT")
        _log(f"           감지경로:  {', '.join(d['sources']) or '(없음)'}", "RESULT")
        _log("", "RESULT")

    if callback:
        callback("done", {"devices": devices})

    return devices


# ===================================================================
# CLI 테스트
# ===================================================================
if __name__ == "__main__":
    import sys
    import json

    def _cli_callback(phase, detail):
        if phase == "scan" and detail.get("found"):
            pass  # _log가 이미 처리
        elif phase == "scan" and detail["scanned"] % 50 == 0:
            _log(f"서브넷 스캔 진행: {detail['scanned']}/254")
        elif phase == "wsd":
            _log(f"WS-Discovery: {detail['count']}대 응답")
        elif phase == "windows":
            _log(f"Windows 프린터: {detail['count']}대")
        elif phase == "identify":
            pass  # _log가 이미 처리

    if len(sys.argv) > 1 and sys.argv[1] == "--scan":
        subnet = sys.argv[2] if len(sys.argv) > 2 else None
        print(f"\n  복합기 탐색 시작 (서브넷: {subnet or '자동감지'})")
        print(f"  1순위: WS-Discovery 멀티캐스트")
        print(f"  2순위: Windows 등록 프린터")
        print(f"  3순위: 멀티포트 서브넷 스캔")
        print()
        devices = discover_devices(subnet=subnet, callback=_cli_callback, debug=True)

    elif len(sys.argv) > 1:
        target_ips = sys.argv[1:]
        print(f"\n  지정 IP 검사: {target_ips}")
        print()
        devices = discover_devices(ips=target_ips, callback=_cli_callback, debug=True)

    else:
        print()
        print(f"  복합기 탐색 도구")
        print(f"  {'='*40}")
        print(f"  사용법:")
        print(f"    python {sys.argv[0]} --scan [subnet]     # 전체 자동 탐색")
        print(f"    python {sys.argv[0]} IP1 IP2 ...         # 특정 IP 검사")
        print()
        print(f"  예시:")
        print(f"    python {sys.argv[0]} --scan")
        print(f"    python {sys.argv[0]} --scan 192.168.11")
        print(f"    python {sys.argv[0]} 192.168.11.227 192.168.11.185")
        sys.exit(0)

    # JSON 출력
    print(f"\n  JSON 결과:")
    print(json.dumps(devices, ensure_ascii=False, indent=2))
