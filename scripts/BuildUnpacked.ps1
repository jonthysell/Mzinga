param()

[string] $Product = "Mzinga"
[string] $Target = "Unpacked"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
