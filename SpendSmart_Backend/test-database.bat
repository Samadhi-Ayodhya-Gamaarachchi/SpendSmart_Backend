echo "Testing database creation..."
dotnet ef database update
echo "Database update completed"
echo "Starting application..."
dotnet run --urls=https://localhost:7211
pause
