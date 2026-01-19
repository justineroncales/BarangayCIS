# Restore Database Script
# Run this script to restore the database on the client machine

param(
    [string]$ServerName = "(localdb)\mssqllocaldb",
    [string]$BackupFile = "$PSScriptRoot\BarangayCIS.bak",
    [string]$DatabaseName = "BarangayCIS"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Restoring Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if backup file exists
if (-not (Test-Path $BackupFile)) {
    Write-Host "ERROR: Backup file not found: $BackupFile" -ForegroundColor Red
    exit 1
}

Write-Host "Backup file: $BackupFile" -ForegroundColor Yellow
Write-Host "Server: $ServerName" -ForegroundColor Yellow
Write-Host "Database: $DatabaseName" -ForegroundColor Yellow
Write-Host ""

# Check if database already exists
Write-Host "Checking if database exists..." -ForegroundColor Green
$dbExists = sqlcmd -S $ServerName -Q "SELECT name FROM sys.databases WHERE name = '$DatabaseName'" -h -1 -W 2>
if (-not [string]::IsNullOrWhiteSpace($dbExists)) {
    Write-Host "Database already exists. Skipping restore." -ForegroundColor Yellow
    Write-Host "To restore anyway, drop the database first or use -Force parameter." -ForegroundColor Yellow
    exit 0
}

# Get database file paths (use LocalDB default location)
$localDbPath = Join-Path $env:USERPROFILE "AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB"
if (-not (Test-Path $localDbPath)) {
    $localDbPath = Join-Path $env:LOCALAPPDATA "Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB"
}

$mdfPath = Join-Path $localDbPath "$DatabaseName.mdf"
$ldfPath = Join-Path $localDbPath "$DatabaseName_log.ldf"

Write-Host "Restoring database..." -ForegroundColor Green

# Get logical file names from backup
$fileListQuery = "RESTORE FILELISTONLY FROM DISK = '$BackupFile'"
$fileList = sqlcmd -S $ServerName -Q $fileListQuery -h -1 -W

# Extract logical names (simplified - assumes standard names)
$logicalDataName = "$DatabaseName"
$logicalLogName = "$DatabaseName_log"

# Restore database
$restoreQuery = "RESTORE DATABASE [$DatabaseName] FROM DISK = '$BackupFile' WITH REPLACE, MOVE '$logicalDataName' TO '$mdfPath', MOVE '$logicalLogName' TO '$ldfPath'"

Write-Host "Executing restore command..." -ForegroundColor Yellow
sqlcmd -S $ServerName -Q $restoreQuery

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Database restored successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "ERROR: Failed to restore database" -ForegroundColor Red
    Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Ensure SQL Server LocalDB is installed and running" -ForegroundColor White
    Write-Host "2. Check file permissions" -ForegroundColor White
    Write-Host "3. Verify backup file is not corrupted" -ForegroundColor White
    Write-Host ""
    exit 1
}
