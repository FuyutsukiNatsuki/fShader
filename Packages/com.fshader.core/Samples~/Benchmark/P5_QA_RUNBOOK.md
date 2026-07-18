# P5 VR / VRChat QA Runbook

日本語の具体的な操作手順は [`P5_MEASUREMENT_GUIDE_JA.md`](P5_MEASUREMENT_GUIDE_JA.md) を参照してください。このRunbookはリリース判定用のチェックリストです。

## 1. QAシーンの準備

1. Unityメニューの `Tools/fShader/P5/Create or Refresh QA Scene` を実行する。
2. `Assets/fShaderDevelopment/P5/Scenes/fShaderP5QA.unity` を開く。
3. ConsoleにC#、Shader、UdonSharp、LTCGIエラーがないことを確認する。
4. `Tools/fShader/P5/Capture Golden Screenshots` を実行し、`Assets/fShaderDevelopment/P5/Golden` の3枚を比較する。

- `P5_Overview.png`
- `P5_Refraction_LTCGI.png`
- `P5_Refraction_AB.png`

QA生成器はVRC Scene Descriptorを維持し、VRChat Mirror、公式LTCGI Controller prefab、Screen、Emitterを配置します。LTCGIランタイムをfShaderパッケージへ複製しません。

## 2. Build gate

VRChat SDKのBuild & TestをWindows向けに実行します。ローカルworld bundleが生成され、fShader、LTCGI、UdonSharpエラーがないことを合格条件とします。リリースbuild後は `Library/fShader/P5_VARIANTS.json` を確認するか、`Tools/fShader/P5/Write Variant Report` を実行します。

Variant stripperは保守的です。出荷用debug variantと、結露OFF時のdroplet-normalという不成立な組合せだけを除去します。Stereo、Fog、Lightmap、GPU instancing、各Mode、Screen Refraction、LTCGI variantは維持します。

## 3. 目視マトリクス

各行をSingle Pass InstancedとMulti Passで確認します。

| View | Water | Ice | Glass | LTCGI |
|---|---|---|---|---|
| 左右眼 | Waveと屈折が両眼で一致 | Frost、Crack、Mistが安定 | Condensationと屈折が一致 | ON/OFF差があり片眼だけにならない |
| VRChat Mirror | reflected cameraにFresnelが追従 | Rimと内部効果が安定 | 元のplayer cameraを参照しない | ちらつきや片眼寄与がない |
| Photo Camera | desktop構図と整合 | Mist billboardが維持 | screen textureが横ずれしない | 一貫して撮影される |

失敗条件は、片眼のみのGrab sampling、SPSI眼境界の継ぎ目、Mirror内Fresnelがplayer cameraへ追従、Photo Camera UV横ずれ、描画方式の片方だけでLTCGIが消える、左右眼で透明ソートが変わることです。

## 4. Performance capture

`P5_MEASUREMENT_GUIDE_JA.md` のマーカーと30秒A/B手順を使用します。HMD、片眼解像度、リフレッシュレート、VRC品質、画面占有率、透明レイヤー数を固定し、同じ位置でFPS、GPU ms、CPU render msを記録します。BatchesとSetPassはUnity側の補助値として区別します。

Liteは広い面積と反復配置の既定候補、Plus BalancedはScreen RefractionとLTCGIを必要な場合だけ有効化、Heavyは画面占有率と個数を制限した見せ場向けとします。Mirrorは対象物を再描画するため、実際の解像度とlayer maskで別測定します。

## 5. 回帰成果物

次を測定結果と一緒に保存します。

- `P5_Overview.png`
- `P5_Refraction_LTCGI.png`
- `P5_Refraction_AB.png`
- `P5_ENVIRONMENT.json`
- `P5_VARIANTS.json`
- `P5_VRCSDK_BUILD.json`
- 記入済み `BENCHMARK_RESULTS.md`

Golden画像はDesktop/Photo Camera構図の基準です。SPSI、Multi Pass、Mirror、Photo Camera、HMD性能の実機確認を代替しません。
