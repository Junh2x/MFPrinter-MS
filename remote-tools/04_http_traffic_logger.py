import sys
import json
import os
import re
import time
import threading
from datetime import datetime
from pathlib import Path
from http.server import HTTPServer, BaseHTTPRequestHandler
from urllib.parse import urlparse

try:
    import requests
    import urllib3
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
except ImportError:
    print("requests 패키지 필요: pip install requests urllib3")
    sys.exit(1)

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)

LOG_FILE = RESULT_DIR / f"04_traffic_log_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"

traffic_log = []


def log(msg, color=None):
    colors = {"red": "\033[91m", "green": "\033[92m", "yellow": "\033[93m", "cyan": "\033[96m"}
    reset = "\033[0m"
    prefix = colors.get(color, "") if color else ""
    suffix = reset if color else ""
    ts = datetime.now().strftime("%H:%M:%S")
    print(f"[{ts}] {prefix}{msg}{suffix}")
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(f"[{ts}] {msg}\n")


def replay_mode(target_ip):
    """자동 방문 모드 - 주요 페이지를 순차적으로 방문하며 모든 요청/응답 기록"""
    log(f"자동 탐색 모드: {target_ip}", "cyan")

    session = requests.Session()
    session.verify = False

    base = f"http://{target_ip}"

    # 1단계: 메인 페이지 접근 & 모든 리소스 URL 수집
    log("메인 페이지 분석 중...", "yellow")
    try:
        r = session.get(base, timeout=10, allow_redirects=True)
        record_traffic("GET", r.url, r)

        # 프레임셋 감지 (많은 복합기가 frameset 사용)
        frames = re.findall(r'(?:src|href)=["\']([^"\']+)', r.text, re.I)
        internal_urls = set()
        for frame in frames:
            if frame.startswith("/"):
                internal_urls.add(f"{base}{frame}")
            elif frame.startswith("http") and target_ip in frame:
                internal_urls.add(frame)

        log(f"  발견된 내부 URL: {len(internal_urls)}개", "green")

        # 2단계: 모든 내부 URL 방문
        visited = {r.url}
        depth2_urls = set()

        for url in sorted(internal_urls):
            if url in visited:
                continue
            visited.add(url)
            try:
                r2 = session.get(url, timeout=10, allow_redirects=True)
                record_traffic("GET", url, r2)

                # 2차 링크도 수집
                if r2.status_code == 200 and "text" in r2.headers.get("Content-Type", ""):
                    links = re.findall(r'(?:src|href|action)=["\']([^"\']+)', r2.text, re.I)
                    for link in links: 
                        if link.startswith("/"):
                            full = f"{base}{link}"
                            if full not in visited:
                                depth2_urls.add(full)
                        elif link.startswith("http") and target_ip in link:
                            if link not in visited:
                                depth2_urls.add(link)
            except:
                continue

        log(f"  2차 URL: {len(depth2_urls)}개", "yellow")

        # 3단계: 2차 URL 방문 (최대 100개)
        for url in sorted(depth2_urls)[:100]:
            if url in visited:
                continue
            visited.add(url)
            try:
                r3 = session.get(url, timeout=10, allow_redirects=True)
                record_traffic("GET", url, r3)
            except:
                continue

    except Exception as e:
        log(f"메인 페이지 접근 실패: {e}", "red")

    # 결과 저장
    save_traffic_log(target_ip)


def record_traffic(method, url, response):
    """트래픽 기록"""
    entry = {
        "timestamp": datetime.now().isoformat(),
        "method": method,
        "url": url,
        "request_headers": dict(response.request.headers) if response.request else {},
        "status_code": response.status_code,
        "response_headers": dict(response.headers),
        "content_type": response.headers.get("Content-Type", ""),
        "content_length": len(response.content),
    }

    # 텍스트 응답은 본문도 저장 (100KB 이하)
    ct = response.headers.get("Content-Type", "")
    if any(t in ct for t in ["text", "json", "xml", "javascript"]) and len(response.content) < 102400:
        try:
            entry["response_body"] = response.text[:10000]
        except:
            pass

    # 요청 본문
    if response.request and response.request.body:
        body = response.request.body
        if isinstance(body, bytes):
            try:
                body = body.decode("utf-8")
            except:
                body = f"<binary {len(body)} bytes>"
        entry["request_body"] = str(body)[:5000]

    traffic_log.append(entry)

    # 상태에 따라 컬러 출력
    status = response.status_code
    ct_short = ct.split(";")[0].strip()[:30]
    if status < 300:
        log(f"  {method} {url} → {status} ({ct_short})", "green")
    elif status < 400:
        log(f"  {method} {url} → {status} REDIRECT", "yellow")
    else:
        log(f"  {method} {url} → {status}", "red")


def save_traffic_log(target_ip):
    """트래픽 로그 저장"""
    ip_safe = target_ip.replace(".", "_")
    ts = datetime.now().strftime("%Y%m%d_%H%M%S")

    # JSON 전체 로그
    json_file = RESULT_DIR / f"traffic_{ip_safe}_{ts}.json"
    with open(json_file, "w", encoding="utf-8") as f:
        json.dump(traffic_log, f, ensure_ascii=False, indent=2)
    log(f"트래픽 로그 저장: {json_file} ({len(traffic_log)}건)", "green")

    # 요약 텍스트
    summary_file = RESULT_DIR / f"traffic_{ip_safe}_{ts}_summary.txt"
    with open(summary_file, "w", encoding="utf-8") as f:
        f.write(f"HTTP 트래픽 요약 - {target_ip}\n")
        f.write(f"캡처 시각: {datetime.now().isoformat()}\n")
        f.write(f"총 {len(traffic_log)}건\n")
        f.write("=" * 80 + "\n\n")

        for i, entry in enumerate(traffic_log):
            f.write(f"--- [{i+1}] {entry['method']} {entry['url']} ---\n")
            f.write(f"Status: {entry['status_code']}\n")
            f.write(f"Content-Type: {entry.get('content_type', '')}\n")
            f.write(f"Content-Length: {entry.get('content_length', 0)}\n")
            if entry.get("request_body"):
                f.write(f"Request Body: {entry['request_body'][:500]}\n")
            if entry.get("response_body"):
                f.write(f"Response Body (preview):\n{entry['response_body'][:1000]}\n")
            f.write("\n")

    log(f"트래픽 요약 저장: {summary_file}", "green")

    # URL 목록만 별도 저장
    urls_file = RESULT_DIR / f"traffic_{ip_safe}_{ts}_urls.txt"
    with open(urls_file, "w", encoding="utf-8") as f:
        for entry in traffic_log:
            f.write(f"{entry['status_code']} {entry['method']} {entry['url']}\n")
    log(f"URL 목록 저장: {urls_file}", "green")


def main():
    if len(sys.argv) < 2:
        print(f"Usage: python {sys.argv[0]} --replay <IP>")
        sys.exit(0)

    if sys.argv[1] == "--replay":
        if len(sys.argv) < 3:
            print("IP를 지정하세요")
            sys.exit(1)
        replay_mode(sys.argv[2])
    else:
        replay_mode(sys.argv[1])


if __name__ == "__main__":
    main()
