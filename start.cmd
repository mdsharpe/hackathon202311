@echo off
wt ^
-d "%~dp0\" powershell -NoExit -Command dotnet watch --project .\src\Server run

exit
