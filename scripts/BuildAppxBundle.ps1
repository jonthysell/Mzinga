param(
    [string]$Product,
    [string]$Target,
    [boolean]$Clean = $True,
    [string]$BuildArgs = "",
    [string]$ProjectPath = "src\$Product.$Target\$Product.$Target.wapproj",
    [string]$PackageCertificateKeyFile = "$Product.$Target_TemporaryKey.pfx"
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

Write-Host "Build appxbundle..."
try
{
    New-Item -Path "$OutputRoot\$TargetOutputDirectory" -ItemType "directory"
    msbuild $BuildArgs.Split() -restore -p:Configuration=Release -p:AppxBundle=Always -p:UapAppxPackageBuildMode=StoreUpload -p:RestoreForWinStore=true -p:PackageCertificateKeyFile="$PackageCertificateKeyFile" -p:AppxPackageDir="$RepoRoot\$OutputRoot\$TargetOutputDirectory" "$ProjectPath"
    if (!$?) {
    	throw 'Build failed!'
    }
}
finally
{
    Set-Location -Path "$StartingLocation"
}
