# Build Tips - Barangay CIS API

## Common Build Error: File Locked

If you get an error like:
```
The file is locked by: "BarangayCIS.API (PID)"
```

This means the API is currently running and the build can't overwrite the executable.

## Solutions

### Option 1: Use the Rebuild Script (Easiest)
```bash
cd backend
rebuild-api.bat
```

This will automatically stop the running process and rebuild.

### Option 2: Stop Manually
1. Find the process:
   ```powershell
   Get-Process -Name "BarangayCIS.API"
   ```

2. Stop it:
   ```powershell
   Stop-Process -Name "BarangayCIS.API" -Force
   ```
   
   Or in CMD:
   ```cmd
   taskkill /F /IM BarangayCIS.API.exe
   ```

3. Then build:
   ```bash
   dotnet build
   ```

### Option 3: Close the Terminal
If you started the API in a terminal window, simply close that window, then rebuild.

## Quick Commands

### Stop the API
```cmd
taskkill /F /IM BarangayCIS.API.exe
```

### Rebuild and Run
```bash
cd backend
rebuild-api.bat
cd BarangayCIS.API
dotnet run
```

### Check if API is Running
```powershell
Get-Process -Name "BarangayCIS.API" -ErrorAction SilentlyContinue
```

## Best Practices

1. **Stop before building**: Always stop the running API before rebuilding
2. **Use the scripts**: The provided batch/PowerShell scripts handle this automatically
3. **One instance only**: Don't run multiple instances of the API on the same port

## Note

The start scripts (`start-api.bat` and `start-api.ps1`) now automatically stop any existing instances before starting, so you can safely use them even if the API is already running.


