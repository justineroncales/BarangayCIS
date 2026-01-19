# Quick Start - Building Installable Application

## For Developers

### Step 1: Build the Installer

```powershell
# Run from project root (d:\brgy)
.\build-installer.ps1
```

This will:
- Build backend as self-contained executable (includes .NET runtime)
- Build frontend React app
- Create launcher script
- Generate installation documentation

### Step 2: Create Distribution Package (Optional)

```powershell
.\create-installer.ps1
```

This creates a ZIP file ready for distribution.

### Step 3: Test the Build

1. Navigate to `dist\BarangayCIS-Installer`
2. Run `Start-BarangayCIS.bat`
3. Verify application starts correctly

## For Clients

### Installation Steps

1. **Extract** the `BarangayCIS-Installer` folder to desired location
   - Recommended: `C:\Program Files\BarangayCIS`

2. **Install Prerequisites** (if not already installed):
   - .NET 8.0 Desktop Runtime
   - SQL Server LocalDB

3. **Run** `Start-BarangayCIS.bat`

4. **Login** with:
   - Username: `admin`
   - Password: `admin123`

## What Gets Built

```
BarangayCIS-Installer/
├── Backend/
│   ├── BarangayCIS.API.exe (self-contained)
│   ├── appsettings.json
│   └── [all dependencies]
├── Frontend/
│   └── [built React app]
├── Database/
│   ├── BarangayCIS.bak (if exported)
│   └── restore-database.ps1 (if exported)
├── Start-BarangayCIS.bat
└── README.txt
```

## Notes

- Backend is **self-contained** - no need to install .NET separately (but recommended for updates)
- Database uses **LocalDB** - lightweight SQL Server
- Application runs on **localhost:5000** - no network required
- All data is stored **locally** on the client machine
- **Report Builder** included - create custom reports with drag-and-drop interface
- Reports can be exported to **Excel, PDF, or Word** format
- Database backup can be included in installer (run `export-database.ps1` first)
