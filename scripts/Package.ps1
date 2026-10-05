param([string]$Dotnet = 'dotnet')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
Push-Location $repo
try {
    # A fresh publish directory prevents stale files entering the delivery.
    $folder = "$repo/artifacts/package-$([Guid]::NewGuid().ToString('N'))/win-x64"
    & "$PSScriptRoot/Build.ps1" -Dotnet $Dotnet -OutputPath $folder
    # Put tester instructions beside the EXE; package only published files.
    Copy-Item -LiteralPath "$repo/docs/PROTOTYPE_RUN_NOTES.md" -Destination "$folder/RUN_NOTES.md"
    $archive = "$repo/artifacts/DragonHatchling-win-x64.zip"
    Compress-Archive -Path $folder -DestinationPath $archive -Force
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
    Set-Content -LiteralPath "$archive.sha256" -Value "$hash  DragonHatchling-win-x64.zip" -Encoding ascii
    Write-Output "Delivery: $archive"
    Write-Output "SHA256: $hash"
} finally {
    Pop-Location
}
