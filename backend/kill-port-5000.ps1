# Kill any process using port 5000
Write-Host "Finding processes using port 5000..." -ForegroundColor Yellow

# Method 1: Using Get-NetTCPConnection
$connections = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
if ($connections) {
    foreach ($conn in $connections) {
        $pid = $conn.OwningProcess
        try {
            $proc = Get-Process -Id $pid -ErrorAction Stop
            Write-Host "Found process: $($proc.ProcessName) (PID: $pid)" -ForegroundColor Cyan
            Write-Host "Stopping process..." -ForegroundColor Yellow
            Stop-Process -Id $pid -Force -ErrorAction Stop
            Write-Host "Process stopped successfully!" -ForegroundColor Green
        } catch {
            Write-Host "Could not stop process $pid : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "No active connections found on port 5000" -ForegroundColor Yellow
}

# Method 2: Try netstat approach
Write-Host "`nChecking with netstat..." -ForegroundColor Yellow
$netstatOutput = netstat -ano | Select-String ":5000"
if ($netstatOutput) {
    foreach ($line in $netstatOutput) {
        if ($line -match '\s+(\d+)$') {
            $pid = $matches[1]
            try {
                $proc = Get-Process -Id $pid -ErrorAction Stop
                Write-Host "Found process: $($proc.ProcessName) (PID: $pid)" -ForegroundColor Cyan
                Write-Host "Stopping process..." -ForegroundColor Yellow
                Stop-Process -Id $pid -Force -ErrorAction Stop
                Write-Host "Process stopped successfully!" -ForegroundColor Green
            } catch {
                Write-Host "Could not stop process $pid : $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
} else {
    Write-Host "No processes found with netstat" -ForegroundColor Yellow
}

# Method 3: Kill all dotnet processes (be careful!)
Write-Host "`nChecking for dotnet processes..." -ForegroundColor Yellow
$dotnetProcs = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcs) {
    Write-Host "Found $($dotnetProcs.Count) dotnet process(es)" -ForegroundColor Cyan
    $response = Read-Host "Do you want to kill all dotnet processes? (y/n)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        $dotnetProcs | Stop-Process -Force
        Write-Host "All dotnet processes stopped!" -ForegroundColor Green
    }
} else {
    Write-Host "No dotnet processes found" -ForegroundColor Yellow
}

# Method 4: Kill BarangayCIS.API specifically
Write-Host "`nChecking for BarangayCIS.API processes..." -ForegroundColor Yellow
$apiProcs = Get-Process -Name "BarangayCIS.API" -ErrorAction SilentlyContinue
if ($apiProcs) {
    Write-Host "Found $($apiProcs.Count) BarangayCIS.API process(es)" -ForegroundColor Cyan
    $apiProcs | Stop-Process -Force
    Write-Host "BarangayCIS.API processes stopped!" -ForegroundColor Green
} else {
    Write-Host "No BarangayCIS.API processes found" -ForegroundColor Yellow
}

Write-Host "`nDone! Try starting the API again." -ForegroundColor Green
Start-Sleep -Seconds 2
