[string] $Product = "Mzinga"
[string] $Target = "MacOSArm64"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-t:BundleApp -p:RuntimeIdentifier=osx-arm64 -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:SelfContained=true" -ProjectPath "src\$Product.Viewer\$Product.Viewer.csproj"

# Remove everything except the app bundle
Get-Childitem "$PSScriptRoot\..\bld\$Product.$Target\" -Exclude "MzingaViewer.app" | Remove-Item -Recurse

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -Clean $False -BuildArgs "-t:Publish -p:RuntimeIdentifier=osx-arm64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:SelfContained=true"

# Remove unbundled MzingaViewer
Remove-Item "$PSScriptRoot\..\bld\$Product.$Target\MzingaViewer"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
