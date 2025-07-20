#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs AutomationExercise tests and automatically opens the generated HTML report
.DESCRIPTION
    This script runs the test suite and automatically opens the generated HTML report in the default browser.
    It also provides options for different test execution modes.
.PARAMETER TestFilter
    Filter to run specific tests (optional)
.PARAMETER Parallel
    Run tests in parallel (default: true)
.PARAMETER Headless
    Run browser in headless mode (default: false)
.PARAMETER OpenReport
    Automatically open the report after tests complete (default: true)
.PARAMETER Verbosity
    Test output verbosity level (minimal, normal, detailed)
.EXAMPLE
    .\RunTestsWithReport.ps1
    .\RunTestsWithReport.ps1 -TestFilter "SearchProducts" -Headless $true
    .\RunTestsWithReport.ps1 -Parallel $false -Verbosity detailed
#>

param(
    [string]$TestFilter = "",
    [bool]$Parallel = $true,
    [bool]$Headless = $false,
    [bool]$OpenReport = $true,
    [ValidateSet("minimal", "normal", "detailed")]
    [string]$Verbosity = "normal",
    [ValidateSet("chromium", "firefox", "webkit")]
    [string]$Browser = "chromium",
    [int]$Timeout = 30000,
    [int]$SlowMotion = 0
)

# Colors for output
$Green = "`e[32m"
$Red = "`e[31m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Magenta = "`e[35m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

Write-Host "${Cyan}🚀 AutomationExercise Test Runner with Reporting${Reset}" -ForegroundColor Cyan
Write-Host "${Blue}=============================================${Reset}" -ForegroundColor Blue

# Get current timestamp for this test run
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
Write-Host "${Blue}📅 Test Run Started: ${timestamp}${Reset}" -ForegroundColor Blue

# Build the project first
Write-Host "${Yellow}🔨 Building project...${Reset}" -ForegroundColor Yellow
try {
    dotnet build --configuration Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "${Red}❌ Build failed!${Reset}" -ForegroundColor Red
        exit 1
    }
    Write-Host "${Green}✅ Build successful${Reset}" -ForegroundColor Green
}
catch {
    Write-Host "${Red}❌ Build error: $($_.Exception.Message)${Reset}" -ForegroundColor Red
    exit 1
}

# Construct test command
$testCmd = "dotnet test"

# Add configuration
$testCmd += " --configuration Release --no-build"

# Add logger
$testCmd += " --logger `"console;verbosity=$Verbosity`""

# Add TRX logger for additional reporting
$testCmd += " --logger `"trx;LogFileName=TestResults_$timestamp.trx`""

# Add test filter if specified
if ($TestFilter) {
    $testCmd += " --filter `"$TestFilter`""
    Write-Host "${Yellow}🔍 Test Filter: $TestFilter${Reset}" -ForegroundColor Yellow
}

# Add parallel execution
if ($Parallel) {
    $testCmd += " --maxcpucount"
    Write-Host "${Yellow}⚡ Parallel Execution: Enabled${Reset}" -ForegroundColor Yellow
} else {
    Write-Host "${Yellow}🐌 Parallel Execution: Disabled${Reset}" -ForegroundColor Yellow
}

# Set environment variables for test configuration
$env:TEST_BROWSER = $Browser
$env:TEST_HEADLESS = $Headless.ToString()
$env:TEST_TIMEOUT = $Timeout.ToString()
$env:TEST_SLOWMO = $SlowMotion.ToString()

Write-Host "${Yellow}🌐 Browser: $Browser${Reset}" -ForegroundColor Yellow
if ($Headless) {
    Write-Host "${Yellow}👻 Headless Mode: Enabled${Reset}" -ForegroundColor Yellow
} else {
    Write-Host "${Yellow}🖥️ Headless Mode: Disabled${Reset}" -ForegroundColor Yellow
}
Write-Host "${Yellow}⏱️ Timeout: ${Timeout}ms${Reset}" -ForegroundColor Yellow
if ($SlowMotion -gt 0) {
    Write-Host "${Yellow}🐌 Slow Motion: ${SlowMotion}ms${Reset}" -ForegroundColor Yellow
}

Write-Host "${Blue}===============================================${Reset}" -ForegroundColor Blue
Write-Host "${Magenta}🧪 Running Tests...${Reset}" -ForegroundColor Magenta
Write-Host "${Blue}Command: $testCmd${Reset}" -ForegroundColor Blue
Write-Host "${Blue}===============================================${Reset}" -ForegroundColor Blue

# Record start time
$startTime = Get-Date

# Execute tests
try {
    Invoke-Expression $testCmd
    $testExitCode = $LASTEXITCODE
}
catch {
    Write-Host "${Red}❌ Test execution error: $($_.Exception.Message)${Reset}" -ForegroundColor Red
    $testExitCode = 1
}

# Record end time and calculate duration
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "${Blue}===============================================${Reset}" -ForegroundColor Blue
Write-Host "${Magenta}📊 Test Execution Complete${Reset}" -ForegroundColor Magenta
Write-Host "${Blue}⏱️ Duration: $($duration.ToString('mm\:ss\.fff'))${Reset}" -ForegroundColor Blue

# Find the generated HTML report (check project root first, then bin directories)
$reportDir = Join-Path (Get-Location) "TestReports"
if (-not (Test-Path $reportDir)) {
    $reportDir = Join-Path (Get-Location) "bin\Debug\net8.0\TestReports"
}
if (-not (Test-Path $reportDir)) {
    $reportDir = Join-Path (Get-Location) "bin\Release\net8.0\TestReports"
}
if (Test-Path $reportDir) {
    $latestReport = Get-ChildItem -Path $reportDir -Filter "TestReport_*.html" | 
                   Sort-Object CreationTime -Descending | 
                   Select-Object -First 1

    if ($latestReport) {
        Write-Host "${Green}📋 HTML Report Generated: $($latestReport.Name)${Reset}" -ForegroundColor Green
        Write-Host "${Blue}📂 Report Path: $($latestReport.FullName)${Reset}" -ForegroundColor Blue
        
        # Automatically open the report
        if ($OpenReport) {
            Write-Host "${Cyan}🌐 Opening report in default browser...${Reset}" -ForegroundColor Cyan
            try {
                if ($IsWindows -or $env:OS -eq "Windows_NT") {
                    Start-Process $latestReport.FullName
                } elseif ($IsLinux) {
                    Start-Process "xdg-open" -ArgumentList $latestReport.FullName
                } elseif ($IsMacOS) {
                    Start-Process "open" -ArgumentList $latestReport.FullName
                } else {
                    Write-Host "${Yellow}⚠️ Could not determine OS. Please open the report manually.${Reset}" -ForegroundColor Yellow
                }
            }
            catch {
                Write-Host "${Yellow}⚠️ Could not auto-open report: $($_.Exception.Message)${Reset}" -ForegroundColor Yellow
                Write-Host "${Blue}Please open manually: $($latestReport.FullName)${Reset}" -ForegroundColor Blue
            }
        }
    } else {
        Write-Host "${Yellow}⚠️ No HTML report found in TestReports directory${Reset}" -ForegroundColor Yellow
    }
} else {
    Write-Host "${Yellow}⚠️ TestReports directory not found${Reset}" -ForegroundColor Yellow
}

# Check for TRX report
$trxReports = Get-ChildItem -Path "TestResults" -Filter "*.trx" -ErrorAction SilentlyContinue
if ($trxReports) {
    $latestTrx = $trxReports | Sort-Object CreationTime -Descending | Select-Object -First 1
    Write-Host "${Blue}📄 TRX Report: $($latestTrx.FullName)${Reset}" -ForegroundColor Blue
}

# Display final status
Write-Host "${Blue}===============================================${Reset}" -ForegroundColor Blue
if ($testExitCode -eq 0) {
    Write-Host "${Green}🎉 All tests completed successfully!${Reset}" -ForegroundColor Green
} else {
    Write-Host "${Red}❌ Some tests failed. Check the report for details.${Reset}" -ForegroundColor Red
}
Write-Host "${Blue}===============================================${Reset}" -ForegroundColor Blue

# Exit with the same code as the tests
exit $testExitCode 