$ErrorActionPreference = "Continue"

$Tag = git describe --tags --abbrev=0 --match v[0-9]* HEAD 2>&1
if ($LASTEXITCODE -ne 0) {
    $Tag = "v0.0"
}

$Version = [Version]::Parse($Tag.TrimStart('v'))
if ($Version.Build -lt 0 -and $env:GITHUB_RUN_NUMBER) {
    $Version = [Version]::new($Version.Major, [Math]::Max($Version.Minor, 0), $env:GITHUB_RUN_NUMBER)
}

Write-Output "BUILD_VERSION=$Version" >> $env:GITHUB_ENV
$Version.ToString()
exit 0