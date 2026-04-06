"""캐논 고급박스 설정 자동화"""
import re
import sys
import time
from datetime import datetime
from pathlib import Path
import requests
import urllib3
urllib3.disable_warnings()

IP = "192.168.11.227"
BASE = f"http://{IP}:8000"

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)
LOG_FILE = RESULT_DIR / f"canon_advbox_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"advbox_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def dummy():
    return str(int(time.time() * 1000))


def get_token(html):
    m = re.search(r'name=["\']Token["\'][^>]*value=["\']([^"\']+)', html)
    return m.group(1) if m else None


def main():
    s = requests.Session()
    s.verify = False
    s.headers.update({
        "User-Agent": UA,
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Encoding": "gzip, deflate",
        "Accept-Language": "ko-KR,ko;q=0.9",
        "Upgrade-Insecure-Requests": "1",
    })

    log("=== 캐논 고급박스 설정 ===\n")

    # Step 1: 포털 → 쿠키
    r = s.get(f"{BASE}/", timeout=10)
    if r.status_code != 200:
        log(f"[FAIL] 포털 접속 실패: {r.status_code}")
        return False
    log(f"[1] 포털 OK")
    time.sleep(1)

    # Step 2: nativetop → iR 쿠키
    r = s.get(f"{BASE}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={dummy()}",
              timeout=10, headers={"Referer": f"{BASE}/"})
    if "iR" not in s.cookies:
        log("[FAIL] iR 쿠키 미설정")
        return False
    log(f"[2] nativetop OK — iR={s.cookies['iR']}")
    time.sleep(1)

    # Step 3: 고급박스 설정 페이지 → Token 추출
    settings_url = f"{BASE}/rps/cdsuperbox.cgi?Flag=Init_Data&PageFlag=c_superbox.tpl&FuncTypeFlag=SettingPage&Dummy={dummy()}"
    r = s.get(settings_url, timeout=10)
    save("3_settings_page", r)
    token = get_token(r.text)
    if not token:
        log(f"[FAIL] Token 추출 실패")
        log(f"  응답: {r.status_code} ({len(r.content)}B)")
        return False
    log(f"[3] 설정 페이지 OK — Token={token[:15]}...")
    time.sleep(1)

    # Step 4: 고급박스 설정 POST (페이로드 고정값)
    payload = {
        "OpenOutSide": "1",          # 외부 공개
        "PermitMakeDir": "1",        # 폴더 생성 허용
        "ReadOnlyMode": "0",         # 읽기 전용 아님
        "PermitManage": "0",         # 관리
        "PermitFileType": "0",       # 파일 타입
        "OperationLogValid": "1",    # 운영 로그
        "Setting_SMB": "",           # SMB
        "Setting_WebDAV": "",        # WebDAV
        "Setting_DOCLIB": "1",       # 문서 라이브러리
        "WebDav_AuthType": "0",      # WebDAV 인증 타입
        "WebDav_UseSSL": "1",        # WebDAV SSL
        "AutoDelete": "1",           # 자동 삭제
        "AutoDeleteTime_HH": "00",   # 자동 삭제 시
        "AutoDeleteTime_MM": "00",   # 자동 삭제 분
        "Flag": "Exec_Data",
        "PageFlag": "c_sboxlist.tpl",
        "FuncTypeFlag": "SettingPage",
        "CoreNXAction": "./cdsuperbox.cgi",
        "CoreNXPage": "c_superbox.tpl",
        "disp": "",
        "Dummy": dummy(),
        "Token": token,
    }

    r = s.post(f"{BASE}/rps/cdsuperbox.cgi",
               data=payload,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Origin": BASE,
                        "Referer": settings_url},
               timeout=10)
    save("4_settings_result", r)

    log(f"[4] 설정 POST → {r.status_code} ({len(r.content)}B)")

    if "ERR" in r.text:
        log(f"[FAIL] 설정 실패")
        return False

    log(f"\n>>> 고급박스 설정 완료!")
    return True


if __name__ == "__main__":
    main()
