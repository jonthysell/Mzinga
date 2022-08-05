param()

[string] $Product = "MzingaViewer"
[string] $Target = "WinStore"

& "$PSScriptRoot\BuildAppxBundle.ps1" -Product $Product -Target $Target -ProjectPath "src\Mzinga.Viewer.WinStore\Mzinga.Viewer.WinStore.wapproj" -PackageCertificateKeyFile "Mzinga.Viewer.WinStore_TemporaryKey.pfx"
