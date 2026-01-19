# PowerShell script to import all Excel files from Downloads folder
# Usage: .\ImportResidentsFromDownloads.ps1 -BhwName "Emily rotairo"

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = "http://localhost:5000",
    
    [Parameter(Mandatory=$false)]
    [string]$BhwName = "Emily rotairo",
    
    [Parameter(Mandatory=$false)]
    [string]$DownloadsPath = $null
)

# Get Downloads folder path
if ([string]::IsNullOrWhiteSpace($DownloadsPath)) {
    $DownloadsPath = Join-Path $env:USERPROFILE "Downloads"
}

if (-not (Test-Path $DownloadsPath)) {
    Write-Host "ERROR: Downloads folder not found: $DownloadsPath" -ForegroundColor Red
    exit 1
}

Write-Host "Importing all Excel files from: $DownloadsPath" -ForegroundColor Green
Write-Host "Assigning BHW: $BhwName" -ForegroundColor Green
Write-Host "API URL: $ApiUrl" -ForegroundColor Green

# Find all Excel files
$excelFiles = Get-ChildItem -Path $DownloadsPath -Filter "*.xlsx" -File
$excelFiles += Get-ChildItem -Path $DownloadsPath -Filter "*.xls" -File

if ($excelFiles.Count -eq 0) {
    Write-Host "No Excel files found in Downloads folder." -ForegroundColor Yellow
    exit 0
}

Write-Host "`nFound $($excelFiles.Count) Excel file(s):" -ForegroundColor Cyan
foreach ($file in $excelFiles) {
    Write-Host "  - $($file.Name)" -ForegroundColor Cyan
}

Write-Host "`nStarting batch import..." -ForegroundColor Yellow

# URL encode the BHW name
Add-Type -AssemblyName System.Web
$encodedBhwName = [System.Web.HttpUtility]::UrlEncode($BhwName)

# Build the API endpoint URL
$uri = "$ApiUrl/api/residents/import-from-downloads?bhwName=$encodedBhwName"

# If custom Downloads path is provided, add it to the URL
if (-not [string]::IsNullOrWhiteSpace($DownloadsPath) -and $DownloadsPath -ne (Join-Path $env:USERPROFILE "Downloads")) {
    $encodedDownloadsPath = [System.Web.HttpUtility]::UrlEncode($DownloadsPath)
    $uri += "&downloadsPath=$encodedDownloadsPath"
}

try {
    # Disable SSL certificate validation for localhost (development only)
    if ($ApiUrl -like "*localhost*") {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
    }
    
    Write-Host "Calling API: $uri" -ForegroundColor Cyan
    $response = Invoke-RestMethod -Uri $uri -Method Post -ContentType "application/json" -ErrorAction Stop
    
    Write-Host "`nBatch import completed!" -ForegroundColor Green
    Write-Host "Files processed: $($response.filesProcessed)" -ForegroundColor Green
    Write-Host "Total imported: $($response.totalImported)" -ForegroundColor Green
    Write-Host "Total errors: $($response.totalErrors)" -ForegroundColor $(if ($response.totalErrors -gt 0) { "Yellow" } else { "Green" })
    
    if ($response.results -and $response.results.Count -gt 0) {
        Write-Host "`nFile-by-file results:" -ForegroundColor Cyan
        foreach ($result in $response.results) {
            Write-Host "`n  File: $($result.fileName)" -ForegroundColor White
            Write-Host "    Imported: $($result.importedCount)" -ForegroundColor Green
            Write-Host "    Errors: $($result.errorCount)" -ForegroundColor $(if ($result.errorCount -gt 0) { "Yellow" } else { "Green" })
            
            if ($result.errors -and $result.errors.Count -gt 0) {
                Write-Host "    Error details:" -ForegroundColor Yellow
                foreach ($error in $result.errors) {
                    Write-Host "      - $error" -ForegroundColor Yellow
                }
            }
        }
    }
}
catch {
    Write-Host "ERROR: Failed to import residents from Downloads" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host $_.ErrorDetails.Message -ForegroundColor Red
    }
    exit 1
}
