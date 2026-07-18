# Changelog
## 1.0.0 - 2026-07-18

- Prepared the first fShader Plus release and updated the Core dependency to 1.0.0 through VPM metadata.
- Added Plus setup/LTCGI documentation, package license, starter materials, and gallery scene.
- Retained LTCGI `>=1.6.3 <1.7.0`, standard one-ForwardBase shaders, and Screen Refraction/LTCGI OFF defaults.
- Runtime Plus shader code remains equivalent to the P5.1-validated 0.6.1 package.
## 1.0.0 - 2026-07-18

- Prepared the first fShader Plus release and updated the Core dependency to 1.0.0 through VPM metadata.
- Added Plus setup/LTCGI documentation, package license, starter materials, and gallery scene.
- Retained LTCGI `>=1.6.3 <1.7.0`, standard one-ForwardBase shaders, and Screen Refraction/LTCGI OFF defaults.
- Runtime Plus shader code remains equivalent to the P5.1-validated 0.6.1 package.
## 0.6.1 - 2026-07-18

- Updated the Core dependency to 0.6.1.
- Added generated QA-only Water/Glass refraction OFF/ON comparison materials; runtime Plus shader code is unchanged from 0.6.0.

## 0.6.0 - 2026-07-18

- Applied stereo-safe screen refraction and VRChat mirror-aware camera handling to Plus Water, Ice, Glass, and LTCGI paths.
- Added P5 variant/build regression coverage while preserving one ForwardBase pass per public Plus shader.
## 0.5.0 - 2026-07-18

- Added external LTCGI 1.6.3 dependency metadata and API v2 integration for Plus Water and Glass.
- Pinned LTCGI below 1.7.0 to avoid the optional VRC Light Volumes adapter ProgramAsset error when VRC Light Volumes is not installed.
- Added separate diffuse/specular strengths, roughness-aware LTCGI input, brightness clamping, and Glass condensation diffuse boost.
- Added `LTCGI=_LTCGI` shader tags and a local OFF variant that excludes LTCGI includes and calculations.
- Kept standard Plus materials at one ForwardBase pass and LTCGI OFF by default.
- Added third-party notices and retained LTCGI as an official external package without copying its source or assets.



## 0.4.0 - 2026-07-18

- Implemented Plus Water with independent dual normals, up to four world-space waves, absorption, multi-scale foam, crest emphasis, surface caustics, and box-projected probes.
- Implemented Plus Ice with view-angle absorption, two-scale frost, parallax cracks, internal color, back light, and expanded sparkle controls.
- Implemented Plus Glass with packed droplet/trail/micro-fog condensation, local surface controls, distance fade, absorption, and box-projected probes.
- Added optional Water and Glass screen-refraction Hidden shaders using the shared named GrabPass; standard Plus shaders remain GrabPass-free.
- Kept all three standard Plus variants at one ForwardBase pass and one reflection cubemap sample.
- Updated the Core dependency to 0.4.0.

## 0.3.0 - 2026-07-18

- Updated the Core dependency to 0.3.0.
- Plus shaders continue to use the shared PBR layer while Plus-specific mode features remain scheduled for P3.

## 0.2.0 - 2026-07-18

- Applied the shared P1 PBR material contract to Plus Water, Ice, and Glass.
- Retained a single ForwardBase pass and local material keywords.
- Added BRP lightmap, light probe, vertex-light, and reflection-probe support through Core.

## 0.1.0 - 2026-07-18

- Added the P0 embedded package foundation.
- Added Plus Water, Ice, and Glass shader entry points.
