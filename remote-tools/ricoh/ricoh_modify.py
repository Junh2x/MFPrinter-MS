"""리코 수신지 수정"""
import sys
import time
from ricoh_common import *

init_log("modify")


def modify_destination(s, wim_token, reg_no, name, display_name, folder_path, folder_user, folder_pass):
    log(f"\n=== 수신지 수정 ===")
    log(f"  등록번호={reg_no}, 이름={name}, 키표시={display_name}")
    log(f"  폴더={folder_path}, 유저={folder_user}")

    # 위자드 초기화 (mode=MODUSER, entryIndexIn 필수)
    r = s.post(f"{BASE}/web/entry/ko/address/adrsGetUserWizard.cgi",
               data={"mode": "MODUSER", "outputSpecifyModeIn": "PROGRAMMED",
                     "entryIndexIn": reg_no, "wimToken": wim_token},
               headers=ajax_headers(), timeout=10)
    save("wizard_init_mod", r)
    log(f"  위자드 초기화 → {r.status_code}")
    time.sleep(1)

    url = f"{BASE}/web/entry/ko/address/adrsSetUserWizard.cgi"
    h = ajax_headers()

    # Step 1: BASE
    r = s.post(url, headers=h, timeout=10, data=[
        ("mode", "MODUSER"), ("step", "BASE"), ("wimToken", wim_token),
        ("entryIndexIn", reg_no), ("entryNameIn", name), ("entryDisplayNameIn", display_name),
        ("entryTagInfoIn", "2"), ("entryTagInfoIn", "10"), ("entryTagInfoIn", "6"), ("entryTagInfoIn", "1"),
    ])
    log(f"[Step1] BASE → {r.status_code} ({r.text.strip()[:50]})")

    # Step 2: FAX
    r = s.post(url, headers=h, timeout=10,
               data={"mode": "MODUSER", "step": "FAX", "wimToken": wim_token, "faxDestIn": ""})
    log(f"[Step2] FAX → {r.status_code} ({r.text.strip()[:50]})")

    # Step 3: FOLDER
    pw = b64(folder_pass) if folder_pass else ""
    r = s.post(url, headers=h, timeout=10, data={
        "mode": "MODUSER", "step": "FOLDER", "wimToken": wim_token,
        "folderProtocolIn": "SMB_O", "folderPortNoIn": "21", "folderServerNameIn": "",
        "folderPathNameIn": folder_path, "folderAuthUserNameIn": folder_user,
        "wk_folderPasswordIn": "", "folderPasswordIn": pw,
        "wk_folderPasswordConfirmIn": "", "folderPasswordConfirmIn": pw,
    })
    log(f"[Step3] FOLDER → {r.status_code} ({r.text.strip()[:50]})")

    # Step 4: CONFIRM
    r = s.post(url, headers=h, timeout=10, data=[
        ("wimToken", wim_token), ("stepListIn", "BASE"), ("stepListIn", "FAX"),
        ("stepListIn", "FOLDER"), ("mode", "MODUSER"), ("step", "CONFIRM"),
    ])
    log(f"[Step4] CONFIRM → {r.status_code}")
    log(f"  응답: {r.text[:200]}")

    if name in r.text:
        log(f"\n>>> 성공! '{name}' 수정 확인됨")
    return True


if __name__ == "__main__":
    REG_NO = "00011"
    NAME = "TEST_RICOH_MOD"
    DISPLAY = "TEST_KEY_MOD"
    FOLDER = r"\\192.168.11.98\TEST_SCAN_MOD"
    FOLDER_USER = "TEST_USER_MOD"
    FOLDER_PASS = "1234"

    args = sys.argv[1:]
    if len(args) >= 1: REG_NO = args[0]
    if len(args) >= 2: NAME = args[1]
    if len(args) >= 3: FOLDER = args[2]
    if len(args) >= 4: FOLDER_USER = args[3]
    if len(args) >= 5: FOLDER_PASS = args[4]

    s = create_session()
    wim_token = login(s)
    if not wim_token:
        sys.exit(1)
    time.sleep(1)

    modify_destination(s, wim_token, REG_NO, NAME, DISPLAY, FOLDER, FOLDER_USER, FOLDER_PASS)
