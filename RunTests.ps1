# AutomationExercise.Tests - Test Runner Script
# Provides easy test execution with various configuration options

param(
    [string]$TestCategory = "All",
    [bool]$Headless = $false,
    [int]$SlowMotion = 0,
    [bool]$Logging = $false,
    [ValidateSet("chromium", "firefox", "webkit")]
    [string]$Browser = "chromium",
    [bool]$Parallel = $true,
    [string]$Output = "console",
    [int]$Timeout = 30000
)

Write-Host "=== AutomationExercise.Tests Runner ===" -ForegroundColor Green
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Category: $TestCategory" -ForegroundColor White
Write-Host "  Browser: $Browser" -ForegroundColor White
Write-Host "  Headless: $Headless" -ForegroundColor White
Write-Host "  Timeout: $Timeout ms" -ForegroundColor White
Write-Host "  Slow Motion: $SlowMotion ms" -ForegroundColor White
Write-Host "  Logging: $Logging" -ForegroundColor White
Write-Host "  Parallel: $Parallel" -ForegroundColor White
Write-Host "  Output: $Output" -ForegroundColor White
Write-Host ""

# Build the project first
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Install Playwright browsers if needed
if (!(Test-Path "bin/Release/net8.0/playwright.ps1")) {
    Write-Host "Installing Playwright browsers..." -ForegroundColor Yellow
    dotnet build
    & "bin/Release/net8.0/playwright.ps1" install
}

# Build test command
$testCommand = "dotnet test --no-build --configuration Release"

# Add parallel execution
if ($Parallel) {
    $testCommand += " --parallel"
}

# Add output format
switch ($Output.ToLower()) {
    "trx" {
        $testCommand += " --logger trx --results-directory TestResults"
    }
    "detailed" {
        $testCommand += " --logger `"console;verbosity=detailed`""
    }
    default {
        $testCommand += " --logger console"
    }
}

# Add category filter
if ($TestCategory -ne "All") {
    $testCommand += " --filter Category=$TestCategory"
}

# Set environment variables for test configuration
$env:TEST_BROWSER = $Browser
$env:TEST_HEADLESS = $Headless.ToString()
$env:TEST_TIMEOUT = $Timeout.ToString()
$env:TEST_SLOWMO = $SlowMotion.ToString()
$env:TEST_LOGGING = $Logging.ToString()

# Execute tests
Write-Host "Running tests..." -ForegroundColor Yellow
Write-Host "Command: $testCommand" -ForegroundColor Gray
Write-Host ""

Invoke-Expression $testCommand

# Display results
if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== Tests Completed Successfully ===" -ForegroundColor Green
    
    if ($Output -eq "trx") {
        Write-Host "Test results saved to TestResults/ directory" -ForegroundColor Yellow
    }
    
    if (Test-Path "screenshots") {
        $screenshotCount = (Get-ChildItem "screenshots" -Filter "*.png" | Measure-Object).Count
        if ($screenshotCount -gt 0) {
            Write-Host "Screenshots available in screenshots/ directory ($screenshotCount files)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host ""
    Write-Host "=== Tests Failed ===" -ForegroundColor Red
    Write-Host "Check the output above for details" -ForegroundColor Yellow
    
    if (Test-Path "screenshots") {
        $screenshotCount = (Get-ChildItem "screenshots" -Filter "*.png" | Measure-Object).Count
        if ($screenshotCount -gt 0) {
            Write-Host "Failure screenshots available in screenshots/ directory ($screenshotCount files)" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "=== Test Runner Complete ===" -ForegroundColor Blue 