"""리코 수신지 삭제"""
import sys
import time
from ricoh_common import *

init_log("delete")


def delete_destination(s, wim_token, entry_id):
    """삭제 — entryIndex는 조회 시 반환되는 ID값 (등록번호 아님)"""
    log(f"\n=== 수신지 삭제 ===")
    log(f"  entryIndex={entry_id}")

    # multipart/form-data로 전송
    r = s.post(f"{BASE}/web/entry/ko/address/adrsDeleteEntries.cgi",
               files={
                   "entryIndex": (None, f"{entry_id},"),
                   "wimToken": (None, wim_token),
               },
               headers={
                   "X-Requested-With": "XMLHttpRequest",
                   "Accept": "text/plain, */*",
                   "Referer": f"{BASE}/web/entry/ko/address/adrsList.cgi",
               },
               timeout=10)
    save("delete", r)
    log(f"  삭제 응답 → {r.status_code} ({len(r.content)}B)")
    log(f"  응답: {r.text[:200]}")

    log(f"\n>>> 삭제 완료 (entryIndex={entry_id})")
    return True


if __name__ == "__main__":
    # entryIndex: 주소록 조회 시 반환되는 ID값 (첫 번째 필드)
    # 등록번호(00011)가 아니라 ID값(예: 12)을 사용
    ENTRY_ID = "12"

    args = sys.argv[1:]
    if len(args) >= 1: ENTRY_ID = args[0]

    s = create_session()
    wim_token = login(s)
    if not wim_token:
        sys.exit(1)
    time.sleep(1)

    # 삭제 전 목록 확인
    log("\n=== 현재 목록 ===")
    entries = list_addresses(s)
    for e in entries:
        marker = " <<<" if e["id"] == ENTRY_ID else ""
        log(f"  [ID:{e['id']}] {e['regNo']} {e['name']} | 폴더:{e['folder']}{marker}")
    time.sleep(1)

    delete_destination(s, wim_token, ENTRY_ID)
    time.sleep(1)

    # 삭제 후 확인
    log("\n=== 삭제 후 목록 ===")
    entries = list_addresses(s)
    for e in entries:
        log(f"  [ID:{e['id']}] {e['regNo']} {e['name']}")
