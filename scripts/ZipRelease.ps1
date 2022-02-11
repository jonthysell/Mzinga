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

Write-Host "Create package..."

Compress-Archive -Path "$TargetOutputDirectory" -DestinationPath "$TargetOutputPackageName"

Set-Location -Path "$StartingLocation"
