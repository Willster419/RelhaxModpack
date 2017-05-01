@ECHO OFF

echo Administrative permissions required. Detecting permissions...
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Success: Administrative permissions confirmed.
) else (
    echo Failure: Current permissions inadequate.
    pause
    exit
)
echo "Installing notepad++"
%~dp0notepadPlusPlus.exe /S
echo "Installing git"
%~dp0gitInstall.exe /SILENT /LOADINF="gitInstall.inf"
echo "Installing tortoiseGit"
%~dp0tortoiseGit.msi /quiet /norestart
echo copyping required notepad++ settings...
copy "config.xml" "%APPDATA%\Notepad++\config.xml"
echo "Cloneing the repo..."
"C:\Program Files (x86)\Git\cmd\git.exe" "clone" "https://github.com/Willster419/RelhaxModpack.git" "RelhaxModpack"
echo "You now need to run the setup wizard for tortoiseGit"
echo "(programs->tortoiseGit->settings->general->run wizard)"
pause