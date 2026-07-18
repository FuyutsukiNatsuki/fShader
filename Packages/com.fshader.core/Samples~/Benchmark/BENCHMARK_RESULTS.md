# fShader Benchmark Results

## Environment

| Field | Value |
|---|---|
| Date | 2026-07-18 |
| fShader version | 1.0.0（runtimeは0.6.1-p5.1検証版と同等） |
| Unity version | 2022.3.22f1 |
| VRChat SDK | Worlds 3.10.4 |
| VRChat local build | Passed（`.vrcw`生成済み） |
| GPU | NVIDIA GeForce RTX 3060, D3D11, 12113 MB |
| CPU | AMD Ryzen 7 8700F 8-Core Processor |
| Color space | Linear |
| HMD | Pico 4（PC VR、接続方式未記録） |
| Resolution | Golden基準 1600x900、Pico 4片眼解像度は未記録 |
| Refresh rate | 90 Hz |
| Stereo mode | Project設定 SPSI（`stereoRenderingPathRaw=1`）、実機でのSPSI / Multi Pass切替確認は未実施 |
| Quality level | VRC High |

## Build and variant evidence

| Check | Result |
|---|---|
| fShader EditMode tests | 67 / 67 Passed |
| C# / Shader import | Error 0 |
| VRChat SDK package全体tests | 166 / 168 Passed。失敗2件はSDK自身の `GraphNodeTests.CheckHelpURLsForSystemNodes` と `UICompilerTests.CompareAssemblies` で、fShader専用67件は全成功 |
| VRChat SDK local world build | 1.0.0のruntime同等版0.6.1-p5.1でPassed、StandaloneWindows64 `.vrcw`生成済み |
| VRChat SDK Build & Test | Passed。VRChatローカルクライアントで `fShaderP5QA` 読み込み済み |
| Shader preprocess input | 0.6.0-p5: 1472 variants / 9 shader passes |
| Shader preprocess output | 0.6.0-p5: 1472 variants |
| Shipping debug variants | release buildへ混入なし |
| Invalid droplet-normal dependency | Inspectorが結露依存を強制し、該当variant混入なし |

保存済みレポートは `Assets/fShaderDevelopment/P5/Benchmark/P5_VRCSDK_BUILD.json` と `P5_VARIANTS.json` です。最新build reportは `Library/fShader/P5_VARIANTS.json` にも出力されます。

## ユーザー目視確認（2026-07-18）

| 項目 | 結果 |
|---|---|
| Desktop Local Test | 大きな表示問題なし |
| VRChat Mirror | 表示上の問題なし |
| FPS | Pico 4測定で60–63 FPS |
| 屈折 | P5.1の各測定ポイントで性能値を取得。背景線の歪み・左右眼差の目視判定は未記録 |

この記録はユーザーの目視結果であり、CPU/GPU frame timeの数値を推定したものではありません。

## Measurements

日本語の測定手順は `P5_MEASUREMENT_GUIDE_JA.md` を参照してください。EditorのGolden画像からGPU値を推定せず、VRChat Build & Test内で同じカメラ位置・画面占有率・品質を使って記録します。

| Edition | Mode | Preset | Coverage | Layers | Screen refraction | LTCGI | FPS | GPU ms | CPU render ms | Batches | SetPass | Status |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| Lite | Water | Balanced | 25% | 1 | Off | Off | | | | | | HMD測定待ち |
| Lite | Ice | Balanced | 25% | 1 | Off | Off | | | | | | HMD測定待ち |
| Lite | Glass | Balanced | 25% | 2 | Off | Off | | | | | | HMD測定待ち |
| Plus | Water | Balanced | 25% | 1 | Off | Off | | | | | | HMD測定待ち |
| Plus | Ice | Balanced | 25% | 1 | Off | Off | | | | | | HMD測定待ち |
| Plus | Glass | Balanced | 25% | 2 | Off | Off | | | | | | HMD測定待ち |
| Plus | Water | Heavy | 25% | 1 | On | On | | | | | | HMD測定待ち |
| Plus | Glass | Heavy | 25% | 2 | On | On | | | | | | HMD測定待ち |

## Refraction A/B（P5.1）

各行はOFFとONをそれぞれ1枚だけ画面に入れ、同じ距離で30秒測ります。

| Mode | Condition / Marker | Coverage | FPS | GPU ms | CPU ms | 背景線の歪み | 左右眼差/継ぎ目 | 判定 |
|---|---|---|---:|---:|---|---|---|---|
| Water | OFF / WaterOff | マーカー基準 | 63 | 15 | 未測定 | 未記録 | 未記録 | 基準値 |
| Water | ON / WaterOn | マーカー基準 | 60 | 15 | 未測定 | 未記録 | 未記録 | GPU表示差0 ms、FPS差-3 |
| Glass | OFF / GlassOff | マーカー基準 | 62 | 15 | 未測定 | 未記録 | 未記録 | 基準値 |
| Glass | ON / GlassOn | マーカー基準 | 63 | 15 | 未測定 | 未記録 | 未記録 | GPU表示差0 ms、FPS差+1 |
| Water + Glass | Visual A/B | 4パネル同時 | 63 | 15 | 未測定 | 未記録 | 未記録 | 参考値 |

### 実機測定の解釈

- 測定環境はRTX 3060 12 GB / Pico 4 / 90 Hz。VRChatデバッグ画面は開けなかったため、取得可能だったFPSとGPU msのみを記録した。
- 5地点のFPSは60–63、GPU表示は全地点15 msで一定だった。
- 取得できた表示精度では、Water/GlassともScreen Refraction ONによる明確なGPU時間増加は検出されなかった。これはコストが完全に0という意味ではなく、表示の丸め・測定揺らぎ以内という意味である。
- 90 FPS維持には約11.11 ms以下が必要。15 msは理論上約66.7 FPS相当で、今回の60–63 FPSと概ね整合する。
- PC VRの広い最低目安45 FPSは満たすが、Pico 4の90 Hz目標には未達。
- CPU時間、片眼解像度、接続方式、背景線の歪み、左右眼差は未記録。CPU frame timeがないため、GPU律速かCPU/VRChat側の制約かは断定しない。

## Validation state

- Desktop deterministic golden capture: 3枚とも生成成功。屈折A/B画像の高コントラスト背景と構図を目視確認済み。
- VRChat SDK Build: 1.0.0のruntime同等版0.6.1-p5.1でPassed。StandaloneWindows64 `.vrcw`生成、C# / Shader / UdonSharp / LTCGI error 0。
- VRChat SDK Build & Test: Passed。Local Testへ入室し、`fShaderP5QA` AssetBundleとworld loadedをclient logで確認済み。
- QA scene: VRC Scene Descriptor、VRChat Mirror、LTCGI Controller / Screen / Emitter、高コントラスト屈折A/B、5種の測定位置を含む。
- Client runtime log: fShader Shader、Udon execution、LTCGI error 0。
- Pico 4 HMD性能: 90 Hz設定で60–63 FPS、GPU 15 ms。屈折ON/OFFのGPU表示差は0 ms。
- SPSI / Multi Passの左右眼・継ぎ目: 手動目視判定待ち。
- Photo Camera: 手動確認待ち。
- GPU frame time: Pico 4で15 msを記録済み。CPU render time、batches、SetPass、overdrawは未測定。
- 自動結果をHMD性能値として扱わない。
