## 캐논
 192.168.11.227
 
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

리스폰스: 응답 HTML 스크립트 태그 내 변수
var adrsList = { 3:{tp:2,nm:"김민규 ",ad:"minkyu0430@naver.com ",ot:"김민규 "}
,1:{tp:7,nm:"쉽지않아 ",ad:"192.168.11.137 ",ot:"쉽지않아 "}
,2:{tp:7,nm:"지선 ",ad:"192.168.11.122 ",ot:"지선 "}
 };
 * JS 코드로 렌더링

신규 수신인 등록폼
http://192.168.11.227:8000/rps/aprop.cgi?
페이로드: 
AMOD=1&AID=11&AIDX=4&ACLS=2&AFION=1&AdrAction=.%2Falframe.cgi%3F&Dummy=1775020117229&Token=28573491314166998385

수신인 등록 API
Request URL
http://192.168.11.227:8000/rps/anewadrs.cgi
Request Method
POST
페이로드:
AID=11&PageFlag=&AIDX=4&ANAME=TestName&ANAMEONE={버튼 이름}&AREAD={버튼이름}&APNO=0&AAD1={수신지 메일/IP}&ACLS=2&DATADIV=0&AdrAction=.%2Faprop.cgi%3F&AMOD=1&Dummy=1775019719707&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=6988968051507699427

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

수신인 등록 - 파일 경로 설정
Request URL
http://192.168.11.227:8000/rps/aprop.cgi
Request Method
POST
페이로드: AID=11&PageFlag=&AIDX=4&ANAME=TEST&ANAMEONE=TESTBTN&AREAD=TEST&APNO=0&AAD1=&ACLS=7&APRTCL=7&APATH=TESTPATH%2FTEST&AUSER=HOSTNAME_TEST&INPUT_PSWD=0&APWORD=1234&PASSCHK=1&PASSCHK=&AdrAction=.%2Falframe.cgi%3F&AMOD=1&Dummy=1775020402698&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=41042378912463347986

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

