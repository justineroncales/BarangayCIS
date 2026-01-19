# Build and Package Script for Barangay CIS
# This script creates an installable package for client deployment

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Barangay CIS - Build Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Configuration
$BuildDir = ".\dist"
$BackendDir = ".\backend\BarangayCIS.API"
$FrontendDir = ".\frontend"
$OutputDir = "$BuildDir\BarangayCIS-Installer"
$DatabaseBackupPath = ".\database-backup"

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $BuildDir) {
    Remove-Item -Path $BuildDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Step 1: Build Backend (Self-contained)
Write-Host "`n[1/4] Building Backend (Self-contained)..." -ForegroundColor Green
Set-Location $BackendDir

# Publish as self-contained Windows executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "..\..\$OutputDir\Backend"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Backend build failed!" -ForegroundColor Red
    exit 1
}

Set-Location ..\..\

# Step 2: Build Frontend
Write-Host "`n[2/4] Building Frontend..." -ForegroundColor Green
Set-Location $FrontendDir

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    npm install
}

# Build React app
Write-Host "Building React app..." -ForegroundColor Yellow
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Frontend build failed!" -ForegroundColor Red
    exit 1
}

Set-Location ..\

# Step 3: Copy Frontend build to output
Write-Host "`n[3/5] Copying Frontend build..." -ForegroundColor Green
Copy-Item -Path "$FrontendDir\dist\*" -Destination "$OutputDir\Frontend" -Recurse -Force

# Step 3.5: Copy Database backup if it exists
Write-Host "`n[3.5/5] Copying Database backup..." -ForegroundColor Green
if (Test-Path $DatabaseBackupPath) {
    $dbBackupFile = Get-ChildItem -Path $DatabaseBackupPath -Filter "*.bak" | Select-Object -First 1
    if ($dbBackupFile) {
        Write-Host "Found database backup: $($dbBackupFile.Name)" -ForegroundColor Yellow
        Copy-Item -Path $dbBackupFile.FullName -Destination "$OutputDir\Database\BarangayCIS.bak" -Force
        Write-Host "Database backup included in installer" -ForegroundColor Green
        
        # Copy restore script if it exists
        $restoreScript = Join-Path $DatabaseBackupPath "restore-database.ps1"
        if (Test-Path $restoreScript) {
            Copy-Item -Path $restoreScript -Destination "$OutputDir\Database\restore-database.ps1" -Force
        }
    } else {
        Write-Host "WARNING: No database backup found. Run .\export-database.ps1 first to include database data." -ForegroundColor Yellow
    }
} else {
    Write-Host "WARNING: Database backup directory not found. Run .\export-database.ps1 first to include database data." -ForegroundColor Yellow
}

# Step 4: Create launcher and installer files
Write-Host "`n[4/5] Creating launcher and installer files..." -ForegroundColor Green

# Create launcher script
$launcherScript = @"
@echo off
title Barangay CIS - Starting...
echo ========================================
echo Barangay CIS - Starting Application
echo ========================================
echo.

REM Change to script directory
cd /d "%~dp0"

REM Check if .NET Runtime is installed (for self-contained, this check is optional)
REM where dotnet >nul 2>&1
REM if %ERRORLEVEL% NEQ 0 (
REM     echo WARNING: .NET Runtime check skipped (using self-contained build)
REM )

REM Check if SQL Server LocalDB is available
echo Checking database connection...
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT 1" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: SQL Server LocalDB not found!
    echo.
    echo The application will attempt to create the database on first run.
    echo If you encounter database errors, please install:
    echo - SQL Server Express with LocalDB, OR
    echo - SQL Server LocalDB standalone
    echo.
    timeout /t 3 >nul
)

REM Restore database from backup if it exists and database doesn't exist
if exist "Database\BarangayCIS.bak" (
    echo Checking if database needs to be restored...
    sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases WHERE name = 'BarangayCIS'" -h -1 -W >nul 2>&1
    if %ERRORLEVEL% NEQ 0 (
        echo Database not found. Restoring from backup...
        if exist "Database\restore-database.ps1" (
            powershell -ExecutionPolicy Bypass -File "Database\restore-database.ps1" -BackupFile "%~dp0Database\BarangayCIS.bak"
        ) else (
            echo Restoring database manually...
            sqlcmd -S "(localdb)\mssqllocaldb" -Q "RESTORE DATABASE [BarangayCIS] FROM DISK = '%~dp0Database\BarangayCIS.bak' WITH REPLACE"
        )
        if %ERRORLEVEL% EQU 0 (
            echo Database restored successfully!
        ) else (
            echo WARNING: Database restore failed. Application will create empty database.
        )
    ) else (
        echo Database already exists. Skipping restore.
    )
)

REM Start Backend API
echo Starting Backend API...
start "Barangay CIS API" /MIN "Backend\BarangayCIS.API.exe"
timeout /t 5 >nul

REM Wait for API to be ready (with timeout)
echo Waiting for API to start...
set /a counter=0
:wait
curl -s http://localhost:5000/api/health >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    goto api_ready
)
set /a counter+=1
if %counter% GTR 30 (
    echo ERROR: API failed to start after 30 seconds!
    echo Please check the Backend window for errors.
    pause
    exit /b 1
)
timeout /t 1 >nul
goto wait

:api_ready
echo API is ready!

REM Get local IP address for network access
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4 Address"') do (
    set LOCAL_IP=%%a
    goto :ip_found
)
:ip_found
set LOCAL_IP=%LOCAL_IP:~1%

REM Open Frontend in default browser
echo Opening application in browser...
timeout /t 2 >nul
start http://localhost:5000

echo.
echo ========================================
echo Application started successfully!
echo ========================================
echo.
echo Backend API: http://localhost:5000
echo Frontend: http://localhost:5000
echo.
echo Network Access:
echo   Local: http://localhost:5000
if defined LOCAL_IP (
    echo   Network: http://%LOCAL_IP%:5000
    echo.
    echo   Other PCs on the same network can access using:
    echo   http://%LOCAL_IP%:5000
)
echo.
echo Default Login:
echo   Username: admin
echo   Password: admin123
echo.
echo IMPORTANT: Change the password after first login!
echo.
echo Press any key to close this window (application will continue running)...
pause >nul
"@

$launcherScript | Out-File -FilePath "$OutputDir\Start-BarangayCIS.bat" -Encoding ASCII

# Create README for installation
$readmeContent = @"
# Barangay CIS - Installation Guide

## Quick Start

1. Install prerequisites: .NET 8.0 Runtime and SQL Server LocalDB
2. Run Start-BarangayCIS.bat
3. Login with: admin / admin123

## System Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Desktop Runtime
- SQL Server LocalDB (or SQL Server Express)

## Installation Steps

### 1. Install Prerequisites

#### A. Install .NET 8.0 Desktop Runtime
1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run the installer
3. Select "Desktop Runtime" (not SDK)

#### B. Install SQL Server LocalDB
Option 1: SQL Server Express (includes LocalDB)
- Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- During installation, select "LocalDB" feature

Option 2: SQL Server LocalDB Standalone
- Download from: https://go.microsoft.com/fwlink/?LinkID=799012

### 2. Install Application

1. Copy the entire "BarangayCIS-Installer" folder to your desired location
   (e.g., C:\Program Files\BarangayCIS)

2. Run "Start-BarangayCIS.bat" to launch the application

### 3. First Run

1. The application will automatically:
   - Restore database from backup (if included in installer)
   - Create the database on first run (if no backup)
   - Seed initial admin user

2. Default Login Credentials:
   - Username: admin
   - Password: admin123

   **IMPORTANT:** Change the password after first login!

### 4. Key Features

- Resident Management with search and pagination
- Certificate Issuance (Clearance, Indigency, etc.)
- Incident & Blotter Recording
- BHW Reports (Population age distribution, catchment area)
- **Report Builder**: Drag-and-drop custom report creation
  - Connect to any database table
  - Export to Excel (.xlsx), PDF, or Word (.docx)
  - Print reports directly
- Health Center management
- Financial tracking
- Project monitoring
- And more...

### 5. Configuration

#### Database Connection
The default connection uses LocalDB. To use a different SQL Server:

1. Edit: Backend\appsettings.json
2. Update the ConnectionStrings section:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=BarangayCIS;User Id=USER;Password=PASSWORD;"
   }
   ```

#### Change API Port
1. Edit: Backend\appsettings.json
2. Add or modify:
   ```json
   "Kestrel": {
     "Endpoints": {
       "Http": {
         "Url": "http://0.0.0.0:5000"
       }
     }
   }
   ```
   Note: Using `0.0.0.0` allows network access. Use `localhost` to restrict to local access only.

#### Network Access
The application is configured to accept connections from other PCs on the same network:
- Server PC: Access via http://localhost:5000
- Other PCs: Access via http://[SERVER_IP]:5000
- Find server IP: Run `ipconfig` in Command Prompt
- Ensure Windows Firewall allows port 5000 (inbound rule)

## Troubleshooting

### Backend won't start
- Check if port 5000 is available
- Verify .NET 8.0 Runtime is installed
- Check Windows Firewall settings

### Database connection errors
- Ensure SQL Server LocalDB is installed and running
- Verify connection string in appsettings.json
- Check SQL Server service is running

### Frontend won't load
- Ensure Backend API is running first
- Check browser console for errors
- Verify API is accessible at http://localhost:5000

### Report Builder issues
- Ensure you have selected at least one table and field
- Check browser console for errors (F12)
- For Excel/PDF/Word exports: Check browser popup blocker settings
- Large reports may take time to generate

## Support

For issues or questions, contact your system administrator.

## Version

Barangay CIS v1.0.0
Built: $(Get-Date -Format "yyyy-MM-dd")
"@

$readmeContent | Out-File -FilePath "$OutputDir\README.txt" -Encoding UTF8

# Create appsettings.json template
$appsettingsTemplate = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BarangayCIS;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForBarangayCIS2024!@#$%^&*()",
    "Issuer": "BarangayCIS",
    "Audience": "BarangayCISUsers",
    "ExpirationInMinutes": 1440
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
}
"@

$appsettingsTemplate | Out-File -FilePath "$OutputDir\Backend\appsettings.json" -Encoding UTF8 -Force

# Create Database directory
New-Item -ItemType Directory -Path "$OutputDir\Database" -Force | Out-Null

# Step 5: Finalize
Write-Host "`n[5/5] Finalizing..." -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output directory: $OutputDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
if (-not (Test-Path "$OutputDir\Database\BarangayCIS.bak")) {
    Write-Host "1. IMPORTANT: Export database data first: .\export-database.ps1" -ForegroundColor Red
    Write-Host "2. Rebuild installer after export: .\build-installer.ps1" -ForegroundColor White
    Write-Host "3. Test the application: $OutputDir\Start-BarangayCIS.bat" -ForegroundColor White
} else {
    Write-Host "1. Database backup included ✓" -ForegroundColor Green
    Write-Host "2. Test the application: $OutputDir\Start-BarangayCIS.bat" -ForegroundColor White
    Write-Host "3. Zip the entire '$OutputDir' folder for distribution" -ForegroundColor White
}
Write-Host ""
Write-Host "Network Access:" -ForegroundColor Cyan
Write-Host "  - Application is configured for network access" -ForegroundColor White
Write-Host "  - Other PCs can access using: http://[SERVER_IP]:5000" -ForegroundColor White
Write-Host "  - See NETWORK-ACCESS.md for detailed instructions" -ForegroundColor White
Write-Host ""
