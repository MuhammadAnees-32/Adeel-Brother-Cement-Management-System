param(
    [Parameter(Mandatory = $true)]
    [string]$DeployDir
)

$startPs1 = @'
$ErrorActionPreference = "Stop"
$appDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe = Join-Path $appDir "AdeelBrotherCement.Api.exe"
$url = "http://localhost:5049"

$running = Get-Process -Name "AdeelBrotherCement.Api" -ErrorAction SilentlyContinue
if (-not $running) {
    $env:ASPNETCORE_ENVIRONMENT = "Production"
    Start-Process -FilePath $exe -WorkingDirectory $appDir -WindowStyle Hidden
    $deadline = (Get-Date).AddSeconds(30)
    do {
        Start-Sleep -Milliseconds 500
        try {
            $null = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2
            break
        } catch {
            if ((Get-Date) -gt $deadline) { break }
        }
    } while ($true)
}

Start-Process $url
'@

$openVbs = @'
Set shell = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
appDir = fso.GetParentFolderName(WScript.ScriptFullName)
ps1 = appDir & "\Start-App.ps1"
shell.Run "powershell -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File """ & ps1 & """", 0, False
'@

$stopVbs = @'
Set processes = GetObject("winmgmts:").ExecQuery("SELECT * FROM Win32_Process WHERE Name='AdeelBrotherCement.Api.exe'")
stopped = 0
For Each proc In processes
    proc.Terminate()
    stopped = stopped + 1
Next

If stopped = 0 Then
    MsgBox "Adeel Brother Cement is not running.", 64, "Adeel Brother Cement"
Else
    MsgBox "Adeel Brother Cement stopped.", 64, "Adeel Brother Cement"
End If
'@

Set-Content -Path (Join-Path $DeployDir "Start-App.ps1") -Value $startPs1 -Encoding UTF8
Set-Content -Path (Join-Path $DeployDir "Open App.vbs") -Value $openVbs -Encoding ASCII
Set-Content -Path (Join-Path $DeployDir "Stop App.vbs") -Value $stopVbs -Encoding ASCII

$Readme = @'
ADEEL AND BROTHER CEMENT
========================

OPEN THE APP (NO BLACK SCREEN)
------------------------------
Double-click: Open App.vbs

- Starts the app in the background (hidden)
- Waits until the server is ready
- Opens your browser to the login page
- Your sales history is saved in data\BusinessData.xlsx

IF YOU SEE "FAILED TO FETCH" OR "CANNOT CONNECT"
------------------------------------------------
The app server is not running. Double-click Open App.vbs again.
Do NOT open the browser manually before starting the app.

LOGIN
-----
admin / Admin@123
OR
MuhammadAnees / MAnees@2026!

You must login each time you open the browser.
Closing the browser logs you out.

STOP THE APP
------------
Double-click: Stop App.vbs

BACKUP
------
Copy this file regularly:
data\BusinessData.xlsx

COPY TO ANOTHER PC
------------------
Copy this whole folder. Use Open App.vbs on that PC too.
'@
Set-Content -Path (Join-Path $DeployDir "README.txt") -Value $Readme -Encoding UTF8
