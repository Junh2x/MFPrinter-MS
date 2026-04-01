param(
    [int]$Interval = 3,
    [string]$Prefix = "capture"
)

$outputDir = "$PSScriptRoot\results\screenshots"
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " 스크린샷 자동 캡처 (${Interval}초 간격)" -ForegroundColor Cyan
Write-Host " 저장 위치: $outputDir" -ForegroundColor Cyan
Write-Host " 종료: Ctrl+C" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$count = 0
try {
    while ($true) {
        $count++
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $filename = "$outputDir\${Prefix}_${timestamp}_${count}.png"

        # 전체 화면 캡처
        $screen = [System.Windows.Forms.Screen]::PrimaryScreen
        $bitmap = New-Object System.Drawing.Bitmap($screen.Bounds.Width, $screen.Bounds.Height)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.CopyFromScreen($screen.Bounds.Location, [System.Drawing.Point]::Empty, $screen.Bounds.Size)
        $bitmap.Save($filename, [System.Drawing.Imaging.ImageFormat]::Png)

        $graphics.Dispose()
        $bitmap.Dispose()

        Write-Host "[$timestamp] 캡처 #$count 저장: $(Split-Path $filename -Leaf)" -ForegroundColor Green
        Start-Sleep -Seconds $Interval
    }
}
catch {
    Write-Host "`n캡처 종료. 총 ${count}장 저장됨." -ForegroundColor Yellow
}
