"""
리코 문서서버 - 로그인, 폴더 목록 조회, 폴더 생성 (HDD 방식)
사용법:
    from ricoh.docserver import ricoh_login, list_folders, create_folder

    session, token = ricoh_login("192.168.11.185")
    token, folders = list_folders(session, "192.168.11.185", token)
    result = create_folder(session, "192.168.11.185", token, "005", "SCAN_FOLDER", "1234")
"""
import re
import time
import base64
from datetime import datetime
import requests
import urllib3

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"


def _b64(text):
    return base64.b64encode(text.encode()).decode()


def _get_wim_token(html):
    m = re.search(r'name=["\']wimToken["\'][^>]*value=["\'](\d+)', html)
    return m.group(1) if m else None


def _post_headers(base, referer_path=""):
    return {
        "Content-Type": "application/x-www-form-urlencoded",
        "Origin": base,
        "Upgrade-Insecure-Requests": "1",
        "Referer": f"{base}{referer_path}" if referer_path else base,
    }


# ---------------------------------------------------------------------------
# 로그인
# ---------------------------------------------------------------------------
def ricoh_login(ip, userid="admin", password="", callback=None):
    """
    리코 복합기 웹 로그인.

    Args:
        ip:       복합기 IP
        userid:   관리자 ID (기본 "admin")
        password: 비밀번호 (기본 빈 문자열)
        callback: 진행 콜백 fn(step, message)

    Returns:
        tuple: (session, wim_token) 성공 시
               (None, None) 실패 시
    """
    base = f"http://{ip}"

    def _cb(step, msg):
        if callback:
            callback(step, msg)

    s = requests.Session()
    s.verify = False
    s.headers.update({"User-Agent": UA})

    # Step 1: 초기 쿠키
    _cb(1, "초기 접속 중...")
    try:
        s.get(f"{base}/", timeout=10, allow_redirects=True)
        if "cookieOnOffChecker" not in {c.name for c in s.cookies}:
            s.cookies.set("cookieOnOffChecker", "on", domain=ip)
    except requests.RequestException as e:
        _cb(1, f"접속 실패: {e}")
        return None, None

    # Step 2: wimToken 추출
    _cb(2, "인증 토큰 추출 중...")
    try:
        r = s.get(f"{base}/web/guest/ko/websys/webArch/authForm.cgi", timeout=10)
        wim_token = _get_wim_token(r.text)
        if not wim_token:
            _cb(2, "wimToken 추출 실패")
            return None, None
    except requests.RequestException as e:
        _cb(2, f"인증 페이지 실패: {e}")
        return None, None

    # Step 3: 로그인
    _cb(3, "로그인 중...")
    try:
        r = s.post(
            f"{base}/web/guest/ko/websys/webArch/login.cgi",
            data={
                "wimToken": wim_token,
                "userid_work": "",
                "userid": _b64(userid),
                "password_work": "",
                "password": _b64(password),
                "open": "",
            },
            headers={
                "Content-Type": "application/x-www-form-urlencoded",
                "Referer": f"{base}/web/guest/ko/websys/webArch/authForm.cgi",
            },
            timeout=10,
        )
        if "risessionid" not in s.cookies and "wimsesid" not in s.cookies:
            _cb(3, "로그인 실패 - 세션 쿠키 미발급")
            return None, None
    except requests.RequestException as e:
        _cb(3, f"로그인 요청 실패: {e}")
        return None, None

    _cb(4, "로그인 성공")
    return s, wim_token


# ---------------------------------------------------------------------------
# 폴더 목록 조회
# ---------------------------------------------------------------------------
def list_folders(session, ip, wim_token=None, callback=None):
    """
    문서서버 폴더 목록 조회.

    Args:
        session:   ricoh_login()에서 받은 세션
        ip:        복합기 IP
        wim_token: 이전 토큰 (없으면 페이지에서 재추출)
        callback:  진행 콜백 fn(step, message)

    Returns:
        tuple: (wim_token, folders)
            folders = [{"id": str, "name": str, "number": str, "has_password": bool}, ...]
    """
    base = f"http://{ip}"

    def _cb(step, msg):
        if callback:
            callback(step, msg)

    _cb(1, "폴더 목록 조회 중...")
    try:
        r = session.get(
            f"{base}/web/entry/ko/webdocbox/folderListPage.cgi",
            headers={"Referer": f"{base}/web/entry/ko/websys/webArch/topPage.cgi"},
            timeout=10,
        )
    except requests.RequestException as e:
        _cb(1, f"폴더 목록 조회 실패: {e}")
        return wim_token, []

    # wimToken 갱신
    new_token = _get_wim_token(r.text)
    if new_token:
        wim_token = new_token

    # 폴더 파싱
    folders = []
    for m in re.finditer(r'docListPage\.cgi\?selectedFolderId=(\d+)[^>]*>([^<]+)</a>', r.text):
        folders.append({
            "id": m.group(1),
            "name": m.group(2).strip(),
            "number": "",
            "has_password": False,
        })

    # 폴더 번호
    folder_nums = re.findall(r'<td[^>]*class="listData"[^>]*>(\d{3})</td>', r.text)
    for i, f in enumerate(folders):
        if i < len(folder_nums):
            f["number"] = folder_nums[i]

    # 비밀번호 여부
    segments = re.findall(r'selectedFolderId.*?(?:iconKey\.gif|---)', r.text, re.DOTALL)
    for i, seg in enumerate(segments):
        if i < len(folders):
            folders[i]["has_password"] = "iconKey.gif" in seg

    _cb(2, f"폴더 {len(folders)}개 발견")
    return wim_token, folders


# ---------------------------------------------------------------------------
# 폴더 생성
# ---------------------------------------------------------------------------
def create_folder(session, ip, wim_token, folder_id, folder_name, password="", callback=None):
    """
    문서서버 폴더 생성 (5단계 프로세스).

    Args:
        session:     ricoh_login()에서 받은 세션
        ip:          복합기 IP
        wim_token:   현재 유효한 wimToken
        folder_id:   폴더 번호 (예: "004", "005")
        folder_name: 폴더 이름
        password:    폴더 비밀번호 (빈 문자열이면 비밀번호 없음)
        callback:    진행 콜백 fn(step, message)

    Returns:
        dict: {
            "success": bool,
            "ip": str,
            "folder_id": str,
            "folder_name": str,
            "message": str,
        }
    """
    base = f"http://{ip}"
    now = datetime.now()
    hour = now.strftime("%H")
    minute = now.strftime("%M")

    def _cb(step, msg):
        if callback:
            callback(step, msg)

    def _fail(msg):
        return {"success": False, "ip": ip, "folder_id": folder_id,
                "folder_name": folder_name, "message": msg}

    headers = _post_headers(base)

    # Step 1: 폴더 생성 폼 진입
    _cb(1, "생성 폼 진입...")
    try:
        r = session.post(
            f"{base}/web/entry/ko/webdocbox/folderPropPage.cgi",
            data=f"wimToken={wim_token}&mode=CREATE&selectedDocIds=&subReturnDsp=&useInputParam=&useSavedPropParam=false&_hour=&_min=&_hour={hour}&_min={minute}",
            headers={**headers, "Referer": f"{base}/web/entry/ko/webdocbox/folderListPage.cgi"},
            timeout=10,
        )
        if r.status_code != 200:
            return _fail(f"Step1 실패: HTTP {r.status_code}")
    except requests.RequestException as e:
        return _fail(f"Step1 실패: {e}")

    # Step 2: 비밀번호 설정 페이지
    _cb(2, "비밀번호 페이지...")
    try:
        r = session.post(
            f"{base}/web/entry/ko/webdocbox/chPasswordPage.cgi",
            data={
                "wimToken": wim_token,
                "targetFolderId": folder_id,
                "changedFolderName": folder_name,
                "mode": "CREATE",
                "targetDocId": folder_id,
                "selectedFolderId": "",
                "title": folder_name,
                "useSavedPropParam": "true",
                "useInputParam": "false",
                "subReturnDsp": "3",
                "dummy": "",
            },
            headers={**headers, "Referer": f"{base}/web/entry/ko/webdocbox/folderPropPage.cgi"},
            timeout=10,
        )
        if r.status_code != 200:
            return _fail(f"Step2 실패: HTTP {r.status_code}")
    except requests.RequestException as e:
        return _fail(f"Step2 실패: {e}")

    # Step 3: 비밀번호 설정 확정
    _cb(3, "비밀번호 설정 중...")
    pw_b64 = _b64(password) if password else ""
    try:
        r = session.post(
            f"{base}/web/entry/ko/webdocbox/commitChPassword.cgi",
            data={
                "wimToken": wim_token,
                "title": folder_name,
                "creator": "", "dataFormat": "", "allPages": "false",
                "cid": "", "convBW": "", "backUp": "",
                "backUpFormatStr": "", "backUpResoStr": "",
                "targetDocId": folder_id,
                "oldPassword": "undefined",
                "newPassword": pw_b64,
                "confirmation": pw_b64,
                "useInputParam": "false", "useSavedParam": "",
                "subReturnDsp": "3", "mode": "CREATE", "wayTo": "",
                "useSavedPropParam": "true", "selectedFolderId": "",
                "ID": "", "dummy": "",
            },
            headers={**headers, "Referer": f"{base}/web/entry/ko/webdocbox/chPasswordPage.cgi"},
            timeout=10,
        )
        if r.status_code != 200:
            return _fail(f"Step3 실패: HTTP {r.status_code}")
    except requests.RequestException as e:
        return _fail(f"Step3 실패: {e}")

    # Step 4: 속성 페이지
    _cb(4, "속성 확인 중...")
    try:
        r = session.post(
            f"{base}/web/entry/ko/webdocbox/folderPropPage.cgi",
            data={
                "wimToken": wim_token,
                "id": "", "jt": "", "el": "",
                "urlLang": "ko", "urlProfile": "entry",
                "pdfThumbnailURI": "", "thumbnailURI": "", "WidthSize": "",
                "subdocCount": "",
                "targetDocId": folder_id,
                "title": "", "creator": "",
                "useInputParam": "false", "useSavedParam": "",
                "subReturnDsp": "3", "mode": "CREATE", "wayTo": "",
                "selectedFolderId": folder_id,
                "useSavedPropParam": "true",
                "ID": "", "simpleErrorMessage": "", "dummy": "",
            },
            headers={**headers, "Referer": f"{base}/web/entry/ko/webdocbox/commitChPassword.cgi"},
            timeout=10,
        )
        if r.status_code != 200:
            return _fail(f"Step4 실패: HTTP {r.status_code}")
    except requests.RequestException as e:
        return _fail(f"Step4 실패: {e}")

    # Step 5: 최종 생성 확정
    _cb(5, "폴더 생성 확정 중...")
    try:
        r = session.post(
            f"{base}/web/entry/ko/webdocbox/putFolderProp.cgi",
            data={
                "wimToken": wim_token,
                "targetFolderId": folder_id,
                "changedFolderName": folder_name,
                "mode": "CREATE",
                "targetDocId": "",
                "selectedFolderId": "",
                "title": "",
                "useSavedPropParam": "true",
                "useInputParam": "false",
                "subReturnDsp": "3",
                "dummy": "",
            },
            headers={**headers, "Referer": f"{base}/web/entry/ko/webdocbox/folderPropPage.cgi"},
            timeout=10,
        )

        if r.status_code != 200 or "ERR" in r.text or "오류" in r.text:
            return _fail(f"Step5 실패: HTTP {r.status_code}")

    except requests.RequestException as e:
        return _fail(f"Step5 실패: {e}")

    _cb(6, "폴더 생성 완료")
    return {
        "success": True,
        "ip": ip,
        "folder_id": folder_id,
        "folder_name": folder_name,
        "message": "폴더 생성 완료",
    }


# ---------------------------------------------------------------------------
# CLI 테스트
# ---------------------------------------------------------------------------
if __name__ == "__main__":
    import sys

    ip = sys.argv[1] if len(sys.argv) > 1 else "192.168.11.185"
    action = sys.argv[2] if len(sys.argv) > 2 else "list"

    def _print_cb(step, msg):
        print(f"  [Step {step}] {msg}")

    print(f"리코 문서서버: {ip}")
    print(f"동작: {action}")
    print("-" * 40)

    # 로그인
    session, token = ricoh_login(ip, callback=_print_cb)
    if not session:
        print("로그인 실패!")
        sys.exit(1)

    if action == "list":
        # 폴더 목록
        token, folders = list_folders(session, ip, token, callback=_print_cb)
        print(f"\n폴더 목록 ({len(folders)}개):")
        for f in folders:
            pw = " [잠금]" if f["has_password"] else ""
            print(f"  [{f['number']}] {f['name']} (id={f['id']}){pw}")

    elif action == "create":
        folder_id = sys.argv[3] if len(sys.argv) > 3 else "005"
        folder_name = sys.argv[4] if len(sys.argv) > 4 else "TEST_FOLDER"
        password = sys.argv[5] if len(sys.argv) > 5 else ""

        # 생성 전 목록
        token, folders = list_folders(session, ip, token, callback=_print_cb)

        # 폴더 생성
        result = create_folder(session, ip, token, folder_id, folder_name, password, callback=_print_cb)
        print(f"\n결과: {'SUCCESS' if result['success'] else 'FAIL'} - {result['message']}")

        # 생성 후 목록
        if result["success"]:
            token, folders = list_folders(session, ip, token, callback=_print_cb)
            print(f"\n생성 후 폴더 목록 ({len(folders)}개):")
            for f in folders:
                pw = " [잠금]" if f["has_password"] else ""
                print(f"  [{f['number']}] {f['name']} (id={f['id']}){pw}")
    else:
        print(f"알 수 없는 동작: {action}")
        print(f"사용법: python {sys.argv[0]} <IP> list")
        print(f"        python {sys.argv[0]} <IP> create <번호> <이름> [비밀번호]")
