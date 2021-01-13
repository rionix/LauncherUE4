@echo off

del /Q ..\Manifest_NonUFSFiles_*.txt

del /Q ..\Engine\Extras
rd /S /Q ..\Engine\Extras

ren ..\MedievalTales.exe TheUnexpectedQuest.exe
ren ..\MedievalTales\Binaries\Win32\MedievalTales*.exe TheUnexpectedQuest.dll

LauncherUE4 ParamsForBFG.ini
