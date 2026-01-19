# Create Windows Installer Package
# This script creates a distributable installer package

param(
    [string]$OutputPath = ".\BarangayCIS-Installer.zip"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Creating Installer Package" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Run build script first
Write-Host "Running build script..." -ForegroundColor Yellow
& ".\build-installer.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Cannot create installer." -ForegroundColor Red
    exit 1
}

# Create ZIP archive
Write-Host "`nCreating ZIP archive..." -ForegroundColor Green
$distDir = ".\dist\BarangayCIS-Installer"

if (-not (Test-Path $distDir)) {
    Write-Host "Build output not found at: $distDir" -ForegroundColor Red
    exit 1
}

# Remove existing ZIP if it exists
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Force
}

# Create ZIP
Compress-Archive -Path "$distDir\*" -DestinationPath $OutputPath -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Installer package created successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Package location: $OutputPath" -ForegroundColor Cyan
Write-Host "Package size: $([math]::Round((Get-Item $OutputPath).Length / 1MB, 2)) MB" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now distribute this ZIP file to clients." -ForegroundColor Yellow
Write-Host ""
