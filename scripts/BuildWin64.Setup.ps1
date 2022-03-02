param()

[string] $Product = "MzingaViewer"
[string] $Target = "Win64.Setup"

& "$PSScriptRoot\BuildSetup.ps1" -Product $Product -Target $Target -BuildArgs "-p:Platform=x64" -ProjectPath "src\Mzinga.Viewer.Setup\Mzinga.Viewer.Setup.wixproj"
