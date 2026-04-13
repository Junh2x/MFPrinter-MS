## 박스 목록 조회
Request URL
http://192.168.11.227:8000/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox&Dummy=1776050390625
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
Mon, 13 Apr 2026 03:19:13 GMT
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
portalLang=ko; com.canon.meap.service.login.session=79304e67049cc10183128f9f34f2f4b5; sessionid=ad23f96ff8f8f4ec5cd68b3670f55aea; iR=3289868020
host
192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/bpbl.cgi?CorePGTAG=18&BoxKind=FaxBox&Dummy=1776050380546
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: CorePGTAG=16&BoxKind=UserBox&Dummy=1776050390625

응답: HTML 내 박스 목록 존재.
box_read("box_01.gif");
//]]>
</script>
</a></span><span class="BoxNumber"><a href="javascript:box_documents('00')">00</a></span></td><td>김민규test</td><td>1</td></tr>
<tr><td><span class="TypeIcon"><a href="javascript:box_documents('01')">
<script language="JavaScript" type="text/javascript">
//<![CDATA[
box_read("box_00.gif");
//]]>
</script>
</a></span><span class="BoxNumber"><a href="javascript:box_documents('01')">01</a></span></td><td></td><td>0</td></tr>
<tr><td><span class="TypeIcon"><a href="javascript:box_documents('02')">
<script language="JavaScript" type="text/javascript">
//<![CDATA[
box_read("box_00.gif");
//]]>
</script>
</a></span><span class="BoxNumber"><a href="javascript:box_documents('02')">02</a></span></td><td></td><td>0</td></tr>
<tr><td><span class="TypeIcon"><a href="javascript:box_documents('03')">
<script language="JavaScript" type="text/javascript">
//<![CDATA[
box_read("box_00.gif");

## 박스 생성
= 별도 존재하지 않음. 기존 박스에 수정을 통해 박스명, pin 번호 설정 가능.

## 박스 삭제
= 별도 존재하지 않음. 기존 박스에 수정을 통해 박스명, PIN번호 초기화.
Request URL
http://192.168.11.227:8000/rps/bpropset.cgi
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
Mon, 13 Apr 2026 03:27:03 GMT
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
645
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; com.canon.meap.service.login.session=79304e67049cc10183128f9f34f2f4b5; sessionid=ad23f96ff8f8f4ec5cd68b3670f55aea; iR=3289868020
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/bprop.cgi?
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: BOX_No=01&BoxName=&PaswdStat=0&PswdChk=0&Password=&RePassword=&URLAdrStat=false&URLAdrID=0&PrtDrvSave=0&CoreNxAction=.%2Fbprop.cgi&CoreIncPartPg=&Dummy=1776050862272&COMADR_BNo_Reload=&COMADR_CNo_Reload=&COMADR_INo_Reload=&COMADR_PNo_Reload=&COMADR_TargetAdrIDs_Reload=&COMADR_SubAdrStat_Reload=&COMADR_RtnCGI_Reload=&COMADR_RemoteAdrs_Reload=&COMADR_RemoteAdrsStat_Reload=&COMADR_Tpl_Reload=&COMADR_All_Reload=&COMADR_Group_Reload=&COMADR_Mail_Reload=&COMADR_G3Fax_Reload=&COMADR_IFax_Reload=&COMADR_Printer_Reload=&COMADR_File_Reload=&COMADR_DB_Reload=&COMADR_WebDav_Reload=&COMADR_AirFaxFlg_Reload=&URLAdrID_Reload=&Token=35908706882639490879

## 박스 수정
Request URL
http://192.168.11.227:8000/rps/bpropset.cgi
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
Mon, 13 Apr 2026 03:23:31 GMT
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
685
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; com.canon.meap.service.login.session=79304e67049cc10183128f9f34f2f4b5; sessionid=ad23f96ff8f8f4ec5cd68b3670f55aea; iR=3289868020
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/bprop.cgi?
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드:BOX_No=00&BoxName=%EA%B9%80%EB%AF%BC%EA%B7%9Ctest1&PaswdStat=1&PswdChk=1&Password=1234&RePassword=1234&URLAdrStat=false&URLAdrID=0&PrtDrvSave=0&CoreNxAction=.%2Fbprop.cgi&CoreIncPartPg=&Dummy=1776050650594&COMADR_BNo_Reload=&COMADR_CNo_Reload=&COMADR_INo_Reload=&COMADR_PNo_Reload=&COMADR_TargetAdrIDs_Reload=&COMADR_SubAdrStat_Reload=&COMADR_RtnCGI_Reload=&COMADR_RemoteAdrs_Reload=&COMADR_RemoteAdrsStat_Reload=&COMADR_Tpl_Reload=&COMADR_All_Reload=&COMADR_Group_Reload=&COMADR_Mail_Reload=&COMADR_G3Fax_Reload=&COMADR_IFax_Reload=&COMADR_Printer_Reload=&COMADR_File_Reload=&COMADR_DB_Reload=&COMADR_WebDav_Reload=&COMADR_AirFaxFlg_Reload=&URLAdrID_Reload=&Token=37260454751476893091

## 박스 내 파일 목록 조회
Request URL
http://192.168.11.227:8000/rps/bcomdocs.cgi
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
Mon, 13 Apr 2026 03:20:41 GMT
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
43
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; com.canon.meap.service.login.session=79304e67049cc10183128f9f34f2f4b5; sessionid=ad23f96ff8f8f4ec5cd68b3670f55aea; iR=3289868020
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/blogin.cgi
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: BOX_No=00&DocStart=1&DIDS=&Dummy=1776050440

응답: HTML 내 응답 존재 
//<![CDATA[
box_doc_read("doc_rn.gif");
//]]>
</script>
</a>
</td>
<td>
<a href="javascript:doc_pages('3221259797')">
20260413100932
</a>
</td>
<td>
A4
<img height="28" width="32" src="media/pori_p.gif" alt="" />
</td>
<td>
1
</td>
<td>
2026 04/13 10:09:36
</td>
</tr>


## 박스 내 파일 다운로드
Request URL
http://192.168.11.227:8000/rps/image.jpg?BOX_No=00&DocID=3221259797&PageNo=1&Mode=PJPEG&EFLG=true&Dummy=1776050550229
Request Method
GET
Status Code
200 OK
Remote Address
192.168.11.227:8000
Referrer Policy
strict-origin-when-cross-origin
connection
Keep-Alive
content-type
image/jpeg
date
Mon, 13 Apr 2026 03:22:03 GMT
keep-alive
timeout=30
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
portalLang=ko; com.canon.meap.service.login.session=79304e67049cc10183128f9f34f2f4b5; sessionid=ad23f96ff8f8f4ec5cd68b3670f55aea; iR=3289868020
host
192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/image.jpg?BOX_No=00&DocID=3221259797&PageNo=1&Mode=PJPEG&EFLG=false&Dummy=1776050547162
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드:BOX_No=00&DocID=3221259797&PageNo=1&Mode=PJPEG&EFLG=true&Dummy=1776050550229

## 박스 내 파일 삭제


## 박스 내 파일목록 진입(박스 선택, 비밀번호 없는 경우)
Request URL
http://192.168.11.227:8000/rps/blogin.cgi
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
Mon, 13 Apr 2026 03:41:20 GMT
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
53
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; com.canon.meap.service.login.session=79304e67049cc10183128f9f34f2f4b5; sessionid=ad23f96ff8f8f4ec5cd68b3670f55aea; iR=3289868020
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox&FromTopPage=1&Dummy=1776051613754
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: BOX_No=01&BoxKind=UserBox&Dummy=1776051719698&Cookie=

## 박스 내 파일목록 조회 (비밀번호 있는 경우)
Request URL
http://192.168.11.227:8000/rps/blogin.cgi?
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
Mon, 13 Apr 2026 03:42:48 GMT
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
105
content-type
application/x-www-form-urlencoded
cookie
portalLang=ko; com.canon.meap.service.login.session=79304e67049cc10183128f9f34f2f4b5; sessionid=ad23f96ff8f8f4ec5cd68b3670f55aea; iR=3289868020
host
192.168.11.227:8000
origin
http://192.168.11.227:8000
referer
http://192.168.11.227:8000/rps/blogin.cgi
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: BOX_No=00&DocID=&PgStart=&PIDS=&Password=1234&URLDirect=&BoxKind=UserBox&CorePGTAG=16&Dummy=1776051807966
