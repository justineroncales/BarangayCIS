# Barangay CIS API Startup Script
Write-Host "Stopping any existing BarangayCIS.API processes..." -ForegroundColor Yellow
Get-Process -Name "BarangayCIS.API" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Host ""
Write-Host "Starting Barangay CIS API..." -ForegroundColor Green
Write-Host "API will be available at:" -ForegroundColor Yellow
Write-Host "  - HTTP: http://localhost:5000" -ForegroundColor Cyan
Write-Host "  - HTTPS: https://localhost:5001" -ForegroundColor Cyan
Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""

Set-Location BarangayCIS.API
dotnet run

