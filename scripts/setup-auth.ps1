# Creates the node_modules link required by auth/client TypeScript build.
$AuthClient = Join-Path $PSScriptRoot "..\auth\client"
$ClientModules = Join-Path $PSScriptRoot "..\client\node_modules"
$LinkPath = Join-Path $AuthClient "node_modules"

if (Test-Path $LinkPath) {
    Write-Host "auth/client/node_modules already exists." -ForegroundColor Green
    exit 0
}

if (-not (Test-Path $ClientModules)) {
    Write-Host "Run npm install in client/ first." -ForegroundColor Red
    exit 1
}

cmd /c mklink /J "$LinkPath" "$ClientModules"
Write-Host "Linked auth/client/node_modules -> client/node_modules" -ForegroundColor Green
