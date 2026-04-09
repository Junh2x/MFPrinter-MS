 문서서버 폴더 조회: 추가 확인 필요
 리코 문서서버: 192.168.11.185
동작: list
----------------------------------------
  [Step 1] 초기 접속 중...
  [Step 2] 인증 토큰 추출 중...
  [Step 3] 로그인 중...
  [Step 4] 로그인 성공
  [Step 1] 폴더 목록 조회 중...
  [Step 2] 폴더 2개 발견

폴더 목록 (2개):
  [003] 공유 폴더 (id=0)
  [] TEST_FOLDER (id=3) [잠금]

  * 실제로 공유 폴더는 번호가 없고, TEST_FOLDER가 폴더 번호 003임.
  * 폴더 번호 테이블 첫번째 TR이 빈 값인데, 빈 값 처리 안돼고 한 칸씩 위로 밀려서 인식, 기록되는 문제


## 문서서버 폴더 생성: 성공

PS C:\Users\hyunji\Desktop\TEST\MFPrinter-MS\python-func> python -m ricoh.docserver 192.168.11.185 create 006 MY_TEST_FOLDER
<frozen runpy>:128: RuntimeWarning: 'ricoh.docserver' found in sys.modules after import of package 'ricoh', but prior to execution of 'ricoh.docserver'; this may result in unpredictable behaviour
리코 문서서버: 192.168.11.185
동작: create
----------------------------------------
  [Step 1] 초기 접속 중...
  [Step 2] 인증 토큰 추출 중...
  [Step 3] 로그인 중...
  [Step 4] 로그인 성공
  [Step 1] 폴더 목록 조회 중...
  [Step 2] 폴더 2개 발견
  [Step 1] 생성 폼 진입...
  [Step 2] 비밀번호 페이지...
  [Step 3] 비밀번호 설정 중...
  [Step 4] 속성 확인 중...
  [Step 5] 폴더 생성 확정 중...
  [Step 6] 폴더 생성 완료

결과: SUCCESS - 폴더 생성 완료
  [Step 1] 폴더 목록 조회 중...
  [Step 2] 폴더 3개 발견

생성 후 폴더 목록 (3개):
  [003] 공유 폴더 (id=0)
  [006] TEST_FOLDER (id=3) [잠금]
  [] MY_TEST_FOLDER (id=6)

* 암호 설정도 성공





