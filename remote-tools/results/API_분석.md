## 캐논
 192.168.11.227
 
- 진입 step별 토큰 발급됨
토큰 A > 토큰 B > ...
- HTTP 검증 로직 있음
- 주소 리스트 조회 성공, 등록 실패(토큰 B 안됨)
- 캐논 http 서버측에서 보안 로직에 의해 막히는 것으로 확인



http://192.168.11.227:8000/
페이로드 없음

수신/저장 파일 이용: 
http://192.168.11.227:8000/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox&FromTopPage=1&Dummy=1775018938239

스크립트 실행 후 리디렉션됨.

박스 선택:
http://192.168.11.227:8000/rps/bcomdocs.cgi

주소록:
http://192.168.11.227:8000/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy=1775019273549
페이로드: RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy=1775019273549

주소 리스트:
http://192.168.11.227:8000/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy=1775019015477

주소 리스트 조회
http://192.168.11.227:8000/rps/albody.cgi
페이로드: AID=1&FILTER_ID=0&Dummy=1775019074136
쿠키: portalLang=ko; sessionid=43ea476de0f32d3a09758ade3cd9d731; iR=3489871917
성공 요청:
Request URL
http://192.168.11.227:8000/rps/albody.cgi
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.227:8000
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache,no-store,max-age=0
connection
Keep-Alive
content-type
text/html;charset=UTF-8
date
Wed, 01 Apr 2026 06:22:35 GMT
expires
Thu, 01 Jan 1970 00:00:00 GMT
keep-alive
timeout=30
pragma
no-cache
server
CANON HTTP Server
transfer-encoding
chunked
x-content-type-options
nosniff
x-frame-options
DENY
x-xss-protection
1; mode=block
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
cache-control
max-age=0
connection
keep-alive
content-length
38
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; sessionid=43ea476de0f32d3a09758ade3cd9d731; iR=3489871917
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/alframe.cgi?
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36


리스폰스: 응답 HTML 스크립트 태그 내 변수
var adrsList = { 3:{tp:2,nm:"김민규 ",ad:"minkyu0430@naver.com ",ot:"김민규 "}
,1:{tp:7,nm:"쉽지않아 ",ad:"192.168.11.137 ",ot:"쉽지않아 "}
,2:{tp:7,nm:"지선 ",ad:"192.168.11.122 ",ot:"지선 "}
 };
 * JS 코드로 렌더링

신규 수신인 등록폼
http://192.168.11.227:8000/rps/aprop.cgi?
페이로드: 
AMOD=1&AID=11&AIDX=4&ACLS=2&AFION=1&AdrAction=.%2Falframe.cgi%3F&Dummy=17750201
성공요청:
Request URL
http://192.168.11.227:8000/rps/aprop.cgi?
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.227:8000
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache,no-store,max-age=0

connection
Keep-Alive
content-type
text/html;charset=UTF-8
date
Wed, 01 Apr 2026 06:23:23 GMT
expires
Thu, 01 Jan 1970 00:00:00 GMT
keep-alive
timeout=30
pragma
no-cache
server
CANON HTTP Server
transfer-encoding
chunked
x-content-type-options
nosniff
x-frame-options
DENY
x-xss-protection
1; mode=block
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
cache-control
max-age=0
connection
keep-alive
content-length
109
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; sessionid=43ea476de0f32d3a09758ade3cd9d731; iR=3489871917
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/albody.cgi
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36

수신인 등록 API
Request URL
http://192.168.11.227:8000/rps/anewadrs.cgi
Request Method
POST
페이로드:
AID=11&PageFlag=&AIDX=4&ANAME=TestName&ANAMEONE={버튼 이름}&AREAD={버튼이름}&APNO=0&AAD1={수신지 메일/IP}&ACLS=2&DATADIV=0&AdrAction=.%2Faprop.cgi%3F&AMOD=1&Dummy=1775019719707&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=6988968051507699427

성공요청:
Request URL
http://192.168.11.227:8000/rps/anewadrs.cgi
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.227:8000
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache,no-store,max-age=0
connection
Keep-Alive
content-type
text/html;charset=UTF-8
date
Wed, 01 Apr 2026 06:27:42 GMT
expires
Thu, 01 Jan 1970 00:00:00 GMT
keep-alive
timeout=30
pragma
no-cache
server
CANON HTTP Server
transfer-encoding
chunked
x-content-type-options
nosniff
x-frame-options
DENY
x-xss-protection
1; mode=block
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
cache-control
max-age=0
connection
keep-alive
content-length
322
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; sessionid=43ea476de0f32d3a09758ade3cd9d731; iR=3489871917
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/aprop.cgi
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML,
페이로드:AID=11&PageFlag=&AIDX=4&ANAME=NAME_TEST&ANAMEONE=BTNNAME_TEST&AREAD=NAME_TEST&APNO=0&AAD1=HOSTNAME_TEST&ACLS=7&APRTCL=7&APATH=FOLDERPATH_TEST&AUSER=USERNAME_TEST&INPUT_PSWD=0&APWORD=1234&PASSCHK=1&PASSCHK=1&AdrAction=.%2Faprop.cgi%3F&AMOD=1&Dummy=1775024900660&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=3514409393787254048


* 토큰 필요

수신인 삭제
Request URL
http://192.168.11.227:8000/rps/adelete.cgi?
Request Method
POST
페이로드: AMOD=1&AID=11&AIDX=4&ACLS=2&AFION=1&AdrAction=.%2Falframe.cgi%3F&Dummy=1775019821827&Token=15591628172147294623

수신인 수정
Request URL
Request URL
http://192.168.11.227:8000/rps/amodadrs.cgi
Request Method
POST
페이로드: AID=11&PageFlag=&AIDX=3&ANAME=%EA%B9%80%EB%AF%BC%EA%B7%9C2&AREAD=%EA%B9%80%EB%AF%BC%EA%B7%9C2&ANAMEONE=%EA%B9%80%EB%AF%BC%EA%B7%9C2&APNO=0&AAD1=minkyu0430%40naver.com2&ACLS=2&DATADIV=0&AMOD=2&AdrAction=.%2Falframe.cgi%3F&Dummy=1775019996997&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=5956002592321590259

수신인 등록 - 파일
Request URL
http://192.168.11.227:8000/rps/anewadrs.cgi
Request Method
POST
페이로드:
AID=11&PageFlag=&AIDX=4&ANAME={TEST}&ANAMEONE={TESTBTN}&AREAD={TEST}&APNO=0&AAD1={HOSTNAME}&ACLS=7&APRTCL=7&APATH={TESTPATH%2FTEST}&AUSER={HOSTNAME_TEST}&INPUT_PSWD=0&APWORD={1234}&PASSCHK=1&PASSCHK=1&AdrAction=.%2Faprop.cgi%3F&AMOD=1&Dummy=1775020490407&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token={8439098852789897010}
* CSRF 토큰?

수신인 등록 - 파일 경로 설정 폼
Request URL
http://192.168.11.227:8000/rps/aprop.cgi
Request Method
POST
페이로드: AID=11&PageFlag=&AIDX=4&ANAME=TEST&ANAMEONE=TESTBTN&AREAD=TEST&APNO=0&AAD1=&ACLS=7&APRTCL=7&APATH=TESTPATH%2FTEST&AUSER=HOSTNAME_TEST&INPUT_PSWD=0&APWORD=1234&PASSCHK=1&PASSCHK=&AdrAction=.%2Falframe.cgi%3F&AMOD=1&Dummy=1775020402698&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=41042378912463347986

성공요청:
Request URL
http://192.168.11.227:8000/rps/aprop.cgi?
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.227:8000
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache,no-store,max-age=0
connection
Keep-Alive
content-type
text/html;charset=UTF-8
date
Wed, 01 Apr 2026 06:23:54 GMT
expires
Thu, 01 Jan 1970 00:00:00 GMT
keep-alive
timeout=30
pragma
no-cache
server
CANON HTTP Server
transfer-encoding
chunked
x-content-type-options
nosniff
x-frame-options
DENY
x-xss-protection
1; mode=block
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
cache-control
max-age=0
connection
keep-alive
content-length
175
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; sessionid=43ea476de0f32d3a09758ade3cd9d731; iR=3489871917
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/aprop.cgi?
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0

수신지 추가 - 파일 - 호스트 설정
Request URL
http://192.168.11.227:8000/rps/aprop.cgi
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.227:8000
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache,no-store,max-age=0
connection
Keep-Alive
content-type
text/html;charset=UTF-8
date
Wed, 01 Apr 2026 06:26:19 GMT
expires
Thu, 01 Jan 1970 00:00:00 GMT
keep-alive
timeout=30
pragma
no-cache
server
CANON HTTP Server
transfer-encoding
chunked
x-content-type-options
nosniff
x-frame-options
DENY
x-xss-protection
1; mode=block
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
cache-control
max-age=0
connection
keep-alive
content-length
323
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; sessionid=43ea476de0f32d3a09758ade3cd9d731; iR=3489871917
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/aprop.cgi
upgrade-insecure-requests
1
페이로드: AID=11&PageFlag=&AIDX=4&ANAME=NAME_TEST&ANAMEONE=BTNNAME_TEST&AREAD=NAME_TEST&APNO=0&AAD1=HOSTNAME_TEST&ACLS=7&APRTCL=7&APATH=FOLDERPATH_TEST&AUSER=USERNAME_TEST&INPUT_PSWD=0&APWORD=1234&PASSCHK=1&PASSCHK=&AdrAction=.%2Falframe.cgi%3F&AMOD=1&Dummy=1775024818010&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=2108494624937473164

수신인 상세정보 테스트 데이터:
004
명칭	TEST
종류	파일
원터치 버튼 명칭	TESTBTN
프로토콜	Windows (SMB)
호스트명	HOSTNAME
폴더 경로	TESTPATH/TEST
사용자명	HOSTNAME_TEST
송신전에 확인	해제

박스 리스트:
Request URL
http://192.168.11.227:8000/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox&Dummy=1775021704332
Request Method
GET
페이로드: CorePGTAG=16&BoxKind=UserBox&Dummy=1775021704332

박스 정보 조회:
Request URL
http://192.168.11.227:8000/rps/blogin.cgi
Request Method
POST
페이로드: BOX_No=00&BoxKind=UserBox&Dummy=1775021746449&Cookie=



성공 요청
Request URL
http://192.168.11.227:8000/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy=1775023968866
Request Method
GET
Status Code
200 OK
Remote Address
192.168.11.227:8000
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache,no-store,max-age=0
connection
Keep-Alive
content-type
text/html;charset=UTF-8
date
Wed, 01 Apr 2026 06:13:31 GMT
expires
Thu, 01 Jan 1970 00:00:00 GMT
keep-alive
timeout=30
pragma
no-cache
server
CANON HTTP Server
transfer-encoding
chunked
x-content-type-options
nosniff
x-frame-options
DENY
x-xss-protection
1; mode=block
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
cache-control
max-age=0
connection
keep-alive
cookie
portalLang=ko; sessionid=43ea476de0f32d3a09758ade3cd9d731; iR=3489871917
host
192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy=1775023929866
upgrade-insecure-requests
1

페이로드: CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy=1775023968866