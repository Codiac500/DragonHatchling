param([string]$Dotnet = 'dotnet')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
Push-Location $repo
try {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    & $Dotnet publish src/DragonHatchling.Desktop/DragonHatchling.Desktop.csproj `
        -c Release -r win-x64 --self-contained true -o artifacts/win-x64
    if ($LASTEXITCODE -ne 0) { throw "Publish failed: $LASTEXITCODE" }
} finally {
    Pop-Location
}
