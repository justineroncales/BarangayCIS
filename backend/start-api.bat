@echo off
echo Stopping any existing BarangayCIS.API processes...
taskkill /F /IM BarangayCIS.API.exe >nul 2>&1
timeout /t 1 /nobreak >nul

echo.
echo Starting Barangay CIS API...
echo API will be available at:
echo   - HTTP: http://localhost:5000
echo   - HTTPS: https://localhost:5001
echo   - Swagger: http://localhost:5000/swagger
echo.

cd BarangayCIS.API
dotnet run
pause

