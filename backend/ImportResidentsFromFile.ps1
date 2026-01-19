# PowerShell script to import residents from Excel file using the API
# Usage: .\ImportResidentsFromFile.ps1 -FilePath "C:\Users\Justine\Downloads\Resident Template BLOCK 5.xlsx"

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,
    
    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = "http://localhost:5000",
    
    [Parameter(Mandatory=$false)]
    [string]$BhwName = "Emily rotairo"
)

# Check if file exists
if (-not (Test-Path $FilePath)) {
    Write-Host "ERROR: File not found: $FilePath" -ForegroundColor Red
    exit 1
}

Write-Host "Importing residents from: $FilePath" -ForegroundColor Green
Write-Host "Assigning BHW: $BhwName" -ForegroundColor Green
Write-Host "API URL: $ApiUrl" -ForegroundColor Green

# URL encode the file path and BHW name
Add-Type -AssemblyName System.Web
$encodedFilePath = [System.Web.HttpUtility]::UrlEncode($FilePath)
$encodedBhwName = [System.Web.HttpUtility]::UrlEncode($BhwName)

# For local file path, we'll use the filePath query parameter
$uri = "$ApiUrl/api/residents/import?filePath=$encodedFilePath&bhwName=$encodedBhwName"

try {
    # Disable SSL certificate validation for localhost (development only)
    if ($ApiUrl -like "*localhost*") {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
    }
    
    Write-Host "Calling API: $uri" -ForegroundColor Cyan
    $response = Invoke-RestMethod -Uri $uri -Method Post -ContentType "application/json" -ErrorAction Stop
    
    Write-Host "`nImport completed!" -ForegroundColor Green
    Write-Host "Imported: $($response.importedCount)" -ForegroundColor Green
    Write-Host "Errors: $($response.errorCount)" -ForegroundColor $(if ($response.errorCount -gt 0) { "Yellow" } else { "Green" })
    
    if ($response.errors -and $response.errors.Count -gt 0) {
        Write-Host "`nErrors:" -ForegroundColor Yellow
        $response.errors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    }
}
catch {
    Write-Host "ERROR: Failed to import residents" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host $_.ErrorDetails.Message -ForegroundColor Red
    }
    exit 1
}

