"""리코 로그인 + 주소록 조회 + 수신지 추가 테스트"""
import re
import sys
import time
import base64
from datetime import datetime
from pathlib import Path
from urllib.parse import quote
import requests
import urllib3
urllib3.disable_warnings()

IP = "192.168.11.185"
BASE = f"http://{IP}"

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)
LOG_FILE = RESULT_DIR / f"ricoh_test_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"ricoh_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def b64(text):
    """리코 방식 base64 인코딩"""
    return base64.b64encode(text.encode()).decode()


def login(s, userid, password):
    """로그인 → wimToken 획득"""
    # 0. 메인 페이지 → 초기 쿠키 (cookieOnOffChecker 등)
    s.get(f"{BASE}/", timeout=10, allow_redirects=True)
    if "cookieOnOffChecker" not in {c.name for c in s.cookies}:
        s.cookies.set("cookieOnOffChecker", "on", domain=IP)
    log(f"[0] 초기 쿠키 설정 OK")
    time.sleep(1)

    # 1. 로그인 폼에서 wimToken 가져오기
    r = s.get(f"{BASE}/web/guest/ko/websys/webArch/authForm.cgi", timeout=10)
    save("1_authform", r)

    wim_match = re.search(r'name=["\']wimToken["\'][^>]*value=["\']([^"\']+)', r.text)
    if not wim_match:
        wim_match = re.search(r'wimToken["\s:=]+["\']?(\d+)', r.text)
    if not wim_match:
        log("[FAIL] wimToken 추출 실패")
        log(f"  응답 일부: {r.text[:500]}")
        return None

    wim_token = wim_match.group(1)
    log(f"[1] wimToken = {wim_token}")

    # 2. 로그인 POST
    login_data = {
        "wimToken": wim_token,
        "userid_work": "",
        "userid": b64(userid),
        "password_work": "",
        "password": b64(password),
        "open": "",
    }
    r = s.post(f"{BASE}/web/guest/ko/websys/webArch/login.cgi",
               data=login_data,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/web/guest/ko/websys/webArch/authForm.cgi"},
               timeout=10)
    save("2_login", r)

    if "risessionid" not in s.cookies and "wimsesid" not in s.cookies:
        log(f"[FAIL] 로그인 실패 — cookies={dict(s.cookies)}")
        return None

    log(f"[2] 로그인 OK — cookies={dict(s.cookies)}")

    # 로그인 후 새 wimToken 가져오기 (주소록 페이지에서)
    r = s.get(f"{BASE}/web/entry/ko/address/adrsList.cgi", timeout=10)
    save("3_adrslist_page", r)

    new_token = re.search(r'name=["\']wimToken["\'][^>]*value=["\'](\d+)', r.text)
    if new_token:
        wim_token = new_token.group(1)
        log(f"[3] 새 wimToken = {wim_token}")
    else:
        log(f"[WARN] 주소록 페이지에서 wimToken 미발견")

    return wim_token


def list_addresses(s):
    """주소록 조회"""
    ts = str(int(time.time() * 1000))
    r = s.get(f"{BASE}/web/entry/ko/address/adrsListLoadEntry.cgi",
              params={"_": ts, "listCountIn": "50", "getCountIn": "1"},
              headers={"X-Requested-With": "XMLHttpRequest",
                       "Accept": "text/plain, */*",
                       "Referer": f"{BASE}/web/entry/ko/address/adrsList.cgi"},
              timeout=10)
    save("4_list", r)
    log(f"[조회] 응답: {r.status_code} ({len(r.content)}B)")

    # 응답 파싱: [[id,type,regNo,name,userCode,lastUsed,fax,email,folder], ...]
    entries = []
    for m in re.finditer(r"\[(\d+),(\d+),'(\d+)','([^']*)',([^,]*),([^,]*),'([^']*)',(?:'([^']*)'|),(?:'([^']*)'|)\]", r.text):
        entry = {
            "id": m.group(1),
            "type": m.group(2),
            "regNo": m.group(3),
            "name": m.group(4),
            "fax": m.group(7),
            "email": m.group(8) or "",
            "folder": m.group(9) or "",
        }
        entries.append(entry)
        log(f"  [{entry['regNo']}] {entry['name']} | 팩스:{entry['fax']} | 메일:{entry['email']} | 폴더:{entry['folder']}")

    if not entries:
        log(f"  파싱 실패 — 원본: {r.text[:300]}")

    return entries


def add_destination(s, wim_token, reg_no, name, display_name, folder_path, folder_user, folder_pass):
    """수신지 추가 (간단입력 4단계)"""
    log(f"\n=== 수신지 추가 ===")
    log(f"  등록번호={reg_no}, 이름={name}, 키표시={display_name}")
    log(f"  폴더={folder_path}, 유저={folder_user}")

    # 추가 폼 페이지 로드 → 위자드 세션 초기화 + wimToken 갱신
    r = s.get(f"{BASE}/web/entry/ko/address/adrsGetUser.cgi?outputSpecifyModeIn=SETTINGS",
              headers={"Referer": f"{BASE}/web/entry/ko/address/adrsList.cgi"},
              timeout=10)
    save("4b_addform", r)
    form_token = re.search(r'name=["\']wimToken["\'][^>]*value=["\'](\d+)', r.text)
    if form_token:
        wim_token = form_token.group(1)
        log(f"  추가 폼 wimToken = {wim_token}")
    else:
        log(f"  [WARN] 추가 폼에서 wimToken 미발견, 기존 토큰 사용")
    time.sleep(1)

    headers = {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
        "X-Requested-With": "XMLHttpRequest",
        "Accept": "text/plain, */*",
        "Referer": f"{BASE}/web/entry/ko/address/adrsList.cgi",
    }
    url = f"{BASE}/web/entry/ko/address/adrsSetUserWizard.cgi"

    # Step 1: BASE (기본 정보)
    data1 = [
        ("mode", "ADDUSER"),
        ("step", "BASE"),
        ("wimToken", wim_token),
        ("entryIndexIn", reg_no),
        ("entryNameIn", name),
        ("entryDisplayNameIn", display_name),
        ("entryTagInfoIn", "2"),
        ("entryTagInfoIn", "10"),
        ("entryTagInfoIn", "6"),
        ("entryTagInfoIn", "1"),
    ]
    r = s.post(url, data=data1, headers=headers, timeout=10)
    save("5_step1_base", r)
    log(f"[Step1] BASE → {r.status_code} ({r.text.strip()})")

    # Step 2: FAX (팩스 — 빈값)
    data2 = {
        "mode": "ADDUSER",
        "step": "FAX",
        "wimToken": wim_token,
        "faxDestIn": "",
    }
    r = s.post(url, data=data2, headers=headers, timeout=10)
    save("6_step2_fax", r)
    log(f"[Step2] FAX → {r.status_code} ({r.text.strip()})")

    # Step 3: FOLDER (폴더 수신처)
    pw_b64 = b64(folder_pass) if folder_pass else ""
    data3 = {
        "mode": "ADDUSER",
        "step": "FOLDER",
        "wimToken": wim_token,
        "folderProtocolIn": "SMB_O",
        "folderPortNoIn": "21",
        "folderServerNameIn": "",
        "folderPathNameIn": folder_path,
        "folderAuthUserNameIn": folder_user,
        "wk_folderPasswordIn": "",
        "folderPasswordIn": pw_b64,
        "wk_folderPasswordConfirmIn": "",
        "folderPasswordConfirmIn": pw_b64,
    }
    r = s.post(url, data=data3, headers=headers, timeout=10)
    save("7_step3_folder", r)
    log(f"[Step3] FOLDER → {r.status_code} ({r.text.strip()})")

    # Step 4: CONFIRM (확정)
    data4 = [
        ("wimToken", wim_token),
        ("stepListIn", "BASE"),
        ("stepListIn", "FAX"),
        ("stepListIn", "FOLDER"),
        ("mode", "ADDUSER"),
        ("step", "CONFIRM"),
    ]
    r = s.post(url, data=data4, headers=headers, timeout=10)
    save("8_step4_confirm", r)
    log(f"[Step4] CONFIRM → {r.status_code}")
    log(f"  응답: {r.text[:300]}")

    if name in r.text:
        log(f"\n>>> 성공! '{name}' 등록 확인됨")
        return True
    else:
        log(f"\n>>> 응답에서 '{name}' 미확인")
        return True  # 응답 형식 확인 필요


if __name__ == "__main__":
    USERID = "admin"
    PASSWORD = ""

    # 추가할 수신지 정보
    REG_NO = "00011"
    NAME = "TEST_RICOH"
    DISPLAY = "TEST_KEY"
    FOLDER = r"\\192.168.11.98\TEST_SCAN"
    FOLDER_USER = "TEST_USER"
    FOLDER_PASS = "1234"

    args = sys.argv[1:]
    if len(args) >= 1: REG_NO = args[0]
    if len(args) >= 2: NAME = args[1]
    if len(args) >= 3: FOLDER = args[2]
    if len(args) >= 4: FOLDER_USER = args[3]
    if len(args) >= 5: FOLDER_PASS = args[4]

    s = requests.Session()
    s.verify = False
    s.headers.update({"User-Agent": UA})

    log("=== 리코 테스트 시작 ===\n")

    # 1. 로그인
    wim_token = login(s, USERID, PASSWORD)
    if not wim_token:
        log("로그인 실패. 종료.")
        sys.exit(1)
    time.sleep(1)

    # 2. 주소록 조회
    log("\n=== 주소록 조회 ===")
    entries = list_addresses(s)
    log(f"  총 {len(entries)}건")
    time.sleep(1)

    # 3. 수신지 추가
    add_destination(s, wim_token, REG_NO, NAME, DISPLAY, FOLDER, FOLDER_USER, FOLDER_PASS)
    time.sleep(1)

    # 4. 추가 후 재조회
    log("\n=== 추가 후 재조회 ===")
    entries = list_addresses(s)
    for e in entries:
        if NAME in e.get("name", ""):
            log(f"  >>> '{NAME}' 확인됨!")
