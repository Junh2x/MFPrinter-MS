"""리코 공통 — 로그인, 세션, 주소록 조회"""
import re
import time
import base64
from datetime import datetime
from pathlib import Path
import requests
import urllib3
urllib3.disable_warnings()

IP = "192.168.11.185"
BASE = f"http://{IP}"
UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)

_log_file = None


def init_log(prefix):
    global _log_file
    _log_file = RESULT_DIR / f"ricoh_{prefix}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    if _log_file:
        with open(_log_file, "a", encoding="utf-8") as f:
            f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"ricoh_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def b64(text):
    return base64.b64encode(text.encode()).decode()


def dummy():
    return str(int(time.time() * 1000))


def create_session():
    s = requests.Session()
    s.verify = False
    s.headers.update({"User-Agent": UA})
    return s


def login(s, userid="admin", password=""):
    """로그인 → wimToken 반환"""
    s.get(f"{BASE}/", timeout=10, allow_redirects=True)
    if "cookieOnOffChecker" not in {c.name for c in s.cookies}:
        s.cookies.set("cookieOnOffChecker", "on", domain=IP)
    log("[0] 초기 쿠키 OK")
    time.sleep(1)

    r = s.get(f"{BASE}/web/guest/ko/websys/webArch/authForm.cgi", timeout=10)
    wim_match = re.search(r'name=["\']wimToken["\'][^>]*value=["\']([^"\']+)', r.text)
    if not wim_match:
        log("[FAIL] wimToken 추출 실패")
        return None
    wim_token = wim_match.group(1)
    log(f"[1] wimToken = {wim_token}")

    r = s.post(f"{BASE}/web/guest/ko/websys/webArch/login.cgi",
               data={"wimToken": wim_token, "userid_work": "", "userid": b64(userid),
                     "password_work": "", "password": b64(password), "open": ""},
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/web/guest/ko/websys/webArch/authForm.cgi"},
               timeout=10)

    if "risessionid" not in s.cookies and "wimsesid" not in s.cookies:
        log(f"[FAIL] 로그인 실패")
        return None
    log(f"[2] 로그인 OK")

    r = s.get(f"{BASE}/web/entry/ko/address/adrsList.cgi", timeout=10)
    new_token = re.search(r'name=["\']wimToken["\'][^>]*value=["\'](\d+)', r.text)
    if new_token:
        wim_token = new_token.group(1)
        log(f"[3] 주소록 wimToken = {wim_token}")
    else:
        log(f"[WARN] 주소록 wimToken 미발견")

    return wim_token


def list_addresses(s):
    """주소록 조회 → 엔트리 리스트"""
    r = s.get(f"{BASE}/web/entry/ko/address/adrsListLoadEntry.cgi",
              params={"_": dummy(), "listCountIn": "50", "getCountIn": "1"},
              headers={"X-Requested-With": "XMLHttpRequest",
                       "Referer": f"{BASE}/web/entry/ko/address/adrsList.cgi"},
              timeout=10)
    entries = []
    for m in re.finditer(r"\[(\d+),(\d+),'(\d+)','([^']*)',([^,]*),([^,]*),'([^']*)',(?:'([^']*)'|),(?:'([^']*)'|)\]", r.text):
        entries.append({
            "id": m.group(1), "type": m.group(2), "regNo": m.group(3),
            "name": m.group(4), "fax": m.group(7),
            "email": m.group(8) or "", "folder": m.group(9) or "",
        })
    return entries


def ajax_headers():
    return {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
        "X-Requested-With": "XMLHttpRequest",
        "Accept": "text/plain, */*",
        "Referer": f"{BASE}/web/entry/ko/address/adrsList.cgi",
    }
