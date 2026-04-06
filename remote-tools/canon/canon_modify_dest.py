"""캐논 수신지 수정 — API.md 캡처 기반"""
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
LOG_FILE = RESULT_DIR / f"canon_modify_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"mod_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def dummy():
    return str(int(time.time() * 1000))


def get_token_from_url(html):
    m = re.search(r'Token=(\d+)', html)
    return m.group(1) if m else None


def get_token_from_hidden(html):
    m = re.search(r'name=["\']Token["\'][^>]*value=["\']([^"\']+)', html)
    return m.group(1) if m else None


def init_session():
    """세션 초기화: 포털 → nativetop → 쿠키 확보"""
    s = requests.Session()
    s.verify = False
    s.headers.update({
        "User-Agent": UA,
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Encoding": "gzip, deflate",
        "Accept-Language": "ko-KR,ko;q=0.9",
        "Upgrade-Insecure-Requests": "1",
    })

    r = s.get(f"{BASE}/", timeout=10)
    if r.status_code != 200:
        log(f"[FAIL] 포털 접속 실패: {r.status_code}")
        return None
    log(f"[1] 포털 OK")
    time.sleep(1)

    s.get(f"{BASE}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={dummy()}",
          timeout=10, headers={"Referer": f"{BASE}/"})
    if "iR" not in s.cookies:
        log("[FAIL] iR 쿠키 미설정")
        return None
    log(f"[2] nativetop OK — iR={s.cookies['iR']}")
    time.sleep(1)

    return s


def get_token_a(s):
    """asublist에서 Token A 획득"""
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={dummy()}", timeout=10)
    token = get_token_from_url(r.text)
    if not token:
        log("[FAIL] Token A 추출 실패")
        return None, r
    log(f"[3] Token A = {token[:15]}...")
    return token, r


def modify_destination(aid, slot, name, btn_name, host_ip, folder, user, password, change_pw):
    s = init_session()
    if not s:
        return False

    log(f"\n=== 캐논 수신지 수정 ===")
    log(f"  AID={aid}, AIDX={slot}")
    log(f"  이름={name}, 버튼명칭={btn_name}")
    log(f"  호스트={host_ip}, 폴더={folder}, 유저={user}")
    log(f"  비밀번호 변경={'예' if change_pw else '아니오'}")

    # Token A 획득
    token_a, _ = get_token_a(s)
    if not token_a:
        return False
    time.sleep(1)

    # alframe (AID 컨텍스트)
    s.get(f"{BASE}/rps/alframe.cgi?AID={aid}", timeout=10)
    log(f"[4] alframe OK")
    time.sleep(1)

    # albody (목록 확인)
    r = s.post(f"{BASE}/rps/albody.cgi",
               data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/alframe.cgi?"},
               timeout=10)
    save("5_albody", r)
    log(f"[5] albody OK")
    time.sleep(1)

    # aprop (수정 폼 열기: AMOD=2) → Token B
    body = (f"AMOD=2&AID={aid}&AIDX={slot}&ACLS=7&AFION=1"
            f"&AdrAction=.%2Falframe.cgi%3F&Dummy={dummy()}&Token={token_a}")
    r = s.post(f"{BASE}/rps/aprop.cgi?",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/albody.cgi"},
               timeout=10)
    save("6_aprop_edit", r)
    token_b = get_token_from_hidden(r.text)
    if not token_b:
        log("[FAIL] Token B 추출 실패 (수정 폼)")
        return False
    log(f"[6] Token B = {token_b[:15]}... (수정 폼)")
    time.sleep(1)

    # amodadrs.cgi (최종 수정)
    passchk = "1" if change_pw else "0"
    pw_val = password if change_pw else ""

    body = (f"AID={aid}&PageFlag=&AIDX={slot}"
            f"&ANAME={name}&AAD1={host_ip}&APATH={folder}&AUSER={user}"
            f"&INPUT_PSWD=0&PASSCHK={passchk}&APWORD={pw_val}"
            f"&ACLS=7&APRTCL=7&APNO=0"
            f"&AREAD={name}&ANAMEONE={btn_name}"
            f"&AMOD=0&Dummy={dummy()}"
            f"&AdrAction=.%2Falframe.cgi%3F"
            f"&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
            f"&Token={token_b}")
    r = s.post(f"{BASE}/rps/amodadrs.cgi",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Origin": BASE,
                        "Referer": f"{BASE}/rps/aprop.cgi"},
               timeout=10)
    save("7_modify", r)

    if "ERR" in r.text and "ERR_SUBMIT_FORM" not in r.text:
        log(f"[FAIL] 수정 실패 ({len(r.content)}B)")
        return False

    log(f"[7] 수정 응답 OK ({len(r.content)}B)")
    log(f"\n>>> 수정 완료!")
    return True


if __name__ == "__main__":
    AID = "11"
    SLOT = 1           # 수정할 슬롯 번호
    NAME = "TEST_NAME_MODIFIED"
    BTN = "TEST_BTN_MODIFIED"
    HOST = "192.168.11.98"
    FOLDER = "TEST_SCAN_MOD"
    USER = "TEST_USER_MOD"
    PASS = ""
    CHANGE_PW = False  # 비밀번호 변경 여부

    # 인자: python canon_modify_dest.py [slot] [name] [btn] [host] [folder] [user] [pass]
    args = sys.argv[1:]
    if len(args) >= 1: SLOT = int(args[0])
    if len(args) >= 2: NAME = args[1]
    if len(args) >= 3: BTN = args[2]
    if len(args) >= 4: HOST = args[3]
    if len(args) >= 5: FOLDER = args[4]
    if len(args) >= 6: USER = args[5]
    if len(args) >= 7:
        PASS = args[6]
        CHANGE_PW = True

    log(f"설정: AID={AID}, SLOT={SLOT}")
    log(f"  이름={NAME}, 버튼명칭={BTN}")
    log(f"  호스트={HOST}, 폴더={FOLDER}, 유저={USER}")
    log(f"  비밀번호 변경={'예 (****)' if CHANGE_PW else '아니오'}")
    log("")

    success = modify_destination(AID, SLOT, NAME, BTN, HOST, FOLDER, USER, PASS, CHANGE_PW)
    sys.exit(0 if success else 1)
