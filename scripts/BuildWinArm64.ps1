param()

[string] $Product = "Mzinga"
[string] $Target = "WinArm64"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=win-arm64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
