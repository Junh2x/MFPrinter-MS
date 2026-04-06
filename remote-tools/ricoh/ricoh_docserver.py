"""리코 문서 서버 — 폴더 조회 + 생성"""
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
LOG_FILE = RESULT_DIR / f"ricoh_docserver_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"docsvr_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def b64(text):
    return base64.b64encode(text.encode()).decode()


def get_wim_token(html):
    m = re.search(r'name=["\']wimToken["\'][^>]*value=["\'](\d+)', html)
    return m.group(1) if m else None


def login(s):
    """로그인 → 세션 확보"""
    s.get(f"{BASE}/", timeout=10, allow_redirects=True)
    if "cookieOnOffChecker" not in {c.name for c in s.cookies}:
        s.cookies.set("cookieOnOffChecker", "on", domain=IP)
    log("[0] 초기 쿠키 OK")
    time.sleep(1)

    r = s.get(f"{BASE}/web/guest/ko/websys/webArch/authForm.cgi", timeout=10)
    wim_match = re.search(r'name=["\']wimToken["\'][^>]*value=["\']([^"\']+)', r.text)
    if not wim_match:
        log("[FAIL] wimToken 추출 실패")
        return False
    wim_token = wim_match.group(1)

    r = s.post(f"{BASE}/web/guest/ko/websys/webArch/login.cgi",
               data={"wimToken": wim_token, "userid_work": "", "userid": b64("admin"),
                     "password_work": "", "password": b64(""), "open": ""},
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/web/guest/ko/websys/webArch/authForm.cgi"},
               timeout=10)
    if "wimsesid" not in s.cookies:
        log("[FAIL] 로그인 실패")
        return False
    log("[1] 로그인 OK")
    return True


def list_folders(s):
    """문서 서버 폴더 목록 조회"""
    r = s.get(f"{BASE}/web/entry/ko/webdocbox/folderListPage.cgi",
              headers={"Referer": f"{BASE}/web/entry/ko/websys/webArch/topPage.cgi"},
              timeout=10)
    save("list_folders", r)

    wim_token = get_wim_token(r.text)
    log(f"[조회] folderListPage → {r.status_code} ({len(r.content)}B)")
    if wim_token:
        log(f"  wimToken = {wim_token}")

    # 폴더 정보 파싱 시도
    folders = re.findall(r'folderName["\s:=]+["\']([^"\']+)', r.text)
    if folders:
        for f in folders:
            log(f"  폴더: {f}")
    else:
        # 대체 파싱: title이나 id 패턴
        folder_ids = re.findall(r'targetFolderId["\s:=]+["\']?(\d+)', r.text)
        folder_names = re.findall(r'<title[^>]*>([^<]*폴더[^<]*)</title>', r.text)
        if folder_ids:
            log(f"  폴더 ID: {folder_ids}")
        if folder_names:
            log(f"  폴더명: {folder_names}")

    return wim_token, r.text


def create_folder(s, wim_token, folder_id, folder_name, password=""):
    """문서 서버 폴더 생성 (4단계)"""
    log(f"\n=== 폴더 생성 ===")
    log(f"  ID={folder_id}, 이름={folder_name}, 비밀번호={'***' if password else '없음'}")

    now = datetime.now()
    hour = now.strftime("%H")
    minute = now.strftime("%M")

    headers = {
        "Content-Type": "application/x-www-form-urlencoded",
        "Origin": BASE,
        "Upgrade-Insecure-Requests": "1",
    }

    # Step 1: 폴더 생성 폼 진입
    r = s.post(f"{BASE}/web/entry/ko/webdocbox/folderPropPage.cgi",
               data={
                   "wimToken": wim_token,
                   "mode": "CREATE",
                   "selectedDocIds": "",
                   "subReturnDsp": "",
                   "useInputParam": "",
                   "useSavedPropParam": "false",
                   "_hour": "",
                   "_min": "",
                   "_hour": hour,
                   "_min": minute,
               },
               headers={**headers, "Referer": f"{BASE}/web/entry/ko/webdocbox/folderListPage.cgi"},
               timeout=10)
    save("create_1_form", r)
    log(f"[Step1] 생성 폼 진입 → {r.status_code}")
    time.sleep(1)

    # Step 2: 비밀번호 설정 페이지 (폴더명/ID 전달)
    r = s.post(f"{BASE}/web/entry/ko/webdocbox/chPasswordPage.cgi",
               data={
                   "wimToken": wim_token,
                   "targetFolderId": folder_id,
                   "changedFolderName": folder_name,
                   "mode": "CREATE",
                   "targetDocId": folder_id,
                   "selectedFolderId": "",
                   "title": folder_name,
                   "useSavedPropParam": "true",
                   "useInputParam": "false",
                   "subReturnDsp": "3",
                   "dummy": "",
               },
               headers={**headers, "Referer": f"{BASE}/web/entry/ko/webdocbox/folderPropPage.cgi"},
               timeout=10)
    save("create_2_password_page", r)
    log(f"[Step2] 비밀번호 페이지 → {r.status_code}")
    time.sleep(1)

    # Step 3: 비밀번호 설정 완료
    pw_b64 = b64(password) if password else ""
    r = s.post(f"{BASE}/web/entry/ko/webdocbox/commitChPassword.cgi",
               data={
                   "wimToken": wim_token,
                   "title": folder_name,
                   "creator": "",
                   "dataFormat": "",
                   "allPages": "false",
                   "cid": "",
                   "convBW": "",
                   "backUp": "",
                   "backUpFormatStr": "",
                   "backUpResoStr": "",
                   "targetDocId": folder_id,
                   "oldPassword": "undefined",
                   "newPassword": pw_b64,
                   "confirmation": pw_b64,
                   "useInputParam": "false",
                   "useSavedParam": "",
                   "subReturnDsp": "3",
                   "mode": "CREATE",
                   "wayTo": "",
                   "useSavedPropParam": "true",
                   "selectedFolderId": "",
                   "ID": "",
                   "dummy": "",
               },
               headers={**headers, "Referer": f"{BASE}/web/entry/ko/webdocbox/chPasswordPage.cgi"},
               timeout=10)
    save("create_3_commit_pw", r)
    log(f"[Step3] 비밀번호 설정 → {r.status_code}")
    time.sleep(1)

    # Step 4: 폴더 생성 확정
    r = s.post(f"{BASE}/web/entry/ko/webdocbox/folderPropPage.cgi",
               data={
                   "wimToken": wim_token,
                   "id": "",
                   "jt": "",
                   "el": "",
                   "urlLang": "ko",
                   "urlProfile": "entry",
                   "pdfThumbnailURI": "",
                   "thumbnailURI": "",
                   "WidthSize": "",
                   "subdocCount": "",
                   "targetDocId": folder_id,
                   "title": "",
                   "creator": "",
                   "useInputParam": "false",
                   "useSavedParam": "",
                   "subReturnDsp": "3",
                   "mode": "CREATE",
                   "wayTo": "",
                   "selectedFolderId": folder_id,
                   "useSavedPropParam": "true",
                   "ID": "",
                   "simpleErrorMessage": "",
                   "dummy": "",
               },
               headers={**headers, "Referer": f"{BASE}/web/entry/ko/webdocbox/commitChPassword.cgi"},
               timeout=10)
    save("create_4_confirm", r)
    log(f"[Step4] 생성 확정 → {r.status_code} ({len(r.content)}B)")

    if folder_name in r.text:
        log(f"\n>>> 성공! '{folder_name}' 생성 확인됨")
    else:
        log(f"\n>>> 생성 완료 (응답에서 '{folder_name}' 미확인 — 목록 재조회로 확인)")

    return True


if __name__ == "__main__":
    FOLDER_ID = "003"
    FOLDER_NAME = "TEST_FOLDER"
    PASSWORD = "1234"

    args = sys.argv[1:]
    if len(args) >= 1: FOLDER_ID = args[0]
    if len(args) >= 2: FOLDER_NAME = args[1]
    if len(args) >= 3: PASSWORD = args[2]

    log(f"설정: ID={FOLDER_ID}, 이름={FOLDER_NAME}")
    log(f"  비밀번호={'***' if PASSWORD else '없음'}")
    log("")

    s = requests.Session()
    s.verify = False
    s.headers.update({"User-Agent": UA})

    if not login(s):
        sys.exit(1)
    time.sleep(1)

    # 폴더 목록 조회
    log("\n=== 폴더 목록 조회 ===")
    wim_token, _ = list_folders(s)
    if not wim_token:
        log("[FAIL] wimToken 없음")
        sys.exit(1)
    time.sleep(1)

    # 폴더 생성
    create_folder(s, wim_token, FOLDER_ID, FOLDER_NAME, PASSWORD)
    time.sleep(1)

    # 생성 후 재조회
    log("\n=== 생성 후 재조회 ===")
    list_folders(s)
