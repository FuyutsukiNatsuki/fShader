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
    $packageRoot = Join-Path $repositoryRoot "Packages/$packageId"
    $manifestPath = Join-Path $packageRoot "package.json"
    $manifest = Get-Content -Raw -Encoding utf8 -LiteralPath $manifestPath | ConvertFrom-Json
    if ($manifest.name -ne $packageId) { throw "Package id mismatch: $manifestPath" }
    if (-not (Test-Path -LiteralPath (Join-Path $packageRoot "fSHaderLicense.md"))) { throw "fSHaderLicense.md is required: $packageId" }
    $missingMeta = Get-ChildItem -LiteralPath $packageRoot -Recurse -File | Where-Object { $_.Extension -ne ".meta" -and -not (Test-Path -LiteralPath ($_.FullName + ".meta")) }
    if ($missingMeta) { throw "Every UnityPackage asset requires a .meta file: $($missingMeta.FullName -join ', ')" }
    $manifests[$packageId] = $manifest
}

$coreVersion = [string]$manifests["com.fshader.core"].version
$plusVersion = [string]$manifests["com.fshader.plus"].version
if ($coreVersion -ne $plusVersion) { throw "Core and Plus versions must match: $coreVersion / $plusVersion" }
if ($ExpectedVersion -and $coreVersion -ne $ExpectedVersion) { throw "Expected $ExpectedVersion but package version is $coreVersion" }
if ([string]$manifests["com.fshader.plus"].vpmDependencies."com.fshader.core" -ne $coreVersion) { throw "Plus must depend on Core $coreVersion" }
if ([string]$manifests["com.fshader.plus"].vpmDependencies."at.pimaker.ltcgi" -ne ">=1.6.3 <1.7.0") { throw "Plus LTCGI range must remain >=1.6.3 <1.7.0" }

function New-UnityPackage {
    param(
        [Parameter(Mandatory = $true)][string]$PackageId,
        [Parameter(Mandatory = $true)][string]$OutputPath
    )
    $packageRoot = Join-Path $repositoryRoot "Packages/$PackageId"
    $stagingRoot = Join-Path $releaseRoot ".unitypackage-$PackageId"
    if (Test-Path -LiteralPath $stagingRoot) { Remove-Item -LiteralPath $stagingRoot -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $stagingRoot | Out-Null
    $seenGuids = @{}
    foreach ($metaFile in Get-ChildItem -LiteralPath $packageRoot -Recurse -File -Filter "*.meta") {
        $guidLine = Select-String -LiteralPath $metaFile.FullName -Pattern '^guid:\s*([0-9a-fA-F]+)\s*$' | Select-Object -First 1
        if (-not $guidLine) { throw "Unity GUID is missing: $($metaFile.FullName)" }
        $guid = $guidLine.Matches[0].Groups[1].Value.ToLowerInvariant()
        if ($seenGuids.ContainsKey($guid)) { throw "Duplicate Unity GUID: $guid" }
        $seenGuids[$guid] = $true
        $assetPath = $metaFile.FullName.Substring(0, $metaFile.FullName.Length - 5)
        $relativePath = $assetPath.Substring($repositoryRoot.Length + 1).Replace('\', '/')
        $entryRoot = Join-Path $stagingRoot $guid
        New-Item -ItemType Directory -Force -Path $entryRoot | Out-Null
        Copy-Item -LiteralPath $metaFile.FullName -Destination (Join-Path $entryRoot "asset.meta")
        [IO.File]::WriteAllText((Join-Path $entryRoot "pathname"), $relativePath, [Text.UTF8Encoding]::new($false))
        if (Test-Path -LiteralPath $assetPath -PathType Leaf) { Copy-Item -LiteralPath $assetPath -Destination (Join-Path $entryRoot "asset") }
    }
    if (Test-Path -LiteralPath $OutputPath) { Remove-Item -LiteralPath $OutputPath -Force }
    & tar.exe -czf $OutputPath -C $stagingRoot "."
    if ($LASTEXITCODE -ne 0) { throw "tar failed while building $OutputPath" }
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
$liteName = "fShader-Lite-$coreVersion.unitypackage"
$plusName = "fShader-Plus-$coreVersion.unitypackage"
$litePath = Join-Path $releaseRoot $liteName
$plusPath = Join-Path $releaseRoot $plusName
New-UnityPackage -PackageId "com.fshader.core" -OutputPath $litePath
New-UnityPackage -PackageId "com.fshader.plus" -OutputPath $plusPath

$boothStaging = Join-Path $releaseRoot ".booth-$coreVersion"
if (Test-Path -LiteralPath $boothStaging) { Remove-Item -LiteralPath $boothStaging -Recurse -Force }
New-Item -ItemType Directory -Force -Path $boothStaging | Out-Null
Copy-Item -LiteralPath $litePath -Destination $boothStaging
Copy-Item -LiteralPath $plusPath -Destination $boothStaging
Copy-Item -LiteralPath (Join-Path $repositoryRoot "fSHaderLicense.md") -Destination $boothStaging
Copy-Item -LiteralPath (Join-Path $repositoryRoot "THIRD_PARTY_NOTICES.md") -Destination $boothStaging
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "BOOTH_README_JA.md") -Destination (Join-Path $boothStaging "README_JA.md")

$boothHashLines = [System.Collections.Generic.List[string]]::new()
foreach ($fileName in @($liteName, $plusName, "fSHaderLicense.md", "THIRD_PARTY_NOTICES.md", "README_JA.md")) {
    $filePath = Join-Path $boothStaging $fileName
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $filePath).Hash.ToLowerInvariant()
    $boothHashLines.Add("$hash  $fileName")
}
[IO.File]::WriteAllLines((Join-Path $boothStaging "SHA256SUMS.txt"), $boothHashLines, [Text.UTF8Encoding]::new($false))

$boothZipName = "fShader-$coreVersion-Booth.zip"
$boothZipPath = Join-Path $releaseRoot $boothZipName
if (Test-Path -LiteralPath $boothZipPath) { Remove-Item -LiteralPath $boothZipPath -Force }
Compress-Archive -Path (Join-Path $boothStaging "*") -DestinationPath $boothZipPath -CompressionLevel Optimal
Remove-Item -LiteralPath $boothStaging -Recurse -Force

$releaseHashLines = [System.Collections.Generic.List[string]]::new()
foreach ($filePath in @($litePath, $plusPath, $boothZipPath)) {
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $filePath).Hash.ToLowerInvariant()
    $releaseHashLines.Add("$hash  $([IO.Path]::GetFileName($filePath))")
}
[IO.File]::WriteAllLines((Join-Path $releaseRoot "BOOTH_SHA256SUMS.txt"), $releaseHashLines, [Text.UTF8Encoding]::new($false))
Write-Host "Built fShader $coreVersion Booth artifacts in $releaseRoot"
