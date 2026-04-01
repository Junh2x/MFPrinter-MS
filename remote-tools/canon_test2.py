"""캐논 수신지 등록 - 폼 hidden 필드 그대로 파싱해서 전송"""
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
LOG_FILE = RESULT_DIR / f"canon_test2_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0",
    "Origin": BASE,
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
    "Accept-Language": "ko-KR,ko;q=0.9",
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
    p = f"canon2_{ts}_{name}"
    (RESULT_DIR / f"{p}_meta.json").write_text(json.dumps({
        "url": r.url, "status": r.status_code, "method": r.request.method,
        "req_headers": dict(r.request.headers),
        "req_body": r.request.body if r.request.body else None,
        "resp_headers": dict(r.headers),
    }, ensure_ascii=False, indent=2), encoding="utf-8")
    (RESULT_DIR / f"{p}_body.html").write_text(r.text, encoding="utf-8")


def parse_form(html, form_name="SUBMIT_FORM"):
    """HTML에서 특정 form의 hidden 필드를 모두 추출. 중복 키 지원."""
    # form 태그 찾기
    pattern = rf'<form[^>]*name=["\']?{form_name}["\']?[^>]*>(.*?)</form>'
    m = re.search(pattern, html, re.DOTALL | re.I)
    if not m:
        return None, []

    form_html = m.group(0)

    # action 추출
    action_m = re.search(r'action=["\']([^"\']*)', form_html, re.I)
    action = action_m.group(1) if action_m else ""

    # 모든 hidden input 추출 (순서 유지, 중복 키 허용)
    fields = []
    for inp in re.finditer(r'<input[^>]+>', form_html, re.I):
        tag = inp.group(0)
        type_m = re.search(r'type=["\']([^"\']+)', tag, re.I)
        if type_m and type_m.group(1).lower() != "hidden":
            continue
        name_m = re.search(r'name=["\']([^"\']+)', tag, re.I)
        val_m = re.search(r'value=["\']([^"\']*)', tag, re.I)
        if name_m:
            fields.append((name_m.group(1), val_m.group(1) if val_m else ""))

    return action, fields


def main():
    slot = int(sys.argv[1]) if len(sys.argv) > 1 else 5
    s = requests.Session()
    s.verify = False
    s.headers.update(HEADERS)

    log("=" * 50)
    log(f"캐논 수신지 등록 테스트 v2 (슬롯 {slot})")
    log("=" * 50)

    # 1. 세션
    log("\n[1] 포털")
    r = s.get(f"{BASE}/", timeout=TIMEOUT)
    log(f"  → {r.status_code}, 쿠키: {dict(s.cookies)}")

    log("\n[2] 주소록 진입")
    r = s.get(f"{BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR&Dummy={int(time.time()*1000)}", timeout=TIMEOUT)
    log(f"  → {r.status_code}")

    # 2. 주소 리스트
    log("\n[3] 주소 리스트")
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={int(time.time()*1000)}", timeout=TIMEOUT,
              headers={"Referer": f"{BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR"})
    log(f"  → {r.status_code}")
    save("03_asublist", r)

    token_a = re.search(r'Token=(\d+)', r.text)
    token_a = token_a.group(1) if token_a else None
    log(f"  Token A: {token_a[:20] if token_a else 'None'}...")

    if not token_a:
        log("Token A 실패. 종료.")
        return

    # 3. 등록 폼 1단계 - 이메일 타입으로 열기
    log("\n[4] 등록 폼 (이메일)")
    form_url = f"{BASE}/rps/aprop.cgi?AMOD=1&AID=11&AIDX={slot}&ACLS=2&AFION=1&AdrAction=.%2Falframe.cgi%3F&Dummy={int(time.time()*1000)}&Token={token_a}"
    r = s.get(form_url, timeout=TIMEOUT, headers={"Referer": f"{BASE}/rps/albody.cgi"})
    log(f"  → {r.status_code}")
    save("04_form_email", r)

    action1, fields1 = parse_form(r.text)
    log(f"  폼 action: {action1}, 필드 {len(fields1)}개")
    if not fields1:
        log("폼 파싱 실패. 종료.")
        return

    # 4. 타입 변경 - 파일(SMB)로 변경
    log("\n[5] 타입 변경 (파일)")
    # SUBMIT_FORM 필드를 기반으로, ACLS만 7로 변경하고 form_clear 시뮬레이션
    change_fields = []
    for name, val in fields1:
        if name == "ACLS":
            change_fields.append((name, "7"))
        elif name == "Dummy":
            change_fields.append((name, str(int(time.time()*1000))))
        elif name == "AMOD":
            change_fields.append((name, "1"))
        elif name == "PageFlag":
            change_fields.append((name, ""))
        else:
            change_fields.append((name, val))

    r = s.post(f"{BASE}/rps/aprop.cgi?", data=change_fields, timeout=TIMEOUT,
               headers={"Referer": f"{BASE}/rps/aprop.cgi?"})
    log(f"  → {r.status_code}")
    save("05_form_file", r)

    action2, fields2 = parse_form(r.text)
    log(f"  폼 action: {action2}, 필드 {len(fields2)}개")
    if not fields2:
        log("파일 폼 파싱 실패. 종료.")
        return

    # 필드 내용 확인
    for n, v in fields2:
        if v:
            log(f"    {n}={v[:50]}")

    # 5. 상세 설정 - 호스트명, 경로 등 입력
    log("\n[6] 상세 설정")
    detail_fields = []
    for name, val in fields2:
        if name == "ANAME":
            detail_fields.append((name, "JA_TEST_SMB"))
        elif name == "ANAMEONE":
            detail_fields.append((name, "JA_TEST_SMB"))
        elif name == "AREAD":
            detail_fields.append((name, "JA_TEST_SMB"))
        elif name == "AAD1":
            detail_fields.append((name, "192.168.11.99"))
        elif name == "APRTCL":
            detail_fields.append((name, "7"))  # Windows (SMB)
        elif name == "APATH":
            detail_fields.append((name, "scan_test"))
        elif name == "AUSER":
            detail_fields.append((name, "testuser"))
        elif name == "APWORD":
            detail_fields.append((name, "testpass"))
        elif name == "Dummy":
            detail_fields.append((name, str(int(time.time()*1000))))
        else:
            detail_fields.append((name, val))

    r = s.post(f"{BASE}/rps/aprop.cgi", data=detail_fields, timeout=TIMEOUT,
               headers={"Referer": f"{BASE}/rps/aprop.cgi"})
    log(f"  → {r.status_code}")
    save("06_detail", r)

    action3, fields3 = parse_form(r.text)
    log(f"  폼 action: {action3}, 필드 {len(fields3)}개")
    if not fields3:
        log("상세 폼 파싱 실패. 종료.")
        return

    # 필드 값 확인
    for n, v in fields3:
        if v:
            log(f"    {n}={v[:50]}")

    # 6. 실제 등록 - 브라우저 JS와 동일하게 AdrAction 변경 후 submit
    log("\n[7] 등록 POST")
    reg_fields = []
    for name, val in fields3:
        if name == "AdrAction":
            reg_fields.append((name, "./aprop.cgi?"))  # JS: sform.AdrAction.value = "./aprop.cgi?"
        elif name == "Dummy":
            reg_fields.append((name, str(int(time.time()*1000))))
        elif name == "PASSCHK":
            reg_fields.append((name, "1"))  # 둘 다 1로
        elif name == "APRTCL":
            reg_fields.append((name, "7"))  # Windows (SMB)
        else:
            reg_fields.append((name, val))

    # 전송할 필드 로그
    log("  전송 필드:")
    for n, v in reg_fields:
        log(f"    {n}={v[:60]}")

    r = s.post(f"{BASE}/rps/anewadrs.cgi", data=reg_fields, timeout=TIMEOUT,
               headers={"Referer": f"{BASE}/rps/aprop.cgi"})
    log(f"  → {r.status_code} ({len(r.content)}B)")
    save("07_register", r)

    if "ERR_SUBMIT_FORM" in r.text:
        log(">>> 등록 실패: ERR_SUBMIT_FORM")
        # 에러 메시지 추출
        for line in r.text.split("\n"):
            if "Cannot" in line or "cannot" in line:
                log(f"  에러: {line.strip()[:200]}")
    else:
        log(">>> ERR_SUBMIT_FORM 없음 — 성공 가능성!")

    # 7. 최종 목록 확인
    log("\n[8] 최종 목록")
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={int(time.time()*1000)}", timeout=TIMEOUT)
    r2 = s.post(f"{BASE}/rps/albody.cgi",
                data={"AID": "11", "FILTER_ID": "0", "Dummy": str(int(time.time()*1000))},
                timeout=TIMEOUT)
    match = re.search(r'var\s+adrsList\s*=\s*\[([^\]]*)\]', r2.text, re.DOTALL)
    if match:
        entries = re.findall(r'id:(\d+),tp:(\d+),nm:"([^"]*)"', match.group(1))
        for idx, tp, name in entries:
            marker = " <<<" if "JA_TEST" in name else ""
            log(f"  [{idx}] {name.strip()}{marker}")
    else:
        log("  목록 파싱 실패")


if __name__ == "__main__":
    main()
