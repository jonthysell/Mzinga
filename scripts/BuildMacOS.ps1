[string] $Product = "Mzinga"
[string] $Target = "MacOS"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Mzinga_Engine:Publish;Mzinga_Trainer:Publish;Mzinga_Perft:Publish -p:RuntimeIdentifier=osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
