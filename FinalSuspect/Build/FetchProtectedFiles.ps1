<#
.SYNOPSIS
    Fetch protected files from GitHub private repo.
    Called by MSBuild PreBuild. exit 0 on success, exit 1 on failure.
#>

param([string]$ProjectDir)

# ---- Paths ----
$tokenFile       = "$ProjectDir\Build\token.env"
$localOfflineDir = "$ProjectDir\Build\.protected"
$githubBase      = "https://raw.githubusercontent.com/NewLezaiYa/FS_BUILD_ASSETS/main"

# ---- Expected files ----
$files = @(
    @{ dest = "$ProjectDir\DataHandling\FinalAntiCheat\Core\ExeChecker.cs"; name = "ExeChecker.cs"; hash = "12A24CFA78D59E9AE5F99B87B913439630F122FF224812539D2E1CC4C0C8D9FE" }
    @{ dest = "$ProjectDir\DataHandling\FinalAntiCheat\Core\DllChecker.cs"; name = "DllChecker.cs"; hash = "83E838B7B5BD139A4120850D6171F2C6257177DE1C72D30BEA0E1B433C3DD096" }
    @{ dest = "$ProjectDir\Build\PostBuildProtect.ps1";           name = "PostBuildProtect.ps1"; hash = "F3CC24E52FF14E39FB6829E66FD9E8DBDBF5A9D46D276882CDF413E35BE68240" }
)

# ---- Read token ----
$token = $null
$tokenSource = $null

if ($env:FS_ASSETS_TOKEN) {
    $token = $env:FS_ASSETS_TOKEN
    $tokenSource = "environment variable"
} elseif (Test-Path $tokenFile) {
    $content = (Get-Content -Path $tokenFile -Raw) -replace "`r", ""
    if ($content -match 'FS_ASSETS_TOKEN\s*=\s*(.+?)\s*$') {
        $t = $Matches[1].Trim().Trim('"').Trim("'").Trim()
        if ($t -and $t -ne 'ghp_xxxxxxxxxxxx') { $token = $t; $tokenSource = "token.env" }
    }
}

if (-not $token) {
    Write-Host "   BUILD ERROR: Need GitHub Token"
    exit 1
}

# ---- SHA256 Helper ----
$sha256 = [System.Security.Cryptography.SHA256]::Create()
function Check-Hash($path, $expected) {
    if (-not (Test-Path $path)) { return $false }
    try {
        $bytes = [System.IO.File]::ReadAllBytes($path)
        $hashBytes = $sha256.ComputeHash($bytes)
        $hashStr = [System.BitConverter]::ToString($hashBytes).Replace('-', '').ToUpper()
        return ($hashStr -eq $expected)
    } catch { return $false }
}

# ---- Process ----
foreach ($f in $files) {
    if (Check-Hash $f.dest $f.hash) { continue }

    # Strategy 1: local cache
    $cache = "$localOfflineDir\$($f.name)"
    if ((Test-Path $cache) -and (Check-Hash $cache $f.hash)) {
        Copy-Item $cache $f.dest -Force
        if (Check-Hash $f.dest $f.hash) { continue }
    }

    # Strategy 2: GitHub download
    try {
        Invoke-WebRequest -Uri "$githubBase/$($f.name)" -Headers @{Authorization="Bearer $token"} -OutFile $f.dest -UseBasicParsing
    } catch {
        Write-Host "[FetchProtectedFiles] GitHub download failed for $($f.name)"
    }

    if (Check-Hash $f.dest $f.hash) {
        New-Item -ItemType Directory -Path $localOfflineDir -Force -ErrorAction SilentlyContinue | Out-Null
        Copy-Item $f.dest $cache -Force -ErrorAction SilentlyContinue
        continue
    }

    Write-Host "[FetchProtectedFiles] FATAL: Cannot obtain $($f.name)"
    exit 1
}

exit 0
