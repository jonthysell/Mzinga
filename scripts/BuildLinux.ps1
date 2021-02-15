param()

[string] $Product = "Mzinga"
[string] $Target = "Linux"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-p:RuntimeIdentifier=linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
