$IP = "192.168.11.227"
$BASE = "http://${IP}:8000"
$SLOT = 5

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

Write-Host "=== Canon Register Test (PowerShell) ===" -ForegroundColor Cyan

# 1. Portal
Write-Host "`n[1] Portal" -ForegroundColor Yellow
$r = Invoke-WebRequest -Uri "$BASE/" -WebSession $session -UseBasicParsing
Write-Host "  -> $($r.StatusCode)"

# 2. Address book entry
Write-Host "[2] Nativetop" -ForegroundColor Yellow
$dummy = [long]([datetimeoffset]::UtcNow.ToUnixTimeMilliseconds())
$url2 = "${BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR" + "&" + "Dummy=$dummy"
$r = Invoke-WebRequest -Uri $url2 -WebSession $session -UseBasicParsing
Write-Host "  -> $($r.StatusCode)"

# 3. Address list (Token A)
Write-Host "[3] Asublist" -ForegroundColor Yellow
$dummy = [long]([datetimeoffset]::UtcNow.ToUnixTimeMilliseconds())
$url3 = "${BASE}/rps/asublist.cgi?CorePGTAG=24" + "&" + "AMOD=0" + "&" + "FromTopPage=1" + "&" + "Dummy=$dummy"
$referer3 = "${BASE}/rps/nativetop.cgi?CorePGTAG=PGTAG_ADR_USR"
$r = Invoke-WebRequest -Uri $url3 -WebSession $session -UseBasicParsing -Headers @{Referer=$referer3}
$tokenA = [regex]::Match($r.Content, 'Token=(\d+)').Groups[1].Value
Write-Host "  -> $($r.StatusCode), Token A: $($tokenA.Substring(0,[Math]::Min(20,$tokenA.Length)))..."

# 4. Registration form (email type)
Write-Host "[4] Aprop (email)" -ForegroundColor Yellow
$dummy = [long]([datetimeoffset]::UtcNow.ToUnixTimeMilliseconds())
$url4 = "${BASE}/rps/aprop.cgi?AMOD=1" + "&" + "AID=11" + "&" + "AIDX=$SLOT" + "&" + "ACLS=2" + "&" + "AFION=1" + "&" + "AdrAction=.%2Falframe.cgi%3F" + "&" + "Dummy=$dummy" + "&" + "Token=$tokenA"
$r = Invoke-WebRequest -Uri $url4 -WebSession $session -UseBasicParsing -Headers @{Referer="${BASE}/rps/albody.cgi"}

# Parse SUBMIT_FORM hidden fields
$matches1 = [regex]::Matches($r.Content, 'name="([^"]+)"\s+value="([^"]*)"')
$tokenB1 = ($matches1 | Where-Object { $_.Groups[1].Value -eq "Token" } | Select-Object -First 1).Groups[2].Value
Write-Host "  -> $($r.StatusCode), Token B1: $($tokenB1.Substring(0,[Math]::Min(20,$tokenB1.Length)))..., Fields: $($matches1.Count)"

# 5. Type change (file/SMB) - form_clear simulation
Write-Host "[5] Aprop (file type)" -ForegroundColor Yellow
$formLeave = @("AID","AIDX","AdrAction","ACLS","ANAME","ANAMEONE","APWORD","APNO","AREAD","DATADIV","AFCLS","AFINT","APNOL","AFION")
$parts5 = @()
foreach ($m in $matches1) {
    $n = $m.Groups[1].Value
    $v = $m.Groups[2].Value
    if ($n -eq "ACLS") { $v = "7" }
    elseif ($n -eq "AMOD") { $v = "1" }
    elseif ($n -eq "PageFlag") { $v = "" }
    elseif ($n -eq "Dummy") { $v = [string]([long]([datetimeoffset]::UtcNow.ToUnixTimeMilliseconds())) }
    elseif ($formLeave -contains $n) { }
    else { $v = "" }
    $parts5 += "$n=$([uri]::EscapeDataString($v))"
}
$body5 = $parts5 -join "&"
$r = Invoke-WebRequest -Uri "${BASE}/rps/aprop.cgi?" -Method POST -Body $body5 -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing -Headers @{Referer="${BASE}/rps/aprop.cgi?"; Origin=$BASE}
$matches2 = [regex]::Matches($r.Content, 'name="([^"]+)"\s+value="([^"]*)"')
$tokenB2 = ($matches2 | Where-Object { $_.Groups[1].Value -eq "Token" } | Select-Object -First 1).Groups[2].Value
Write-Host "  -> $($r.StatusCode), Token B2: $($tokenB2.Substring(0,[Math]::Min(20,$tokenB2.Length)))..., Fields: $($matches2.Count)"

# 6. Detail setup (fill SMB fields)
Write-Host "[6] Aprop (detail)" -ForegroundColor Yellow
$parts6 = @()
foreach ($m in $matches2) {
    $n = $m.Groups[1].Value
    $v = $m.Groups[2].Value
    if ($n -eq "ANAME") { $v = "JA_TEST_PS" }
    elseif ($n -eq "ANAMEONE") { $v = "JA_TEST_PS" }
    elseif ($n -eq "AREAD") { $v = "JA_TEST_PS" }
    elseif ($n -eq "AAD1") { $v = "192.168.11.99" }
    elseif ($n -eq "APRTCL") { $v = "7" }
    elseif ($n -eq "APATH") { $v = "scan_test" }
    elseif ($n -eq "AUSER") { $v = "testuser" }
    elseif ($n -eq "APWORD") { $v = "testpass" }
    elseif ($n -eq "Dummy") { $v = [string]([long]([datetimeoffset]::UtcNow.ToUnixTimeMilliseconds())) }
    $parts6 += "$n=$([uri]::EscapeDataString($v))"
}
$body6 = $parts6 -join "&"
$r = Invoke-WebRequest -Uri "${BASE}/rps/aprop.cgi" -Method POST -Body $body6 -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing -Headers @{Referer="${BASE}/rps/aprop.cgi"; Origin=$BASE}
$matches3 = [regex]::Matches($r.Content, 'name="([^"]+)"\s+value="([^"]*)"')
$tokenB3 = ($matches3 | Where-Object { $_.Groups[1].Value -eq "Token" } | Select-Object -First 1).Groups[2].Value
Write-Host "  -> $($r.StatusCode), Token B3: $($tokenB3.Substring(0,[Math]::Min(20,$tokenB3.Length)))..., Fields: $($matches3.Count)"

# 7. Register POST
Write-Host "[7] Register (anewadrs.cgi)" -ForegroundColor Yellow
$parts7 = @()
foreach ($m in $matches3) {
    $n = $m.Groups[1].Value
    $v = $m.Groups[2].Value
    if ($n -eq "AdrAction") { $v = "./aprop.cgi?" }
    elseif ($n -eq "Dummy") { $v = [string]([long]([datetimeoffset]::UtcNow.ToUnixTimeMilliseconds())) }
    elseif ($n -eq "PASSCHK") { $v = "1" }
    elseif ($n -eq "APRTCL") { $v = "7" }
    $parts7 += "$n=$([uri]::EscapeDataString($v))"
}
$body7 = $parts7 -join "&"

Write-Host "  Body: $body7" -ForegroundColor Gray

$r = Invoke-WebRequest -Uri "${BASE}/rps/anewadrs.cgi" -Method POST -Body $body7 -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing -Headers @{Referer="${BASE}/rps/aprop.cgi"; Origin=$BASE}
Write-Host "  -> $($r.StatusCode), Size: $($r.Content.Length)"

if ($r.Content -match "ERR_SUBMIT_FORM") {
    Write-Host ">>> FAIL: ERR_SUBMIT_FORM" -ForegroundColor Red
} else {
    Write-Host ">>> NO ERR - possibly SUCCESS!" -ForegroundColor Green
}

# 8. Check list
Write-Host "`n[8] Final list" -ForegroundColor Yellow
$dummy = [long]([datetimeoffset]::UtcNow.ToUnixTimeMilliseconds())
$url8 = "${BASE}/rps/asublist.cgi?CorePGTAG=24" + "&" + "AMOD=0" + "&" + "FromTopPage=1" + "&" + "Dummy=$dummy"
$r = Invoke-WebRequest -Uri $url8 -WebSession $session -UseBasicParsing
$body8 = "AID=11" + "&" + "FILTER_ID=0" + "&" + "Dummy=$dummy"
$r2 = Invoke-WebRequest -Uri "${BASE}/rps/albody.cgi" -Method POST -Body $body8 -ContentType "application/x-www-form-urlencoded" -WebSession $session -UseBasicParsing
$entries = [regex]::Matches($r2.Content, 'nm:"([^"]+)"')
foreach ($e in $entries) {
    $name = $e.Groups[1].Value.Trim()
    if ($name -like "*JA_TEST*") {
        Write-Host "  $name <<<" -ForegroundColor Green
    } else {
        Write-Host "  $name"
    }
}

# Save
$body7 | Out-File -FilePath ".\remote-tools\results\canon_ps_body.txt" -Encoding utf8
$r.Content | Out-File -FilePath ".\remote-tools\results\canon_ps_response.html" -Encoding utf8
Write-Host "`nSaved to results\"
