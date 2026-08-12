$ErrorActionPreference = "Stop"

# Helper to print colored text
function Write-Color($Text, $Color) {
    Write-Host $Text -ForegroundColor $Color
}

Write-Color "=========================================================================" "Green"
Write-Color "  Assignment & Submission Management System - Startup Launcher" "Green"
Write-Color "=========================================================================" "Green"
Write-Host ""

$ProjectDir = $PSScriptRoot
$LocalEnvDir = Join-Path $ProjectDir ".local_env"
$NodeLocalDir = Join-Path $LocalEnvDir "node"
$DotnetLocalDir = Join-Path $LocalEnvDir "dotnet"

# Helper to check Node version
function Get-NodeVersion {
    try {
        $nodeVerStr = node -v 2>$null
        if ($nodeVerStr -match "v(\d+)\.(\d+)\.(\d+)") {
            return [version]"$($matches[1]).$($matches[2]).$($matches[3])"
        }
    } catch {}
    return $null
}

# Helper to check Dotnet version
function Get-DotnetVersion {
    try {
        $dotnetCmd = "dotnet"
        $localDotnet = "$env:LocalAppData\Microsoft\dotnet\dotnet.exe"
        if (Test-Path $localDotnet) {
            $dotnetCmd = $localDotnet
        }
        $dotnetVerStr = & $dotnetCmd --version 2>$null
        if ($dotnetVerStr -match "^(\d+)\.(\d+)\.(\d+)") {
            return [version]"$($matches[1]).$($matches[2]).$($matches[3])"
        }
    } catch {}
    return $null
}

# 1. Version Detection
Write-Host "Checking System Requirements..."
$nodeReq = [version]"18.18.0"
$dotnetReq = [version]"8.0.0"

$currentNode = Get-NodeVersion
$currentDotnet = Get-DotnetVersion

$mismatch = $false
$nodeMismatch = $false
$dotnetMismatch = $false

if ($null -eq $currentNode -or $currentNode -lt $nodeReq) {
    $nodeMismatch = $true
    $mismatch = $true
    Write-Color "[!] Node.js version mismatch. Found: $($currentNode -replace '^$','None'). Required: >= $nodeReq" "Yellow"
} else {
    Write-Host "[OK] Node.js version $currentNode"
}

if ($null -eq $currentDotnet -or $currentDotnet.Major -ne $dotnetReq.Major) {
    $dotnetMismatch = $true
    $mismatch = $true
    Write-Color "[!] .NET SDK version mismatch. Found: $($currentDotnet -replace '^$','None'). Required: 8.x.x" "Yellow"
} else {
    Write-Host "[OK] .NET SDK version $currentDotnet"
}

Write-Host ""

# 2. Local Setup Logic
$useLocal = $false

if ($mismatch) {
    Write-Color "WARNING: Version mismatch detected. Running the project with your current system setup may fail." "Red"
    Write-Host "Would you like to install the required versions locally in this project folder?"
    Write-Host "This will NOT touch your system OS. It acts like an isolated Virtual Environment."
    $response = Read-Host "Install locally? (Y/N)"

    if ($response -match "^[Yy]") {
        $useLocal = $true
        if (!(Test-Path $LocalEnvDir)) {
            New-Item -ItemType Directory -Path $LocalEnvDir | Out-Null
        }

        if ($nodeMismatch) {
            Write-Host "`nDownloading Portable Node.js v20 LTS..."
            $nodeZipUrl = "https://nodejs.org/dist/v20.11.1/node-v20.11.1-win-x64.zip"
            $nodeZipPath = Join-Path $LocalEnvDir "node.zip"
            Invoke-WebRequest -Uri $nodeZipUrl -OutFile $nodeZipPath
            
            Write-Host "Extracting Node.js..."
            Expand-Archive -Path $nodeZipPath -DestinationPath $LocalEnvDir -Force
            Rename-Item -Path (Join-Path $LocalEnvDir "node-v20.11.1-win-x64") -NewName "node"
            Remove-Item $nodeZipPath
            Write-Color "Local Node.js setup complete." "Green"
        }

        if ($dotnetMismatch) {
            Write-Host "`nDownloading .NET 8 SDK installer..."
            $dotnetScriptUrl = "https://dot.net/v1/dotnet-install.ps1"
            $dotnetScriptPath = Join-Path $LocalEnvDir "dotnet-install.ps1"
            Invoke-WebRequest -Uri $dotnetScriptUrl -OutFile $dotnetScriptPath

            Write-Host "Installing local .NET SDK (This might take a minute)..."
            & $dotnetScriptPath -InstallDir $DotnetLocalDir -Channel 8.0 -NoPath
            Write-Color "Local .NET SDK setup complete." "Green"
        }
    } else {
        Write-Color "`n[!] Proceeding with system versions despite mismatch. Expect potential build errors in red text!" "Red"
        Start-Sleep -Seconds 3
    }
} else {
    # Even if no mismatch, check if local env exists and use it if it's there
    if ((Test-Path $NodeLocalDir) -or (Test-Path $DotnetLocalDir)) {
        $useLocal = $true
    }
}

# 3. Path Overriding (if using local env)
$ActiveDotnetCmd = "dotnet"

if ($useLocal) {
    Write-Host "`nInjecting local environment variables..."
    $envPathPrefix = ""
    
    if (Test-Path $NodeLocalDir) {
        $envPathPrefix += "$NodeLocalDir;"
    }
    
    if (Test-Path $DotnetLocalDir) {
        $envPathPrefix += "$DotnetLocalDir;"
        $env:DOTNET_ROOT = $DotnetLocalDir
        $ActiveDotnetCmd = Join-Path $DotnetLocalDir "dotnet.exe"
    }

    $env:PATH = "$envPathPrefix$env:PATH"
    Write-Color "Running strictly within Isolated Project Environment." "Cyan"
} else {
    # Check LocalAppData fallback (used in original run.bat)
    $localDotnet = "$env:LocalAppData\Microsoft\dotnet\dotnet.exe"
    if (Test-Path $localDotnet) {
        $ActiveDotnetCmd = $localDotnet
    }
}

Write-Host ""
Write-Host "[1/3] Launching ASP.NET Core Backend API Server (Port 5000)..."
if (!(Test-Path $LocalEnvDir)) { New-Item -ItemType Directory -Path $LocalEnvDir | Out-Null }
$backendScript = Join-Path $LocalEnvDir "run_backend.cmd"
Set-Content -Path $backendScript -Value "@echo off`nset PATH=$env:PATH`nset DOTNET_ROOT=$env:DOTNET_ROOT`ncd /d `"$ProjectDir\backend`"`n`"$ActiveDotnetCmd`" run --project AssignmentSubmission.Api`npause"
Start-Process -FilePath $backendScript -WindowStyle Normal

Write-Host "[2/3] Launching Next.js Frontend Web Application (Port 3000)..."
$frontendScript = Join-Path $LocalEnvDir "run_frontend.cmd"
Set-Content -Path $frontendScript -Value "@echo off`nset PATH=$env:PATH`ncd /d `"$ProjectDir\frontend`"`nnode node_modules\next\dist\bin\next dev`npause"
Start-Process -FilePath $frontendScript -WindowStyle Normal

Write-Host ""
Write-Host "[3/3] Waiting for servers to initialize (8 seconds)..."
Start-Sleep -Seconds 8

Write-Host ""
Write-Host "Opening Application in your default web browser..."
Start-Process "http://localhost:3000"

Write-Host ""
Write-Color "=========================================================================" "Green"
Write-Color "  SUCCESS! The system is now up and running cleanly." "Green"
Write-Color " " "Green"
Write-Color "  - Web Application UI : http://localhost:3000" "Green"
Write-Color "  - Backend REST API   : http://localhost:5000/api" "Green"
Write-Color "  - Swagger API Specs  : http://localhost:5000/swagger" "Green"
Write-Color "=========================================================================" "Green"
Write-Host ""
Write-Host "Press any key to exit..."
$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null
