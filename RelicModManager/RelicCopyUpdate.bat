@ECHO OFF
ECHO Updating Application...
ping 127.0.0.1 -n 3 > nul
del  /Q RelhaxModpack.exe 2> nul
copy /Y RelhaxModpack_update.exe RelhaxModpack.exe 2> nul
del /Q RelicModManager_update.exe 2> nul
del /Q RelhaxModpack_update.exe 2> nul
del /Q RelicModManager.exe 2> nul
ECHO Starting Application...
start "" "RelhaxModpack.exe" 2> nul
