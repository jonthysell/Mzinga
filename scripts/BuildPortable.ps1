param()

[string] $Product = "Mzinga"
[string] $Target = "Portable"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-p:Configuration=Release -target:Publish"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
