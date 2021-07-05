param()

[string] $Product = "Mzinga"
[string] $Target = "Linux"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:TrimMode=link"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
