@echo off
wt ^
-d "%~dp0\" powershell -NoExit -Command dotnet watch -p .\src\Server run

exit
