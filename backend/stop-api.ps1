# Stop Barangay CIS API Server Script
Write-Host "Stopping Barangay CIS API server..." -ForegroundColor Yellow
Write-Host ""

# Stop by process name
$processes = Get-Process -Name "BarangayCIS.API" -ErrorAction SilentlyContinue
if ($processes) {
    Write-Host "Found BarangayCIS.API process(es), stopping..." -ForegroundColor Cyan
    $processes | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "Stopped BarangayCIS.API process(es)" -ForegroundColor Green
}

# Also try to stop by port 5000
$port = 5000
$connections = netstat -ano | Select-String ":$port.*LISTENING"
if ($connections) {
    foreach ($connection in $connections) {
        $parts = $connection -split '\s+'
        $pid = $parts[-1]
        if ($pid -match '^\d+$') {
            try {
                $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($proc) {
                    Write-Host "Stopping process on port $port (PID: $pid, Name: $($proc.ProcessName))..." -ForegroundColor Cyan
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                }
            } catch {
                # Process might already be stopped
            }
        }
    }
}

Start-Sleep -Seconds 1

Write-Host ""
Write-Host "API server stopped!" -ForegroundColor Green
Write-Host ""
