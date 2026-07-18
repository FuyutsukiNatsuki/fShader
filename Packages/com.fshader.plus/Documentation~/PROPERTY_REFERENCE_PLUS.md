# fShader Plus Property Reference

Plus固有PropertyはCoreの`Documentation~/PROPERTY_REFERENCE.md`へ統合して記載しています。本書では安定契約の要点だけを示します。

- Water追加: `_WaveNormalMap2`、`_WaveCount`、`_AbsorptionColor`、`_WaterThickness`、`_FSWaterCaustics`、`_FSBoxProjection`、`_FSScreenRefraction`
- Ice追加: `_AbsorptionColor`、`_FrostScaleA/B`、`_CrackParallax`、`_FSIceBackLight`、`_SparkleDensity`、`_FSBoxProjection`
- Glass追加: `_AbsorptionColor`、packed `_CondensationMap`、`_DropletStrength`、`_TrailStrength`、`_MicroFogStrength`、`_CondensationRoughness`、`_FSScreenRefraction`
- LTCGI: `_LTCGI`、`_LTCGIDiffuseStrength`、`_LTCGISpecularStrength`、`_LTCGIMaxBrightness`、Glass専用`_LTCGICondensationDiffuse`

Hidden Screen ShaderはInspector管理です。直接Shaderを差し替えず、`_FSScreenRefraction`のUIを使用してください。
