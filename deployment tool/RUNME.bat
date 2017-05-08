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

echo "copying filezilla site manager"
copy %~dp0filezilla.xml "%APPDATA%\FileZilla"

echo "Cloneing the repo..."
"C:\Program Files (x86)\Git\cmd\git.exe" "clone" "https://github.com/Willster419/RelhaxModpack.git" "RelhaxModpack"

echo "copying shortcuts"
MD C:\Relhax\Working_on_MOD
copy %~dp0RELHAX.lnk %public%\desktop
copy %~dp0FileZilla.lnk c:\Relhax
copy %~dp0ModpackPatchCheck.lnk c:\Relhax
copy %~dp0ModpackTest.lnk c:\Relhax
copy %~dp0MODXML.lnk c:\Relhax
copy %~dp0RelhaxModpack.exe c:\Relhax
copy %~dp0Settings.lnk c:\Relhax
copy %~dp0Slack.lnk c:\Relhax
copy %~dp0MediaFire.url c:\Relhax

echo "You now need to run the setup wizard for tortoiseGit"
echo "(programs->tortoiseGit->settings->general->run wizard)"

pause