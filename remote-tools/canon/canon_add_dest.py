"""캐논 수신지 추가 (SMB/파일 타입) — 검증된 8단계 흐름"""
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
LOG_FILE = RESULT_DIR / f"canon_add_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"add_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def get_token_from_url(html):
    """asublist 응답에서 Token A 추출 (URL 파라미터)"""
    m = re.search(r'Token=(\d+)', html)
    return m.group(1) if m else None


def get_token_from_hidden(html):
    """aprop 응답에서 Token B 추출 (hidden input)"""
    m = re.search(r'name=["\']Token["\'][^>]*value=["\']([^"\']+)', html)
    return m.group(1) if m else None


def find_empty_slots(html, aid):
    """albody 응답에서 사용 중인 슬롯 파악, 빈 슬롯 목록 반환"""
    used = set()
    for m in re.finditer(r'\{[^}]*idx:(\d+)[^}]*nm:"([^"]*)"', html):
        idx = int(m.group(1))
        name = m.group(2).strip()
        if name:
            used.add(idx)
    return used


def dummy():
    return str(int(time.time() * 1000))


def add_destination(aid, slot, name, host_ip, folder, user, password, btn_name=None):
    s = requests.Session()
    s.verify = False
    s.headers.update({
        "User-Agent": UA,
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Encoding": "gzip, deflate",
        "Accept-Language": "ko-KR,ko;q=0.9",
        "Upgrade-Insecure-Requests": "1",
    })

    if btn_name is None:
        btn_name = name

    log(f"=== 캐논 수신지 추가 ===")
    log(f"  AID={aid}, 슬롯={slot}, 이름={name}, 버튼명칭={btn_name}")
    log(f"  호스트={host_ip}, 폴더={folder}, 유저={user}")

    # Step 1: 메인 포털 → sessionid, portalLang 쿠키
    r = s.get(f"{BASE}/", timeout=10)
    if r.status_code != 200:
        log(f"[FAIL] 포털 접속 실패: {r.status_code}")
        return False
    log(f"[1] 포털 OK — cookies={dict(s.cookies)}")
    time.sleep(1)

    # Step 2: nativetop → iR 쿠키 (필수!)
    r = s.get(f"{BASE}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={dummy()}",
              timeout=10, headers={"Referer": f"{BASE}/"})
    if "iR" not in s.cookies:
        log("[FAIL] iR 쿠키 미설정")
        return False
    log(f"[2] nativetop OK — iR={s.cookies['iR']}")
    time.sleep(1)

    # Step 3: asublist → Token A
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={dummy()}",
              timeout=10)
    save("3_asublist", r)
    token_a = get_token_from_url(r.text)
    if not token_a:
        log("[FAIL] Token A 추출 실패")
        return False
    log(f"[3] Token A = {token_a[:15]}...")
    time.sleep(1)

    # Step 4: alframe (AID 컨텍스트 설정)
    r = s.get(f"{BASE}/rps/alframe.cgi?AID={aid}", timeout=10)
    log(f"[4] alframe OK")
    time.sleep(1)

    # Step 5: albody (목록 조회 — 빈 슬롯 확인)
    r = s.post(f"{BASE}/rps/albody.cgi",
               data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/alframe.cgi?AID={aid}"},
               timeout=10)
    save("5_albody", r)
    used = find_empty_slots(r.text, aid)
    if used:
        log(f"[5] 사용 중 슬롯: {sorted(used)}")
    else:
        log(f"[5] 목록 조회 완료")

    if slot in used:
        log(f"[WARN] 슬롯 {slot}은 이미 사용 중! 덮어쓰기 시도합니다.")
    time.sleep(1)

    # Step 6: aprop (이메일 폼 진입 ACLS=2) → Token B1
    body = (f"AMOD=1&AID={aid}&AIDX={slot}&ACLS=2&AFION=1"
            f"&AdrAction=.%2Falframe.cgi%3F&Dummy={dummy()}&Token={token_a}")
    r = s.post(f"{BASE}/rps/aprop.cgi?",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/albody.cgi"},
               timeout=10)
    save("6_aprop_email", r)
    token_b1 = get_token_from_hidden(r.text)
    if not token_b1:
        log("[FAIL] Token B1 추출 실패")
        return False
    log(f"[6] Token B1 = {token_b1[:15]}...")
    time.sleep(1)

    # Step 7: aprop (파일 타입 변경 ACLS=7, Token 비움) → Token B2
    body = (f"AID={aid}&PageFlag=&AIDX={slot}&ANAME=&ANAMEONE=&AREAD=&APNO=0&AAD1="
            f"&ACLS=7&DATADIV=&AdrAction=.%2Falframe.cgi%3F&AMOD=1"
            f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=")
    r = s.post(f"{BASE}/rps/aprop.cgi?",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/aprop.cgi?"},
               timeout=10)
    save("7_aprop_file", r)
    token_b2 = get_token_from_hidden(r.text)
    if not token_b2:
        log("[FAIL] Token B2 추출 실패")
        return False
    log(f"[7] Token B2 = {token_b2[:15]}...")
    time.sleep(1)

    # Step 8: aprop (파일 설정 — PageFlag, PASSCHK=1&빈값) → Token B3
    body = (f"AID={aid}&PageFlag=a_rfn_f.tpl&AIDX={slot}"
            f"&ANAME={name}&ANAMEONE={name}&AREAD={btn_name}&APNO=0"
            f"&AAD1={host_ip}&ACLS=7&APRTCL=7"
            f"&APATH={folder}&AUSER={user}&INPUT_PSWD=0&APWORD={password}"
            f"&PASSCHK=1&PASSCHK="
            f"&AdrAction=.%2Falframe.cgi%3F&AMOD=1"
            f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
            f"&Token={token_b2}")
    r = s.post(f"{BASE}/rps/aprop.cgi",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/aprop.cgi?"},
               timeout=10)
    save("8_aprop_setting", r)
    token_b3 = get_token_from_hidden(r.text)
    if not token_b3:
        log("[FAIL] Token B3 추출 실패")
        return False
    log(f"[8] Token B3 = {token_b3[:15]}...")
    time.sleep(1)

    # Step 9: aprop (폴더 설정 — PageFlag 없음, Token B3 재사용) → Token B4
    body = (f"AID={aid}&PageFlag=&AIDX={slot}"
            f"&ANAME={name}&ANAMEONE={name}&AREAD={btn_name}&APNO=0"
            f"&AAD1={host_ip}&ACLS=7&APRTCL=7"
            f"&APATH={folder}&AUSER={user}&INPUT_PSWD=0&APWORD={password}"
            f"&PASSCHK=1&PASSCHK="
            f"&AdrAction=.%2Falframe.cgi%3F&AMOD=1"
            f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
            f"&Token={token_b3}")
    r = s.post(f"{BASE}/rps/aprop.cgi",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/aprop.cgi"},
               timeout=10)
    save("9_aprop_folder", r)
    token_b4 = get_token_from_hidden(r.text)
    if not token_b4:
        log("[FAIL] Token B4 추출 실패")
        return False
    log(f"[9] Token B4 = {token_b4[:15]}...")
    time.sleep(1)

    # Step 10: anewadrs.cgi (최종 등록 — PASSCHK 둘 다 1, AdrAction 변경)
    body = (f"AID={aid}&PageFlag=&AIDX={slot}"
            f"&ANAME={name}&ANAMEONE={name}&AREAD={btn_name}&APNO=0"
            f"&AAD1={host_ip}&ACLS=7&APRTCL=7"
            f"&APATH={folder}&AUSER={user}&INPUT_PSWD=0&APWORD={password}"
            f"&PASSCHK=1&PASSCHK=1"
            f"&AdrAction=.%2Faprop.cgi%3F&AMOD=1"
            f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
            f"&Token={token_b4}")
    r = s.post(f"{BASE}/rps/anewadrs.cgi",
               data=body,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/aprop.cgi"},
               timeout=10)
    save("10_register", r)

    if "ERR_SUBMIT_FORM" in r.text:
        log(f"[FAIL] 등록 실패 — ERR_SUBMIT_FORM ({len(r.content)}B)")
        return False

    log(f"[10] 등록 응답 OK ({len(r.content)}B)")

    # 검증: 목록에서 등록한 이름 확인
    time.sleep(2)
    log("\n=== 등록 검증 ===")

    # 새 세션으로 검증 (등록 세션과 분리)
    s2 = requests.Session()
    s2.verify = False
    s2.headers.update({"User-Agent": UA})

    s2.get(f"{BASE}/", timeout=10)
    s2.get(f"{BASE}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={dummy()}", timeout=10)
    s2.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={dummy()}", timeout=10)
    s2.get(f"{BASE}/rps/alframe.cgi?AID={aid}", timeout=10)
    r2 = s2.post(f"{BASE}/rps/albody.cgi",
                 data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
                 headers={"Content-Type": "application/x-www-form-urlencoded"},
                 timeout=10)
    save("11_verify", r2)
    log(f"  검증 응답: {r2.status_code} ({len(r2.content)}B)")

    found = False

    # 방법1: var adrsList = [...]
    match = re.search(r'var\s+adrsList\s*=\s*\[(.*?)\];', r2.text, re.DOTALL)
    if match:
        for m in re.finditer(r'idx:(\d+)[^}]*nm:"([^"]*)"', match.group(1)):
            idx, nm = m.group(1), m.group(2).strip()
            if nm:
                marker = " <<<" if name in nm else ""
                log(f"  [{idx}] {nm}{marker}")
                if name in nm:
                    found = True

    # 방법2: nm:"..." 패턴 직접 검색
    if not found:
        entries = list(re.finditer(r'idx:(\d+)[^}]*nm:"([^"]+)"', r2.text))
        if entries:
            for m in entries:
                idx, nm = m.group(1), m.group(2).strip()
                marker = " <<<" if name in nm else ""
                log(f"  [{idx}] {nm}{marker}")
                if name in nm:
                    found = True

    # 방법3: 등록한 이름이 응답 어딘가에 있는지
    if not found and name in r2.text:
        log(f"  '{name}'이 응답 HTML에 존재 (정확한 위치 파싱 실패)")
        found = True

    # 디버그: 파싱 실패 시 응답 내용 일부 출력
    if not found:
        log("  목록 파싱 실패 — 디버그 정보:")
        # var 선언 찾기
        for m in re.finditer(r'var\s+(\w+)\s*=\s*[\[\{]', r2.text):
            log(f"    var {m.group(1)} at pos {m.start()}")
        # 응답 앞부분에서 script 내용 확인
        scripts = re.findall(r'<script[^>]*>(.*?)</script>', r2.text[:5000], re.DOTALL)
        for i, sc in enumerate(scripts[:3]):
            sc_clean = sc.strip()[:200]
            if sc_clean and 'CacheImage' not in sc_clean:
                log(f"    script[{i}]: {sc_clean}")

    if found:
        log(f"\n>>> 성공! '{name}'이 AID={aid} 목록에 확인됨")
    else:
        log(f"\n>>> 목록에서 '{name}' 미확인 (등록 응답은 정상, 검증HTML 확인 필요)")
        log(f"    저장 파일: {RESULT_DIR}/add_*_11_verify.html")

    return True


if __name__ == "__main__":
    AID = "11"
    SLOT = 1
    NAME = "TEST_NAME"
    HOST = "192.168.11.98"
    FOLDER = "TEST_SCAN"
    USER = "TEST_USER"
    PASS = "123456"
    BTN = "TEST_BTN_NAME"

    # 인자: python canon_add_dest.py [slot] [name] [host] [folder] [user] [pass] [btn_name]
    args = sys.argv[1:]
    if len(args) >= 1: SLOT = int(args[0])
    if len(args) >= 2: NAME = args[1]
    if len(args) >= 3: HOST = args[2]
    if len(args) >= 4: FOLDER = args[3]
    if len(args) >= 5: USER = args[4]
    if len(args) >= 6: PASS = args[5]
    if len(args) >= 7: BTN = args[6]

    log(f"설정: AID={AID}, SLOT={SLOT}")
    log(f"  이름={NAME}, 버튼명칭={BTN}")
    log(f"  호스트={HOST}, 폴더={FOLDER}")
    log(f"  유저={USER}, 패스={'***' if PASS else '(없음)'}")
    log("")

    success = add_destination(AID, SLOT, NAME, HOST, FOLDER, USER, PASS, BTN)
    sys.exit(0 if success else 1)
