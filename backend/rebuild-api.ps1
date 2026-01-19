# Rebuild Barangay CIS API Script
Write-Host "Stopping any running BarangayCIS.API processes..." -ForegroundColor Yellow
Get-Process -Name "BarangayCIS.API" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Host "Building BarangayCIS.API..." -ForegroundColor Green
Set-Location BarangayCIS.API
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To run the API, use: dotnet run" -ForegroundColor Cyan
    Write-Host "Or use: .\start-api.ps1" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "Build failed!" -ForegroundColor Red
}


