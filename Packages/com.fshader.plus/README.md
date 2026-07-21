# fShader Plus 1.2.0

PlusはWater / Ice / Glass / Standardへ追加ディテール、距離Fade、Box Projected Reflection Probe、Water/Glassの任意Screen Refractionを追加し、Water/Glass/StandardはLTCGI API v2へ対応します。通常Shaderは1 ForwardBase drawを維持し、Screen Refractionは既定OFFです。不透明PBRのStandardもBox ProjectionとLTCGIを利用できます。Plus Water/GlassもCoreと同じくカスタムレンダーキュー指定とTransparent ZWriteトグルで、重なった透明面の描画順を調整できます。

VPM依存:

- `com.fshader.core` 1.2.0
- `at.pimaker.ltcgi >=1.6.3 <1.7.0`

LTCGI 1.6.xへ固定する理由は、1.7.xの任意VRC Light Volumes AdapterがVRCLightVolumes未導入環境でUdonSharpProgramAssetエラーになる組み合わせを確認しているためです。LTCGI本体はfShaderへ複製していません。

Package Managerの`Plus Starter Materials` SampleにはBalanced MaterialとGallery Sceneがあります。設定は`Documentation~/PLUS_GUIDE_JA.md`、Property契約はCoreの`Documentation~/PROPERTY_REFERENCE.md`を参照してください。
