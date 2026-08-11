# =====================================================================
#  NexusStrap - Safe cleanup / uninstall script
#  Removes ONLY files and registry entries owned by NexusStrap.
#  It will NOT touch: %LOCALAPPDATA%\Roblox, your Documents, Downloads,
#  desktop personal files, Chrome/Edge/Brave, or any other application.
#
#  Usage:  powershell -ExecutionPolicy Bypass -File .\cleanup-nexusstrap.ps1
# =====================================================================

$ErrorActionPreference = "Continue"

$appNames  = @("NexusStrap", "NexusStrap-QA")
$basePaths = @(
    (Join-Path $env:LOCALAPPDATA "NexusStrap"),
    (Join-Path $env:LOCALAPPDATA "NexusStrap-QA")
)

Write-Host "=== NexusStrap cleanup ===" -ForegroundColor Cyan

# 1) Stop running NexusStrap processes
Write-Host "`n[1/5] Stopping running NexusStrap processes..." -ForegroundColor Yellow
foreach ($name in $appNames) {
    Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 1

# 2) Try the built-in quiet uninstaller first (removes Roblox versions,
#    downloads, registry protocol keys and shortcuts the app itself manages)
Write-Host "`n[2/5] Running NexusStrap's own uninstaller (best effort)..." -ForegroundColor Yellow
foreach ($base in $basePaths) {
    $exe = Join-Path $base "NexusStrap.exe"
    if (Test-Path $exe) {
        try {
            Start-Process -FilePath $exe -ArgumentList "-uninstall", "-quiet" -Wait -WindowStyle Hidden -ErrorAction Stop
            Start-Sleep -Seconds 2
        } catch {
            Write-Host "Could not run built-in uninstaller from $exe : $($_.Exception.Message)"
        }
    }
}

# 3) Remove the application folder(s) (data, config, cache, versions, exe...)
Write-Host "`n[3/5] Removing NexusStrap data folders..." -ForegroundColor Yellow
foreach ($base in $basePaths) {
    if (Test-Path $base) {
        Remove-Item -LiteralPath $base -Recurse -Force -ErrorAction Continue
        Write-Host "Removed: $base"
    } else {
        Write-Host "Not present: $base"
    }
}

# 4) Remove desktop / start menu shortcuts pointing at NexusStrap
Write-Host "`n[4/5] Removing NexusStrap shortcuts..." -ForegroundColor Yellow
$shortcutFolders = @(
    [Environment]::GetFolderPath("Desktop"),
    [Environment]::GetFolderPath("CommonDesktopDirectory"),
    (Join-Path ([Environment]::GetFolderPath("StartMenu")) "Programs")
)

foreach ($folder in $shortcutFolders | Select-Object -Unique) {
    if (-not (Test-Path $folder)) { continue }

    Get-ChildItem -LiteralPath $folder -Filter "*.lnk" -File -ErrorAction SilentlyContinue | ForEach-Object {
        try {
            $sh = New-Object -ComObject WScript.Shell
            $target = $sh.CreateShortcut($_.FullName).TargetPath

            if ($target -and ($target -like "*\NexusStrap*.exe")) {
                Remove-Item -LiteralPath $_.FullName -Force -ErrorAction Continue
                Write-Host "Removed shortcut: $($_.FullName)"
            }
        } catch { }
    }
}

# 5) Remove NexusStrap registry keys (per-user only)
Write-Host "`n[5/5] Removing NexusStrap registry keys..." -ForegroundColor Yellow
foreach ($name in $appNames) {
    $uninstallKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$name"
    $apisKey      = "HKCU:\Software\$name"

    if (Test-Path $uninstallKey) { Remove-Item -LiteralPath $uninstallKey -Recurse -Force;  Write-Host "Removed: $uninstallKey" }
    if (Test-Path $apisKey)      { Remove-Item -LiteralPath $apisKey      -Recurse -Force;  Write-Host "Removed: $apisKey" }
}

Write-Host "`n=== Cleanup finished. NexusStrap is fully removed. ===" -ForegroundColor Green
Write-Host "Your Roblox installation (if any) and all other files were left untouched."