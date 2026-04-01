"""
복합기 웹 트래픽 로깅 프록시

사용법:
  python proxy_logger.py <복합기IP> [포트]
  예: python proxy_logger.py 192.168.11.227 8000

브라우저에서 http://localhost:9000/ 으로 접속하면
복합기(192.168.11.227:8000)로 중계하면서 모든 요청/응답을 기록합니다.
"""
import sys
import json
import threading
from datetime import datetime
from pathlib import Path
from http.server import HTTPServer, BaseHTTPRequestHandler
from urllib.parse import urlparse

import requests
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)

traffic = []
counter = 0
lock = threading.Lock()


class ProxyHandler(BaseHTTPRequestHandler):
    target_base = ""
    target_ip = ""
    target_port = ""
    local_port = 9000

    def do_REQUEST(self, method):
        global counter

        # 요청 본문 읽기
        content_length = int(self.headers.get("Content-Length", 0))
        body = self.rfile.read(content_length) if content_length > 0 else b""

        # 대상 URL 구성
        target_url = f"{self.target_base}{self.path}"

        # 요청 헤더 복사 (Host 제외)
        headers = {}
        for key, val in self.headers.items():
            if key.lower() not in ("host", "transfer-encoding"):
                headers[key] = val

        # 복합기로 전달
        try:
            r = requests.request(
                method, target_url,
                headers=headers, data=body,
                timeout=30, verify=False,
                allow_redirects=False
            )
        except Exception as e:
            self.send_error(502, f"Proxy error: {e}")
            return

        # 응답 본문 - 텍스트면 URL 치환
        resp_body = r.content
        ct = r.headers.get("Content-Type", "")
        if any(t in ct for t in ["text", "json", "xml", "javascript", "html"]):
            try:
                text = resp_body.decode("utf-8")
                # 원본 IP:포트 → localhost:프록시포트로 치환
                text = text.replace(f"http://{self.target_ip}:{self.target_port}", f"http://localhost:{self.local_port}")
                text = text.replace(f"https://{self.target_ip}:{self.target_port}", f"http://localhost:{self.local_port}")
                text = text.replace(f"http://{self.target_ip}", f"http://localhost:{self.local_port}")
                text = text.replace(f"https://{self.target_ip}", f"http://localhost:{self.local_port}")
                text = text.replace(f"//{self.target_ip}:{self.target_port}", f"//localhost:{self.local_port}")
                text = text.replace(f"//{self.target_ip}", f"//localhost:{self.local_port}")
                resp_body = text.encode("utf-8")
            except:
                pass

        # 로그 기록
        with lock:
            counter += 1
            idx = counter

        entry = {
            "index": idx,
            "timestamp": datetime.now().isoformat(),
            "method": method,
            "url": target_url,
            "path": self.path,
            "request_headers": dict(self.headers),
            "request_body": "",
            "status_code": r.status_code,
            "response_headers": dict(r.headers),
            "response_body_size": len(resp_body),
        }

        # 요청 본문 저장
        if body:
            try:
                entry["request_body"] = body.decode("utf-8")
            except:
                entry["request_body"] = f"<binary {len(body)} bytes>"

        # 텍스트 응답이면 본문도 저장
        if any(t in ct for t in ["text", "json", "xml", "javascript", "html"]):
            try:
                entry["response_body"] = resp_body.decode("utf-8")
            except:
                pass

        traffic.append(entry)

        # 콘솔 출력
        ts = datetime.now().strftime("%H:%M:%S")
        req_info = ""
        if body:
            req_info = f" | body={entry['request_body'][:100]}"
        print(f"[{ts}] #{idx:03d} {method} {self.path} → {r.status_code} ({len(resp_body)}B){req_info}")

        # 클라이언트에 응답 전달
        self.send_response(r.status_code)
        for key, val in r.headers.items():
            if key.lower() not in ("transfer-encoding", "content-encoding", "content-length"):
                self.send_header(key, val)
        self.send_header("Content-Length", str(len(resp_body)))
        self.end_headers()
        self.wfile.write(resp_body)

    def do_GET(self):
        self.do_REQUEST("GET")

    def do_POST(self):
        self.do_REQUEST("POST")

    def do_PUT(self):
        self.do_REQUEST("PUT")

    def do_DELETE(self):
        self.do_REQUEST("DELETE")

    def do_OPTIONS(self):
        self.do_REQUEST("OPTIONS")

    def do_PROPFIND(self):
        self.do_REQUEST("PROPFIND")

    def log_message(self, format, *args):
        pass  # 기본 로그 끄기


def save_traffic(target_ip):
    """트래픽 저장"""
    ip_safe = target_ip.replace(".", "_").replace(":", "_")
    ts = datetime.now().strftime("%Y%m%d_%H%M%S")

    # JSON 전체
    json_file = RESULT_DIR / f"proxy_{ip_safe}_{ts}.json"
    with open(json_file, "w", encoding="utf-8") as f:
        json.dump(traffic, f, ensure_ascii=False, indent=2)

    # 요약 텍스트
    summary_file = RESULT_DIR / f"proxy_{ip_safe}_{ts}_summary.txt"
    with open(summary_file, "w", encoding="utf-8") as f:
        f.write(f"프록시 트래픽 로그 - {target_ip}\n")
        f.write(f"기록 시각: {datetime.now().isoformat()}\n")
        f.write(f"총 {len(traffic)}건\n")
        f.write("=" * 80 + "\n\n")

        for e in traffic:
            f.write(f"--- #{e['index']:03d} {e['method']} {e['path']} → {e['status_code']} ---\n")
            f.write(f"URL: {e['url']}\n")
            if e.get("request_body"):
                f.write(f"Request Body: {e['request_body'][:2000]}\n")
            if e.get("response_body"):
                f.write(f"Response Body ({e['response_body_size']}B):\n{e['response_body'][:3000]}\n")
            f.write("\n")

    print(f"\n저장 완료:")
    print(f"  JSON: {json_file}")
    print(f"  요약: {summary_file}")
    print(f"  총 {len(traffic)}건 기록")


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(0)

    target_ip = sys.argv[1]
    target_port = sys.argv[2] if len(sys.argv) > 2 else "80"
    local_port = 9000

    target_base = f"http://{target_ip}:{target_port}"
    ProxyHandler.target_base = target_base
    ProxyHandler.target_ip = target_ip
    ProxyHandler.target_port = target_port
    ProxyHandler.local_port = local_port

    print("=" * 60)
    print(f"  복합기 트래픽 로깅 프록시")
    print(f"  대상: {target_base}")
    print(f"  브라우저에서 접속: http://localhost:{local_port}/")
    print(f"  종료: Ctrl+C")
    print("=" * 60)

    server = HTTPServer(("0.0.0.0", local_port), ProxyHandler)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print(f"\n\n종료 중... {len(traffic)}건 기록됨")
        if traffic:
            save_traffic(target_ip)
        server.server_close()


if __name__ == "__main__":
    main()
