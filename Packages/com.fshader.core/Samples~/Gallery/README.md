# fShader Gallery Textures

Water、Ice、Glass のモード固有表現を最小構成で確認するためのサンプルテクスチャです。
全画像は正方形、Repeat、Mip Map ON、最大Import Size 1024として設定しています。

## 対応表

| File | Shader Property | Import |
|---|---|---|
| `Textures/Water_Base_Calm.png` | `_BaseMap` | sRGB |
| `Textures/Water_WaveNormal_Fine.png` | `_WaveNormalMap` | Normal Map / Linear |
| `Textures/Water_FoamMask.png` | `_FoamMap` (R) | Linear |
| `Textures/Ice_Base_Glacial.png` | `_BaseMap` | sRGB |
| `Textures/Ice_Normal_Crystal.png` | `_NormalMap` | Normal Map / Linear |
| `Textures/Ice_FrostMask.png` | `_FrostMap` (R) | Linear |
| `Textures/Ice_CrackMask.png` | `_CrackMap` (R) | Linear |
| `Textures/Glass_Condensation_RGB.png` | `_CondensationMap` | Linear。R=Droplet、G=Trail、B=Micro Fog、A=1 |
| `Textures/Glass_CondensationNormal.png` | `_CondensationNormal` | Normal Map / Linear |

## 推奨開始値

- Water: Wave Normal ON、Wave Normal Scale 0.35～0.65、Foam ON、Foam Strength 0.35～0.7。
- Ice: Normal Scale 0.25～0.5、Frost ON、Frost Strength 0.45～0.8、Cracks ON、Crack Depth 0.4～0.8。
- Glass: Condensation ON、Droplet Normal ON、Condensation Amount 0.35～0.7。

Base Mapは色見本として用意しています。透明材質の最終色は`_BaseColor`、Waterの
`_ShallowColor`/`_DeepColor`、Iceの`_IceColor`も乗算されるため、必要に応じて白寄りへ調整してください。

生成条件と最終プロンプトは [GENERATION.md](GENERATION.md) に記録しています。

## Starter Materials / Scene

`Materials`にはLite Water Ocean、Lite Ice Frosted、Lite Glass Condensedの開始Materialがあります。`Scenes/fShader Lite Gallery.unity`で3モードを同時に確認できます。Screen RefractionはSampleでもOFF既定です。

Sampleは`Assets/Samples`へコピーされるため、Core Packageを削除しても自動削除されません。不要な場合はImport先を手動削除してください。