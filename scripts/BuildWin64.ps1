param()

[string] $Product = "Mzinga"
[string] $Target = "Win64"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:TrimMode=link"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
