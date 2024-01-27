param()

[string] $Product = "Mzinga"
[string] $Target = "WinX64"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
