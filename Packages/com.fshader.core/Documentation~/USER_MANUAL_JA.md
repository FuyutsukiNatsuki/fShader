# fShader 1.2.3 User Manual

## 1. 対応環境

fShader 1.2.3はUnity 2022.3.22f1、VRChat SDK Worlds 3.10.4、Built-in Render Pipeline、Forward Rendering、Linear Color Space、Windows PC向けVRChat Worldを正式対象とします。DesktopとPC VRの両方で利用できます。

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
3. Materialを作成し、Shaderから`fShader/Lite/Water`、`Ice`、`Glass`、`Standard`のいずれかを選びます。
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

## 6. Ice

IceはFrost、Crack、Fake Subsurface/Back Light、Sparkleを備えます。1.0.1から`Transparent Ice`をONにするとOpacity、厚み、Frost、Crackを反映した半透明描画になります。既定のOpaqueは1.0.0互換で最軽量です。PlusではTransparent時のみ任意のScreen Refractionも利用できます。透明面の重なり、Mirror、広い画面占有では描画順とOverdrawに注意してください。

> 1.2.3でCold Mist（冷気パーティクル）機能は削除されました。冷気を演出したい場合は、任意のParticleSystemを別途配置してください。

## 7. Glass

GlassはTransmission、Fresnel、Reflection Probe、Probe Distortion、結露を備えます。PlusはDroplet/Trail/Micro FogをRGB別に制御します。

Screen Refractionは背景を実際に歪めますがHeavy機能です。透明面の重なり、Mirror、広い画面占有で負荷が増えます。通常はOFFにし、重要な近景だけに使用してください。GrabPassの取得順と透明ソートのため、重なった透明面を物理的に正確には描画できません。

## 8. Standard

Standardは透過や波、霜、ひび、結露、画面屈折を持たない不透明PBR面です。Base/ARMH/Normalによる質感、反射、通常ライティングだけを扱う、最も汎用的な素材モードです。水・氷・ガラス表現が不要なソリッドな小物や環境オブジェクトに使用します。

不透明（Geometry queue）で描画し、Forward・ShadowCaster・Metaパスを持つため、他モードと違って影を落としLightmapへベイクされます。Shaderは`fShader/Lite/Standard`と`fShader/Plus/Standard`で、Inspectorのモード切替に「Standard」タブが加わります。

Plus StandardはBox Projected Reflection ProbeとLTCGI（Diffuse/Specular/Max Brightness）に対応します。Lite Standardはどちらも持たない最軽量構成です。

## 9. 描画とレンダーキュー

Inspectorの「設定 / Settings」タブに「描画 / レンダーキュー (Rendering / Render Queue)」があります。既定ではレンダーキューをModeごとに自動決定し、不透明（Standardや既定のIce）は2000、透過（Water、Glass、Transparent Ice）は3000を使います。

「カスタムレンダーキュー (Custom Render Queue)」をONにすると、0–5000の絶対値を手動で指定できます。値の小さい面が先、大きい面が後に描画されるため、重なり合う透明面の前後関係を手作業で決める主要な手段になります（第15節の既知制約を参照）。

Water/Glassには「透過ZWrite（重なり対策） (Transparent ZWrite)」トグル（既定OFF）があります。ONにすると透明面が深度を書き込み、重なった透明面どうしの前後ソートが安定します。ただし透明面が重なる箇所のブレンドが変わり、奥の面が隠れることがあります。常に望ましいわけではないトレードオフの手段です。

「カリング / 面 (Culling / Faces)」で片面/両面を切り替えられます。既定は「表面のみ (Cull Back)」で従来どおり表面だけを描画します。「両面表示 (Cull Off)」にすると裏面も描画し、板状メッシュを裏からも見せられます（裏面はライティング用の法線を自動反転するため、裏から見ても正しく陰影が付きます）。「裏面のみ (Cull Front)」も選べます。透過モードで両面表示にすると表裏の重なりでOverdrawとソートの乱れが増えるため、Inspectorが警告します。必要な面だけで使用してください。

## 10. Plus LTCGI

Plus Water/Glass/Standardが対応します。公式LTCGI Controller、Screen、Emitterを配置し、LTCGI側ツールでAffected Renderersを更新してからMaterialのLTCGIをONにします。Iceは1.2.3では非対応です。

LTCGIとScreen Refraction、Water 4 Wavesを同時に使う構成はHeavyです。まず個別にON/OFF測定してください。詳しくはPlusパッケージの`Documentation~/PLUS_GUIDE_JA.md`を参照してください。

## 11. Samples

Package ManagerでCoreの`Gallery Textures`をImportすると、Water/Ice/Glass用テクスチャ、Lite Starter Materials、Gallery Sceneが`Assets/Samples`へコピーされます。Plusの`Plus Starter Materials`はPlus用MaterialとSceneを含みます。

SamplesはユーザーAssetとしてコピーされるため、パッケージ削除時に自動削除されません。不要なら`Assets/Samples/fShader...`を手動で削除してください。

## 12. テンプレート

Inspector上部のタブバーが「設定 / Settings」と「テンプレート / Templates」に分かれ、テンプレートは専用の「テンプレート」タブへ移動しました。タブには従来のワンクリック テンプレートに加えて、「ユーザーテンプレート (User Templates)」一覧と「作成 / 読み込み (Create / Import)」欄があります。

ワンクリック テンプレートは、LiteとPlusそれぞれの行に水・氷・ガラス・標準のボタンが並びます。ボタンを押すと、対応するEditionとモードのShaderへ切り替え、調整済みの機能トグルとPBR初期値を適用し、`fShader Lite Gallery` Sampleの対応Textureを割り当てます。Sampleが未Importの場合はPackage Managerから自動でImportしてからTextureを割り当てます。

標準（Standard）テンプレートは専用のTexture設定がないためTextureを割り当てず、粗さ・金属度・反射の妥当な初期値だけを設定します。Sampleのテクスチャを手動で割り当てる代わりに、ワンクリックで整った見た目を得られます。

「作成 / 読み込み」では、現在のMaterialをテンプレートとして書き出せます。テンプレートはJSONとして`Assets/fShader Templates`へ保存され、保存済みのテンプレートJSONを読み込んで別Materialへ適用できます。Textureの参照はGUIDで保持するため、同一プロジェクト内なら書き出したTextureも復元します。別プロジェクトへ持ち込んだ場合は名前検索によるフォールバックになります。

## 13. 性能確認

- 一度に変更する条件は1つだけにします。
- Refraction、LTCGI、Mirrorを別々にOFF/ON比較します。
- GPU/CPU frame time、画面占有率、透明枚数、HMD、片眼解像度、Stereo Modeを記録します。
- Debug画面を開けない場合は取得できたFPS/GPU表示だけを記録し、CPU時間を推定しません。

Pico 4 / RTX 3060 / 90 HzのP5.1測定では60–63 FPS、全地点GPU表示15 msでした。表示精度上、Water/Glass Refraction ON固有のGPU増加は検出されませんでしたが、コスト0とは断定しません。

## 14. アンインストール

1. fShader MaterialをStandard等へ変更するか、使用Objectを削除します。
2. Plusを先に削除し、その後Coreを削除します。
3. LTCGIを他で使っていなければLTCGIを削除します。
4. Import済みSamplesが不要なら`Assets/Samples`から削除します。
5. ConsoleでMissing ScriptとShader errorがないことを確認します。

fShaderはruntime MonoBehaviourを追加しません。

## 15. 既知制約

- PC VR World専用。Quest/Androidは非対応。
- 複数の透明fShader面（Water/Glass/Transparent Ice）が重なると、描画順の都合で一方が抜けたり、前後が入れ替わって見える場合があります。これはBRP Forward + アルファブレンドの原理的な制約で、修正可能なバグではありません（ピクセル単位の正確なソートには深度プリパスが必要で、対象外です）。第9節の「描画とレンダーキュー」でカスタムレンダーキューにより手動整列するか、Water/GlassのTransparent ZWriteをONにして緩和してください。
- Screen RefractionはBRP GrabPassで、透明ソートと多重面に制約があります。
- Reflection Probe未配置では反射が弱い、または黒く見える場合があります。
- 頂点波はメッシュ密度依存。
- HMD SPSI/Multi Pass、Photo Cameraの最終結果は各環境で再確認してください。
- LTCGIは1.6.x固定。1.7.xは初版の互換対象外です。
