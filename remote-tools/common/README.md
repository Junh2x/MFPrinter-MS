# remote-tools

원격 PC에서 복합기 정보 수집 및 테스트용 스크립트.

사용법: `docs/remote-tools-사용법.md` 참고.

## 파일 목록

| 파일 | 역할 |
|------|------|
| 01_setup.ps1 | 환경 설정 (Python, 패키지, 네트워크 정보) |
| 02_discovery.py | 네트워크 복합기 탐색, 포트 스캔, 브랜드 식별 |
| 03_web_api_capture.py | 브랜드별 웹 API 자동 캡처 |
| 04_http_traffic_logger.py | HTTP 트래픽 전체 기록 |
| 05_api_test.py | 수신지 등록/삭제/파일 접근 실제 테스트 |
| 06_screenshot_helper.ps1 | 스크린샷 자동 캡처 (3초 간격) |
