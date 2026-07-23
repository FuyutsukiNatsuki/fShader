# fShader Lite (Core) 1.2.3

Unity 2022.3 Built-in Render PipelineとVRChat Worlds向けの軽量Water / Ice / Glass / Standard Shader、共通PBR、二言語Inspector、テンプレート、ARMH Packer、Mode Preset、レンダーキュー指定、透過ZWrite、Validation、Samplesを提供します。

Standardは水・氷・ガラス表現を持たない不透明PBR面で、影とLightmapに対応します。テンプレートはInspectorの専用タブから各Edition・モードの機能設定とSampleテクスチャをワンクリックで適用でき、現在のMaterialのテンプレート書き出し・読み込み（JSON）にも対応します。レンダーキューは既定でMode別に自動決定し、カスタムレンダーキューで絶対値を指定できます。Water/GlassにはTransparent ZWriteトグルがあり、重なった透明面の前後ソートを安定させられます。

通常Lite ShaderはGrabPass、LTCGI、Scene Depth、ForwardAddを含みません。Lite GlassのScreen Refractionだけが明示的なON操作でHidden共有GrabPass Shaderへ切り替わり、既定はOFFです。

Package Managerの`fShader Lite Gallery` SampleにはWater/Ice/GlassのStarter Material、Gallery Scene、サンプルTextureがあります。`Benchmark Documentation`にはVR/HMD QA手順と実測結果があります。

導入と推奨設定は`Documentation~/USER_MANUAL_JA.md`、Property契約は`Documentation~/PROPERTY_REFERENCE.md`を参照してください。
