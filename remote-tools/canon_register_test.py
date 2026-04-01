"""캐논 iR-ADV C3720 수신지 등록 테스트"""
import re
import sys
import json
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
    """HTTP 응답을 파일로 저장"""
    ts = datetime.now().strftime("%H%M%S")
    prefix = f"canon_test_{ts}_{name}"

    meta = {
        "name": name,
        "url": r.url,
        "method": r.request.method,
        "status_code": r.status_code,
        "request_headers": dict(r.request.headers),
        "response_headers": dict(r.headers),
        "request_body": r.request.body if r.request.body else None,
        "timestamp": datetime.now().isoformat(),
    }
    meta_file = RESULT_DIR / f"{prefix}_meta.json"
    with open(meta_file, "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)

    body_file = RESULT_DIR / f"{prefix}_body.html"
    body_file.write_text(r.text, encoding="utf-8")

    log(f"  저장: {meta_file.name}, {body_file.name}")


def init_session(session):
    """포털 페이지부터 순서대로 접근하여 세션 쿠키 확보"""
    urls = [
        f"http://{IP}:8000/",
        f"http://{IP}:8000/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR",
    ]
    for url in urls:
        r = session.get(url, timeout=TIMEOUT, allow_redirects=True)
        log(f"[세션] GET {url} → {r.status_code}")
        save_response("init_session", r)
    log(f"[세션] 쿠키: {dict(session.cookies)}")


def get_token(session):
    """주소록 페이지에서 Token 추출"""
    # 주소 리스트 원터치 페이지 접근
    url = f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy=9999"
    r = session.get(url, timeout=TIMEOUT)
    log(f"[토큰 취득] GET {url} → {r.status_code}")
    save_response("get_token_asublist", r)

    # Token 추출 시도 - hidden input
    match = re.search(r'name=["\']Token["\'][^>]*value=["\']([^"\']+)', r.text, re.I)
    if match:
        log(f"[토큰] hidden input에서 발견: {match.group(1)[:20]}...")
        return match.group(1)

    # Token 추출 시도 - JS 변수
    match = re.search(r'Token["\s]*[:=]\s*["\']?(\d+)', r.text, re.I)
    if match:
        log(f"[토큰] JS 변수에서 발견: {match.group(1)[:20]}...")
        return match.group(1)

    # Token 추출 시도 - URL 파라미터
    match = re.search(r'Token=(\d+)', r.text)
    if match:
        log(f"[토큰] URL 파라미터에서 발견: {match.group(1)[:20]}...")
        return match.group(1)

    # 못 찾으면 전체 응답에서 Token 관련 부분 출력
    log("[토큰] 자동 추출 실패. Token 관련 텍스트 검색:")
    for line in r.text.split("\n"):
        if "token" in line.lower() or "Token" in line:
            log(f"  > {line.strip()[:200]}")

    # 등록 폼 페이지에서도 시도
    url2 = f"{BASE}/rps/aprop.cgi?AMOD=1&AID=11&AIDX=5&ACLS=7&AFION=1"
    r2 = session.get(url2, timeout=TIMEOUT)
    log(f"[토큰 2차] GET {url2} → {r2.status_code}")
    save_response("get_token_aprop", r2)

    for pattern in [
        r'name=["\']Token["\'][^>]*value=["\']([^"\']+)',
        r'Token["\s]*[:=]\s*["\']?(\d+)',
        r'Token=(\d+)',
    ]:
        match = re.search(pattern, r2.text, re.I)
        if match:
            log(f"[토큰] 2차 시도에서 발견: {match.group(1)[:20]}...")
            return match.group(1)

    log("[토큰] 2차에서도 Token 관련 텍스트:")
    for line in r2.text.split("\n"):
        if "token" in line.lower() or "Token" in line:
            log(f"  > {line.strip()[:200]}")

    return None


def list_addresses(session):
    """수신지 목록 조회"""
    url = f"{BASE}/rps/albody.cgi"
    r = session.post(url, data={"AID": "11", "FILTER_ID": "0", "Dummy": "9999"}, timeout=TIMEOUT)
    log(f"[목록 조회] POST {url} → {r.status_code}")
    save_response("list_addresses", r)

    # adrsList 파싱
    match = re.search(r'var\s+adrsList\s*=\s*\{([^;]+)\}', r.text)
    if match:
        log(f"[목록] adrsList 발견:")
        raw = match.group(1)
        entries = re.findall(r'(\d+):\{tp:(\d+),nm:"([^"]*)"', raw)
        for idx, tp, name in entries:
            type_name = {2: "이메일", 7: "파일(SMB)"}.get(int(tp), f"타입{tp}")
            log(f"  [{idx}] {name.strip()} ({type_name})")
        return entries
    else:
        log("[목록] adrsList 없음")
        # 응답 일부 출력
        log(f"  응답 미리보기: {r.text[:500]}")
        return []


def register_smb(session, token, slot=5):
    """SMB 수신지 등록"""
    url = f"{BASE}/rps/anewadrs.cgi"
    data = {
        "AID": "11",
        "PageFlag": "",
        "AIDX": str(slot),
        "ANAME": "JA_TEST_SMB",
        "ANAMEONE": "JA_TEST_SMB",
        "AREAD": "JA_TEST_SMB",
        "APNO": "0",
        "AAD1": "192.168.11.99",
        "ACLS": "7",
        "APRTCL": "7",
        "APATH": "scan_test",
        "AUSER": "testuser",
        "INPUT_PSWD": "0",
        "APWORD": "testpass",
        "PASSCHK": "1",
        "DATADIV": "0",
        "AdrAction": "./aprop.cgi?",
        "AMOD": "1",
        "Dummy": "9999",
        "AFCLS": "",
        "AFINT": "",
        "APNOL": "",
        "AFION": "1",
        "AUUID": "",
        "Token": token,
    }
    r = session.post(url, data=data, timeout=TIMEOUT)
    log(f"[등록] POST {url} → {r.status_code}")
    log(f"  응답 크기: {len(r.content)}B")
    save_response("register_smb", r)
    return r


def delete_address(session, token, slot=5):
    """수신지 삭제"""
    url = f"{BASE}/rps/adelete.cgi"
    data = {
        "AMOD": "1",
        "AID": "11",
        "AIDX": str(slot),
        "ACLS": "7",
        "AFION": "1",
        "AdrAction": "./alframe.cgi?",
        "Dummy": "9999",
        "Token": token,
    }
    r = session.post(url, data=data, timeout=TIMEOUT)
    log(f"[삭제] POST {url} → {r.status_code}")
    save_response("delete_address", r)
    return r


def main():
    slot = int(sys.argv[1]) if len(sys.argv) > 1 else 5

    session = requests.Session()
    session.verify = False

    print("=" * 50)
    print(f" 캐논 수신지 등록 테스트 (슬롯 {slot:03d})")
    print("=" * 50)

    # 0. 세션 초기화
    print("\n[0] 세션 초기화")
    init_session(session)

    # 1. 현재 목록 조회
    print("\n[1] 현재 수신지 목록")
    list_addresses(session)

    # 2. 토큰 없이 등록 시도
    print(f"\n[2] 토큰 없이 등록 시도 (슬롯 {slot:03d})")
    register_smb(session, token="", slot=slot)

    # 3. 등록 확인
    print("\n[3] 등록 후 목록 확인")
    entries = list_addresses(session)
    found = any(name.strip().startswith("JA_TEST") for _, _, name in entries)

    if found:
        print("\n>>> 토큰 없이 등록 성공! (토큰 불필요)")
        # 삭제
        print(f"\n[4] 테스트 수신지 삭제 (슬롯 {slot:03d})")
        delete_address(session, token="", slot=slot)
        print("\n[5] 삭제 후 목록 확인")
        list_addresses(session)
    else:
        print("\n>>> 토큰 없이 등록 실패. 토큰 필요 확인됨.")
        # 토큰 포함 재시도
        print("\n[4] 토큰 추출 시도")
        token = get_token(session)
        if token:
            print(f"\n[5] 토큰 포함 등록 시도 (슬롯 {slot:03d})")
            register_smb(session, token, slot)
            print("\n[6] 등록 후 목록 확인")
            entries = list_addresses(session)
            found = any(name.strip().startswith("JA_TEST") for _, _, name in entries)
            if found:
                print("\n>>> 토큰 포함 등록 성공!")
                print(f"\n[7] 삭제")
                token2 = get_token(session)
                if token2:
                    delete_address(session, token2, slot)
                    list_addresses(session)
            else:
                print("\n>>> 토큰 포함해도 등록 실패. 추가 조사 필요.")
        else:
            print("  토큰 자동 추출 실패")


if __name__ == "__main__":
    main()
