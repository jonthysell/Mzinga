param()

[string] $Product = "Mzinga"
[string] $Target = "LinuxX64"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
