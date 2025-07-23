Write-Host "Starting SpendSmart Backend Server..."
Write-Host "Current directory: $(Get-Location)"

# Start the backend
Start-Process -FilePath "dotnet" -ArgumentList "bin\Debug\net8.0\SpendSmart_Backend.dll", "--urls", "https://localhost:7211;http://localhost:5110" -WorkingDirectory "$(Get-Location)" -WindowStyle Normal

Write-Host "Backend started! Check the new window for status."
Write-Host "API should be available at: https://localhost:7211"
Write-Host "Swagger UI: https://localhost:7211/swagger"
