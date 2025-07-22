@echo off
echo ===========================================
echo Starting SpendSmart Password Change Test
echo ===========================================
echo.

cd /d "c:\Users\LAKSHAN\Desktop\Software project code\SpendSmart_Backend\SpendSmart_Backend"

echo ✅ Building the project...
dotnet build

if %ERRORLEVEL% neq 0 (
    echo ❌ Build failed! Please fix compilation errors.
    pause
    exit /b 1
)

echo ✅ Build successful!
echo.
echo 🚀 Starting the API server...
echo 📍 API will be available at: http://localhost:5110
echo 📄 Swagger UI at: http://localhost:5110/swagger
echo.
echo 🔐 Password Change Endpoint: POST /api/User/change-password
echo 📧 Email Status Endpoint: GET /api/EmailVerification/status/{userId}
echo 🖼️ Profile Picture Endpoint: GET /api/ProfilePicture/url/{userId}
echo.
echo Press Ctrl+C to stop the server
echo ===========================================

dotnet run --urls=http://localhost:5110
