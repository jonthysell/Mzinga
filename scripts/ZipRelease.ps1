param([string]$Product, [string]$Target)

[string] $OutputRoot = "bld"
[string] $TargetOutputDirectory = "$Product.$Target"
[string] $TargetOutputPackageName = "$Product.$Target.zip"

$StartingLocation = Get-Location
Set-Location -Path "$PSScriptRoot\.."

if (Test-Path "$OutputRoot\$TargetOutputPackageName") {
    Write-Host "Remove old package..."
    Remove-Item "$OutputRoot\$TargetOutputPackageName"
}

Set-Location -Path "$OutputRoot"

if (-Not (Test-Path ".\7za.exe")) {
    Write-Host "Get 7za.exe..."
    curl https://www.7-zip.org/a/7zr.exe -o 7zr.exe 
    curl https://www.7-zip.org/a/7z1900-extra.7z -o 7z1900-extra.7z
    .\7zr.exe e 7z1900-extra.7z -y x64\7za.exe
    Remove-Item 7zr.exe
    Remove-Item 7z1900-extra.7z
}

Write-Host "Create package..."
.\7za.exe a "$TargetOutputPackageName" "$TargetOutputDirectory" -tzip -mx9

Set-Location -Path "$StartingLocation"
