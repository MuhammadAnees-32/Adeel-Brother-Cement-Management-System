# Repairs deploy folder without losing data. Stops the app, republishes, keeps wwwroot + data.

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$DeployDir = Join-Path $Root "deploy\AdeelBrotherCement"
$StagingDir = Join-Path $Root "deploy\AdeelBrotherCement_staging"
$ApiProject = Join-Path $Root "src\AdeelBrotherCement.Api\AdeelBrotherCement.Api.csproj"
$ClientDir = Join-Path $Root "client"

function Stop-RunningApp {
    Get-Process -Name "AdeelBrotherCement.Api" -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Stopping app (PID $($_.Id))..." -ForegroundColor Yellow
        Stop-Process -Id $_.Id -Force
    }
    Start-Sleep -Seconds 2
}

Write-Host "Stopping running app..." -ForegroundColor Cyan
Stop-RunningApp

Write-Host "Building frontend..." -ForegroundColor Cyan
Push-Location $ClientDir
npm run build
Pop-Location

Write-Host "Publishing API to staging..." -ForegroundColor Cyan
if (Test-Path $StagingDir) { Remove-Item $StagingDir -Recurse -Force }
dotnet publish $ApiProject -c Release -r win-x64 --self-contained true -o $StagingDir /p:PublishSingleFile=false

Write-Host "Updating deploy folder..." -ForegroundColor Cyan
$DataBackup = Join-Path $env:TEMP "AdeelBrotherCement-data-backup.xlsx"
$DeployData = Join-Path $DeployDir "data\BusinessData.xlsx"
if (Test-Path $DeployData) {
    Copy-Item $DeployData $DataBackup -Force
}

New-Item -ItemType Directory -Path $DeployDir -Force | Out-Null
Get-ChildItem $StagingDir | Copy-Item -Destination $DeployDir -Recurse -Force

$WwwRoot = Join-Path $DeployDir "wwwroot"
if (Test-Path $WwwRoot) { Remove-Item $WwwRoot -Recurse -Force }
New-Item -ItemType Directory -Path $WwwRoot -Force | Out-Null
Copy-Item (Join-Path $ClientDir "dist\*") $WwwRoot -Recurse -Force

$buildVersion = Get-Date -Format "yyyyMMddHHmmss"
Set-Content -Path (Join-Path $WwwRoot "app-version.json") -Value "{ `"version`": `"$buildVersion`" }" -Encoding UTF8
Write-Host "App version: $buildVersion" -ForegroundColor Green

$DataDir = Join-Path $DeployDir "data"
New-Item -ItemType Directory -Path $DataDir -Force | Out-Null
if (Test-Path $DataBackup) {
    Copy-Item $DataBackup (Join-Path $DataDir "BusinessData.xlsx") -Force
    Remove-Item $DataBackup -Force -ErrorAction SilentlyContinue
} elseif (-not (Test-Path (Join-Path $DataDir "BusinessData.xlsx"))) {
    $RootData = Join-Path $Root "data\BusinessData.xlsx"
    if (Test-Path $RootData) {
        Copy-Item $RootData (Join-Path $DataDir "BusinessData.xlsx") -Force
    }
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

Remove-Item $StagingDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Repair complete!" -ForegroundColor Green
Write-Host "Double-click Open App.vbs in deploy\AdeelBrotherCement" -ForegroundColor Green
