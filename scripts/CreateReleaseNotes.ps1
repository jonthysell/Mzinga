param()

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

[string] $OutputRoot = "bld"

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Getting release notes..."
try
{
    $ChangelogLines = Get-Content "CHANGELOG.md"

    $LastReleaseHeaders = $ChangelogLines | Select-String -Pattern "^## .* ##$" | Select -First 2

    $FirstIndex = [array]::IndexOf($ChangelogLines, $LastReleaseHeaders[0]) + 1
    $LastIndex = [array]::IndexOf($ChangelogLines, $LastReleaseHeaders[1]) - 1

    $ReleaseNotes = $ChangelogLines[$FirstIndex..$LastIndex] -Match "\*"

    $ReleaseNotes += ""
    $ReleaseNotes += "See the [Read Me](./README.md) for installation instructions and the [Changelog](./CHANGELOG.md) for the complete change history."

    $ReleaseNotes | ForEach-Object { Write-Host $_ }

    if (-not (Test-Path "$OutputRoot")) {
        New-Item "$OutputRoot" -Type Directory | Out-Null
    }

    Set-Content "$OutputRoot\ReleaseNotes.md" $ReleaseNotes
}
finally
{
    Set-Location -Path "$StartingLocation"
}
