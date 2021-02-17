[string] $Product = "Mzinga"
[string] $Target = "MacOS"

# Disabled creating the app bundle until crash is fixed: https://github.com/jonthysell/Mzinga/issues/100
# & "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true" -ProjectPath "src\$Product.Viewer\$Product.Viewer.csproj"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-t:Publish -p:RuntimeIdentifier=osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
