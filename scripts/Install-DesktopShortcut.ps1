# Creates a desktop shortcut to open the app (no black screen).

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$DeployDir = Join-Path $Root "deploy\AdeelBrotherCement"
$OpenVbs = Join-Path $DeployDir "Open App.vbs"

if (-not (Test-Path $OpenVbs)) {
    Write-Host "Deploy folder not found. Run repair-deploy.ps1 first." -ForegroundColor Red
    exit 1
}

$desktop = [Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path $desktop "Adeel Brother Cement.lnk"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = "wscript.exe"
$shortcut.Arguments = "`"$OpenVbs`""
$shortcut.WorkingDirectory = $DeployDir
$shortcut.IconLocation = Join-Path $DeployDir "AdeelBrotherCement.Api.exe,0"
$shortcut.Description = "Open Adeel Brother Cement"
$shortcut.Save()

Write-Host "Desktop shortcut created:" -ForegroundColor Green
Write-Host $shortcutPath
Write-Host ""
Write-Host "Double-click 'Adeel Brother Cement' on your desktop to open the app." -ForegroundColor Green
