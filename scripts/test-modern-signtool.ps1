# Test signing with the modern signtool from D drive
$signtool = "D:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
$cert = ".\differ-signing-cert.pfx"
$testFile = Join-Path $env:TEMP "test-signing-$(Get-Random).exe"

Write-Host "Creating test file..." -ForegroundColor Cyan
"MZ" | Out-File -FilePath $testFile -Encoding ASCII -NoNewline

Write-Host "Signing with modern signtool..." -ForegroundColor Cyan
Write-Host "Signtool: $signtool" -ForegroundColor Gray
Write-Host ""

& $signtool sign /f $cert /fd SHA256 /v $testFile

Write-Host ""
if ($LASTEXITCODE -eq 0) {
    Write-Host "[SUCCESS] Modern signtool works!" -ForegroundColor Green
} else {
    Write-Host "[FAILED] Exit code: $LASTEXITCODE" -ForegroundColor Red
}

# Cleanup
Remove-Item $testFile -Force -ErrorAction SilentlyContinue
