"""캐논 iR-ADV C3720 수신지 등록/삭제 테스트"""
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
LOG_FILE = RESULT_DIR / f"canon_register_test_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(f"  {line}")
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save_response(name, r):
    ts = datetime.now().strftime("%H%M%S")
    prefix = f"canon_test_{ts}_{name}"
    meta = {
        "name": name, "url": r.url, "method": r.request.method,
        "status_code": r.status_code,
        "request_headers": dict(r.request.headers),
        "response_headers": dict(r.headers),
        "request_body": r.request.body if r.request.body else None,
    }
    (RESULT_DIR / f"{prefix}_meta.json").write_text(
        json.dumps(meta, ensure_ascii=False, indent=2), encoding="utf-8")
    (RESULT_DIR / f"{prefix}_body.html").write_text(r.text, encoding="utf-8")
    log(f"  저장: {prefix}")


def extract_token(html):
    """HTML에서 Token 추출 (여러 패턴 시도)"""
    for pattern in [
        r'name=["\']Token["\'][^>]*value=["\']([^"\']+)',
        r'value=["\']([^"\']+)["\'][^>]*name=["\']Token["\']',
        r'Token=(\d{10,})',
    ]:
        m = re.search(pattern, html, re.I)
        if m:
            return m.group(1)
    return None


def init_session(session):
    """포털 → 주소록 페이지 순서로 접근하여 세션 확보"""
    # 1) 포털
    r = session.get(f"{BASE}/", timeout=TIMEOUT)
    log(f"[세션] 포털 → {r.status_code}")
    save_response("01_portal", r)

    # 2) 주소록 진입
    r = session.get(f"{BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR&Dummy={int(time.time()*1000)}",
                    timeout=TIMEOUT)
    log(f"[세션] 주소록진입 → {r.status_code}")
    save_response("02_nativetop", r)

    log(f"[세션] 쿠키: {dict(session.cookies)}")
    return session


def get_address_list(session):
    """주소 리스트 원터치 페이지 → 목록 조회 + Token A 획득"""
    # asublist.cgi (프레임 페이지)
    url = f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={int(time.time()*1000)}"
    r = session.get(url, timeout=TIMEOUT,
                    headers={"Referer": f"{BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR"})
    log(f"[목록] asublist → {r.status_code}")
    save_response("03_asublist", r)
    token_a = extract_token(r.text)
    log(f"[목록] Token A: {token_a[:20] if token_a else 'None'}...")

    # albody.cgi (목록 본문)
    r2 = session.post(f"{BASE}/rps/albody.cgi",
                      data={"AID": "11", "FILTER_ID": "0", "Dummy": str(int(time.time()*1000))},
                      headers={"Referer": url}, timeout=TIMEOUT)
    log(f"[목록] albody → {r2.status_code}")
    save_response("04_albody", r2)

    # adrsList 파싱 (배열 형태)
    entries = []
    match = re.search(r'var\s+adrsList\s*=\s*\[([^\]]*)\]', r2.text, re.DOTALL)
    if match:
        raw = match.group(1)
        entries = re.findall(r'id:(\d+),tp:(\d+),nm:"([^"]*)"', raw)
        for idx, tp, name in entries:
            type_name = {2: "이메일", 7: "파일(SMB)"}.get(int(tp), f"타입{tp}")
            log(f"  [{idx}] {name.strip()} ({type_name})")
    else:
        log("[목록] adrsList 없음")

    return token_a, entries


def register_smb(session, token_a, slot=5):
    """SMB 수신지 등록: aprop.cgi(폼) → anewadrs.cgi(등록)"""

    # 1) 등록 폼 페이지 (Token A 사용 → Token B 획득)
    form_url = (f"{BASE}/rps/aprop.cgi?AMOD=1&AID=11&AIDX={slot}&ACLS=7"
                f"&AFION=1&AdrAction=.%2Falframe.cgi%3F"
                f"&Dummy={int(time.time()*1000)}&Token={token_a}")
    r = session.get(form_url, timeout=TIMEOUT,
                    headers={"Referer": f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0"})
    log(f"[등록] 폼 페이지 → {r.status_code}")
    save_response("05_aprop_form", r)

    token_b = extract_token(r.text)
    log(f"[등록] Token B: {token_b[:20] if token_b else 'None'}...")

    if not token_b:
        log("[등록] Token B 추출 실패!")
        return False

    # 2) 실제 등록 POST (Token B 사용)
    body = (
        f"AID=11&PageFlag=&AIDX={slot}"
        f"&ANAME=JA_TEST_SMB&ANAMEONE=JA_TEST_SMB&AREAD=JA_TEST_SMB"
        f"&APNO=0&AAD1=192.168.11.99&ACLS=7&APRTCL=7"
        f"&APATH=scan_test&AUSER=testuser&INPUT_PSWD=0&APWORD=testpass"
        f"&PASSCHK=1&PASSCHK=1"
        f"&AdrAction=.%2Faprop.cgi%3F&AMOD=1"
        f"&Dummy={int(time.time()*1000)}"
        f"&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
        f"&Token={token_b}"
    )
    r2 = session.post(f"{BASE}/rps/anewadrs.cgi", data=body,
                      headers={"Content-Type": "application/x-www-form-urlencoded",
                               "Referer": form_url},
                      timeout=TIMEOUT)
    log(f"[등록] POST → {r2.status_code} ({len(r2.content)}B)")
    save_response("06_register", r2)

    # 성공/실패 확인
    if "ERR_SUBMIT_FORM" in r2.text:
        log("[등록] 실패: ERR_SUBMIT_FORM 반환")
        return False
    else:
        log("[등록] 에러 폼 없음 — 성공 가능성 있음")
        return True


def delete_smb(session, token_a, slot=5):
    """수신지 삭제"""
    body = (
        f"AMOD=1&AID=11&AIDX={slot}&ACLS=7&AFION=1"
        f"&AdrAction=.%2Falframe.cgi%3F"
        f"&Dummy={int(time.time()*1000)}&Token={token_a}"
    )
    r = session.post(f"{BASE}/rps/adelete.cgi", data=body,
                     headers={"Content-Type": "application/x-www-form-urlencoded"},
                     timeout=TIMEOUT)
    log(f"[삭제] POST → {r.status_code}")
    save_response("07_delete", r)


def main():
    slot = int(sys.argv[1]) if len(sys.argv) > 1 else 5

    session = requests.Session()
    session.verify = False

    print("=" * 50)
    print(f" 캐논 수신지 등록 테스트 (슬롯 {slot:03d})")
    print("=" * 50)

    # 1. 세션 초기화
    print("\n[1] 세션 초기화")
    init_session(session)

    # 2. 목록 조회 + Token A
    print("\n[2] 현재 목록 조회")
    token_a, entries = get_address_list(session)

    if not token_a:
        print("\n>>> Token A 추출 실패. 종료.")
        return

    # 3. SMB 수신지 등록 (Token A → 폼 → Token B → 등록)
    print(f"\n[3] SMB 수신지 등록 (슬롯 {slot:03d})")
    success = register_smb(session, token_a, slot)

    # 4. 등록 확인
    print("\n[4] 등록 후 목록 확인")
    token_a2, entries2 = get_address_list(session)
    found = any("JA_TEST" in name for _, _, name in entries2)

    if found:
        print("\n>>> 등록 성공!")
        # 5. 삭제
        print(f"\n[5] 삭제 (슬롯 {slot:03d})")
        if token_a2:
            delete_smb(session, token_a2, slot)
            print("\n[6] 삭제 후 목록 확인")
            get_address_list(session)
    else:
        print(f"\n>>> 등록 {'실패' if not success else '확인 불가'}.")


if __name__ == "__main__":
    main()
