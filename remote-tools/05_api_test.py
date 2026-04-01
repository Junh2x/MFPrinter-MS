import sys
import json
import os
import re
import time
from datetime import datetime
from pathlib import Path

try:
    import requests
    import urllib3
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
except ImportError:
    print("requests 패키지 필요: pip install requests urllib3")
    sys.exit(1)

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)
TIMEOUT = 10

TEST_PREFIX = "JA_TEST_"
test_results = []


def log(msg, color=None):
    colors = {"red": "\033[91m", "green": "\033[92m", "yellow": "\033[93m", "cyan": "\033[96m"}
    reset = "\033[0m"
    prefix = colors.get(color, "") if color else ""
    suffix = reset if color else ""
    ts = datetime.now().strftime("%H:%M:%S")
    print(f"[{ts}] {prefix}{msg}{suffix}")


def record_test(test_name, success, details):
    """테스트 결과 기록"""
    result = {
        "test": test_name,
        "success": success,
        "details": details,
        "timestamp": datetime.now().isoformat()
    }
    test_results.append(result)
    color = "green" if success else "red"
    symbol = "PASS" if success else "FAIL"
    log(f"  [{symbol}] {test_name}: {details}", color)


# ============================================================
# RICOH 테스트
# ============================================================
def test_ricoh(ip, username="admin", password=""):
    log("=" * 60, "cyan")
    log(f" RICOH 핵심 기능 테스트 - {ip}", "cyan")
    log("=" * 60, "cyan")

    session = requests.Session()
    session.verify = False
    base = f"http://{ip}"

    # --- 테스트 1: REST API 접근 ---
    log("\n[TEST 1] REST API 접근", "yellow")
    try:
        r = session.get(f"{base}/rws/service-api/", timeout=TIMEOUT)
        record_test("REST_API_접근", r.status_code < 400,
                    f"status={r.status_code}, content_type={r.headers.get('Content-Type','')}")
    except Exception as e:
        record_test("REST_API_접근", False, str(e))

    # --- 테스트 2: 주소록 목록 조회 ---
    log("\n[TEST 2] 주소록 목록 조회", "yellow")
    addr_entries = []
    try:
        r = session.get(f"{base}/rws/service-api/addressbook/entries", timeout=TIMEOUT)
        record_test("주소록_목록조회", r.status_code < 400,
                    f"status={r.status_code}, size={len(r.content)}B")
        if r.status_code < 400:
            try:
                data = r.json()
                # JSON 구조 저장
                json_file = RESULT_DIR / f"ricoh_{ip.replace('.','_')}_addressbook_structure.json"
                with open(json_file, "w", encoding="utf-8") as f:
                    json.dump(data, f, ensure_ascii=False, indent=2)
                log(f"  주소록 JSON 구조 저장: {json_file.name}", "green")

                if isinstance(data, list):
                    addr_entries = data
                elif isinstance(data, dict):
                    addr_entries = data.get("entries", data.get("data", []))
                log(f"  주소록 항목 수: {len(addr_entries)}", "cyan")
            except:
                log(f"  JSON 파싱 실패, HTML 응답일 수 있음", "yellow")
    except Exception as e:
        record_test("주소록_목록조회", False, str(e))

    # --- 테스트 3: 수신지 등록 ---
    log("\n[TEST 3] 수신지 등록 (테스트)", "yellow")
    test_entry_id = None
    test_entry_data = {
        "name": f"{TEST_PREFIX}{int(time.time())}",
        "displayName": f"{TEST_PREFIX}SCAN_DEST",
        "type": "smb",
        "destination": {
            "path": "\\\\192.168.1.1\\test_share",
            "userName": "test_user",
            "password": "test_pass"
        }
    }

    # 다양한 POST 형식 시도
    for content_type, body in [
        ("application/json", json.dumps(test_entry_data)),
        ("application/x-www-form-urlencoded", f"name={test_entry_data['name']}&displayName={test_entry_data['displayName']}"),
    ]:
        try:
            r = session.post(
                f"{base}/rws/service-api/addressbook/entries",
                data=body,
                headers={"Content-Type": content_type},
                timeout=TIMEOUT
            )
            record_test(f"수신지_등록({content_type.split('/')[-1]})", r.status_code < 400,
                        f"status={r.status_code}, response={r.text[:200]}")

            if r.status_code < 400:
                try:
                    resp_data = r.json()
                    test_entry_id = resp_data.get("id") or resp_data.get("entryId")
                except:
                    pass
                break
        except Exception as e:
            record_test(f"수신지_등록({content_type.split('/')[-1]})", False, str(e))

    # --- 테스트 4: 등록한 수신지 삭제 ---
    if test_entry_id:
        log("\n[TEST 4] 수신지 삭제", "yellow")
        try:
            r = session.delete(
                f"{base}/rws/service-api/addressbook/entries/{test_entry_id}",
                timeout=TIMEOUT
            )
            record_test("수신지_삭제", r.status_code < 400,
                        f"status={r.status_code}, id={test_entry_id}")
        except Exception as e:
            record_test("수신지_삭제", False, str(e))

    # --- 테스트 5: Document Server 파일 목록 ---
    log("\n[TEST 5] Document Server 파일 목록", "yellow")
    try:
        r = session.get(f"{base}/rws/service-api/documents", timeout=TIMEOUT)
        record_test("문서서버_파일목록", r.status_code < 400,
                    f"status={r.status_code}, size={len(r.content)}B")
        if r.status_code < 400:
            json_file = RESULT_DIR / f"ricoh_{ip.replace('.','_')}_documents_structure.json"
            json_file.write_text(r.text, encoding="utf-8")
            log(f"  문서 목록 저장: {json_file.name}", "green")
    except Exception as e:
        record_test("문서서버_파일목록", False, str(e))

    # --- 테스트 6: eSCL 스캔 테스트 ---
    log("\n[TEST 6] eSCL 스캔 가능 여부", "yellow")
    try:
        r = session.get(f"http://{ip}/eSCL/ScannerCapabilities", timeout=TIMEOUT)
        record_test("eSCL_스캔능력", r.status_code == 200,
                    f"status={r.status_code}")
        if r.status_code == 200:
            caps_file = RESULT_DIR / f"ricoh_{ip.replace('.','_')}_escl_capabilities.xml"
            caps_file.write_text(r.text, encoding="utf-8")
    except Exception as e:
        record_test("eSCL_스캔능력", False, str(e))


# ============================================================
# CANON 테스트
# ============================================================
def test_canon(ip, username="7654321", password="7654321"):
    log("=" * 60, "cyan")
    log(f" CANON 핵심 기능 테스트 - {ip}", "cyan")
    log("=" * 60, "cyan")

    session = requests.Session()
    session.verify = False
    base = f"http://{ip}"

    # --- 테스트 1: WebDAV Advanced Box ---
    log("\n[TEST 1] WebDAV Advanced Box 접근", "yellow")
    for scheme in ["http", "https"]:
        for path in ["/WebDAV/AdvancedBox/", "/WebDAV/"]:
            url = f"{scheme}://{ip}{path}"
            try:
                r = session.request("PROPFIND", url, timeout=TIMEOUT, verify=False,
                                     headers={"Depth": "1"})
                record_test(f"WebDAV_{path}", r.status_code in [200, 207],
                            f"status={r.status_code}")
                if r.status_code in [200, 207]:
                    webdav_file = RESULT_DIR / f"canon_{ip.replace('.','_')}_webdav_response.xml"
                    webdav_file.write_text(r.text, encoding="utf-8")
                    log(f"  WebDAV 응답 저장: {webdav_file.name}", "green")
            except Exception as e:
                record_test(f"WebDAV_{path}", False, str(e))

    # --- 테스트 2: SMB Advanced Box ---
    log("\n[TEST 2] SMB Advanced Box 접근", "yellow")
    try:
        import subprocess
        result = subprocess.run(
            ["net", "view", f"\\\\{ip}", "/all"],
            capture_output=True, text=True, timeout=10
        )
        has_share = "share" in result.stdout.lower() or "advance" in result.stdout.lower()
        record_test("SMB_공유폴더", has_share, result.stdout[:200] + result.stderr[:200])
    except Exception as e:
        record_test("SMB_공유폴더", False, str(e))

    # --- 테스트 3: Remote UI 로그인 & 주소록 ---
    log("\n[TEST 3] Remote UI 접근", "yellow")
    try:
        r = session.get(f"{base}/", timeout=TIMEOUT, allow_redirects=True)
        record_test("RemoteUI_접근", r.status_code < 400,
                    f"status={r.status_code}, url={r.url}")

        # 메인 페이지 HTML 저장
        html_file = RESULT_DIR / f"canon_{ip.replace('.','_')}_main_page.html"
        html_file.write_text(r.text, encoding="utf-8")
    except Exception as e:
        record_test("RemoteUI_접근", False, str(e))

    # --- 테스트 4: eSCL ---
    log("\n[TEST 4] eSCL 스캔 가능 여부", "yellow")
    for scheme in ["https", "http"]:
        try:
            r = session.get(f"{scheme}://{ip}/eSCL/ScannerCapabilities",
                          timeout=TIMEOUT, verify=False)
            record_test(f"eSCL_스캔능력({scheme})", r.status_code == 200,
                        f"status={r.status_code}")
            if r.status_code == 200:
                caps_file = RESULT_DIR / f"canon_{ip.replace('.','_')}_escl_capabilities.xml"
                caps_file.write_text(r.text, encoding="utf-8")
                break
        except Exception as e:
            record_test(f"eSCL_스캔능력({scheme})", False, str(e))


# ============================================================
# SINDOH 테스트
# ============================================================
def test_sindoh(ip, username="admin", password="admin"):
    log("=" * 60, "cyan")
    log(f" SINDOH 핵심 기능 테스트 - {ip}", "cyan")
    log("=" * 60, "cyan")

    session = requests.Session()
    session.verify = False
    base = f"http://{ip}"

    # --- 테스트 1: 웹 인터페이스 접근 ---
    log("\n[TEST 1] 웹 인터페이스 접근", "yellow")
    try:
        r = session.get(base, timeout=TIMEOUT, allow_redirects=True)
        record_test("웹인터페이스_접근", r.status_code < 400,
                    f"status={r.status_code}, size={len(r.content)}B, url={r.url}")

        html_file = RESULT_DIR / f"sindoh_{ip.replace('.','_')}_main_page.html"
        html_file.write_text(r.text, encoding="utf-8")

        # iframe/frame src 추출
        frames = re.findall(r'(?:frame|iframe)[^>]+src=["\']([^"\']+)', r.text, re.I)
        log(f"  프레임 발견: {frames}", "cyan")
        for frame_src in frames:
            frame_url = frame_src if frame_src.startswith("http") else f"{base}{frame_src}"
            try:
                r2 = session.get(frame_url, timeout=TIMEOUT)
                safe_name = re.sub(r'[^\w]', '_', frame_src)[:50]
                frame_file = RESULT_DIR / f"sindoh_{ip.replace('.','_')}_frame_{safe_name}.html"
                frame_file.write_text(r2.text, encoding="utf-8")
                log(f"  프레임 저장: {frame_file.name}", "green")
            except:
                pass

    except Exception as e:
        record_test("웹인터페이스_접근", False, str(e))

    # --- 테스트 2: eSCL ---
    log("\n[TEST 2] eSCL 스캔 가능 여부", "yellow")
    try:
        r = session.get(f"http://{ip}/eSCL/ScannerCapabilities", timeout=TIMEOUT)
        record_test("eSCL_스캔능력", r.status_code == 200,
                    f"status={r.status_code}")
        if r.status_code == 200:
            caps_file = RESULT_DIR / f"sindoh_{ip.replace('.','_')}_escl_capabilities.xml"
            caps_file.write_text(r.text, encoding="utf-8")
    except Exception as e:
        record_test("eSCL_스캔능력", False, str(e))

    # --- 테스트 3: 다양한 API 경로 탐색 ---
    log("\n[TEST 3] API 경로 탐색", "yellow")
    api_paths = [
        "/cgi-bin/home.cgi", "/cgi-bin/top.cgi", "/cgi-bin/main.cgi",
        "/cgi-bin/scan.cgi", "/cgi-bin/box.cgi", "/cgi-bin/addr.cgi",
        "/cgi-bin/fax.cgi", "/cgi-bin/copy.cgi", "/cgi-bin/print.cgi",
        "/cgi-bin/net.cgi", "/cgi-bin/sys.cgi", "/cgi-bin/sec.cgi",
        "/sws/app/scan/scanbox", "/sws/app/addr", "/sws/app/box",
        "/sws/app/setting", "/sws/app/system",
        "/ws/scan", "/ws/system", "/ws/addressbook",
    ]
    for path in api_paths:
        url = f"{base}{path}"
        try:
            r = session.get(url, timeout=5)
            if r.status_code < 400:
                record_test(f"경로탐색_{path}", True, f"status={r.status_code}, size={len(r.content)}B")
                # 성공한 경로의 응답 저장
                safe = re.sub(r'[^\w]', '_', path)[:50]
                resp_file = RESULT_DIR / f"sindoh_{ip.replace('.','_')}{safe}.html"
                resp_file.write_text(r.text, encoding="utf-8")
        except:
            pass


# ============================================================
# MAIN
# ============================================================
def main():
    if len(sys.argv) < 3:
        print(f"Usage: python {sys.argv[0]} <brand> <ip> [username] [password]")
        sys.exit(0)

    brand = sys.argv[1].lower()
    ip = sys.argv[2]
    username = sys.argv[3] if len(sys.argv) > 3 else None
    password = sys.argv[4] if len(sys.argv) > 4 else None

    if brand == "ricoh":
        test_ricoh(ip, username or "admin", password or "")
    elif brand == "canon":
        test_canon(ip, username or "7654321", password or "7654321")
    elif brand == "sindoh":
        test_sindoh(ip, username or "admin", password or "admin")
    elif brand == "all":
        ips = ip.split(",")
        brands = ["sindoh", "ricoh", "canon"]
        for b, i in zip(brands, ips):
            if brand == "all":
                test_fn = {"ricoh": test_ricoh, "canon": test_canon, "sindoh": test_sindoh}
                test_fn[b](i.strip())
    else:
        log(f"지원하지 않는 브랜드: {brand}", "red")
        sys.exit(1)

    # 테스트 결과 요약
    log(f"\n{'='*60}", "cyan")
    log(" 테스트 결과 요약", "cyan")
    log(f"{'='*60}", "cyan")

    passed = sum(1 for t in test_results if t["success"])
    failed = sum(1 for t in test_results if not t["success"])
    log(f" 총 {len(test_results)}건: PASS {passed}, FAIL {failed}", "cyan")

    for t in test_results:
        color = "green" if t["success"] else "red"
        symbol = "PASS" if t["success"] else "FAIL"
        log(f"  [{symbol}] {t['test']}: {t['details'][:80]}", color)

    # 결과 저장
    result_file = RESULT_DIR / f"test_results_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
    with open(result_file, "w", encoding="utf-8") as f:
        json.dump(test_results, f, ensure_ascii=False, indent=2)
    log(f"\n결과 저장: {result_file}", "green")


if __name__ == "__main__":
    main()
