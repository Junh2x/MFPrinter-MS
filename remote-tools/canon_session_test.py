"""브라우저 세션을 그대로 사용해서 anewadrs.cgi POST 테스트"""
import requests
import urllib3
urllib3.disable_warnings()

IP = "192.168.11.227"
BASE = f"http://{IP}:8000"

s = requests.Session()
s.verify = False
s.cookies.set("sessionid", "43ea476de0f32d3a09758ade3cd9d731", domain=IP)
s.cookies.set("iR", "1678673125", domain=IP)
s.cookies.set("portalLang", "ko", domain=IP)

TOKEN = "18938464501302351469"
SLOT = 8

data = [
    ("AID", "11"), ("PageFlag", ""), ("AIDX", str(SLOT)),
    ("ANAME", "JA_FINAL"), ("ANAMEONE", "JA_FINAL"), ("AREAD", "JA_FINAL"),
    ("APNO", "0"), ("AAD1", "1234"), ("ACLS", "7"), ("APRTCL", "7"),
    ("APATH", "1234"), ("AUSER", "1234"), ("INPUT_PSWD", "0"), ("APWORD", "1234"),
    ("PASSCHK", "1"), ("PASSCHK", "1"),
    ("AdrAction", "./aprop.cgi?"), ("AMOD", "1"),
    ("Dummy", "1775028500000"),
    ("AFCLS", ""), ("AFINT", ""), ("APNOL", ""),
    ("AFION", "1"), ("AUUID", ""), ("Token", TOKEN),
]

headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0",
    "Origin": BASE,
    "Referer": f"{BASE}/rps/aprop.cgi",
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
    "Accept-Encoding": "gzip, deflate",
    "Accept-Language": "ko-KR,ko;q=0.9",
    "Upgrade-Insecure-Requests": "1",
    "Cache-Control": "max-age=0",
}

print(f"Cookie: {dict(s.cookies)}")
print(f"Token: {TOKEN}")
print(f"Slot: {SLOT}")

r = s.post(f"{BASE}/rps/anewadrs.cgi", data=data, headers=headers, timeout=10)
print(f"Status: {r.status_code}, Size: {len(r.content)}")

if "ERR_SUBMIT_FORM" in r.text:
    print(">>> FAIL: ERR_SUBMIT_FORM")
else:
    print(">>> NO ERR!")

# 응답 저장
from pathlib import Path
(Path(__file__).parent / "results" / "canon_session_test_response.html").write_text(r.text, encoding="utf-8")
(Path(__file__).parent / "results" / "canon_session_test_body.txt").write_text(r.request.body, encoding="utf-8")
print("응답 저장됨")
