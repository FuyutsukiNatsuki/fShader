# fShader 1.2.2 Shader Property Reference

公開Shader名とProperty名は1.x系の安定契約です。Propertyの直接操作は可能ですが、Toggleとlocal keyword、Screen Refraction用Hidden Shaderの同期が必要なため、通常はfShader Inspectorを使用してください。

## Shader名

| Edition | Water | Ice | Glass | Standard |
|---|---|---|---|---|
| Lite | `fShader/Lite/Water` | `fShader/Lite/Ice` | `fShader/Lite/Glass` | `fShader/Lite/Standard` |
| Plus | `fShader/Plus/Water` | `fShader/Plus/Ice` | `fShader/Plus/Glass` | `fShader/Plus/Standard` |

Screen Refraction ON時はInspectorが`Hidden/fShader/Lite/GlassScreenRefraction`、`Hidden/fShader/Plus/WaterScreenRefraction`、`Hidden/fShader/Plus/GlassScreenRefraction`へ切り替えます。Hidden Shaderを直接選択しないでください。

## 共通Property

| Property | 型 | 意味 |
|---|---|---|
| `_BaseMap` | 2D sRGB | Base Color texture |
| `_BaseColor` | Color | Base Map乗算色。Water/Glassはalphaも使用 |
| `_ARMHMap` | 2D Linear | R=AO、G=Roughness、B=Metallic、A=Height |
| `_AOStrength` | 0–1 | 間接拡散AO強度 |
| `_Roughness` | 0.02–1 | ARMH未使用部の粗さ |
| `_Metallic` | 0–1 | ARMH未使用部の金属度 |
| `_NormalMap` | Normal Map | 共通法線 |
| `_NormalScale` | 0–2 | 共通法線強度 |
| `_HeightScale` | 0–0.1 | Mode固有のHeight補助 |
| `_Opacity` | 0–1 | 表面不透明度 |
| `_ReflectionStrength` | 0–2 | Reflection Probe寄与 |
| `_IOR` | 1–2.5 | Fresnel/屈折近似用IOR |
| `_FSVertexColor` | Toggle | Mode別Vertex Color契約を有効化 |
| `_FSDebugView` | Float | Inspector管理のDebug View |
| `_FSQueueOverride` | Toggle | 全fShader Shaderに存在。カスタムレンダーキュー有効化。OFF時はMode別の自動キュー（不透明2000 / 透過3000）、ON時はMaterialのRender Queueを絶対値（0–5000）で使用 |

レンダーキューは既定でMode別に自動決定され（不透明2000 / 透過3000）、`_FSQueueOverride`をONにした時だけMaterialのRender Queueを絶対値として使用します。

## Water

| Property | Edition | 意味 |
|---|---|---|
| `_ShallowColor`, `_DeepColor` | 両方 | 浅部/深部色 |
| `_FSWaterWaveNormal` | 両方 | Wave Normal機能 |
| `_WaveNormalMap` | 両方 | Wave Normal A |
| `_WaveNormalMap2` | Plus | Wave Normal B |
| `_WaveNormalScale`, `_WaveNormalScale2` | 両方/Plus | Normal強度 |
| `_WaveScaleA`, `_WaveScaleB` | Plus | 2層UV scale |
| `_WaveSpeedA`, `_WaveSpeedB` | 両方 | XYスクロール速度 |
| `_FSWaterVertexWaves` | 両方 | World Space頂点波 |
| `_WaveCount` | Plus | 1–4波 |
| `_WaveAmplitude`, `_WaveLength`, `_WaveDirection` | 両方 | 頂点波形状 |
| `_WaveTimeScale` | Plus | 波時間倍率 |
| `_FresnelStrength` | 両方 | 視線角反射強度 |
| `_FSWaterFoam`, `_FoamMap`, `_FoamStrength` | 両方 | Foam機能 |
| `_FoamColor`, `_FoamDetailScale`, `_FoamCrestStrength` | Plus | Plus Foam追加制御 |
| `_FSWaterCaustics`, `_CausticsMap`, `_CausticsColor`, `_CausticsStrength` | Plus | 表面Caustics |
| `_AbsorptionColor`, `_AbsorptionStrength` | Plus | 吸収近似 |
| `_WaterThickness`, `_DepthStrength`, `_DepthBias` | Plus | 手動/Height/Vertex深さ補助 |
| `_RefractionStrength` | 両方 | LiteはProbe Distortion、Plus HiddenではScreen offset |
| `_FSBoxProjection` | Plus | Box Projected Reflection Probe |
| `_FSScreenRefraction`, `_RefractionStrength` | Plus | Transparent Ice用の任意Screen Refraction |
| `_FSScreenRefraction` | Plus | Heavy Screen Refraction切替 |
| `_FSTransparentZWrite` | 両方 | 透過ZWrite（重なり対策）。既定OFF。ONで透明面が深度を書き込み前後ソートが安定するが、重なり部のブレンドが変わる |

Water Vertex ColorはR=Foam、G=Wave Weight、B=Depth Tint、A=Opacityです。

## Ice

| Property | Edition | 意味 |
|---|---|---|
| `_FSIceTransparent` | 両方 | Opaque/Transparent切替（既定OFF） |
| `_Opacity` | 両方 | Transparent Iceの基礎不透明度 |
| `_IceColor`, `_IceThickness` | 両方 | 氷色と手動厚み。Transparent時は厚みが不透明度にも寄与 |
| `_AbsorptionColor`, `_AbsorptionStrength` | Plus | View Angle吸収 |
| `_FSIceFrost`, `_FrostMap`, `_FrostStrength` | 両方 | Frost機能 |
| `_FrostColor`, `_FrostScaleA`, `_FrostScaleB`, `_FrostEdge` | Plus | 2-scale/edge Frost |
| `_FSIceCracks`, `_CrackMap`, `_CrackDepth` | 両方 | Crack表現 |
| `_CrackParallax`, `_CrackGlowColor`, `_CrackGlowStrength` | Plus | Parallax/internal color |
| `_FSIceScatter`, `_ScatterColor`, `_ScatterStrength` | Lite | Fake Subsurface |
| `_FSIceBackLight`, `_BackLightColor`, `_BackLightStrength`, `_BackLightThickness` | Plus | Back Light |
| `_FSIceSparkle`, `_SparkleStrength`, `_SparkleDistance` | 両方 | Sparkleと距離Fade |
| `_SparkleDensity`, `_SparkleSize` | Plus | Plus Sparkle形状 |
| `_FSBoxProjection` | Plus | Box Projected Reflection Probe |
| `_FSScreenRefraction`, `_RefractionStrength` | Plus | Transparent Ice用の任意Screen Refraction |

Ice Vertex ColorはR=Frost、G=Crack、B=Sparkle、A=Thicknessです。

## Glass

| Property | Edition | 意味 |
|---|---|---|
| `_TransmissionColor`, `_GlassThickness` | 両方 | 透過色と手動厚み |
| `_AbsorptionColor`, `_AbsorptionStrength` | Plus | 厚み吸収近似 |
| `_RefractionStrength` | 両方 | Probe/Screen Distortion |
| `_FSGlassCondensation` | 両方 | 結露機能 |
| `_CondensationMap` | 両方 | Lite mask、Plus RGB packed map |
| `_CondensationAmount`, `_DropletSpeed` | 両方 | 量とUV速度 |
| `_FSGlassDropletNormal`, `_CondensationNormal` | 両方 | 結露Normal |
| `_CondensationColor` | Plus | 結露色 |
| `_DropletStrength`, `_TrailStrength`, `_MicroFogStrength` | Plus | R/G/B別強度 |
| `_CondensationRoughness`, `_CondensationOpacity` | Plus | 局所表面変化 |
| `_CondensationFadeDistance` | Plus | Trail距離Fade |
| `_CondensationNormalScale` | Plus | 結露Normal強度 |
| `_FSBoxProjection` | Plus | Box Projected Reflection Probe |
| `_FSScreenRefraction`, `_RefractionStrength` | Plus | Transparent Ice用の任意Screen Refraction |
| `_FSScreenRefraction` | Lite/Plus | Heavy Hidden Shader切替 |
| `_FSTransparentZWrite` | 両方 | 透過ZWrite（重なり対策）。既定OFF。ONで透明面が深度を書き込み前後ソートが安定するが、重なり部のブレンドが変わる |

Glass Vertex ColorはR=Condensation、G=Thickness、B=Variation、A=Opacityです。

## Standard

Standardは不透明PBR面で、Mode固有Propertyを持たず「共通Property」だけを使用します。透過、波、霜、ひび、結露、Screen Refractionはありません。Plus Standardのみ以下を追加します。

| Property | Edition | 意味 |
|---|---|---|
| `_FSBoxProjection` | Plus | Box Projected Reflection Probe |
| `_LTCGI`ほか`_LTCGI*` | Plus | LTCGI寄与（下記「LTCGI」参照） |

Lite Standardはbox projectionもLTCGIも持ちません。

## LTCGI

Plus Water/Glass/Standardです。

| Property | 意味 |
|---|---|
| `_LTCGI` | local LTCGI variant |
| `_LTCGIDiffuseStrength` | diffuse寄与 |
| `_LTCGISpecularStrength` | specular寄与 |
| `_LTCGIMaxBrightness` | スパイク抑制上限 |
| `_LTCGICondensationDiffuse` | Glass結露のdiffuse boost |

## 内部PropertyとKeyword

`_FSVersion`、`_FSEdition`、`_FSMode`、`_FSFeatureFlags`はInspectorとMigration用です。直接変更しないでください。

local keywordは`FSHADER_NORMALMAP`、`FSHADER_MASKMAP`、`FSHADER_HEIGHT`、`FSHADER_VERTEX_WAVE`、`FSHADER_MODE_DETAIL`、`FSHADER_RECEIVE_SHADOW`、`FSHADER_LTCGI`、`FSHADER_DEBUG`等です。Material Propertyだけをスクリプトで変更した場合は、Inspector相当のkeyword同期が必要です。

fShader 1.2.2にはruntime C# APIやMonoBehaviour APIはありません。公開契約はShader名、Property名、Vertex Color channel、Editor menuです。
