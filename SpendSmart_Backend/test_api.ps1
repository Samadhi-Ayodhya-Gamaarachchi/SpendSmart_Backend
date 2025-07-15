# PowerShell script to test CRUD operations

Write-Host "=== Testing SpendSmart Admin CRUD Operations ===" -ForegroundColor Green

$baseUrl = "http://localhost:5110/api/AdminProfile"

Write-Host "`n1. Testing GET All Admins..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri $baseUrl -Method GET
    Write-Host "✅ GET All Admins - Status: Success" -ForegroundColor Green
    Write-Host "Current Admins Count: $($response.Count)" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ GET All Admins - Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n2. Testing CREATE Admin..." -ForegroundColor Yellow
$newAdmin = @{
    name = "Test Admin"
    email = "testadmin@spendsmart.com"
    password = "TestPassword123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $baseUrl -Method POST -Body $newAdmin -ContentType "application/json"
    Write-Host "✅ CREATE Admin - Status: Success" -ForegroundColor Green
    Write-Host "Created Admin ID: $($response.id)" -ForegroundColor Cyan
    $global:testAdminId = $response.id
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ CREATE Admin - Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorBody = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorBody)
        $errorText = $reader.ReadToEnd()
        Write-Host "Error Details: $errorText" -ForegroundColor Red
    }
}

if ($global:testAdminId) {
    Write-Host "`n3. Testing GET Single Admin..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/$global:testAdminId" -Method GET
        Write-Host "✅ GET Single Admin - Status: Success" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 2
    } catch {
        Write-Host "❌ GET Single Admin - Error: $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host "`n4. Testing UPDATE Admin..." -ForegroundColor Yellow
    $updateAdmin = @{
        name = "Updated Test Admin"
        email = "updated.testadmin@spendsmart.com"
        currentPassword = "TestPassword123"
        password = "NewPassword123"
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/$global:testAdminId" -Method PUT -Body $updateAdmin -ContentType "application/json"
        Write-Host "✅ UPDATE Admin - Status: Success" -ForegroundColor Green
    } catch {
        Write-Host "❌ UPDATE Admin - Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $errorBody = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorBody)
            $errorText = $reader.ReadToEnd()
            Write-Host "Error Details: $errorText" -ForegroundColor Red
        }
    }

    Write-Host "`n5. Testing DELETE Admin..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/$global:testAdminId" -Method DELETE
        Write-Host "✅ DELETE Admin - Status: Success" -ForegroundColor Green
    } catch {
        Write-Host "❌ DELETE Admin - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== CRUD Testing Complete ===" -ForegroundColor Green
