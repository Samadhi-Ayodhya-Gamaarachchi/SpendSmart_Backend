@echo off
echo Starting SpendSmart Backend Server...
cd /d "c:\Users\LAKSHAN\Desktop\Software project code\SpendSmart_Backend\SpendSmart_Backend"
echo Running from: %CD%
dotnet run --launch-profile https
pause
