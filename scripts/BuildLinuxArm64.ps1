param()

[string] $Product = "Mzinga"
[string] $Target = "LinuxArm64"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=linux-arm64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:SelfContained=true"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
