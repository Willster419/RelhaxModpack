@ECHO OFF

echo "Administrative permissions required. Detecting permissions..."
net session >nul 2>&1
if %errorLevel% == 0 (
    echo "Success: Administrative permissions confirmed."
) else (
    echo "Failure: Current permissions inadequate."
    pause
    exit
)
echo "Installing notepad++"
%~dp0notepadPlusPlus.exe /S

echo "copyping required notepad++ settings..."
copy %~dp0config.xml "%APPDATA%\Notepad++\config.xml"

echo "Installing git"
%~dp0gitInstall.exe /SILENT /LOADINF="gitInstall.inf"

echo "Installing tortoiseGit"
%~dp0tortoiseGit.msi /quiet /norestart

echo "Installing Slack"
%~dp0slacksetup.msi /quiet /norestart

echo "filezilla install exe version"
if exist "%programfiles%\FileZilla FTP Client\uninstall.exe" "%programfiles%\FileZilla FTP Client\uninstall.exe" /S
if exist "%programfiles(x86)%\FileZilla FTP Client\uninstall.exe" "%programfiles(x86)%\FileZilla FTP Client\uninstall.exe" /S
%~dp0FileZilla.exe /S

echo "copying filezilla configuration"
copy %~dp0filezilla.xml "%APPDATA%\FileZilla"

echo "Cloneing the repo..."
"C:\Program Files (x86)\Git\cmd\git.exe" "clone" "https://github.com/Willster419/RelhaxModpack.git" "RelhaxModpack"

echo "copying shortcuts"
MD C:\Relhax\Working_on_MOD
copy %~dp0RELHAX.lnk %public%\desktop
copy %~dp0FileZilla.lnk C:\Relhax
copy %~dp0ModpackPatchCheck.lnk C:\Relhax
copy %~dp0ModpackTest.lnk C:\Relhax
copy %~dp0MODXML.lnk C:\Relhax
copy %~dp0RelhaxModpack.exe C:\Relhax
copy %~dp0Remote.exe C:\Relhax
copy %~dp0Settings.lnk C:\Relhax
copy %~dp0Slack.lnk C:\Relhax
copy %~dp0XMLValidation.url C:\Relhax

echo "You now need to run the setup wizard for tortoiseGit"
echo "(programs->tortoiseGit->settings->general->run wizard)"

pause

SHUTDOWN /r /t 120 /c "Shutdown in progress, you have 2 minutes to save your work untill pc restart"