param(
    [Parameter(Mandatory=$true)]
    [string]$SolutionFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Running Pre-Release Build & Test" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Clean Solution
Write-Host "[Step 1/3] Cleaning solution..." -ForegroundColor Yellow
dotnet clean $SolutionFile --nologo -v quiet
Write-Host "  [+] Solution cleaned." -ForegroundColor Green
Write-Host ""

# 2. Build Solution
Write-Host "[Step 2/3] Building solution..." -ForegroundColor Yellow
dotnet build $SolutionFile -c Release --nologo -v quiet
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}
Write-Host "  [+] Solution built successfully." -ForegroundColor Green
Write-Host ""

# 3. Run Tests and Collect Coverage
Write-Host "[Step 3/3] Running tests and collecting coverage..." -ForegroundColor Yellow
$projectRoot = Split-Path $SolutionFile -Parent
$testResultsDir = Join-Path $projectRoot "TestResults"
if (Test-Path $testResultsDir) {
    Remove-Item -Path $testResultsDir -Recurse -Force
}

# The --collect:"XPlat Code Coverage" generates a coverage.cobertura.xml file
# inside a randomly named folder within the TestResults directory.
dotnet test $SolutionFile --nologo --collect:"XPlat Code Coverage"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed with exit code $LASTEXITCODE"
}
Write-Host "  [+] All tests passed." -ForegroundColor Green

# Find the latest coverage file
$coverageFile = Get-ChildItem -Path $testResultsDir -Filter "coverage.cobertura.xml" -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $coverageFile) {
    Write-Host "  [!] Could not find coverage.cobertura.xml file." -ForegroundColor Yellow
    exit 0 # Don't fail the build, just warn
}

# Parse the coverage file
[xml]$coverageXml = Get-Content -Path $coverageFile.FullName

$lineRate = [double]$coverageXml.coverage.'line-rate'
$branchRate = [double]$coverageXml.coverage.'branch-rate'

$lineCoverage = "{0:P1}" -f $lineRate
$branchCoverage = "{0:P1}" -f $branchRate

Write-Host "  [+] Code Coverage Summary:" -ForegroundColor Green
Write-Host "      - Line Coverage:   $lineCoverage"
Write-Host "      - Branch Coverage: $branchCoverage"
Write-Host ""
