@echo off
echo Starting SpendSmart API Server...
echo.
cd /d "c:\Users\LAKSHAN\Desktop\Software project code\SpendSmart_Backend\SpendSmart_Backend"
echo Current directory: %CD%
echo.
echo Starting dotnet run...
dotnet run --urls=http://localhost:5110
pause