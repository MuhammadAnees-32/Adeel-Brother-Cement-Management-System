# Packages the app into a single deploy folder.
# Output: deploy/AdeelBrotherCement/

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$DeployDir = Join-Path $Root "deploy\AdeelBrotherCement"
$ApiProject = Join-Path $Root "src\AdeelBrotherCement.Api\AdeelBrotherCement.Api.csproj"
$ClientDir = Join-Path $Root "client"
$DeployDataFile = Join-Path $DeployDir "data\BusinessData.xlsx"
$RootDataFile = Join-Path $Root "data\BusinessData.xlsx"
$DataBackup = Join-Path $env:TEMP "AdeelBrotherCement-BusinessData-backup.xlsx"

function Stop-RunningApp {
    $names = @("AdeelBrotherCement.Api", "AdeelBrotherCement.Api.exe")
    foreach ($name in $names) {
        Get-Process -Name $name -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "Stopping running app (PID $($_.Id))..." -ForegroundColor Yellow
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }
    }
    Start-Sleep -Seconds 2
}

function Remove-DeployFolder {
    if (-not (Test-Path $DeployDir)) { return }

    for ($i = 1; $i -le 3; $i++) {
        try {
            Remove-Item $DeployDir -Recurse -Force -ErrorAction Stop
            return
        } catch {
            if ($i -eq 3) { throw }
            Write-Host "Deploy folder is locked. Retrying in 2 seconds... ($i/3)" -ForegroundColor Yellow
            Stop-RunningApp
            Start-Sleep -Seconds 2
        }
    }
}

Write-Host "Checking for running app..." -ForegroundColor Cyan
Stop-RunningApp

if (Test-Path $DeployDataFile) {
    Copy-Item $DeployDataFile $DataBackup -Force
    Write-Host "Backed up deploy data file." -ForegroundColor Green
}

Write-Host "Building frontend..." -ForegroundColor Cyan
Push-Location $ClientDir
npm run build
Pop-Location

Write-Host "Publishing API..." -ForegroundColor Cyan
try {
    Remove-DeployFolder
} catch {
    Write-Host ""
    Write-Host "Could not rebuild deploy folder because files are in use." -ForegroundColor Red
    Write-Host "1. Close Start.bat / the black command window" -ForegroundColor Yellow
    Write-Host "2. Close any AdeelBrotherCement.Api.exe from Task Manager" -ForegroundColor Yellow
    Write-Host "3. Run publish.ps1 again" -ForegroundColor Yellow
    exit 1
}

dotnet publish $ApiProject -c Release -r win-x64 --self-contained true -o $DeployDir /p:PublishSingleFile=false

Write-Host "Copying frontend into wwwroot..." -ForegroundColor Cyan
$WwwRoot = Join-Path $DeployDir "wwwroot"
New-Item -ItemType Directory -Path $WwwRoot -Force | Out-Null
Copy-Item (Join-Path $ClientDir "dist\*") $WwwRoot -Recurse -Force

Write-Host "Creating data folder..." -ForegroundColor Cyan
$DataDir = Join-Path $DeployDir "data"
New-Item -ItemType Directory -Path $DataDir -Force | Out-Null

if (Test-Path $DataBackup) {
    Copy-Item $DataBackup (Join-Path $DataDir "BusinessData.xlsx") -Force
    Remove-Item $DataBackup -Force -ErrorAction SilentlyContinue
    Write-Host "Restored deploy BusinessData.xlsx" -ForegroundColor Green
} elseif (Test-Path $RootDataFile) {
    Copy-Item $RootDataFile (Join-Path $DataDir "BusinessData.xlsx") -Force
    Write-Host "Copied data\BusinessData.xlsx from project root" -ForegroundColor Green
} else {
    Write-Host "No existing data file - a new workbook will be created on first run." -ForegroundColor Yellow
}

$StartBat = '@echo off
title Adeel Brother Cement
cd /d "%~dp0"
echo Starting Adeel Brother Cement...
echo Open http://localhost:5049 in your browser
echo.
AdeelBrotherCement.Api.exe
pause'
Set-Content -Path (Join-Path $DeployDir "Start.bat") -Value $StartBat -Encoding ASCII

& (Join-Path $PSScriptRoot "Write-DeployLaunchers.ps1") -DeployDir $DeployDir

Write-Host ""
Write-Host "Done! Deploy folder ready at:" -ForegroundColor Green
Write-Host $DeployDir
Write-Host ""
Write-Host "To open app: double-click Open App.vbs (no black screen)" -ForegroundColor Green
