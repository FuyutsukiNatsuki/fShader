# fShader P5 測定ガイド（日本語）

この文書は、VRChat Build & Test 上で屈折の見え方と描画負荷を同じ条件で比較するための手順です。QAシーンの診断用マテリアルは屈折強度 `0.50` に固定しています。製品用 Heavy プリセットの `0.14` は変更していません。

## 1. QAシーンを更新する

1. Unityメニューの `Tools/fShader/P5/Create or Refresh QA Scene` を実行します。
2. `Assets/fShaderDevelopment/P5/Scenes/fShaderP5QA.unity` を開きます。
3. ConsoleにC#、Shader、UdonSharp、LTCGIエラーがないことを確認します。
4. VRChat SDKの `Build & Test` でローカルテストを起動します。

シーン奥の縞模様が屈折診断ステージです。左から `WATER OFF`、`WATER ON`、`GLASS OFF`、`GLASS ON` の順です。床には次の測定位置があります。

- `MEASURE VISUAL A/B`: 4枚を同時に見比べる位置
- `MEASURE WATER OFF / ON`: Waterの各パネルを1枚だけ測る位置
- `MEASURE GLASS OFF / ON`: Glassの各パネルを1枚だけ測る位置
- `MEASURE MIRROR`: Mirrorの負荷を比較する位置

## 2. 屈折を目視確認する

1. `MEASURE VISUAL A/B` の中央に立ちます。
2. 4枚のパネルの後ろにある白い縦線と、シアン・黄・マゼンタの横線を見ます。
3. 頭または視点を左右へゆっくり動かします。
4. `WATER ON` と `GLASS ON` では線が法線模様に沿って揺れたりずれたりし、対応する `OFF` では線が安定して見えることを確認します。
5. WaterとGlassを別々に比較し、モード間の色や透明度の違いを屈折ON/OFF差と混同しないようにします。

合格の目安:

- OFFは背景線が安定し、ONだけに連続した歪みが見える。
- 左右眼で歪み位置が大きくずれない。
- Single Pass Instancedの眼境界に継ぎ目が出ない。
- MirrorやVRChat Photo Cameraで片眼だけの効果、横ずれ、ちらつきが出ない。

通常視点、VRChat Mirror、Photo Cameraの3種類で確認します。HMDが使える場合はSingle Pass InstancedとMulti Passの両方で同じ項目を確認してください。EditorのGolden画像は構図の回帰確認用であり、HMD左右眼の合格根拠にはしません。

## 3. VRChat内のFPS・フレーム時間を測る

1. `右Shift + ~ + 1` でVRChatのデバッグ表示を開きます。
2. `Performance` タブを選び、サンプリングをONにします。マウス操作が必要な場合は `Tab` でカーソルを切り替えます。

デバッグ画面を開けない場合は、利用できるFPS/GPU表示だけで測定を続けて構いません。その場合は表示元、取得できた項目、取得できなかった項目を明記し、CPU時間などをFPSから推定して埋めないでください。
3. 品質設定、画面解像度、リフレッシュレート、アバター、カメラ位置を固定します。
4. 対象パネルが同じ画面占有率になるようにし、ほかの透明物やMirrorが映り込まないようにします。
5. 対象モードの `MEASURE ... OFF` でOFFパネル1枚だけを画面に入れ、30秒待ってFPS、CPU frame time、GPU frame timeを記録します。
6. 同じモードの `MEASURE ... ON` に移動し、同じ距離・角度・画面占有率でONパネル1枚だけを30秒測ります。
7. WaterとGlassを分けて同じ比較を行います。
8. Mirror負荷は `MEASURE MIRROR` でMirror OFF/ONだけを切り替え、ほかの条件を変えずに測ります。

一度に変える条件は1つだけです。推奨する比較順は次のとおりです。

1. Water Refraction OFF → ON
2. Glass Refraction OFF → ON
3. Mirror OFF → ON
4. LTCGI OFF → ON

Desktopは既定で90 FPS上限になる場合があるため、FPSが同じでも負荷が同じとは限りません。上限に張り付いている場合は、CPU/GPUのフレーム時間（ms）を優先します。換算の目安は次のとおりです。

| FPS | 1フレームの時間 |
|---:|---:|
| 90 | 11.11 ms |
| 72 | 13.89 ms |
| 60 | 16.67 ms |
| 45 | 22.22 ms |

`1000 ÷ FPS = ms` です。Desktop 60 FPS以上、PC VR 45 FPS以上は広い最低目安にすぎません。合否は端末の目標FPSと、同一条件でのOFF→ON差を基準に決めてください。

## 4. Unity側の補助測定

Unity ProfilerやFrame Debuggerを使える場合は、同じカメラ位置でBatches、SetPass、透明オーバードローを記録します。ただしEditor値は公開VRChatクライアントの実測値ではありません。VRChat内のCPU/GPU frame timeと混ぜず、補助資料として別欄へ記録します。

## 5. 記録テンプレート

結果は `BENCHMARK_RESULTS.md` に記録します。数値が取れない場合は推測せず、`未測定` と書いてください。

| 日時 | 端末/GPU | 表示 | 品質 | 対象 | 条件 | 画面占有率 | FPS | CPU ms | GPU ms | 30秒間の異常 | 判定 |
|---|---|---|---|---|---|---:|---:|---:|---:|---|---|
| | | Desktop/HMD | | Water/Glass/Mirror/LTCGI | OFF/ON | | | | | | |

あわせてHMD名、片眼解像度、リフレッシュレート、VRChat品質、透明レイヤー数を残してください。数値だけでなく、ちらつき、左右眼差、ソート崩れ、Photo Cameraの横ずれも記録します。
