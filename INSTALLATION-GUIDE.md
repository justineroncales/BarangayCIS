# Barangay CIS - Installation Guide

## Overview

This guide will help you install and deploy the Barangay CIS application on client laptops.

## System Requirements

### Minimum Requirements

- **OS**: Windows 10 (64-bit) or Windows 11
- **RAM**: 4 GB minimum (8 GB recommended)
- **Disk Space**: 500 MB for application + database space
- **Network**: Not required (runs locally)

### Prerequisites

Before installing the application, you need to install:

1. **.NET 8.0 Desktop Runtime** (if not using self-contained build)

   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Select "Desktop Runtime" (not SDK)
   - Size: ~50 MB

2. **SQL Server LocalDB** (for database)
   - Option A: SQL Server Express (includes LocalDB)
     - Download: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
     - During installation, select "LocalDB" feature
   - Option B: SQL Server LocalDB Standalone
     - Download: https://go.microsoft.com/fwlink/?LinkID=799012
   - Size: ~200 MB

## Installation Steps

### Step 1: Build the Installer Package

On your development machine:

```powershell
# Navigate to project root
cd d:\brgy

# Optional: Export database data to include in installer
# This step is required if you want to include existing data
.\export-database.ps1

# Run the build script
.\build-installer.ps1

# Create ZIP package (optional)
.\create-installer.ps1
```

This will create a `dist\BarangayCIS-Installer` folder with all necessary files.

**Note**: If you ran `export-database.ps1` before building, the database backup will be automatically included in the installer and restored on first run.

### Step 2: Transfer to Client Machine

1. Copy the entire `BarangayCIS-Installer` folder to the client's laptop
2. Recommended location: `C:\Program Files\BarangayCIS` or `C:\BarangayCIS`
3. Ensure the folder has write permissions (for database files)

### Step 3: Install Prerequisites on Client Machine

#### Install .NET 8.0 Desktop Runtime

1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run the installer
3. Select "Desktop Runtime" (not SDK)
4. Complete the installation

#### Install SQL Server LocalDB

1. Download SQL Server Express or LocalDB standalone
2. Run the installer
3. Follow the installation wizard
4. Ensure LocalDB feature is selected

### Step 4: Run the Application

1. Navigate to the installation folder
2. Double-click `Start-BarangayCIS.bat`
3. The application will:
   - Start the backend API
   - Open the frontend in your default browser
   - Create the database automatically on first run
   - Restore database from backup if included (first run only)

### Step 5: First Login

1. Open your browser (should open automatically)
2. Navigate to: http://localhost:5000
3. Login with default credentials:
   - **Username**: `admin`
   - **Password**: `admin123`
4. **IMPORTANT**: Change the password immediately after first login!

## Configuration

### Database Connection

The default connection uses LocalDB. To use a different SQL Server:

1. Edit: `Backend\appsettings.json`
2. Update the ConnectionStrings section:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=BarangayCIS;User Id=USER;Password=PASSWORD;Trusted_Connection=False;"
   }
   ```

### Change API Port

1. Edit: `Backend\appsettings.json`
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
   **Note:** Using `0.0.0.0` allows network access from other PCs. Use `localhost` to restrict to local access only.

### Network Access (Access from Other PCs)

The application is configured to accept connections from other PCs on the same network by default.

**To access from another PC on the same network:**

1. **Find the server PC's IP address:**

   - On the server PC, open Command Prompt
   - Run: `ipconfig`
   - Look for "IPv4 Address" (e.g., `192.168.1.100`)

2. **Configure Windows Firewall:**

   - Open Windows Firewall on the server PC
   - Add inbound rule for port 5000 (TCP)
   - Or temporarily disable firewall for testing

3. **Access from other PCs:**
   - Open a web browser on another PC
   - Navigate to: `http://[SERVER_IP]:5000`
   - Example: `http://192.168.1.100:5000`

**Note:** The application automatically detects if it's being accessed via network IP and adjusts the API connection accordingly.

**Security Note:** Only allow network access on trusted networks. For production, consider adding authentication or VPN requirements.

### Report Builder Usage

The Report Builder allows you to create custom reports from any database table:

1. Navigate to **Report Builder** from the sidebar
2. Select a table from the dropdown
3. Drag and drop fields to add them to your report
4. Add filters, sorting, and grouping as needed
5. Click **Execute Report** to view results
6. Export to Excel, PDF, or Word using the export buttons
7. Use the browser's print function (Ctrl+P) to print reports

## Troubleshooting

### Backend Won't Start

**Problem**: API doesn't start or shows errors

**Solutions**:

- Check if port 5000 is already in use
- Verify .NET 8.0 Runtime is installed
- Check Windows Firewall settings
- Review error messages in the Backend window

### Database Connection Errors

**Problem**: Cannot connect to database

**Solutions**:

- Ensure SQL Server LocalDB is installed and running
- Verify connection string in `appsettings.json`
- Check SQL Server service is running:
  ```powershell
  sqllocaldb info
  sqllocaldb start mssqllocaldb
  ```

### Frontend Won't Load

**Problem**: Browser shows error or blank page

**Solutions**:

- Ensure Backend API is running first (check the Backend window)
- Check browser console for errors (F12)
- Verify API is accessible: http://localhost:5000/api/health
- Try clearing browser cache

### Application Runs Slowly

**Problem**: Application is slow or unresponsive

**Solutions**:

- Check available disk space (database needs space)
- Ensure sufficient RAM (4GB minimum)
- Close other applications
- Check database file size and consider archiving old data

### Report Builder Issues

**Problem**: Report Builder not working or export fails

**Solutions**:

- Ensure you have selected at least one table and field
- Check browser console for errors (F12)
- Verify database connection is active
- For Excel exports: Ensure browser allows file downloads
- For PDF/Word exports: Check browser popup blocker settings
- Large reports may take time to generate - be patient

## Uninstallation

To uninstall the application:

1. Stop the application (close browser and Backend window)
2. Delete the installation folder
3. (Optional) Uninstall SQL Server LocalDB if not needed
4. (Optional) Uninstall .NET 8.0 Runtime if not needed

**Note**: Database files are stored in SQL Server LocalDB. To completely remove data:

- Use SQL Server Management Studio or sqlcmd to drop the database
- Or delete LocalDB instance: `sqllocaldb delete mssqllocaldb`

## Support

For technical support or issues:

1. Check the README.txt in the installation folder
2. Review error messages in the Backend console window
3. Check Windows Event Viewer for system errors
4. Contact your system administrator

## Key Features

The application includes the following main features:

- **Resident Management**: Complete database of all barangay residents with search and pagination
- **Certificate Issuance**: Digital certificates (Clearance, Indigency, Residency, etc.)
- **Incident & Blotter**: Digital recording and tracking of incidents and complaints
- **BHW Reports**: Population age distribution and catchment area reports
- **Report Builder**: Drag-and-drop custom report creation tool
  - Connect to any database table
  - Filter, sort, and group data
  - Export to Excel (.xlsx), PDF, or Word (.docx)
  - Print reports directly from browser
- **Health Center**: Medical records, vaccinations, and health tracking
- **Financial Management**: Budget tracking and expense monitoring
- **Project Monitoring**: Track ongoing barangay projects
- **Staff & Tasks**: Staff scheduling and task management
- **And more...**

## Version Information

- **Application Version**: 1.0.0
- **.NET Version**: 8.0
- **Database**: SQL Server LocalDB
- **Build Date**: See README.txt in installation folder

---

**Important Notes**:

- Always backup your database before major updates
- Keep the installation folder in a secure location
- Regularly update the application when new versions are available
- Change default passwords immediately after installation
- Report Builder allows creating custom reports from any database table
- All reports can be exported to Excel, PDF, or Word format
