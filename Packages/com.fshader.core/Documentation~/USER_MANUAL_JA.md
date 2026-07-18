# fShader 1.0.0 User Manual

## 1. 対応環境

fShader 1.0.0はUnity 2022.3.22f1、VRChat SDK Worlds 3.10.4、Built-in Render Pipeline、Forward Rendering、Linear Color Space、Windows PC向けVRChat Worldを正式対象とします。DesktopとPC VRの両方で利用できます。

Quest/Android、URP/HDRP、Deferred Rendering、VRChat Avatarは正式対象外です。PlusはLTCGI 1.6.3以上1.7.0未満を必要とします。

## 2. 導入

### VCC/VPM

公開後はVCCのSettings > Packages > Add Repositoryへ次を登録します。

`https://fuyutsukinatsuki.github.io/fShader/index.json`

Liteは`fShader Lite (Core)`を追加します。Plusを追加するとCoreとLTCGI依存もVPMが解決します。LTCGIは1.7.xへ更新しないでください。1.7.xはVRCLightVolumes未導入環境でUdonSharpProgramAssetエラーになる既知の組み合わせがあります。

Private Repository期間中は一般VCCから取得できません。Release ZIPを展開したフォルダーをVCCのUser Packagesへ登録するか、Unityプロジェクトの`Packages`へEmbedded Packageとして配置します。

### 初回確認

1. Project Settings > Player > Color SpaceをLinearにします。
2. Built-in Render Pipelineであることを確認します。
3. Materialを作成し、Shaderから`fShader/Lite/Water`、`Ice`、`Glass`のいずれかを選びます。
4. Inspector上部で日本語/English、Lite/Plus、Modeを切り替えます。
5. Reflection ProbeまたはSkybox由来の反射を用意します。
6. Inspector最下部のValidationを確認します。

## 3. LiteとPlusの選択

| 目的 | 推奨 |
|---|---|
| 多数配置、広い水面、低負荷重視 | Lite |
| Water/GlassのLTCGI、追加ディテール | Plus Balanced |
| 画面空間屈折を見せる近景 | Plus Showcaseを起点に個別調整 |

Liteの通常ShaderはGrabPass、LTCGI、Scene Depth、ForwardAddを持ちません。Plusの通常Shaderも1 ForwardBase drawを基本とし、Screen Refractionを有効にした時だけHidden GrabPass Shaderへ切り替わります。

## 4. 共通PBR

- Base MapはsRGB。
- Normal MapはTexture TypeをNormal Mapに設定。
- ARMHはLinearで、R=AO、G=Roughness、B=Metallic、A=Height。
- ARMHがない場合はAO 1、Material Roughness/Metallic、Height 0.5相当を使用します。
- 個別AO/Roughness/Metallic/Height画像は`Tools > fShader > ARMH Texture Packer`で1枚へまとめます。

AOやMaskをsRGBのまま使うとInspectorが警告します。Heightは控えめな疑似表現用で、実際の厚み測定やテッセレーションではありません。

## 5. Water

Liteは2方向Wave Normal、最大2頂点波、Reflection Probe、Fresnel、Foam、Probe Distortionを備えます。Plusは独立2 Normal、最大4 World Space波、吸収、2-scale Foam、Crest、Surface Caustics、Box Projection、任意Screen Refraction、LTCGIを追加します。

頂点波はメッシュ分割数に依存します。大きなPlaneを少数頂点のまま使う場合は頂点波をOFFにし、Wave Normalだけを使ってください。

## 6. IceとCold Mist

IceはFrost、Crack、Fake Subsurface/Back Light、Sparkleを備えます。冷気煙はShader単体ではなくParticleSystemを生成します。

1. IceのMeshRendererを選択します。
2. Liteは`Tools > fShader > Create Cold Mist Lite for Selected Ice`、Plusは`Create Cold Mist Plus for Selected Ice`を実行します。
3. 生成されたParticleSystemの範囲と色を確認します。

Liteは最大24粒子、Plusは最大64粒子が既定です。生成物にruntime MonoBehaviourは残りません。

## 7. Glass

GlassはTransmission、Fresnel、Reflection Probe、Probe Distortion、結露を備えます。PlusはDroplet/Trail/Micro FogをRGB別に制御します。

Screen Refractionは背景を実際に歪めますがHeavy機能です。透明面の重なり、Mirror、広い画面占有で負荷が増えます。通常はOFFにし、重要な近景だけに使用してください。GrabPassの取得順と透明ソートのため、重なった透明面を物理的に正確には描画できません。

## 8. Plus LTCGI

Plus Water/Glassだけが対応します。公式LTCGI Controller、Screen、Emitterを配置し、LTCGI側ツールでAffected Renderersを更新してからMaterialのLTCGIをONにします。Iceは1.0.0では非対応です。

LTCGIとScreen Refraction、Water 4 Wavesを同時に使う構成はHeavyです。まず個別にON/OFF測定してください。詳しくはPlusパッケージの`Documentation~/PLUS_GUIDE_JA.md`を参照してください。

## 9. Samples

Package ManagerでCoreの`Gallery Textures`をImportすると、Water/Ice/Glass用テクスチャ、Lite Starter Materials、Gallery Sceneが`Assets/Samples`へコピーされます。Plusの`Plus Starter Materials`はPlus用MaterialとSceneを含みます。

SamplesはユーザーAssetとしてコピーされるため、パッケージ削除時に自動削除されません。不要なら`Assets/Samples/fShader...`を手動で削除してください。

## 10. 性能確認

- 一度に変更する条件は1つだけにします。
- Refraction、LTCGI、Mirrorを別々にOFF/ON比較します。
- GPU/CPU frame time、画面占有率、透明枚数、HMD、片眼解像度、Stereo Modeを記録します。
- Debug画面を開けない場合は取得できたFPS/GPU表示だけを記録し、CPU時間を推定しません。

Pico 4 / RTX 3060 / 90 HzのP5.1測定では60–63 FPS、全地点GPU表示15 msでした。表示精度上、Water/Glass Refraction ON固有のGPU増加は検出されませんでしたが、コスト0とは断定しません。

## 11. アンインストール

1. fShader MaterialをStandard等へ変更するか、使用Objectを削除します。
2. Plusを先に削除し、その後Coreを削除します。
3. LTCGIを他で使っていなければLTCGIを削除します。
4. Import済みSamplesが不要なら`Assets/Samples`から削除します。
5. ConsoleでMissing ScriptとShader errorがないことを確認します。

fShaderはruntime MonoBehaviourを追加しません。Cold Mistも標準ParticleSystemだけで動作します。

## 12. 既知制約

- PC VR World専用。Quest/Androidは非対応。
- Screen RefractionはBRP GrabPassで、透明ソートと多重面に制約があります。
- Reflection Probe未配置では反射が弱い、または黒く見える場合があります。
- 頂点波はメッシュ密度依存。
- HMD SPSI/Multi Pass、Photo Cameraの最終結果は各環境で再確認してください。
- LTCGIは1.6.x固定。1.7.xは初版の互換対象外です。
