@echo off
echo Stopping Barangay CIS API server...
echo.

REM Try to stop by process name first
taskkill /F /IM BarangayCIS.API.exe >nul 2>&1

REM Also try to stop by port (find process using port 5000)
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5000 ^| findstr LISTENING') do (
    echo Stopping process on port 5000 (PID: %%a)...
    taskkill /F /PID %%a >nul 2>&1
)

REM Also check for dotnet processes that might be running the API
for /f "tokens=2" %%a in ('netstat -ano ^| findstr :5000 ^| findstr LISTENING') do (
    for /f "tokens=5" %%b in ('netstat -ano ^| findstr :5000 ^| findstr LISTENING') do (
        echo Stopping process (PID: %%b)...
        taskkill /F /PID %%b >nul 2>&1
    )
)

timeout /t 1 /nobreak >nul

echo.
echo API server stopped!
echo.
pause
