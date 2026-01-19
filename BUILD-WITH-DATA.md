# Building Installer with Database Data

## Overview

To include all your database data in the installer, you need to export the database first, then build the installer.

## Step-by-Step Process

### Step 1: Export Database

Before building the installer, export your database:

```powershell
# From project root
.\export-database.ps1
```

This will:
- Create a backup of your database (`BarangayCIS.bak`)
- Create restore scripts
- Save everything to `.\database-backup\` folder

**Important**: Make sure your database contains all the data you want to include!

### Step 2: Build Installer

After exporting the database, build the installer:

```powershell
.\build-installer.ps1
```

The build script will automatically:
- Include the database backup file
- Include the restore script
- Set up automatic database restoration on first run

### Step 3: Test

Test the installer to ensure database is restored correctly:

```powershell
cd .\dist\BarangayCIS-Installer
.\Start-BarangayCIS.bat
```

The application will:
1. Check if database exists
2. If not, restore from backup automatically
3. Start the application with all your data

## What Gets Included

```
BarangayCIS-Installer/
├── Backend/
│   └── [API files]
├── Frontend/
│   └── [React app]
├── Database/
│   ├── BarangayCIS.bak (database backup)
│   └── restore-database.ps1 (restore script)
├── Start-BarangayCIS.bat
└── README.txt
```

## Database Restoration

The launcher script (`Start-BarangayCIS.bat`) automatically:
1. Checks if database exists
2. If not found, restores from `Database\BarangayCIS.bak`
3. If database exists, skips restoration (preserves existing data)

## Manual Database Restoration

If automatic restoration fails, you can restore manually:

```powershell
cd .\Database
.\restore-database.ps1
```

Or using SQL:

```sql
RESTORE DATABASE [BarangayCIS]
FROM DISK = 'C:\path\to\BarangayCIS.bak'
WITH REPLACE
```

## Updating Database Data

To update the database data in the installer:

1. Make changes to your database
2. Run `.\export-database.ps1` again
3. Rebuild installer: `.\build-installer.ps1`

## Troubleshooting

### Database backup not found

**Error**: "WARNING: No database backup found"

**Solution**: Run `.\export-database.ps1` first before building

### Database restore fails

**Error**: "Database restore failed"

**Solutions**:
- Ensure SQL Server LocalDB is installed
- Check backup file exists in `Database\` folder
- Verify SQL Server service is running
- Check file permissions

### Database already exists

**Message**: "Database already exists. Skipping restore."

**Solution**: This is normal. To force restore:
1. Drop existing database first
2. Or delete the database files manually
3. Run launcher again

## Notes

- **Backup size**: Database backups can be large (depends on data)
- **First run**: Database restoration happens automatically on first run
- **Subsequent runs**: Database is not restored if it already exists
- **Data safety**: Always backup before restoring on client machines
