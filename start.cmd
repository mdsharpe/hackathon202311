@echo off
wt ^
-d "%~dp0\src\Server" powershell -NoExit -Command dotnet watch run

exit
