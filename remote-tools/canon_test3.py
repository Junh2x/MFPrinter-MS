"""캐논 수신지 등록 - 최소 단계 테스트"""
import re
import sys
import json
import time
from datetime import datetime
from pathlib import Path
import requests
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

IP = "192.168.11.227"
BASE = f"http://{IP}:8000"
TIMEOUT = 10

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)
LOG_FILE = RESULT_DIR / f"canon_test3_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0",
    "Origin": BASE,
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8",
    "Accept-Language": "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7",
    "Accept-Encoding": "gzip, deflate",
    "Cache-Control": "max-age=0",
    "Upgrade-Insecure-Requests": "1",
}


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    p = f"canon3_{ts}_{name}"
    (RESULT_DIR / f"{p}_meta.json").write_text(json.dumps({
        "url": r.url, "status": r.status_code,
        "req_headers": dict(r.request.headers),
        "req_body": r.request.body if r.request.body else None,
    }, ensure_ascii=False, indent=2), encoding="utf-8")
    (RESULT_DIR / f"{p}_body.html").write_text(r.text, encoding="utf-8")


def parse_submit_form(html):
    """SUBMIT_FORM만 파싱"""
    m = re.search(r'<form[^>]*name=["\']?SUBMIT_FORM["\']?[^>]*>(.*?)</form>', html, re.DOTALL | re.I)
    if not m:
        return []
    form_html = m.group(1)
    fields = []
    for inp in re.finditer(r'<input[^>]+>', form_html, re.I):
        tag = inp.group(0)
        name_m = re.search(r'name=["\']([^"\']+)', tag, re.I)
        val_m = re.search(r'value=["\']([^"\']*)', tag, re.I)
        if name_m:
            fields.append((name_m.group(1), val_m.group(1) if val_m else ""))
    return fields


def main():
    slot = int(sys.argv[1]) if len(sys.argv) > 1 else 6
    s = requests.Session()
    s.verify = False
    s.headers.update(HEADERS)

    log(f"=== 캐논 최소단계 테스트 (슬롯 {slot}) ===")

    # 1. 세션 확보
    log("\n[1] 포털")
    s.get(f"{BASE}/", timeout=TIMEOUT)
    time.sleep(1)

    log("[2] 주소록")
    s.get(f"{BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR&Dummy={int(time.time()*1000)}", timeout=TIMEOUT)
    time.sleep(1)

    # 2. 주소 리스트 → Token A
    log("[3] 주소 리스트")
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={int(time.time()*1000)}",
              timeout=TIMEOUT, headers={"Referer": f"{BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR"})
    token_a = re.search(r'Token=(\d+)', r.text)
    token_a = token_a.group(1) if token_a else None
    log(f"  Token A: {token_a[:20]}...")
    save("03_asublist", r)
    time.sleep(1)

    # 3. 등록 폼 (파일 타입으로 바로 열기)
    log("[4] 등록 폼 (ACLS=7 파일)")
    form_url = (f"{BASE}/rps/aprop.cgi?AMOD=1&AID=11&AIDX={slot}&ACLS=7"
                f"&AFION=1&AdrAction=.%2Falframe.cgi%3F"
                f"&Dummy={int(time.time()*1000)}&Token={token_a}")
    r = s.get(form_url, timeout=TIMEOUT, headers={"Referer": f"{BASE}/rps/albody.cgi"})
    log(f"  → {r.status_code}")
    save("04_form", r)

    fields = parse_submit_form(r.text)
    log(f"  SUBMIT_FORM 필드 {len(fields)}개")
    token_b = None
    for n, v in fields:
        if n == "Token" and v:
            token_b = v
            break
    log(f"  Token B: {token_b[:20] if token_b else 'None'}...")
    time.sleep(2)

    # 4. 바로 등록 POST — aprop.cgi 중간단계 없이
    log("[5] 등록 POST (anewadrs.cgi)")
    reg_fields = []
    for n, v in fields:
        if n == "ANAME":
            reg_fields.append((n, "JA_TEST_V3"))
        elif n == "ANAMEONE":
            reg_fields.append((n, "JA_TEST_V3"))
        elif n == "AREAD":
            reg_fields.append((n, "JA_TEST_V3"))
        elif n == "AAD1":
            reg_fields.append((n, "192.168.11.99"))
        elif n == "APRTCL":
            reg_fields.append((n, "7"))
        elif n == "APATH":
            reg_fields.append((n, "scan_test"))
        elif n == "AUSER":
            reg_fields.append((n, "testuser"))
        elif n == "APWORD":
            reg_fields.append((n, "testpass"))
        elif n == "AdrAction":
            reg_fields.append((n, "./aprop.cgi?"))
        elif n == "Dummy":
            reg_fields.append((n, str(int(time.time()*1000))))
        elif n == "PASSCHK":
            reg_fields.append((n, "1"))
        else:
            reg_fields.append((n, v))

    log("  전송 필드:")
    for n, v in reg_fields:
        if v:
            log(f"    {n}={v[:60]}")

    r = s.post(f"{BASE}/rps/anewadrs.cgi", data=reg_fields, timeout=TIMEOUT,
               headers={"Referer": form_url})
    log(f"  → {r.status_code} ({len(r.content)}B)")
    save("05_register", r)

    if "ERR_SUBMIT_FORM" in r.text:
        log(">>> 실패: ERR_SUBMIT_FORM")
    else:
        log(">>> ERR 없음!")

    # 확인
    time.sleep(1)
    log("\n[6] 최종 목록")
    s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={int(time.time()*1000)}", timeout=TIMEOUT)
    r2 = s.post(f"{BASE}/rps/albody.cgi",
                data={"AID": "11", "FILTER_ID": "0", "Dummy": str(int(time.time()*1000))}, timeout=TIMEOUT)
    match = re.search(r'var\s+adrsList\s*=\s*\[([^\]]*)\]', r2.text, re.DOTALL)
    if match:
        for m in re.finditer(r'nm:"([^"]*)"', match.group(1)):
            name = m.group(1).strip()
            marker = " <<<" if "JA_TEST" in name else ""
            log(f"  {name}{marker}")


if __name__ == "__main__":
    main()
