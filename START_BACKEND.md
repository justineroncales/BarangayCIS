# How to Start the Backend API Server

## Quick Start

The backend API server **MUST** be running for the frontend to work. Follow these steps:

### Method 1: Using Batch File (Windows - Easiest)

1. Open File Explorer
2. Navigate to the `backend` folder
3. Double-click `start-api.bat`

### Method 2: Using Command Prompt

1. Open Command Prompt or PowerShell
2. Navigate to the project:
   ```bash
   cd D:\brgy\backend\BarangayCIS.API
   ```
3. Run:
   ```bash
   dotnet run
   ```

### Method 3: Using PowerShell Script

1. Open PowerShell
2. Navigate to the `backend` folder
3. Run:
   ```powershell
   .\start-api.ps1
   ```

## Verify It's Running

Once started, you should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

## Test the API

1. Open your browser
2. Go to: `http://localhost:5000/swagger`
3. You should see the Swagger API documentation

## Default Login Credentials

- **Username:** `admin`
- **Password:** `admin123`

## Troubleshooting

### Port Already in Use

If you get an error that port 5000 is already in use:

1. Find what's using the port:
   ```powershell
   netstat -ano | findstr :5000
   ```

2. Kill the process (replace PID with the number from step 1):
   ```powershell
   taskkill /PID <PID> /F
   ```

3. Or change the port in `backend/BarangayCIS.API/Properties/launchSettings.json`

### Database Connection Error

Make sure SQL Server LocalDB is installed and running. The default connection uses LocalDB.

### Build Errors

If you get build errors:

1. Restore packages:
   ```bash
   cd backend/BarangayCIS.API
   dotnet restore
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Fix any errors shown

## Keep the Server Running

**Important:** Keep the terminal/command prompt window open while using the application. Closing it will stop the server.

For production, you would run this as a Windows Service or use a process manager, but for development, just keep the terminal open.


