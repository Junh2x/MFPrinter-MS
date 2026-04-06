"""캐논 수신지 삭제 — API.md 캡처 기반"""
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
LOG_FILE = RESULT_DIR / f"canon_delete_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"del_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def dummy():
    return str(int(time.time() * 1000))


def get_token_from_url(html):
    m = re.search(r'Token=(\d+)', html)
    return m.group(1) if m else None


def get_token_from_hidden(html):
    m = re.search(r'name=["\']Token["\'][^>]*value=["\']([^"\']+)', html)
    return m.group(1) if m else None


def delete_destination(aid, slot):
    s = requests.Session()
    s.verify = False
    s.headers.update({
        "User-Agent": UA,
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Encoding": "gzip, deflate",
        "Accept-Language": "ko-KR,ko;q=0.9",
        "Upgrade-Insecure-Requests": "1",
    })

    log(f"=== 캐논 수신지 삭제 ===")
    log(f"  AID={aid}, AIDX={slot}")

    # 포털 → 쿠키
    r = s.get(f"{BASE}/", timeout=10)
    if r.status_code != 200:
        log(f"[FAIL] 포털 접속 실패: {r.status_code}")
        return False
    log(f"[1] 포털 OK")
    time.sleep(1)

    # nativetop → iR 쿠키
    s.get(f"{BASE}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={dummy()}",
          timeout=10, headers={"Referer": f"{BASE}/"})
    if "iR" not in s.cookies:
        log("[FAIL] iR 쿠키 미설정")
        return False
    log(f"[2] nativetop OK — iR={s.cookies['iR']}")
    time.sleep(1)

    # asublist
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={dummy()}", timeout=10)
    log(f"[3] asublist OK")
    time.sleep(1)

    # alframe
    s.get(f"{BASE}/rps/alframe.cgi?AID={aid}", timeout=10)
    log(f"[4] alframe OK")
    time.sleep(1)

    # albody → Token (삭제용)
    r = s.post(f"{BASE}/rps/albody.cgi",
               data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/alframe.cgi?"},
               timeout=10)
    save("5_albody", r)
    token = get_token_from_hidden(r.text)
    if not token:
        log("[FAIL] Token 추출 실패 (albody)")
        return False

    # 삭제 대상 확인
    found_name = None
    for m in re.finditer(r'\{[^}]*idx:(\d+)[^}]*nm:"([^"]*)"', r.text):
        idx = int(m.group(1))
        nm = m.group(2).strip()
        if idx == slot and nm:
            found_name = nm
    if found_name:
        log(f"[5] 삭제 대상: [{slot}] {found_name}")
    else:
        log(f"[5] 슬롯 {slot}에 해당하는 수신지를 목록에서 찾지 못함 (삭제 시도는 계속)")
    log(f"    Token = {token[:15]}...")
    time.sleep(1)

    # adelete.cgi (삭제 실행)
    body = (f"AMOD=0&AID={aid}&AIDX={slot}&ACLS=7&AFION=1"
            f"&AdrAction=.%2Falframe.cgi%3F&Dummy={dummy()}&Token={token}")
    r = s.post(f"{BASE}/rps/adelete.cgi?",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Origin": BASE,
                        "Referer": f"{BASE}/rps/albody.cgi"},
               timeout=10)
    save("6_delete", r)

    if "ERR" in r.text:
        log(f"[FAIL] 삭제 실패 ({len(r.content)}B)")
        return False

    log(f"[6] 삭제 응답 OK ({len(r.content)}B)")
    log(f"\n>>> 삭제 완료! (AIDX={slot})")
    return True


if __name__ == "__main__":
    AID = "11"
    SLOT = 1  # 삭제할 슬롯 번호

    args = sys.argv[1:]
    if len(args) >= 1: SLOT = int(args[0])

    log(f"설정: AID={AID}, SLOT={SLOT}")
    log("")

    success = delete_destination(AID, SLOT)
    sys.exit(0 if success else 1)
