[CmdletBinding()]
param(
    [string]$ExpectedVersion = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$releaseRoot = Join-Path $repositoryRoot "Release"
$packageIds = @("com.fshader.core", "com.fshader.plus")
$manifests = @{}

foreach ($packageId in $packageIds) {
    $manifestPath = Join-Path $repositoryRoot "Packages/$packageId/package.json"
    $manifest = Get-Content -Raw -Encoding utf8 -LiteralPath $manifestPath | ConvertFrom-Json
    if ($manifest.name -ne $packageId) { throw "Package id mismatch: $manifestPath" }
    if (-not $manifest.author.email) { throw "author.email is required: $manifestPath" }
    if (-not $manifest.license) { throw "license is required: $manifestPath" }
    if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot "Packages/$packageId/fSHaderLicense.md"))) { throw "fSHaderLicense.md is required: $packageId" }
    $manifests[$packageId] = $manifest
}

$coreVersion = [string]$manifests["com.fshader.core"].version
$plusVersion = [string]$manifests["com.fshader.plus"].version
if ($coreVersion -ne $plusVersion) { throw "Core and Plus versions must match: $coreVersion / $plusVersion" }
if ($ExpectedVersion -and $coreVersion -ne $ExpectedVersion) { throw "Expected $ExpectedVersion but package version is $coreVersion" }
if ([string]$manifests["com.fshader.plus"].vpmDependencies."com.fshader.core" -ne $coreVersion) {
    throw "Plus must depend on Core $coreVersion"
}
if ([string]$manifests["com.fshader.plus"].vpmDependencies."at.pimaker.ltcgi" -ne ">=1.6.3 <1.7.0") {
    throw "Plus LTCGI range must remain >=1.6.3 <1.7.0"
}

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
$listingPackages = [ordered]@{}
$hashLines = [System.Collections.Generic.List[string]]::new()

foreach ($packageId in $packageIds) {
    $manifest = $manifests[$packageId]
    $version = [string]$manifest.version
    $zipName = "$packageId-$version.zip"
    $zipPath = Join-Path $releaseRoot $zipName
    if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }

    $sourcePath = Join-Path $repositoryRoot "Packages/$packageId"
    Compress-Archive -Path (Join-Path $sourcePath "*") -DestinationPath $zipPath -CompressionLevel Optimal
    $sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath).Hash.ToLowerInvariant()
    $hashLines.Add("$sha256  $zipName")

    $listingManifest = Get-Content -Raw -Encoding utf8 -LiteralPath (Join-Path $sourcePath "package.json") | ConvertFrom-Json
    $releaseUrl = "https://github.com/FuyutsukiNatsuki/fShader/releases/download/$packageId-$version/$zipName"
    $listingManifest | Add-Member -Force -NotePropertyName url -NotePropertyValue $releaseUrl
    $listingManifest | Add-Member -Force -NotePropertyName zipSHA256 -NotePropertyValue $sha256
    $listingPackages[$packageId] = [ordered]@{ versions = [ordered]@{ $version = $listingManifest } }
}

$index = [ordered]@{
    name = "fShader Packages"
    id = "com.fshader.packages"
    url = "https://fuyutsukinatsuki.github.io/fShader/index.json"
    author = [ordered]@{
        name = "FuyutsukiNatsuki"
        email = "FuyutsukiNatsuki@users.noreply.github.com"
        url = "https://github.com/FuyutsukiNatsuki"
    }
    packages = $listingPackages
}

[IO.File]::WriteAllText((Join-Path $releaseRoot "index.json"), (($index | ConvertTo-Json -Depth 30) + "`n"), [Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllLines((Join-Path $releaseRoot "SHA256SUMS.txt"), $hashLines, [Text.UTF8Encoding]::new($false))
Write-Host "Built fShader $coreVersion release artifacts in $releaseRoot"
