param()

[string] $Product = "MzingaViewer"
[string] $Target = "WinArm64.Setup"

& "$PSScriptRoot\BuildSetup.ps1" -Product $Product -Target $Target -BuildArgs "-p:Platform=ARM64" -ProjectPath "src\Mzinga.Viewer.Setup\Mzinga.Viewer.Setup.wixproj"
