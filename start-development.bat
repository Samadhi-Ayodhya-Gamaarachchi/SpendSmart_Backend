@echo off
echo Starting SpendSmart Development Environment...
echo.

echo Starting Backend Server...
start "Backend" cmd /k "cd /d C:\Users\LAKSHAN\Desktop\SpendSmart-Backend\SpendSmart_Backend && dotnet run --urls=https://localhost:7211;http://localhost:5110"

timeout /t 3 /nobreak >nul

echo Starting Frontend Server...
start "Frontend" cmd /k "cd /d C:\Users\LAKSHAN\Desktop\SpendSmart-Backend\Spend-Smart-frontend && npm run dev"

echo.
echo Both servers are starting...
echo Backend: https://localhost:7211 (HTTPS) and http://localhost:5110 (HTTP)
echo Frontend: http://localhost:5173
echo.
pause
