[string] $Product = "Mzinga"
[string] $Target = "MacOS"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true" -ProjectPath "src\$Product.Viewer\$Product.Viewer.csproj"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-t:Publish -p:RuntimeIdentifier=osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true" -Clean $False

# Write-Host "Removing unbundled $Product.Viewer"
# Remove-Item "$PSScriptRoot\..\bld\$Product.$Target\$Product.Viewer"
# Remove-Item "$PSScriptRoot\..\bld\$Product.$Target\$Product.Viewer.pdb"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
