import sys
import json
import os
import re
import time
from datetime import datetime
from pathlib import Path
from urllib.parse import urljoin, urlparse

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

LOG_FILE = RESULT_DIR / f"03_web_api_capture_log_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"


def log(msg, color=None):
    colors = {"red": "\033[91m", "green": "\033[92m", "yellow": "\033[93m", "cyan": "\033[96m"}
    reset = "\033[0m"
    prefix = colors.get(color, "") if color else ""
    suffix = reset if color else ""
    ts = datetime.now().strftime("%H:%M:%S")
    print(f"[{ts}] {prefix}{msg}{suffix}")
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(f"[{ts}] {msg}\n")


def save_response(brand, ip, name, response, extra_info=None):
    """응답을 파일로 저장"""
    safe_name = re.sub(r'[^\w\-]', '_', name)
    ip_safe = ip.replace(".", "_")
    prefix = f"{brand}_{ip_safe}_{safe_name}"

    info = {
        "name": name,
        "url": response.url,
        "method": response.request.method,
        "status_code": response.status_code,
        "request_headers": dict(response.request.headers),
        "response_headers": dict(response.headers),
        "content_type": response.headers.get("Content-Type", ""),
        "content_length": len(response.content),
        "timestamp": datetime.now().isoformat(),
    }
    if response.request.body:
        body = response.request.body
        if isinstance(body, bytes):
            try:
                body = body.decode("utf-8")
            except:
                body = f"<binary {len(body)} bytes>"
        info["request_body"] = body

    if extra_info:
        info.update(extra_info)

    # 메타 정보 저장
    meta_file = RESULT_DIR / f"{prefix}_meta.json"
    with open(meta_file, "w", encoding="utf-8") as f:
        json.dump(info, f, ensure_ascii=False, indent=2)

    # 응답 본문 저장
    ct = response.headers.get("Content-Type", "")
    if "json" in ct:
        ext = "json"
    elif "xml" in ct:
        ext = "xml"
    else:
        ext = "html"

    body_file = RESULT_DIR / f"{prefix}_body.{ext}"
    body_file.write_bytes(response.content)

    log(f"  저장: {prefix}_meta.json + {prefix}_body.{ext}", "green")
    return info


# ============================================================
# RICOH IM C2010
# ============================================================
class RicohProber:
    def __init__(self, ip, username="admin", password=""):
        self.ip = ip
        self.base = f"http://{ip}"
        self.session = requests.Session()
        self.session.verify = False
        self.username = username
        self.password = password

    def login(self):
        log("[RICOH] 로그인 시도...", "yellow")

        # 방법 1: Web Image Monitor 로그인
        login_urls = [
            f"{self.base}/web/guest/en/websys/webArch/login.cgi",
            f"{self.base}/web/guest/ko/websys/webArch/login.cgi",
        ]
        for url in login_urls:
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                save_response("ricoh", self.ip, "login_page", r)
                if r.status_code == 200:
                    log(f"  로그인 페이지 접근 성공: {url}", "green")
                    break
            except:
                continue

        # 로그인 POST 시도
        login_data = {
            "userid_work": self.username,
            "password_work": self.password,
            "submit001": "Login",
        }
        for url in login_urls:
            try:
                r = self.session.post(url, data=login_data, timeout=TIMEOUT, allow_redirects=True)
                save_response("ricoh", self.ip, "login_result", r)
                if r.status_code in [200, 302]:
                    log(f"  로그인 POST 완료 (status: {r.status_code})", "green")
                    break
            except:
                continue

    def probe_rest_api(self):
        """REST API 전체 탐색"""
        log("[RICOH] REST API 탐색...", "yellow")

        api_paths = [
            ("device_info", "/rws/service-api/device"),
            ("device_status", "/rws/service-api/device/status"),
            ("scanner_capabilities", "/rws/service-api/scanner"),
            ("scanner_status", "/rws/service-api/scanner/status"),
            ("documents_list", "/rws/service-api/documents"),
            ("addressbook_entries", "/rws/service-api/addressbook/entries"),
            ("addressbook_config", "/rws/service-api/addressbook"),
            ("network_config", "/rws/service-api/network"),
            ("system_config", "/rws/service-api/system"),
            ("jobs_list", "/rws/service-api/jobs"),
        ]

        results = {}
        for name, path in api_paths:
            url = f"{self.base}{path}"
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                results[name] = {
                    "status": r.status_code,
                    "content_type": r.headers.get("Content-Type", ""),
                    "size": len(r.content)
                }
                if r.status_code < 400:
                    save_response("ricoh", self.ip, f"api_{name}", r)
                    log(f"  [O] {name}: {r.status_code}", "green")
                else:
                    log(f"  [X] {name}: {r.status_code}", "yellow")
            except Exception as e:
                log(f"  [!] {name}: {e}", "red")
                results[name] = {"error": str(e)}

        return results

    def probe_document_server(self):
        """Document Server (HDD) 접근"""
        log("[RICOH] Document Server 탐색...", "yellow")

        paths = [
            ("doc_server_list", "/DRS/document/list"),
            ("doc_server_main", "/web/guest/en/websys/webArch/docServer.cgi"),
            ("doc_server_ko", "/web/guest/ko/websys/webArch/docServer.cgi"),
        ]

        for name, path in paths:
            url = f"{self.base}{path}"
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                save_response("ricoh", self.ip, name, r)
                if r.status_code < 400:
                    log(f"  [O] {name}: {r.status_code} ({len(r.content)}B)", "green")
                else:
                    log(f"  [X] {name}: {r.status_code}", "yellow")
            except Exception as e:
                log(f"  [!] {name}: {e}", "red")

    def probe_addressbook(self):
        """주소록 기능 탐색"""
        log("[RICOH] 주소록 탐색...", "yellow")

        paths = [
            ("addr_web", "/web/guest/en/websys/webArch/addressBook.cgi"),
            ("addr_web_ko", "/web/guest/ko/websys/webArch/addressBook.cgi"),
            ("addr_api", "/rws/service-api/addressbook/entries"),
            ("addr_api_detail", "/rws/service-api/addressbook/entries?limit=10"),
        ]

        for name, path in paths:
            url = f"{self.base}{path}"
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                save_response("ricoh", self.ip, name, r)
                if r.status_code < 400:
                    log(f"  [O] {name}: {r.status_code} ({len(r.content)}B)", "green")
                else:
                    log(f"  [X] {name}: {r.status_code}", "yellow")
            except Exception as e:
                log(f"  [!] {name}: {e}", "red")

    def run_all(self):
        self.login()
        self.probe_rest_api()
        self.probe_document_server()
        self.probe_addressbook()


# ============================================================
# CANON iR-ADV
# ============================================================
class CanonProber:
    def __init__(self, ip, username="7654321", password="7654321"):
        self.ip = ip
        self.base = f"http://{ip}"
        self.session = requests.Session()
        self.session.verify = False
        self.username = username
        self.password = password

    def login(self):
        log("[CANON] 로그인 시도...", "yellow")

        # Remote UI 로그인 페이지
        login_urls = [
            f"{self.base}/login",
            f"{self.base}/",
            f"http://{self.ip}:8000/",
        ]
        for url in login_urls:
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                save_response("canon", self.ip, "login_page", r)
                if r.status_code == 200:
                    log(f"  로그인 페이지: {url}", "green")

                    # CSRF 토큰 추출 시도
                    csrf_match = re.search(r'name=["\']csrf[_-]?token["\'].*?value=["\']([^"\']+)', r.text, re.I)
                    if csrf_match:
                        log(f"  CSRF 토큰 발견: {csrf_match.group(1)[:20]}...", "yellow")

                    # 로그인 폼 액션 URL 추출
                    form_match = re.search(r'<form[^>]+action=["\']([^"\']+)', r.text, re.I)
                    if form_match:
                        log(f"  폼 액션: {form_match.group(1)}", "yellow")
                    break
            except:
                continue

        # 로그인 POST 시도 (다양한 파라미터 조합)
        login_params_list = [
            {"DeptId": self.username, "Password": self.password},
            {"sysid": self.username, "pwd": self.password},
            {"userid": self.username, "password": self.password},
        ]
        for params in login_params_list:
            for url in login_urls[:2]:
                try:
                    r = self.session.post(url, data=params, timeout=TIMEOUT, allow_redirects=True)
                    save_response("canon", self.ip, f"login_attempt_{list(params.keys())[0]}", r)
                    log(f"  로그인 시도 ({list(params.keys())}): status={r.status_code}", "yellow")
                except:
                    continue

    def probe_webdav(self):
        """WebDAV 접근 테스트"""
        log("[CANON] WebDAV 탐색...", "yellow")

        webdav_paths = [
            "/WebDAV/",
            "/WebDAV/AdvancedBox/",
            "/WebDAV/advancedbox/",
        ]

        for path in webdav_paths:
            for scheme in ["http", "https"]:
                url = f"{scheme}://{self.ip}{path}"
                for method in ["PROPFIND", "OPTIONS", "GET"]:
                    try:
                        headers = {}
                        if method == "PROPFIND":
                            headers = {"Depth": "1", "Content-Type": "application/xml"}
                        r = self.session.request(method, url, timeout=TIMEOUT,
                                                  verify=False, headers=headers)
                        save_response("canon", self.ip, f"webdav_{path.replace('/','_')}_{method}", r)
                        if r.status_code < 400:
                            log(f"  [O] {method} {url}: {r.status_code}", "green")
                        else:
                            log(f"  [X] {method} {url}: {r.status_code}", "yellow")
                    except Exception as e:
                        log(f"  [!] {method} {url}: {e}", "red")

    def probe_document_box(self):
        """Mail Box / Advanced Box 접근"""
        log("[CANON] Document Box 탐색...", "yellow")

        paths = [
            ("mailbox", "/rui/MailBox"),
            ("advancedbox", "/rui/AdvancedBox"),
            ("stored_files", "/rui/StoredFiles"),
            ("box_list", "/rui/boxList"),
        ]

        for name, path in paths:
            url = f"{self.base}{path}"
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                save_response("canon", self.ip, name, r)
                status_color = "green" if r.status_code < 400 else "yellow"
                log(f"  [{r.status_code}] {name}: {url}", status_color)
            except Exception as e:
                log(f"  [!] {name}: {e}", "red")

    def probe_addressbook(self):
        """주소록 탐색"""
        log("[CANON] 주소록 탐색...", "yellow")

        paths = [
            ("addr_main", "/rui/AddressBook"),
            ("addr_list", "/rui/addressList"),
            ("addr_dest", "/rui/setDestination"),
            ("addr_oce", "/oce/addressbook"),
        ]

        for name, path in paths:
            url = f"{self.base}{path}"
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                save_response("canon", self.ip, name, r)
                status_color = "green" if r.status_code < 400 else "yellow"
                log(f"  [{r.status_code}] {name}: {url}", status_color)
            except Exception as e:
                log(f"  [!] {name}: {e}", "red")

    def probe_smb_share(self):
        """SMB 공유 확인 (net view)"""
        log("[CANON] SMB Advanced Box 확인...", "yellow")
        try:
            import subprocess
            result = subprocess.run(
                ["net", "view", f"\\\\{self.ip}", "/all"],
                capture_output=True, text=True, timeout=10
            )
            output = result.stdout + result.stderr
            log(f"  net view 결과:\n{output}", "cyan")

            out_file = RESULT_DIR / f"canon_{self.ip.replace('.','_')}_smb_share.txt"
            out_file.write_text(output, encoding="utf-8")
        except Exception as e:
            log(f"  SMB 확인 실패: {e}", "red")

    def run_all(self):
        self.login()
        self.probe_webdav()
        self.probe_document_box()
        self.probe_addressbook()
        self.probe_smb_share()


# ============================================================
# SINDOH D420/D450
# ============================================================
class SindohProber:
    def __init__(self, ip, username="admin", password="admin"):
        self.ip = ip
        self.base = f"http://{ip}"
        self.session = requests.Session()
        self.session.verify = False
        self.username = username
        self.password = password

    def login(self):
        log("[SINDOH] 로그인 시도...", "yellow")

        # 메인 페이지 접근
        try:
            r = self.session.get(self.base, timeout=TIMEOUT)
            save_response("sindoh", self.ip, "main_page", r)
            log(f"  메인 페이지: {r.status_code} ({len(r.content)}B)", "green")

            # 로그인 폼 찾기
            forms = re.findall(r'<form[^>]*>(.*?)</form>', r.text, re.DOTALL | re.I)
            for i, form in enumerate(forms):
                inputs = re.findall(r'<input[^>]+>', form, re.I)
                log(f"  폼 {i}: {len(inputs)} 입력 필드", "yellow")
                for inp in inputs:
                    name_match = re.search(r'name=["\']([^"\']+)', inp, re.I)
                    type_match = re.search(r'type=["\']([^"\']+)', inp, re.I)
                    if name_match:
                        log(f"    - {name_match.group(1)} (type={type_match.group(1) if type_match else 'text'})")
        except Exception as e:
            log(f"  메인 페이지 접근 실패: {e}", "red")

        # 다양한 로그인 URL 시도
        login_paths = [
            "/cgi-bin/login.cgi",
            "/login.cgi",
            "/web/login",
            "/webglue/login",
        ]

        for path in login_paths:
            url = f"{self.base}{path}"
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                if r.status_code < 400:
                    save_response("sindoh", self.ip, f"login_{path.replace('/','_')}", r)
                    log(f"  로그인 경로 발견: {url} ({r.status_code})", "green")

                    # POST 로그인 시도
                    login_data = {
                        "username": self.username,
                        "password": self.password,
                        "id": self.username,
                        "pw": self.password,
                    }
                    r2 = self.session.post(url, data=login_data, timeout=TIMEOUT, allow_redirects=True)
                    save_response("sindoh", self.ip, f"login_post_{path.replace('/','_')}", r2)
                    log(f"  POST 로그인: {r2.status_code}", "yellow")
            except:
                continue

    def crawl_web_interface(self):
        """웹 인터페이스 전체 크롤링 (링크 수집)"""
        log("[SINDOH] 웹 인터페이스 크롤링...", "yellow")
        visited = set()
        to_visit = [self.base + "/"]
        all_links = []

        while to_visit and len(visited) < 50:
            url = to_visit.pop(0)
            if url in visited:
                continue
            visited.add(url)

            try:
                r = self.session.get(url, timeout=TIMEOUT)
                if r.status_code != 200:
                    continue

                # 같은 호스트의 링크만 추출
                links = re.findall(r'href=["\']([^"\']+)', r.text, re.I)
                for link in links:
                    if link.startswith("/"):
                        full_url = f"{self.base}{link}"
                    elif link.startswith("http"):
                        if self.ip not in link:
                            continue
                        full_url = link
                    else:
                        continue

                    if full_url not in visited:
                        to_visit.append(full_url)
                        all_links.append(full_url)

            except:
                continue

        # 발견된 링크 저장
        links_file = RESULT_DIR / f"sindoh_{self.ip.replace('.','_')}_all_links.txt"
        links_file.write_text("\n".join(sorted(set(all_links))), encoding="utf-8")
        log(f"  발견된 링크 {len(set(all_links))}개 저장: {links_file.name}", "green")

    def probe_cgi_endpoints(self):
        """CGI 엔드포인트 탐색"""
        log("[SINDOH] CGI 엔드포인트 탐색...", "yellow")

        paths = [
            # 문서함 관련
            ("docbox_list", "/cgi-bin/docbox.cgi"),
            ("docbox_list2", "/cgi-bin/docbox_list.cgi"),
            ("scan_box", "/cgi-bin/scanbox.cgi"),
            ("shared_box", "/cgi-bin/sharedbox.cgi"),
            ("download", "/cgi-bin/download.cgi"),
            # 주소록 관련
            ("addressbook", "/cgi-bin/addressbook.cgi"),
            ("address_list", "/cgi-bin/address_list.cgi"),
            ("destination", "/cgi-bin/destination.cgi"),
            ("dest_reg", "/cgi-bin/dest_reg.cgi"),
            # 설정 관련
            ("config", "/cgi-bin/config.cgi"),
            ("network", "/cgi-bin/network.cgi"),
            ("status", "/cgi-bin/status.cgi"),
            ("device_info", "/cgi-bin/device_info.cgi"),
            ("system", "/cgi-bin/system.cgi"),
            # WebGlue 관련
            ("webglue_main", "/webglue/"),
            ("webglue_addr", "/webglue/addressbook"),
            ("webglue_scan", "/webglue/scanbox"),
            ("webglue_doc", "/webglue/docbox"),
            ("webglue_config", "/webglue/config"),
            # 기타
            ("top", "/top.html"),
            ("index2", "/index2.html"),
            ("main", "/main.html"),
            ("menu", "/menu.html"),
            ("frame", "/frame.html"),
        ]

        for name, path in paths:
            url = f"{self.base}{path}"
            try:
                r = self.session.get(url, timeout=TIMEOUT)
                if r.status_code < 400:
                    save_response("sindoh", self.ip, f"cgi_{name}", r)
                    log(f"  [O] {name}: {r.status_code} ({len(r.content)}B)", "green")
                elif r.status_code == 401:
                    log(f"  [!] {name}: 401 인증필요", "yellow")
                else:
                    log(f"  [X] {name}: {r.status_code}", "yellow")
            except Exception as e:
                log(f"  [!] {name}: {e}", "red")

    def run_all(self):
        self.login()
        self.crawl_web_interface()
        self.probe_cgi_endpoints()


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

    log("=" * 60, "cyan")
    log(f" {brand.upper()} 웹 API 캡처 - {ip}", "cyan")
    log("=" * 60, "cyan")

    if brand == "ricoh":
        prober = RicohProber(ip, username or "admin", password or "")
    elif brand == "canon":
        prober = CanonProber(ip, username or "7654321", password or "7654321")
    elif brand == "sindoh":
        prober = SindohProber(ip, username or "admin", password or "admin")
    else:
        log(f"지원하지 않는 브랜드: {brand}", "red")
        sys.exit(1)

    prober.run_all()

    log(f"\n완료! 결과 파일: {RESULT_DIR}", "green")


if __name__ == "__main__":
    main()
