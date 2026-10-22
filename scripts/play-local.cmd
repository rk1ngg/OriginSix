@echo off
pushd "%~dp0..\client\out\build\windows-x64\src\Release" || exit /b 1
Main.exe connect /u127.127.127.127 /p44406
popd
