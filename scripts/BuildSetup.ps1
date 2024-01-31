param(
    [string]$Product,
    [string]$Target,
    [boolean]$Clean = $True,
    [string]$BuildArgs = "",
    [string]$ProjectPath = "src\$Product.$Target\$Product.$Target.wixproj"
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

[string] $OutputRoot = "bld"
[string] $TargetOutputDirectory = "$Product.$Target"

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

if ($Clean -and (Test-Path "$OutputRoot\$TargetOutputDirectory")) {
    Write-Host "Clean output folder..."
    Remove-Item "$OutputRoot\$TargetOutputDirectory" -Recurse
}

Write-Host "Build setup..."
try
{
    New-Item -Path "$OutputRoot\$TargetOutputDirectory" -ItemType "directory"
    dotnet msbuild $BuildArgs.Split() -restore -p:Configuration=Release -p:OutputPath="$RepoRoot\$OutputRoot\$TargetOutputDirectory" "$ProjectPath"
    if (!$?) {
    	throw 'Build failed!'
    }
}
finally
{
    Set-Location -Path "$StartingLocation"
}
