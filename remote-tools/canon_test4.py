"""캐논 수신지 등록 - 브라우저 8단계 정확히 재현"""
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
LOG_FILE = RESULT_DIR / f"canon_test4_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"


def log(msg):
    ts = datetime.now().strftime("%H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")


def save(name, r):
    ts = datetime.now().strftime("%H%M%S")
    (RESULT_DIR / f"c4_{ts}_{name}.html").write_text(r.text[:50000], encoding="utf-8")


def get_token(html):
    m = re.search(r'Token=(\d+)', html)
    return m.group(1) if m else None


def get_hidden_token(html):
    m = re.search(r'name=["\']Token["\'][^>]*value=["\']([^"\']+)', html)
    return m.group(1) if m else None


def main():
    slot = int(sys.argv[1]) if len(sys.argv) > 1 else 10
    aid = sys.argv[2] if len(sys.argv) > 2 else "11"
    s = requests.Session()
    s.verify = False

    common = {
        "User-Agent": UA,
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
        "Accept-Encoding": "gzip, deflate",
        "Accept-Language": "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7",
        "Upgrade-Insecure-Requests": "1",
    }
    s.headers.update(common)

    log(f"=== 캐논 8단계 재현 (슬롯 {slot}, AID={aid}) ===")
    dummy = lambda: str(int(time.time() * 1000))

    # Step 1: 메인
    log("\n[1] 메인 포털")
    r = s.get(f"{BASE}/", timeout=10)
    log(f"  → {r.status_code}, cookies={dict(s.cookies)}")
    save("1_portal", r)
    time.sleep(2)

    # Step 1.5: nativetop (iR 쿠키 설정)
    log("[1.5] 주소록 진입 (nativetop)")
    r = s.get(f"{BASE}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={dummy()}",
              timeout=10, headers={"Referer": f"{BASE}/"})
    log(f"  → {r.status_code}, cookies={dict(s.cookies)}")
    time.sleep(1)

    # Step 2: 주소 리스트
    log("[2] 주소 리스트 (asublist)")
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={dummy()}",
              timeout=10, headers={"Referer": f"{BASE}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={dummy()}"})
    log(f"  → {r.status_code}")
    save("2_asublist", r)
    token_a = get_token(r.text)
    log(f"  Token A: {token_a[:20] if token_a else 'None'}...")
    time.sleep(1)

    # Step 2.5: alframe (브라우저가 프레임으로 로드)
    log("[2.5] alframe (프레임)")
    r = s.get(f"{BASE}/rps/alframe.cgi?AID={aid}", timeout=10,
              headers={"Referer": f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0"})
    log(f"  → {r.status_code}")
    save("2b_alframe", r)
    time.sleep(1)

    # Step 3: 주소록 상세 (albody)
    log("[3] 주소록 상세 (albody)")
    r = s.post(f"{BASE}/rps/albody.cgi",
               data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
               headers={"Content-Type": "application/x-www-form-urlencoded",
                         "Origin": BASE,
                         "Referer": f"{BASE}/rps/alframe.cgi?AID={aid}",
                         "Cache-Control": "max-age=0"},
               timeout=10)
    log(f"  → {r.status_code}")
    save("3_albody", r)
    time.sleep(1)

    # Step 4: 신규 수신지 입력 폼 (이메일 타입)
    log(f"[4] 등록 폼 진입 (AIDX={slot}, ACLS=2)")
    body4 = f"AMOD=1&AID={aid}&AIDX={slot}&ACLS=2&AFION=1&AdrAction=.%2Falframe.cgi%3F&Dummy={dummy()}&Token={token_a}"
    r = s.post(f"{BASE}/rps/aprop.cgi?",
               data=body4,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                         "Origin": BASE,
                         "Referer": f"{BASE}/rps/albody.cgi",
                         "Cache-Control": "max-age=0"},
               timeout=10)
    log(f"  → {r.status_code}")
    save("4_aprop_email", r)
    token_b1 = get_hidden_token(r.text)
    log(f"  Token B1: {token_b1[:20] if token_b1 else 'None'}...")
    time.sleep(2)

    # Step 5: 파일 타입 변경 (Token 비움, DATADIV 포함, 파일 필드 없음)
    log("[5] 파일 타입 변경")
    body5 = (f"AID={aid}&PageFlag=&AIDX={slot}&ANAME=&ANAMEONE=&AREAD=&APNO=0&AAD1="
             f"&ACLS=7&DATADIV=&AdrAction=.%2Falframe.cgi%3F&AMOD=1"
             f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=")
    r = s.post(f"{BASE}/rps/aprop.cgi?",
               data=body5,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                         "Origin": BASE,
                         "Referer": f"{BASE}/rps/aprop.cgi?",
                         "Cache-Control": "max-age=0"},
               timeout=10)
    log(f"  → {r.status_code}")
    save("5_aprop_file", r)
    token_b2 = get_hidden_token(r.text)
    log(f"  Token B2: {token_b2[:20] if token_b2 else 'None'}...")
    time.sleep(2)

    # Step 6: 파일 설정 저장 (PageFlag=a_rfn_f.tpl, 호스트만 입력, PASSCHK 두번째 비움)
    log("[6] 파일 설정 저장 (설정 클릭)")
    body6 = (f"AID={aid}&PageFlag=a_rfn_f.tpl&AIDX={slot}"
             f"&ANAME=JA_V4&ANAMEONE=JA_V4&AREAD=JA_V4&APNO=0"
             f"&AAD1=192.168.11.99&ACLS=7&APRTCL=7"
             f"&APATH=&AUSER=&INPUT_PSWD=0&APWORD="
             f"&PASSCHK=1&PASSCHK="
             f"&AdrAction=.%2Falframe.cgi%3F&AMOD=1"
             f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
             f"&Token={token_b2}")
    r = s.post(f"{BASE}/rps/aprop.cgi",
               data=body6,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                         "Origin": BASE,
                         "Referer": f"{BASE}/rps/aprop.cgi?",
                         "Cache-Control": "max-age=0"},
               timeout=10)
    log(f"  → {r.status_code}")
    save("6_aprop_setting", r)
    token_b3 = get_hidden_token(r.text)
    log(f"  Token B3: {token_b3[:20] if token_b3 else 'None'}...")
    time.sleep(2)

    # Step 7: 폴더 설정 입력 (같은 Token 재사용, PASSCHK 두번째 비움)
    log("[7] 폴더 설정 입력")
    body7 = (f"AID={aid}&PageFlag=&AIDX={slot}"
             f"&ANAME=JA_V4&ANAMEONE=JA_V4&AREAD=JA_V4&APNO=0"
             f"&AAD1=192.168.11.99&ACLS=7&APRTCL=7"
             f"&APATH=scan_folder&AUSER=testuser&INPUT_PSWD=0&APWORD=1234"
             f"&PASSCHK=1&PASSCHK="
             f"&AdrAction=.%2Falframe.cgi%3F&AMOD=1"
             f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
             f"&Token={token_b3}")
    r = s.post(f"{BASE}/rps/aprop.cgi",
               data=body7,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                         "Origin": BASE,
                         "Referer": f"{BASE}/rps/aprop.cgi",
                         "Cache-Control": "max-age=0"},
               timeout=10)
    log(f"  → {r.status_code}")
    save("7_aprop_folder", r)
    token_b4 = get_hidden_token(r.text)
    log(f"  Token B4: {token_b4[:20] if token_b4 else 'None'}...")
    time.sleep(2)

    # Step 8: 최종 제출 (새 Token, PASSCHK 둘 다 1, AdrAction 변경)
    log("[8] 최종 제출 (anewadrs.cgi)")
    body8 = (f"AID={aid}&PageFlag=&AIDX={slot}"
             f"&ANAME=JA_V4&ANAMEONE=JA_V4&AREAD=JA_V4&APNO=0"
             f"&AAD1=192.168.11.99&ACLS=7&APRTCL=7"
             f"&APATH=scan_folder&AUSER=testuser&INPUT_PSWD=0&APWORD=1234"
             f"&PASSCHK=1&PASSCHK=1"
             f"&AdrAction=.%2Faprop.cgi%3F&AMOD=1"
             f"&Dummy={dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID="
             f"&Token={token_b4}")
    r = s.post(f"{BASE}/rps/anewadrs.cgi",
               data=body8,
               headers={"Content-Type": "application/x-www-form-urlencoded",
                         "Origin": BASE,
                         "Referer": f"{BASE}/rps/aprop.cgi",
                         "Cache-Control": "max-age=0"},
               timeout=10)
    log(f"  → {r.status_code} ({len(r.content)}B)")
    save("8_register", r)

    if "ERR_SUBMIT_FORM" in r.text:
        log(">>> 실패: ERR_SUBMIT_FORM")
    else:
        log(">>> ERR 없음 — 성공 가능성!")

    # 목록 확인
    time.sleep(1)
    log("\n[9] 최종 목록")
    s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={dummy()}", timeout=10)
    r2 = s.post(f"{BASE}/rps/albody.cgi",
                data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
                headers={"Content-Type": "application/x-www-form-urlencoded"}, timeout=10)
    match = re.search(r'var\s+adrsList\s*=\s*\[([^\]]*)\]', r2.text, re.DOTALL)
    if match:
        for m in re.finditer(r'nm:"([^"]*)"', match.group(1)):
            name = m.group(1).strip()
            marker = " <<<" if "JA_V4" in name else ""
            log(f"  {name}{marker}")
    else:
        log("  목록 파싱 실패")


if __name__ == "__main__":
    main()
