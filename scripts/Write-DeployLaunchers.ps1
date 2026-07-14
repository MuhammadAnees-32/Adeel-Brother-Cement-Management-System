param(
    [Parameter(Mandatory = $true)]
    [string]$DeployDir
)

$openVbs = @'
Set shell = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
appDir = fso.GetParentFolderName(WScript.ScriptFullName)
exePath = appDir & "\AdeelBrotherCement.Api.exe"

isRunning = False
Set processes = GetObject("winmgmts:").ExecQuery("SELECT * FROM Win32_Process WHERE Name='AdeelBrotherCement.Api.exe'")
If processes.Count > 0 Then isRunning = True

If Not isRunning Then
    shell.Run """" & exePath & """", 0, False
    WScript.Sleep 3000
End If

shell.Run "http://localhost:5049", 1, False
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

Set-Content -Path (Join-Path $DeployDir "Open App.vbs") -Value $openVbs -Encoding ASCII
Set-Content -Path (Join-Path $DeployDir "Stop App.vbs") -Value $stopVbs -Encoding ASCII

$Readme = @'
ADEEL AND BROTHER CEMENT
========================

OPEN THE APP (NO BLACK SCREEN)
------------------------------
Double-click: Open App.vbs

- Starts the app in the background (hidden)
- Opens your browser to the login page
- Your sales history is saved in data\BusinessData.xlsx

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
