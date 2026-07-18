# fShader Benchmark / P5 QA

P5のVR・VRChat最適化確認用シーンは次です。

`Assets/fShaderDevelopment/P5/Scenes/fShaderP5QA.unity`

シーンにはLite / PlusのWater・Ice・Glass、Plus Heavy、LTCGI OFF / ON、VRChat Mirror、公式LTCGI Controller / Screen / Emitterに加え、P5.1のWater・Glass屈折OFF / ON比較ステージと測定位置マーカーを配置します。

再生成メニュー:

- `Tools/fShader/P5/Create or Refresh QA Scene`
- `Tools/fShader/P5/Capture Golden Screenshots`
- `Tools/fShader/P5/Write Variant Report`

日本語の測定操作は `P5_MEASUREMENT_GUIDE_JA.md`、リリース判定一覧は `P5_QA_RUNBOOK.md`、記録先は `BENCHMARK_RESULTS.md` です。

Golden画像は `Assets/fShaderDevelopment/P5/Golden`、環境・SDK build・variant記録は `Assets/fShaderDevelopment/P5/Benchmark` にあります。GPU時間はEditor画像から推定せず、VRChat Build & Test中にHMD、解像度、品質、透明レイヤー数、画面占有率を固定して測定してください。
