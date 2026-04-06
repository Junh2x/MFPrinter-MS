"""캐논 주소록/수신지 조회"""
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
LOG_FILE = RESULT_DIR / f"canon_list_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"

# 수신지 타입
TYPE_MAP = {2: "이메일", 7: "파일(SMB)"}


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def dummy():
    return str(int(time.time() * 1000))


def init_session():
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


def parse_book_name_list(html):
    """BookNameList 파싱 — {id: '이름', ...}"""
    match = re.search(r'BookNameList\s*=\s*\{(.*?)\}', html, re.DOTALL)
    if not match:
        return {}
    books = {}
    for m in re.finditer(r'(\d+)\s*:\s*"([^"]*)"', match.group(1)):
        books[int(m.group(1))] = m.group(2).strip()
    return books


def parse_adrs_list(html):
    """adrsList 파싱 — {idx: {tp, nm, ad, ot}, ...}"""
    match = re.search(r'var\s+adrsList\s*=\s*\{(.*?)\};', html, re.DOTALL)
    if not match:
        return {}
    entries = {}
    for m in re.finditer(
        r'(\d+)\s*:\s*\{([^}]*)\}', match.group(1)
    ):
        idx = int(m.group(1))
        props = m.group(2)
        entry = {}
        for pm in re.finditer(r'(\w+)\s*:\s*(?:(\d+)|"([^"]*)")', props):
            key = pm.group(1)
            val = pm.group(2) if pm.group(2) else pm.group(3).strip()
            entry[key] = val
        entries[idx] = entry
    return entries


def list_books(s):
    """주소록 목록 조회 (asublist → BookNameList)"""
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&Dummy={dummy()}", timeout=10)
    books = parse_book_name_list(r.text)
    return books


def list_destinations(s, aid):
    """주소록 내 수신지 목록 조회 (albody → adrsList)"""
    s.get(f"{BASE}/rps/alframe.cgi?AID={aid}", timeout=10)
    time.sleep(1)
    r = s.post(f"{BASE}/rps/albody.cgi",
               data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/alframe.cgi?"},
               timeout=10)
    entries = parse_adrs_list(r.text)
    return entries


def main():
    aid = sys.argv[1] if len(sys.argv) > 1 else None

    s = init_session()
    if not s:
        return

    # 주소록 목록 조회
    log("\n=== 주소록 목록 ===")
    books = list_books(s)
    if books:
        for bid, name in sorted(books.items()):
            log(f"  [{bid:2d}] {name}")
    else:
        log("  주소록 목록 파싱 실패")
    time.sleep(1)

    # 수신지 조회 (AID 지정 시 해당 주소록만, 미지정 시 전체)
    if aid:
        aids = [aid]
    else:
        aids = sorted(books.keys()) if books else ["11"]

    for a in aids:
        book_name = books.get(int(a), f"AID={a}") if books else f"AID={a}"
        log(f"\n=== [{a}] {book_name} — 수신지 목록 ===")
        entries = list_destinations(s, a)
        if entries:
            for idx in sorted(entries.keys()):
                e = entries[idx]
                tp = TYPE_MAP.get(int(e.get('tp', 0)), e.get('tp', '?'))
                nm = e.get('nm', '')
                ad = e.get('ad', '')
                ot = e.get('ot', '')
                log(f"  [{idx:3d}] {nm} | {tp} | {ad} | 버튼: {ot}")
        else:
            log("  (비어있음)")
        time.sleep(1)

    log(f"\n>>> 조회 완료")


if __name__ == "__main__":
    main()
