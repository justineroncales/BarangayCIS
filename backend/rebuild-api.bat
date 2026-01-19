@echo off
echo Stopping any running BarangayCIS.API processes...
taskkill /F /IM BarangayCIS.API.exe >nul 2>&1
timeout /t 1 /nobreak >nul

echo Building BarangayCIS.API...
cd BarangayCIS.API
dotnet build

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
    echo.
    echo To run the API, use: dotnet run
    echo Or use: start-api.bat
) else (
    echo.
    echo Build failed!
)

pause


