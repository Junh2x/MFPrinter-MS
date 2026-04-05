"""
Chrome DevTools Protocol (CDP)을 이용한 네트워크 트래픽 캡처.

Chrome을 --remote-debugging-port=9222 로 실행한 뒤 이 스크립트를 실행하면,
브라우저에서 수행하는 모든 HTTP 요청/응답을 자동으로 기록합니다.

사용법:
  1. Chrome 종료 후 디버깅 모드로 실행:
     chrome.exe --remote-debugging-port=9222 --user-data-dir=C:\\temp\\chrome_debug
  2. 이 스크립트 실행:
     python cdp_capture.py
  3. 브라우저에서 자유롭게 테스트
  4. Ctrl+C로 종료 → 결과 저장
"""
import json
import sys
import time
import base64
from datetime import datetime
from pathlib import Path

try:
    import requests
    import websocket
except ImportError:
    print("필요 패키지 설치: pip install requests websocket-client")
    sys.exit(1)

RESULT_DIR = Path(__file__).parent / "results"
RESULT_DIR.mkdir(exist_ok=True)
RUN_TS = datetime.now().strftime("%Y%m%d_%H%M%S")
JSONL_FILE = RESULT_DIR / f"cdp_capture_{RUN_TS}.jsonl"

CDP_PORT = 9222
traffic = {}  # requestId -> entry
completed = []


def get_ws_url():
    """Chrome 디버깅 WebSocket URL 가져오기"""
    try:
        r = requests.get(f"http://localhost:{CDP_PORT}/json", timeout=5)
        tabs = r.json()
        for tab in tabs:
            if tab.get("type") == "page" and "webSocketDebuggerUrl" in tab:
                print(f"  탭 발견: {tab.get('title', '')[:50]}")
                return tab["webSocketDebuggerUrl"]
    except Exception as e:
        print(f"Chrome 연결 실패: {e}")
        print(f"Chrome을 --remote-debugging-port={CDP_PORT} 로 실행했는지 확인하세요.")
        sys.exit(1)
    print("연결 가능한 탭이 없습니다.")
    sys.exit(1)


def save_entry(entry):
    """한 건을 즉시 파일에 저장"""
    with open(JSONL_FILE, "a", encoding="utf-8") as f:
        f.write(json.dumps(entry, ensure_ascii=False) + "\n")


def on_message(ws, message):
    data = json.loads(message)
    method = data.get("method", "")

    if method == "Network.requestWillBeSent":
        params = data["params"]
        req_id = params["requestId"]
        request = params["request"]

        entry = {
            "requestId": req_id,
            "timestamp": datetime.now().isoformat(),
            "method": request["method"],
            "url": request["url"],
            "request_headers": request.get("headers", {}),
            "request_body": params.get("request", {}).get("postData", ""),
        }
        traffic[req_id] = entry

        ts = datetime.now().strftime("%H:%M:%S")
        body_preview = entry["request_body"][:80] if entry["request_body"] else ""
        body_info = f" | {body_preview}" if body_preview else ""
        print(f"[{ts}] → {request['method']} {request['url'][:100]}{body_info}")

    elif method == "Network.responseReceived":
        params = data["params"]
        req_id = params["requestId"]
        response = params["response"]

        if req_id in traffic:
            traffic[req_id]["status_code"] = response["status"]
            traffic[req_id]["response_headers"] = response.get("headers", {})
            traffic[req_id]["content_type"] = response.get("headers", {}).get("Content-Type", "")
            traffic[req_id]["response_url"] = response.get("url", "")

            ts = datetime.now().strftime("%H:%M:%S")
            print(f"[{ts}] ← {response['status']} {response.get('url', '')[:100]}")

    elif method == "Network.loadingFinished":
        params = data["params"]
        req_id = params["requestId"]

        if req_id in traffic:
            entry = traffic[req_id]
            ct = entry.get("content_type", "")

            # 텍스트 응답 본문 가져오기
            if any(t in ct for t in ["text", "json", "xml", "javascript", "html"]):
                try:
                    ws.send(json.dumps({
                        "id": hash(req_id) & 0x7FFFFFFF,
                        "method": "Network.getResponseBody",
                        "params": {"requestId": req_id}
                    }))
                except:
                    pass

            entry["completed"] = True
            completed.append(entry)
            save_entry(entry)
            del traffic[req_id]

    elif "result" in data and "body" in data.get("result", {}):
        # Network.getResponseBody 응답 처리
        body = data["result"]["body"]
        is_base64 = data["result"].get("base64Encoded", False)

        if not is_base64 and len(body) < 50000:
            # 마지막 완료 항목에 응답 본문 추가
            if completed:
                completed[-1]["response_body"] = body
                # 파일 업데이트 (마지막 줄 덮어쓰기는 복잡하므로 별도 저장)
                body_file = RESULT_DIR / f"cdp_{RUN_TS}_body_{len(completed):04d}.txt"
                body_file.write_text(body[:50000], encoding="utf-8")


def on_error(ws, error):
    print(f"WebSocket 에러: {error}")


def on_close(ws, code, msg):
    print(f"WebSocket 종료: {code} {msg}")


def on_open(ws):
    # Network 도메인 활성화
    ws.send(json.dumps({"id": 1, "method": "Network.enable"}))
    print("네트워크 캡처 시작! 브라우저에서 자유롭게 테스트하세요.\n")


def save_summary():
    """최종 요약 저장"""
    summary_file = RESULT_DIR / f"cdp_capture_{RUN_TS}_summary.txt"
    with open(summary_file, "w", encoding="utf-8") as f:
        f.write(f"CDP 네트워크 캡처 요약\n")
        f.write(f"기록 시각: {datetime.now().isoformat()}\n")
        f.write(f"총 {len(completed)}건\n")
        f.write("=" * 80 + "\n\n")

        for e in completed:
            f.write(f"--- {e['method']} {e['url']} → {e.get('status_code', '?')} ---\n")
            if e.get("request_body"):
                f.write(f"Request Body: {e['request_body'][:2000]}\n")
            if e.get("response_body"):
                f.write(f"Response Body:\n{e['response_body'][:3000]}\n")
            f.write("\n")

    print(f"\n저장 완료:")
    print(f"  실시간 로그: {JSONL_FILE}")
    print(f"  요약: {summary_file}")
    print(f"  응답 본문: {RESULT_DIR}/cdp_{RUN_TS}_body_*.txt")
    print(f"  총 {len(completed)}건 기록")


def main():
    print("=" * 60)
    print("  Chrome DevTools 네트워크 캡처")
    print(f"  실시간 로그: {JSONL_FILE}")
    print("  종료: Ctrl+C")
    print("=" * 60)

    ws_url = get_ws_url()
    print(f"  WebSocket: {ws_url}\n")

    ws = websocket.WebSocketApp(
        ws_url,
        on_open=on_open,
        on_message=on_message,
        on_error=on_error,
        on_close=on_close,
    )

    try:
        ws.run_forever()
    except KeyboardInterrupt:
        print(f"\n\n종료 중... {len(completed)}건 기록됨")
        if completed:
            save_summary()
        ws.close()


if __name__ == "__main__":
    main()
