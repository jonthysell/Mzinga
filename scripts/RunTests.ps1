param(
    [string]$TestProject = "Mzinga.Test",
    [string]$TestArgs = ""
)

[string] $ProjectPath = "src\$TestProject\$TestProject.csproj"

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Testing $TestProject..."

dotnet test $TestArgs.Split() "$ProjectPath"

Set-Location -Path "$StartingLocation"
