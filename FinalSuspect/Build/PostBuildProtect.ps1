param([string]$TargetDll, [string]$Configuration, [string]$BackupDir)

trap {
    $msg = "TRAP: $($_.Exception.Message)"
    Write-Host "[PostBuildProtect] $msg" -ForegroundColor Red
    if ($script:__bak -and (Test-Path $script:__bak -PathType Leaf)) {
        try { Copy-Item $script:__bak $TargetDll -Force -ErrorAction Stop } catch {}
        Write-Host "[PostBuildProtect] Restored backup" -ForegroundColor Yellow
    }
    exit 0
}

$ErrorActionPreference = "Stop"
$script:__bak = ""

if (-not $TargetDll) { Write-Host "[PostBuildProtect] No target" ; exit 0 }
if (-not (Test-Path $TargetDll -PathType Leaf)) { Write-Host "[PostBuildProtect] Not found: $TargetDll" ; exit 0 }

$fullPath = (Resolve-Path $TargetDll -ErrorAction Stop).Path
$bytes = [System.IO.File]::ReadAllBytes($fullPath)
$len   = $bytes.Count

if ($len -lt 512) { Write-Host "[PostBuildProtect] File too small" ; exit 0 }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Post-Build Protection"               -ForegroundColor Cyan
Write-Host "  $fullPath"                            -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($BackupDir) {
    $null = New-Item -ItemType Directory -Path $BackupDir -Force -ErrorAction SilentlyContinue
    $script:__bak = Join-Path $BackupDir "$([IO.Path]::GetFileNameWithoutExtension($TargetDll))_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
    Copy-Item $fullPath $script:__bak -Force
    Write-Host "[1/5] Backup OK" -ForegroundColor Green
}

for ($p = 0; $p -lt $len - 4; $p++) {
    if ($bytes[$p] -eq 82 -and $bytes[$p+1] -eq 83 -and $bytes[$p+2] -eq 68 -and $bytes[$p+3] -eq 83) {
        $end = if ($p + 24 -le $len) { $p + 24 } else { $len }
        for ($i = $p + 4; $i -lt $end; $i++) { $bytes[$i] = 0 }
        $s = $p + 24
        while ($s -lt $len -and $bytes[$s] -ne 0) { $bytes[$s] = 0; $s++ }
        Write-Host "[2/5] PDB stripped at 0x$($p.ToString('X'))" -ForegroundColor Green
        break
    }
}

if ($len -lt 64) { Write-Host "[PostBuildProtect] PE too short" ; exit 0 }
$pe = [BitConverter]::ToInt32($bytes, 0x3C)
if ($pe -le 0 -or $pe -gt $len - 64) {
    Write-Host "[PostBuildProtect] Bad PE offset 0x$($pe.ToString('X'))" -ForegroundColor Yellow
    exit 0
}
$tds = $pe + 8
for ($i = 0; $i -lt 4; $i++) { $bytes[$tds + $i] = [byte](Get-Random -Min 0 -Max 256) }
Write-Host "[3/5] TimeDateStamp randomized" -ForegroundColor Green
$mvid = -1
for ($i = 0; $i -lt $len - 4; $i++) {
    if ($bytes[$i] -eq 66 -and $bytes[$i+1] -eq 83 -and $bytes[$i+2] -eq 74 -and $bytes[$i+3] -eq 66) {
        $mvid = $i + 8
        break
    }
}
if ($mvid -gt 0 -and $mvid -lt $len - 16) {
    for ($i = 0; $i -lt 16; $i++) { $bytes[$mvid + $i] = [byte](Get-Random -Min 0 -Max 256) }
    Write-Host "[4/5] MVID randomized" -ForegroundColor Green
} else {
    Write-Host "[4/5] MVID not found (skip)" -ForegroundColor Yellow
}

if ($pe + 27 -lt $len) { $bytes[$pe + 26] = 0; $bytes[$pe + 27] = 0 }
# Major/MinorImageVersion @ PE+32
if ($pe + 35 -lt $len) { for ($z=0;$z -lt 4;$z++) { $bytes[$pe+32+$z] = 0 } }
# Major/MinorOSVersion @ PE+40
if ($pe + 43 -lt $len) { for ($z=0;$z -lt 4;$z++) { $bytes[$pe+40+$z] = 0 } }
Write-Host "[5/5] PE metadata obfuscated" -ForegroundColor Green

[System.IO.File]::WriteAllBytes($fullPath, $bytes)

exit 0
