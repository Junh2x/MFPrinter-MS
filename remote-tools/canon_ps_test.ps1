$IP = "192.168.11.227"
$BASE = "http://${IP}:8000"
$SLOT = 5

# 세션 유지
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

Write-Host "=== 캐논 수신지 등록 PowerShell 테스트 ===" -ForegroundColor Cyan

# 1. 포털
Write-Host "`n[1] 포털" -ForegroundColor Yellow
$r = Invoke-WebRequest -Uri "$BASE/" -WebSession $session -UseBasicParsing
Write-Host "  -> $($r.StatusCode)"

# 2. 주소록 진입
Write-Host "[2] 주소록 진입" -ForegroundColor Yellow
$dummy = [long](Get-Date -UFormat %s) * 1000
$r = Invoke-WebRequest -Uri "$BASE/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR&Dummy=$dummy" -WebSession $session -UseBasicParsing
Write-Host "  -> $($r.StatusCode)"

# 3. 주소 리스트 (Token A)
Write-Host "[3] 주소 리스트" -ForegroundColor Yellow
$dummy = [long](Get-Date -UFormat %s) * 1000
$r = Invoke-WebRequest -Uri "$BASE/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy=$dummy" -WebSession $session -UseBasicParsing -Headers @{Referer="$BASE/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR"}
$tokenA = [regex]::Match($r.Content, 'Token=(\d+)').Groups[1].Value
Write-Host "  -> $($r.StatusCode), Token A: $($tokenA.Substring(0,20))..."

# 4. 등록 폼 (이메일)
Write-Host "[4] 등록 폼" -ForegroundColor Yellow
$dummy = [long](Get-Date -UFormat %s) * 1000
$r = Invoke-WebRequest -Uri "$BASE/rps/aprop.cgi?AMOD=1&AID=11&AIDX=$SLOT&ACLS=2&AFION=1&AdrAction=.%2Falframe.cgi%3F&Dummy=$dummy&Token=$tokenA" -WebSession $session -UseBasicParsing -Headers @{Referer="$BASE/rps/albody.cgi"}
# 모든 hidden input 파싱
$form1 = [regex]::Matches($r.Content, '<input[^>]+name="([^"]+)"[^>]*value="([^"]*)"')
$tokenB1 = ($form1 | Where-Object { $_.Groups[1].Value -eq "Token" } | Select-Object -First 1).Groups[2].Value
Write-Host "  -> $($r.StatusCode), Token B1: $($tokenB1.Substring(0,20))..., 필드 $($form1.Count)개"

# 5. 타입 변경 (파일) - form_clear 시뮬레이션
Write-Host "[5] 타입 변경 (파일)" -ForegroundColor Yellow
$formLeave = @("AID","AIDX","AdrAction","ACLS","ANAME","ANAMEONE","APWORD","APNO","AREAD","DATADIV","AFCLS","AFINT","APNOL","AFION")
$body5 = ""
foreach ($m in $form1) {
    $name = $m.Groups[1].Value
    $val = $m.Groups[2].Value
    if ($name -eq "ACLS") { $val = "7" }
    elseif ($name -eq "AMOD") { $val = "1" }
    elseif ($name -eq "PageFlag") { $val = "" }
    elseif ($name -eq "Dummy") { $val = [string]([long](Get-Date -UFormat %s) * 1000) }
    elseif ($formLeave -contains $name) { } # 유지
    else { $val = "" } # form_clear
    if ($body5) { $body5 += "&" }
    $body5 += "$name=$([uri]::EscapeDataString($val))"
}
$r = Invoke-WebRequest -Uri "$BASE/rps/aprop.cgi?" -Method POST -Body $body5 -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing -Headers @{Referer="$BASE/rps/aprop.cgi?"; Origin=$BASE}
$form2 = [regex]::Matches($r.Content, '<input[^>]+name="([^"]+)"[^>]*value="([^"]*)"')
$tokenB2 = ($form2 | Where-Object { $_.Groups[1].Value -eq "Token" } | Select-Object -First 1).Groups[2].Value
Write-Host "  -> $($r.StatusCode), Token B2: $($tokenB2.Substring(0,20))..., 필드 $($form2.Count)개"

# 6. 상세 설정
Write-Host "[6] 상세 설정" -ForegroundColor Yellow
$body6 = ""
foreach ($m in $form2) {
    $name = $m.Groups[1].Value
    $val = $m.Groups[2].Value
    if ($name -eq "ANAME") { $val = "JA_TEST_PS" }
    elseif ($name -eq "ANAMEONE") { $val = "JA_TEST_PS" }
    elseif ($name -eq "AREAD") { $val = "JA_TEST_PS" }
    elseif ($name -eq "AAD1") { $val = "192.168.11.99" }
    elseif ($name -eq "APRTCL") { $val = "7" }
    elseif ($name -eq "APATH") { $val = "scan_test" }
    elseif ($name -eq "AUSER") { $val = "testuser" }
    elseif ($name -eq "APWORD") { $val = "testpass" }
    elseif ($name -eq "Dummy") { $val = [string]([long](Get-Date -UFormat %s) * 1000) }
    if ($body6) { $body6 += "&" }
    $body6 += "$name=$([uri]::EscapeDataString($val))"
}
$r = Invoke-WebRequest -Uri "$BASE/rps/aprop.cgi" -Method POST -Body $body6 -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing -Headers @{Referer="$BASE/rps/aprop.cgi"; Origin=$BASE}
$form3 = [regex]::Matches($r.Content, '<input[^>]+name="([^"]+)"[^>]*value="([^"]*)"')
$tokenB3 = ($form3 | Where-Object { $_.Groups[1].Value -eq "Token" } | Select-Object -First 1).Groups[2].Value
Write-Host "  -> $($r.StatusCode), Token B3: $($tokenB3.Substring(0,20))..., 필드 $($form3.Count)개"

# 7. 등록 POST
Write-Host "[7] 등록 POST" -ForegroundColor Yellow
$body7 = ""
foreach ($m in $form3) {
    $name = $m.Groups[1].Value
    $val = $m.Groups[2].Value
    if ($name -eq "AdrAction") { $val = "./aprop.cgi?" }
    elseif ($name -eq "Dummy") { $val = [string]([long](Get-Date -UFormat %s) * 1000) }
    elseif ($name -eq "PASSCHK") { $val = "1" }
    elseif ($name -eq "APRTCL") { $val = "7" }
    if ($body7) { $body7 += "&" }
    $body7 += "$name=$([uri]::EscapeDataString($val))"
}

Write-Host "  Body: $body7" -ForegroundColor Gray

$r = Invoke-WebRequest -Uri "$BASE/rps/anewadrs.cgi" -Method POST -Body $body7 -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing -Headers @{Referer="$BASE/rps/aprop.cgi"; Origin=$BASE}
Write-Host "  -> $($r.StatusCode), Size: $($r.Content.Length)"

if ($r.Content -match "ERR_SUBMIT_FORM") {
    Write-Host ">>> 등록 실패: ERR_SUBMIT_FORM" -ForegroundColor Red
} else {
    Write-Host ">>> ERR 없음 - 성공 가능성!" -ForegroundColor Green
}

# 8. 목록 확인
Write-Host "`n[8] 최종 목록" -ForegroundColor Yellow
$dummy = [long](Get-Date -UFormat %s) * 1000
$r = Invoke-WebRequest -Uri "$BASE/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy=$dummy" -WebSession $session -UseBasicParsing
$r2 = Invoke-WebRequest -Uri "$BASE/rps/albody.cgi" -Method POST -Body "AID=11&FILTER_ID=0&Dummy=$dummy" -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing
$entries = [regex]::Matches($r2.Content, 'nm:"([^"]+)"')
foreach ($e in $entries) {
    $name = $e.Groups[1].Value.Trim()
    $marker = if ($name -like "*JA_TEST*") { " <<<" } else { "" }
    Write-Host "  $name$marker"
}

# 결과 저장
$body7 | Out-File -FilePath "remote-tools\results\canon_ps_body.txt" -Encoding utf8
$r.Content | Out-File -FilePath "remote-tools\results\canon_ps_response.html" -Encoding utf8
Write-Host "`n결과 저장됨: results\canon_ps_body.txt, canon_ps_response.html"
