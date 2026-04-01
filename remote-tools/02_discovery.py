import sys
import socket
import json
import os
import time
import ssl
import concurrent.futures
from datetime import datetime
from pathlib import Path

try:
    import requests
    import urllib3
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
except ImportError:
    print("requests 패키지 필요: pip install requests urllib3")
    sys.exit(1)

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)

LOG_FILE = RESULT_DIR / f"02_discovery_log_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

TIMEOUT = 5
COMMON_PORTS = [80, 443, 631, 8000, 8443, 9100, 161, 515, 21, 139, 445, 9090, 8080]

# ==============================
# 브랜드/모델 식별용 키워드
# ==============================
BRAND_KEYWORDS = {
    "sindoh": ["sindoh", "신도", "d420", "d450", "d-color"],
    "ricoh": ["ricoh", "리코", "im c", "imc", "web image monitor", "savin", "lanier"],
    "canon": ["canon", "캐논", "imagerunner", "ir-adv", "iradv", "remote ui", "meap"],
}
# commit


def log(msg, color=None):
    colors = {"red": "\033[91m", "green": "\033[92m", "yellow": "\033[93m", "cyan": "\033[96m"}
    reset = "\033[0m"
    prefix = colors.get(color, "") if color else ""
    suffix = reset if color else ""
    ts = datetime.now().strftime("%H:%M:%S")
    print(f"[{ts}] {prefix}{msg}{suffix}")
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(f"[{ts}] {msg}\n")


def scan_port(ip, port, timeout=2):
    """단일 포트 스캔"""
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(timeout)
        result = sock.connect_ex((ip, port))
        sock.close()
        return port if result == 0 else None
    except:
        return None


def scan_subnet_for_http(subnet_prefix):
    """서브넷에서 80포트 열린 장치 검색"""
    log(f"서브넷 {subnet_prefix}.0/24 스캔 중... (1~254)", "yellow")
    found = []

    def check(ip):
        if scan_port(ip, 80, timeout=1):
            return ip
        return None

    with concurrent.futures.ThreadPoolExecutor(max_workers=50) as executor:
        futures = {executor.submit(check, f"{subnet_prefix}.{i}"): i for i in range(1, 255)}
        for future in concurrent.futures.as_completed(futures):
            result = future.result()
            if result:
                found.append(result)
                log(f"  발견: {result} (80포트 열림)", "green")

    return sorted(found, key=lambda x: int(x.split(".")[-1]))


def port_scan(ip):
    """주요 포트 스캔"""
    log(f"[{ip}] 포트 스캔 중...", "yellow")
    open_ports = []

    with concurrent.futures.ThreadPoolExecutor(max_workers=20) as executor:
        futures = {executor.submit(scan_port, ip, port): port for port in COMMON_PORTS}
        for future in concurrent.futures.as_completed(futures):
            result = future.result()
            if result:
                open_ports.append(result)

    open_ports.sort()
    port_names = {
        80: "HTTP", 443: "HTTPS", 631: "IPP", 8000: "HTTP-alt",
        8443: "HTTPS-alt", 9100: "JetDirect", 161: "SNMP", 515: "LPD",
        21: "FTP", 139: "NetBIOS", 445: "SMB", 9090: "WebUI-alt", 8080: "HTTP-proxy"
    }
    for p in open_ports:
        name = port_names.get(p, "unknown")
        log(f"  포트 {p} ({name}): OPEN", "green")

    return open_ports


def identify_brand(ip, open_ports):
    """웹 페이지 내용으로 브랜드/모델 식별"""
    log(f"[{ip}] 브랜드 식별 중...", "yellow")
    info = {"ip": ip, "brand": "unknown", "model": "unknown", "web_title": "", "server_header": ""}

    for port in [80, 8000, 443, 8443]:
        if port not in open_ports:
            continue
        scheme = "https" if port in [443, 8443] else "http"
        url = f"{scheme}://{ip}:{port}/"
        try:
            r = requests.get(url, timeout=TIMEOUT, verify=False, allow_redirects=True)
            content = r.text.lower()
            info["server_header"] = r.headers.get("Server", "")
            info["web_title"] = ""

            # title 추출
            if "<title>" in content:
                start = content.index("<title>") + 7
                end = content.index("</title>", start)
                info["web_title"] = r.text[start:end].strip()

            # 브랜드 매칭
            for brand, keywords in BRAND_KEYWORDS.items():
                for kw in keywords:
                    if kw in content or kw in info["server_header"].lower():
                        info["brand"] = brand
                        log(f"  브랜드 식별: {brand.upper()} (키워드: '{kw}')", "green")
                        break
                if info["brand"] != "unknown":
                    break

            # 전체 응답 헤더 저장
            info["response_headers"] = dict(r.headers)
            info["final_url"] = r.url
            info["status_code"] = r.status_code

            if info["brand"] != "unknown":
                break
        except Exception as e:
            log(f"  {url} 접속 실패: {e}", "red")

    return info


def probe_endpoints(ip, brand, open_ports):
    """브랜드별 알려진 엔드포인트 프로브"""
    log(f"[{ip}] 엔드포인트 프로브 중 ({brand})...", "yellow")

    # 공통 엔드포인트
    endpoints = [
        # eSCL (AirScan)
        ("eSCL ScannerCapabilities", "/eSCL/ScannerCapabilities", "GET"),
        ("eSCL ScannerStatus", "/eSCL/ScannerStatus", "GET"),
        # WSD
        ("WSD Metadata", "/wsd", "GET"),
    ]

    # 브랜드별 엔드포인트
    if brand == "ricoh":
        endpoints += [
            ("Ricoh REST API root", "/rws/service-api/", "GET"),
            ("Ricoh Documents API", "/rws/service-api/documents", "GET"),
            ("Ricoh AddressBook API", "/rws/service-api/addressbook/entries", "GET"),
            ("Ricoh Scanner API", "/rws/service-api/scanner", "GET"),
            ("Ricoh DeviceInfo API", "/rws/service-api/device", "GET"),
            ("Ricoh Login CGI", "/web/guest/en/websys/webArch/login.cgi", "GET"),
            ("Ricoh Document Server", "/DRS/document/list", "GET"),
            ("Ricoh Status", "/web/guest/en/websys/status/getUnificationStatus.cgi", "GET"),
        ]
    elif brand == "canon":
        endpoints += [
            ("Canon Remote UI", "/login", "GET"),
            ("Canon WebDAV AdvancedBox", "/WebDAV/AdvancedBox/", "PROPFIND"),
            ("Canon WebDAV root", "/WebDAV/", "PROPFIND"),
            ("Canon Portal", "/portal.html", "GET"),
            ("Canon SSMISession", "/SSMISession", "GET"),
            ("Canon AddressBook", "/oce/addressbook", "GET"),
        ]
    elif brand == "sindoh":
        endpoints += [
            ("Sindoh CGI root", "/cgi-bin/", "GET"),
            ("Sindoh WebGlue", "/webglue/", "GET"),
            ("Sindoh Status", "/status.cgi", "GET"),
            ("Sindoh Config", "/config.cgi", "GET"),
            ("Sindoh AddressBook", "/cgi-bin/addressbook.cgi", "GET"),
            ("Sindoh DocBox", "/cgi-bin/docbox.cgi", "GET"),
        ]

    results = []
    for name, path, method in endpoints:
        for scheme in ["http", "https"]:
            url = f"{scheme}://{ip}{path}"
            try:
                if method == "GET":
                    r = requests.get(url, timeout=TIMEOUT, verify=False, allow_redirects=False)
                elif method == "PROPFIND":
                    r = requests.request("PROPFIND", url, timeout=TIMEOUT, verify=False,
                                         headers={"Depth": "1"}, allow_redirects=False)

                status = r.status_code
                content_type = r.headers.get("Content-Type", "")
                content_length = len(r.content)
                is_success = status < 400

                result = {
                    "name": name,
                    "url": url,
                    "method": method,
                    "status": status,
                    "content_type": content_type,
                    "content_length": content_length,
                    "headers": dict(r.headers),
                }

                # 성공한 경우 응답 본문도 저장 (10KB 이하만)
                if is_success and content_length < 10240:
                    result["body_preview"] = r.text[:5000]

                color = "green" if is_success else "yellow" if status < 500 else "red"
                symbol = "O" if is_success else "X"
                log(f"  [{symbol}] {name}: {status} ({content_type[:30]}, {content_length}B) - {scheme}", color)

                results.append(result)
                if is_success:
                    break  # https 성공하면 http 스킵 또는 반대

            except requests.exceptions.SSLError:
                continue
            except Exception as e:
                results.append({
                    "name": name, "url": url, "method": method,
                    "status": "ERROR", "error": str(e)
                })
                break

    return results


def probe_escl_capabilities(ip):
    """eSCL 상세 정보 수집"""
    log(f"[{ip}] eSCL 상세 정보 수집 중...", "yellow")
    for scheme in ["https", "http"]:
        url = f"{scheme}://{ip}/eSCL/ScannerCapabilities"
        try:
            r = requests.get(url, timeout=TIMEOUT, verify=False)
            if r.status_code == 200 and "xml" in r.headers.get("Content-Type", ""):
                log(f"  eSCL 지원 확인! 응답 크기: {len(r.content)}B", "green")
                return {"supported": True, "url": url, "xml": r.text}
        except:
            continue
    log(f"  eSCL 미지원 또는 접속 불가", "yellow")
    return {"supported": False}


def save_web_pages(ip, brand, open_ports):
    """웹 관리자 페이지 HTML 저장 (로그인 전)"""
    log(f"[{ip}] 웹 페이지 저장 중...", "yellow")
    pages = {}
    for port in [80, 8000, 443, 8443]:
        if port not in open_ports:
            continue
        scheme = "https" if port in [443, 8443] else "http"
        url = f"{scheme}://{ip}:{port}/"
        try:
            r = requests.get(url, timeout=TIMEOUT, verify=False, allow_redirects=True)
            filename = f"{ip.replace('.','_')}_{scheme}_{port}_index.html"
            filepath = RESULT_DIR / filename
            filepath.write_text(r.text, encoding="utf-8")
            pages[url] = {"file": filename, "status": r.status_code, "final_url": r.url}
            log(f"  저장: {filename} ({len(r.content)}B)", "green")
        except Exception as e:
            log(f"  {url} 저장 실패: {e}", "red")
    return pages


def run_full_probe(ip):
    """단일 IP에 대한 전체 프로브"""
    result = {"ip": ip, "timestamp": datetime.now().isoformat()}

    # 포트 스캔
    open_ports = port_scan(ip)
    result["open_ports"] = open_ports

    if not open_ports:
        log(f"[{ip}] 열린 포트 없음 - 연결 불가", "red")
        return result

    # 브랜드 식별
    brand_info = identify_brand(ip, open_ports)
    result["brand_info"] = brand_info
    brand = brand_info["brand"]

    # 엔드포인트 프로브
    endpoints = probe_endpoints(ip, brand, open_ports)
    result["endpoints"] = endpoints

    # eSCL 상세
    escl = probe_escl_capabilities(ip)
    result["escl"] = escl

    # 웹 페이지 저장
    pages = save_web_pages(ip, brand, open_ports)
    result["saved_pages"] = pages

    return result


def get_local_subnet():
    """로컬 IP에서 서브넷 추출"""
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(("8.8.8.8", 80))
        local_ip = s.getsockname()[0]
    finally:
        s.close()
    parts = local_ip.split(".")
    return ".".join(parts[:3]), local_ip


def main():
    log("=" * 60, "cyan")
    log(" 복합기 탐색 검증", "cyan")
    log("=" * 60, "cyan")

    # IP 목록 결정
    if len(sys.argv) > 1 and sys.argv[1] == "--scan":
        subnet, local_ip = get_local_subnet()
        log(f"로컬 IP: {local_ip}, 서브넷: {subnet}.0/24", "cyan")
        ips = scan_subnet_for_http(subnet)
        # 자기 자신 제외
        ips = [ip for ip in ips if ip != local_ip]
        log(f"발견된 장치: {len(ips)}개", "cyan")
    elif len(sys.argv) > 1:
        ips = [arg for arg in sys.argv[1:] if not arg.startswith("-")]
    else:
        print(f"Usage: python {sys.argv[0]} <IP1> [IP2] [IP3]")
        print(f"       python {sys.argv[0]} --scan")
        sys.exit(0)

    if not ips:
        log("탐색된 장치가 없습니다.", "red")
        sys.exit(1)

    # 전체 프로브 실행
    all_results = []
    for ip in ips:
        log(f"\n{'='*50}", "cyan")
        log(f" 프로브 시작: {ip}", "cyan")
        log(f"{'='*50}", "cyan")
        result = run_full_probe(ip)
        all_results.append(result)

    # 결과 저장
    output_file = RESULT_DIR / "discovery_results.json"
    with open(output_file, "w", encoding="utf-8") as f:
        json.dump(all_results, f, ensure_ascii=False, indent=2)
    log(f"\n전체 결과 저장: {output_file}", "green")

    # 요약 출력
    log(f"\n{'='*60}", "cyan")
    log(" 탐색 결과 요약", "cyan")
    log(f"{'='*60}", "cyan")
    for r in all_results:
        brand = r.get("brand_info", {}).get("brand", "unknown")
        title = r.get("brand_info", {}).get("web_title", "")
        ports = r.get("open_ports", [])
        escl = "지원" if r.get("escl", {}).get("supported") else "미지원"
        success_endpoints = [e["name"] for e in r.get("endpoints", [])
                            if isinstance(e.get("status"), int) and e["status"] < 400]

        log(f"\n  {r['ip']}:", "cyan")
        log(f"    브랜드: {brand.upper()}")
        log(f"    웹 타이틀: {title}")
        log(f"    열린 포트: {ports}")
        log(f"    eSCL: {escl}")
        log(f"    성공 엔드포인트: {', '.join(success_endpoints) if success_endpoints else '없음'}")

if __name__ == "__main__":
    main()
