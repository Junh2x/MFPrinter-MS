Write-Host "========================================" -ForegroundColor Cyan
Write-Host " 원격 테스트 환경 설정" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Python 설치 확인
Write-Host "`n[1/4] Python 확인 중..." -ForegroundColor Yellow
$pythonPath = Get-Command python -ErrorAction SilentlyContinue
if ($pythonPath) {
    $pyVer = python --version 2>&1
    Write-Host "  Python 설치됨: $pyVer" -ForegroundColor Green
} else {
    Write-Host "  Python 미설치 - winget으로 설치 시도..." -ForegroundColor Red
    winget install Python.Python.3.12 --accept-package-agreements --accept-source-agreements
    # PATH 갱신
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
}

# 2. 필요 패키지 설치
Write-Host "`n[2/4] Python 패키지 설치 중..." -ForegroundColor Yellow
pip install requests urllib3 2>&1 | Out-Null
Write-Host "  requests, urllib3 설치 완료" -ForegroundColor Green

# 3. 결과 저장 폴더 생성
Write-Host "`n[3/4] 결과 저장 폴더 생성..." -ForegroundColor Yellow
$resultDir = "$PSScriptRoot\results"
$screenshotDir = "$PSScriptRoot\results\screenshots"
New-Item -ItemType Directory -Force -Path $resultDir | Out-Null
New-Item -ItemType Directory -Force -Path $screenshotDir | Out-Null
Write-Host "  $resultDir 생성 완료" -ForegroundColor Green

# 4. 네트워크 정보 수집
Write-Host "`n[4/4] 로컬 네트워크 정보 수집..." -ForegroundColor Yellow
$netInfo = @()
$netInfo += "=== 원격 PC 네트워크 정보 ==="
$netInfo += "수집 시각: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$netInfo += ""
$netInfo += "--- ipconfig ---"
$netInfo += (ipconfig | Out-String)
$netInfo += "--- 방화벽 상태 ---"
$netInfo += (netsh advfirewall show allprofiles state | Out-String)
$netInfo | Out-File -FilePath "$resultDir\00_network_info.txt" -Encoding UTF8
Write-Host "  네트워크 정보 저장 완료: $resultDir\00_network_info.txt" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " 설정 완료" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
