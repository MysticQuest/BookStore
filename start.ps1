# BookStore Startup Script
# Starts both API and Client, then opens browser windows

$ErrorActionPreference = "Stop"

Write-Host "Starting BookStore..." -ForegroundColor Cyan
Write-Host ""

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Kill any existing dotnet processes that might be using our ports
Write-Host "Cleaning up any existing processes..." -ForegroundColor Gray
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "BookStore.Api" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "BookStore.Client" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

# Define URLs
$apiUrl = "http://localhost:5029"
$swaggerUrl = "$apiUrl/swagger"
$clientUrl = "http://localhost:5227"

# Start API in background
Write-Host "Starting API server..." -ForegroundColor Yellow
$apiJob = Start-Job -ScriptBlock {
    param($dir, $url)
    Set-Location $dir
    $env:ASPNETCORE_URLS = $url
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run --project src/BookStore.Api --no-launch-profile
} -ArgumentList $scriptDir, $apiUrl

# Start Client in background
Write-Host "Starting Client server..." -ForegroundColor Yellow
$clientJob = Start-Job -ScriptBlock {
    param($dir, $url)
    Set-Location $dir
    $env:ASPNETCORE_URLS = $url
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run --project src/BookStore.Client --no-launch-profile
} -ArgumentList $scriptDir, $clientUrl

# Wait for servers to start
Write-Host "Waiting for servers to start..." -ForegroundColor Gray
Start-Sleep -Seconds 5

# Detect available browser
$chromePath64 = "C:\Program Files\Google\Chrome\Application\chrome.exe"
$chromePath86 = "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
$firefoxPath64 = "C:\Program Files\Mozilla Firefox\firefox.exe"
$firefoxPath86 = "C:\Program Files (x86)\Mozilla Firefox\firefox.exe"
$browserExe = $null
$browserType = $null

if (Test-Path $chromePath64) {
    $browserExe = $chromePath64
    $browserType = "chrome"
    Write-Host "Using Chrome..." -ForegroundColor Gray
} elseif (Test-Path $chromePath86) {
    $browserExe = $chromePath86
    $browserType = "chrome"
    Write-Host "Using Chrome (x86)..." -ForegroundColor Gray
} elseif (Test-Path $firefoxPath64) {
    $browserExe = $firefoxPath64
    $browserType = "firefox"
    Write-Host "Using Firefox..." -ForegroundColor Gray
} elseif (Test-Path $firefoxPath86) {
    $browserExe = $firefoxPath86
    $browserType = "firefox"
    Write-Host "Using Firefox (x86)..." -ForegroundColor Gray
} else {
    Write-Host "Using default browser (auto-close disabled)..." -ForegroundColor Gray
}

# Open browser windows
Write-Host "Opening browser windows..." -ForegroundColor Green
$swaggerProcess = $null
$clientProcess = $null

if ($browserExe -and $browserType -eq "chrome") {
    try {
        $swaggerProcess = Start-Process $browserExe -ArgumentList "--new-window $swaggerUrl" -PassThru -ErrorAction Stop
        Start-Sleep -Milliseconds 500
        $clientProcess = Start-Process $browserExe -ArgumentList "--new-window $clientUrl" -PassThru -ErrorAction Stop
    } catch {
        Write-Host "Chrome launch failed, using default browser..." -ForegroundColor Yellow
        Start-Process $swaggerUrl
        Start-Process $clientUrl
    }
} elseif ($browserExe -and $browserType -eq "firefox") {
    try {
        $swaggerProcess = Start-Process $browserExe -ArgumentList "-new-window $swaggerUrl" -PassThru -ErrorAction Stop
        Start-Sleep -Milliseconds 500
        $clientProcess = Start-Process $browserExe -ArgumentList "-new-window $clientUrl" -PassThru -ErrorAction Stop
    } catch {
        Write-Host "Firefox launch failed, using default browser..." -ForegroundColor Yellow
        Start-Process $swaggerUrl
        Start-Process $clientUrl
    }
} else {
    # Fallback to default browser (can't track process)
    Start-Process $swaggerUrl
    Start-Process $clientUrl
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  BookStore is running!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  API (Swagger): $swaggerUrl" -ForegroundColor White
Write-Host "  Client:        $clientUrl" -ForegroundColor White
Write-Host ""
Write-Host "  Press Ctrl+C to stop all servers" -ForegroundColor Gray
Write-Host "  (Browser tabs need to be closed manually)" -ForegroundColor DarkGray
Write-Host ""

# Function to cleanup jobs and browser windows
function Stop-AllJobs {
    Write-Host ""
    Write-Host "Stopping servers..." -ForegroundColor Yellow
    Stop-Job -Job $apiJob -ErrorAction SilentlyContinue
    Stop-Job -Job $clientJob -ErrorAction SilentlyContinue
    Remove-Job -Job $apiJob -Force -ErrorAction SilentlyContinue
    Remove-Job -Job $clientJob -Force -ErrorAction SilentlyContinue
    
    # Stop any dotnet processes we started
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    
    # Try to close browser windows by process ID
    Write-Host "Closing browser windows..." -ForegroundColor Yellow
    try {
        if ($swaggerProcess) {
            Stop-Process -Id $swaggerProcess.Id -Force -ErrorAction SilentlyContinue
        }
        if ($clientProcess) {
            Stop-Process -Id $clientProcess.Id -Force -ErrorAction SilentlyContinue
        }
    } catch {
        # Browser processes may have already exited or merged
    }
    
    Write-Host "Servers stopped." -ForegroundColor Green
}

# Register cleanup on script exit
Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { Stop-AllJobs } | Out-Null

try {
    # Keep script running and show output from both jobs
    while ($true) {
        # Receive and display output from API job
        $apiOutput = Receive-Job -Job $apiJob -ErrorAction SilentlyContinue
        if ($apiOutput) {
            $apiOutput | ForEach-Object { Write-Host "[API] $_" -ForegroundColor Blue }
        }
        
        # Receive and display output from Client job
        $clientOutput = Receive-Job -Job $clientJob -ErrorAction SilentlyContinue
        if ($clientOutput) {
            $clientOutput | ForEach-Object { Write-Host "[Client] $_" -ForegroundColor Magenta }
        }
        
        # Check if jobs are still running
        if ($apiJob.State -eq 'Failed' -or $clientJob.State -eq 'Failed') {
            Write-Host "A server has failed!" -ForegroundColor Red
            if ($apiJob.State -eq 'Failed') {
                Receive-Job -Job $apiJob -ErrorAction SilentlyContinue
            }
            if ($clientJob.State -eq 'Failed') {
                Receive-Job -Job $clientJob -ErrorAction SilentlyContinue
            }
            break
        }
        
        Start-Sleep -Milliseconds 500
    }
}
finally {
    Stop-AllJobs
}
