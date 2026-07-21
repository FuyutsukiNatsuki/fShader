# Changelog

## 1.1.0 - 2026-07-21

- Added a new opaque Standard mode (PBR textures, reflection, and lighting only, with no transparency or mode-specific effects) alongside Water, Ice, and Glass.
- Lite Standard ships FORWARD, ShadowCaster, and Meta passes so it casts shadows and bakes into lightmaps.
- Added a one-click Template section to the Inspector that switches the shader and assigns the matching Gallery sample textures, importing the sample automatically when needed.
- Extended the mode toolbar, keyword/render-state synchronization, and shader catalog to cover Standard.

## 1.0.1 - 2026-07-21

- Added opt-in transparent rendering for Lite and Plus Ice while preserving Opaque as the default.
- Made Ice Opacity functional in Transparent mode and increased opacity locally for thickness, Frost, and Cracks.
- Added Inspector render-state synchronization, transparent sorting/overdraw guidance, and QA comparison objects.
- Disabled Lite Ice ShadowCaster and Meta passes while Transparent mode is active.

## 1.0.0 - 2026-07-18

- Prepared the first fShader release for Unity 2022.3.22f1 and VRChat SDK Worlds 3.10.4.
- Added Japanese User Manual, complete Shader Property reference, release checklist, package licenses, and VPM metadata.
- Added importable Lite starter materials and a gallery scene alongside the existing sample textures.
- Added local ZIP/SHA-256/index generation and GitHub Actions for Core/Plus releases and GitHub Pages listing.
- Preserved the P5.1 runtime shader implementation and one-pass/optional-heavy-feature contracts.
## 1.0.0 - 2026-07-18

- Prepared the first fShader release for Unity 2022.3.22f1 and VRChat SDK Worlds 3.10.4.
- Added Japanese User Manual, complete Shader Property reference, release checklist, package licenses, and VPM metadata.
- Added importable Lite starter materials and a gallery scene alongside the existing sample textures.
- Added local ZIP/SHA-256/index generation and GitHub Actions for Core/Plus releases and GitHub Pages listing.
- Preserved the P5.1 runtime shader implementation and one-pass/optional-heavy-feature contracts.
## 0.6.1 - 2026-07-18

- Added a high-contrast Water/Glass refraction OFF/ON diagnostic stage to the generated P5 QA scene.
- Added dedicated per-panel measurement markers, a deterministic diagnostic normal, and a third `P5_Refraction_AB.png` golden image.
- Added the Japanese VRChat measurement guide and expanded benchmark result templates.
- Disabled VRChat Mirror objects only while capturing deterministic golden images, then restored them.
- Updated P5 QA regression coverage to 67 EditMode tests.

## 0.6.0 - 2026-07-18

- Added stereo-safe screen refraction sampling and VRChat mirror-aware camera helpers.
- Added conservative shader variant stripping, P5 QA scene generation, golden capture, and VRChat SDK build evidence.
- Added P5 VR/VRChat runbook and benchmark result records.
## 0.5.0 - 2026-07-18

- Added the P4 LTCGI Inspector section with package-version, Controller, shader-tag, and Heavy-combination validation.
- Added the one-pass Cold Mist Plus shader and editor-only generator with bounded 64-particle defaults and low-cost noise.
- Added P4 LTCGI/effect/package tests and made the test asmdef explicitly use Unity Test Assemblies.
- Updated the shader/material version contract and catalog to 0.5.0-p4.
- Added LTCGI third-party notices and P4 handoff documentation.



## 0.4.0 - 2026-07-18

- Added Plus-mode Inspector UI, Balanced/Showcase presets, cost estimates, mesh-density and heavy-combination warnings.
- Added safe Lite/Plus keyword isolation and Hidden Plus screen-shader catalog support.
- Added box-projected reflection direction support to the shared BRP lighting layer.
- Added P3 contract, pass, heavy-dependency, screen-refraction, catalog, and Lite-to-Plus migration tests.
- Updated the P0 pass contract to account for Lite Ice ShadowCaster and Meta utility passes.

## 0.3.0 - 2026-07-18

- Added Lite Water two-direction wave normals, optional two-wave world-space vertex motion, Fresnel, foam, and probe distortion.
- Added Lite Ice thickness tint, frost, cracks, wrapped-light scattering, distance-faded sparkle, ShadowCaster, and Meta passes.
- Added Lite Glass transmission, probe distortion, packed condensation animation, and droplet normal support.
- Added the optional named shared GrabPass Lite Glass screen-refraction shader; standard Lite materials remain GrabPass-free.
- Added the one-pass unlit Cold Mist shader and editor-only Cold Mist Lite setup wizard with 24-particle defaults.
- Added six Lite mode presets, vertex-color channel debug views, mode feature UI, keyword synchronization, cost summaries, and P2 tests.

## 0.2.0 - 2026-07-18

- Added the shared Base Map, ARMH, Normal Map, opacity, reflection, and debug-view contract.
- Added compact GGX direct lighting, light probes/SH, lightmaps, vertex lights, and reflection probes.
- Added local shader keywords while retaining one ForwardBase pass per public shader.
- Added the bilingual foldout inspector, import validation, quick PBR presets, and cost summary.
- Added the ARMH Texture Packer and P1 editor tests.

## 0.1.0 - 2026-07-18

- Added the P0 embedded package foundation.
- Added Lite Water, Ice, and Glass shader entry points.
- Added the shared minimal forward include and custom inspector shell.
- Added editor tests and benchmark documentation.
