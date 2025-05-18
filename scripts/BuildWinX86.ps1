param()

[string] $Product = "Mzinga"
[string] $Target = "WinX86"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=win-x86 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:SelfContained=true"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
