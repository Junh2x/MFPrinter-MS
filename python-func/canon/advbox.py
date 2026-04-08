"""
캐논 고급박스(Advanced Box) 설정
사용법:
    from canon.advbox import setup_advbox
    result = setup_advbox("192.168.11.227")
    result = setup_advbox("192.168.11.227", port=8000, settings={"OpenOutSide": "0"})
"""
import re
import time
import requests
import urllib3

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"

# 고급박스 기본 설정값
DEFAULT_SETTINGS = {
    "OpenOutSide": "1",          # 외부 공개 (1=ON)
    "PermitMakeDir": "1",        # 폴더 생성 허용
    "ReadOnlyMode": "0",         # 읽기 전용 OFF
    "PermitManage": "0",         # 관리 권한
    "PermitFileType": "0",       # 파일 타입 제한
    "OperationLogValid": "1",    # 운영 로그 ON
    "Setting_SMB": "",           # SMB 설정
    "Setting_WebDAV": "",        # WebDAV 설정
    "Setting_DOCLIB": "1",       # 문서 라이브러리 ON
    "WebDav_AuthType": "0",      # WebDAV 인증 타입
    "WebDav_UseSSL": "1",        # WebDAV SSL
    "AutoDelete": "1",           # 자동 삭제 ON
    "AutoDeleteTime_HH": "00",   # 자동 삭제 시간
    "AutoDeleteTime_MM": "00",   # 자동 삭제 분
}


def _dummy():
    return str(int(time.time() * 1000))


def _get_token(html):
    m = re.search(r'name=["\']Token["\'][^>]*value=["\']([^"\']+)', html)
    return m.group(1) if m else None


def setup_advbox(ip, port=8000, settings=None, callback=None):
    """
    캐논 고급박스 설정을 적용한다.

    Args:
        ip:       복합기 IP
        port:     HTTP 포트 (기본 8000)
        settings: 오버라이드할 설정 dict (DEFAULT_SETTINGS 기반)
        callback: 진행 콜백 fn(step, message)

    Returns:
        dict: {
            "success": bool,
            "ip": str,
            "message": str,
            "settings_applied": dict,  # 실제 적용된 설정
        }
    """
    base = f"http://{ip}:{port}"
    applied = {**DEFAULT_SETTINGS}
    if settings:
        applied.update(settings)

    def _cb(step, msg):
        if callback:
            callback(step, msg)

    s = requests.Session()
    s.verify = False
    s.headers.update({
        "User-Agent": UA,
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Encoding": "gzip, deflate",
        "Accept-Language": "ko-KR,ko;q=0.9",
        "Upgrade-Insecure-Requests": "1",
    })

    # Step 1: 포털 접속 → 쿠키 획득
    _cb(1, "포털 접속 중...")
    try:
        r = s.get(f"{base}/", timeout=10)
        if r.status_code != 200:
            return {"success": False, "ip": ip, "message": f"포털 접속 실패: HTTP {r.status_code}",
                    "settings_applied": None}
    except requests.RequestException as e:
        return {"success": False, "ip": ip, "message": f"연결 실패: {e}",
                "settings_applied": None}

    # Step 2: nativetop → iR 쿠키
    _cb(2, "세션 초기화 중...")
    try:
        r = s.get(
            f"{base}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={_dummy()}",
            timeout=10, headers={"Referer": f"{base}/"}
        )
        if "iR" not in s.cookies:
            return {"success": False, "ip": ip, "message": "iR 쿠키 미발급 - 세션 초기화 실패",
                    "settings_applied": None}
    except requests.RequestException as e:
        return {"success": False, "ip": ip, "message": f"세션 초기화 실패: {e}",
                "settings_applied": None}

    # Step 3: 고급박스 설정 페이지 → Token 추출
    _cb(3, "설정 페이지 토큰 추출 중...")
    settings_url = f"{base}/rps/cdsuperbox.cgi?Flag=Init_Data&PageFlag=c_superbox.tpl&FuncTypeFlag=SettingPage&Dummy={_dummy()}"
    try:
        r = s.get(settings_url, timeout=10)
        token = _get_token(r.text)
        if not token:
            return {"success": False, "ip": ip, "message": "Token 추출 실패",
                    "settings_applied": None}
    except requests.RequestException as e:
        return {"success": False, "ip": ip, "message": f"설정 페이지 접근 실패: {e}",
                "settings_applied": None}

    # Step 4: 설정 POST
    _cb(4, "고급박스 설정 적용 중...")
    payload = {
        **applied,
        "Flag": "Exec_Data",
        "PageFlag": "c_sboxlist.tpl",
        "FuncTypeFlag": "SettingPage",
        "CoreNXAction": "./cdsuperbox.cgi",
        "CoreNXPage": "c_superbox.tpl",
        "disp": "",
        "Dummy": _dummy(),
        "Token": token,
    }

    try:
        r = s.post(
            f"{base}/rps/cdsuperbox.cgi",
            data=payload,
            headers={
                "Content-Type": "application/x-www-form-urlencoded",
                "Origin": base,
                "Referer": settings_url,
            },
            timeout=10,
        )

        if r.status_code != 200 or "ERR" in r.text:
            return {"success": False, "ip": ip, "message": f"설정 실패: HTTP {r.status_code}",
                    "settings_applied": applied}

    except requests.RequestException as e:
        return {"success": False, "ip": ip, "message": f"설정 POST 실패: {e}",
                "settings_applied": applied}

    _cb(5, "완료")
    return {
        "success": True,
        "ip": ip,
        "message": "고급박스 설정 완료",
        "settings_applied": applied,
    }


# ---------------------------------------------------------------------------
# CLI 테스트
# ---------------------------------------------------------------------------
if __name__ == "__main__":
    import sys

    ip = sys.argv[1] if len(sys.argv) > 1 else "192.168.11.227"
    port = int(sys.argv[2]) if len(sys.argv) > 2 else 8000

    def _print_cb(step, msg):
        print(f"  [Step {step}] {msg}")

    print(f"캐논 고급박스 설정: {ip}:{port}")
    print("-" * 40)

    result = setup_advbox(ip, port=port, callback=_print_cb)

    print("-" * 40)
    if result["success"]:
        print(f"SUCCESS: {result['message']}")
        print(f"적용된 설정:")
        for k, v in result["settings_applied"].items():
            print(f"  {k} = {v}")
    else:
        print(f"FAIL: {result['message']}")
