# Test which signtool gets selected
$searchPatterns = @(
    "Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
    "Program Files\Windows Kits\10\bin\*\x64\signtool.exe",
    "Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe",
    "Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\*\x64\signtool.exe"
    # NOTE: Explicitly NOT including ClickOnce
)

Write-Host "Searching for signtool across all drives..." -ForegroundColor Cyan
Write-Host ""

$drives = Get-PSDrive -PSProvider FileSystem | Where-Object { $_.Root -match '^[A-Z]:\\$' }

foreach ($drive in $drives) {
    Write-Host "Checking drive $($drive.Name):\" -ForegroundColor Gray
    foreach ($pathPattern in $searchPatterns) {
        $fullPath = Join-Path $drive.Root $pathPattern
        $found = Get-ChildItem -Path $fullPath -File -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
        if ($found) {
            Write-Host "  [FOUND] $($found.FullName)" -ForegroundColor Green
            
            # Check if it's Windows Kits (good) or ClickOnce (bad)
            if ($found.FullName -like "*Windows Kits*") {
                Write-Host "  [OK] This is a Windows Kits version - GOOD!" -ForegroundColor Green
                
                # Extract SDK version
                if ($found.FullName -match '\\(\d+\.\d+\.\d+\.\d+)\\') {
                    Write-Host "  SDK Version: $($matches[1])" -ForegroundColor Cyan
                }
                
                Write-Host ""
                Write-Host "SELECTED: $($found.FullName)" -ForegroundColor Yellow
                exit 0
            }
        }
    }
}

Write-Host ""
Write-Host "[ERROR] No suitable signtool found!" -ForegroundColor Red
