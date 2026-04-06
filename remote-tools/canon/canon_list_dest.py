"""캐논 주소록 조회 — albody 응답 분석 포함"""
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


def list_destinations(aid):
    s = init_session()
    if not s:
        return False

    log(f"\n=== 캐논 주소록 조회 (AID={aid}) ===")

    # asublist
    r = s.get(f"{BASE}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={dummy()}", timeout=10)
    log(f"[3] asublist OK")
    time.sleep(1)

    # alframe
    s.get(f"{BASE}/rps/alframe.cgi?AID={aid}", timeout=10)
    log(f"[4] alframe OK")
    time.sleep(1)

    # albody
    r = s.post(f"{BASE}/rps/albody.cgi",
               data=f"AID={aid}&FILTER_ID=0&Dummy={dummy()}",
               headers={"Content-Type": "application/x-www-form-urlencoded",
                        "Referer": f"{BASE}/rps/alframe.cgi?"},
               timeout=10)
    log(f"[5] albody OK — {r.status_code} ({len(r.content)}B)")

    # 응답 전체 저장
    ts = datetime.now().strftime("%H%M%S")
    save_path = RESULT_DIR / f"list_{ts}_albody_full.html"
    save_path.write_text(r.text, encoding="utf-8")
    log(f"    저장: {save_path.name}")

    html = r.text
    found = False

    # 파싱 방법 1: var adrsList = [...]
    match = re.search(r'var\s+adrsList\s*=\s*\[(.*?)\];', html, re.DOTALL)
    if match:
        log("  [파싱1] var adrsList 발견")
        for m in re.finditer(r'idx:(\d+)[^}]*nm:"([^"]*)"', match.group(1)):
            log(f"    [{m.group(1)}] {m.group(2)}")
            found = True

    # 파싱 방법 2: idx/nm 패턴 직접
    if not found:
        entries = list(re.finditer(r'idx:(\d+)[^}]*nm:"([^"]+)"', html))
        if entries:
            log("  [파싱2] idx/nm 패턴 발견")
            for m in entries:
                log(f"    [{m.group(1)}] {m.group(2)}")
                found = True

    # 파싱 방법 3: AIDX + ANAME hidden input
    if not found:
        aidx_vals = re.findall(r'name=["\']AIDX["\'][^>]*value=["\']([^"\']+)', html)
        aname_vals = re.findall(r'name=["\']ANAME["\'][^>]*value=["\']([^"\']+)', html)
        if aname_vals and any(v for v in aname_vals):
            log("  [파싱3] hidden input 발견")
            for i, (aidx, aname) in enumerate(zip(aidx_vals, aname_vals)):
                if aname:
                    log(f"    AIDX={aidx}, ANAME={aname}")
                    found = True

    # 파싱 방법 4: onclick에서 AIDX 추출
    if not found:
        clicks = re.findall(r'onclick[^"]*"[^"]*AIDX[=:](\d+)[^"]*"', html)
        if clicks:
            log(f"  [파싱4] onclick AIDX 발견: {clicks}")
            found = True

    # 파싱 방법 5: 테이블 행에서 텍스트 추출
    if not found:
        rows = re.findall(r'<tr[^>]*>(.*?)</tr>', html, re.DOTALL)
        if rows:
            log(f"  [파싱5] <tr> 태그 {len(rows)}개 발견")
            for i, row in enumerate(rows[:10]):
                cells = re.findall(r'<td[^>]*>(.*?)</td>', row, re.DOTALL)
                if cells:
                    clean = [re.sub(r'<[^>]+>', '', c).strip() for c in cells]
                    clean = [c for c in clean if c]
                    if clean:
                        log(f"    row[{i}]: {clean}")
                        found = True

    # 디버그: 데이터 패턴 탐색
    if not found:
        log("\n  === 디버그: 응답 분석 ===")

        # var 선언 모두 출력
        var_decls = re.finditer(r'var\s+(\w+)\s*=\s*', html)
        var_names = [m.group(1) for m in var_decls if m.group(1) not in (
            'title', 'message_tbl', 'target_tbl', 'doccnt', 'elm_len',
            'elm_ctrl', 'type', 'i', 'j', 'nCnt', 'cur_chk', 'end_idx',
            'cancel_form', 'cncl_elm_ctrl', 'cst_idx', 'tmpCode',
            'tmpInteger', 'tmpDecimal', 'l_str', 'l_fmt', 'l_a', 'l_b',
            'l_a_dgt_pos', 'l_b_dgt_pos', 'l_a_dgt', 'l_b_dgt',
            'l_a_val', 'l_b_val', 'l_calc_val', 'l_dgt_val', 'l_calc_str',
            'l_len', 'Object', 'ElementName', 'PformElementName',
            'objCnt', 'ObjCnt', 'CheckBoxFormat', 'Func_name', 'val_num',
            'tmpDataTbl', 'oldListenerFunc', 'submitButton', 'ctrlObj',
        )]
        if var_names:
            log(f"    주요 var 선언: {var_names[:20]}")

        # document.writeln에서 데이터성 내용
        writelns = re.findall(r"document\.writeln\('([^']{10,})'\)", html)
        data_writelns = [w for w in writelns if any(k in w for k in ['192.168', 'TEST', 'JA_', 'scan', 'SCAN'])]
        if data_writelns:
            log(f"    데이터성 writeln:")
            for w in data_writelns[:10]:
                log(f"      {w[:120]}")

        # 응답 내에서 IP 주소 패턴
        ips = re.findall(r'192\.168\.\d+\.\d+', html)
        if ips:
            log(f"    IP 패턴: {set(ips)}")

        # 응답 크기 정보
        log(f"    전체 길이: {len(html)} chars, {len(html.split(chr(10)))} lines")
        log(f"    <script> 태그: {html.count('<script')}개")
        log(f"    <form> 태그: {html.count('<form')}개")
        log(f"    <table> 태그: {html.count('<table')}개")

    if not found:
        log("\n>>> 목록 데이터 파싱 실패 — 저장된 HTML 확인 필요")
        log(f"    {save_path}")
    else:
        log(f"\n>>> 조회 완료")

    return found


if __name__ == "__main__":
    AID = "11"  # 기본: 원터치

    args = sys.argv[1:]
    if len(args) >= 1: AID = args[0]

    log(f"조회 대상: AID={AID}")
    log("")

    list_destinations(AID)
