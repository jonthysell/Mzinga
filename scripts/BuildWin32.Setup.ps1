param()

[string] $Product = "MzingaViewer"
[string] $Target = "Win32.Setup"

& "$PSScriptRoot\BuildSetup.ps1" -Product $Product -Target $Target -BuildArgs "-p:Platform=x86" -ProjectPath "src\Mzinga.Viewer.Setup\Mzinga.Viewer.Setup.wixproj"
