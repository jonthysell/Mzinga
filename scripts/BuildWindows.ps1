param()

[string] $Product = "Mzinga"
[string] $Target = "Windows"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
