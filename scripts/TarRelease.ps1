param([string]$Product, [string]$Target)

[string] $OutputRoot = "bld"
[string] $TargetOutputDirectory = "$Product.$Target"
[string] $TargetOutputPackageName = "$Product.$Target.tar.gz"

$StartingLocation = Get-Location
Set-Location -Path "$PSScriptRoot\.."

if (Test-Path "$OutputRoot\$TargetOutputPackageName") {
    Write-Host "Remove old package..."
    Remove-Item "$OutputRoot\$TargetOutputPackageName"
}

Set-Location -Path "$OutputRoot"

Write-Host "Create package..."
tar -zcvf "$TargetOutputPackageName" "$TargetOutputDirectory"

Set-Location -Path "$StartingLocation"
