# fShader Plus 1.2.2 Guide

## 要件

- fShader Lite (Core) 1.2.2
- LTCGI `>=1.6.3 <1.7.0`
- Unity 2022.3.22f1 / BRP Forward / Linear
- VRChat SDK Worlds 3.10.4

VPM経由ではCoreとLTCGIを自動解決します。手動導入ではCore、LTCGI、Plusの順に配置してください。

## Liteからの移行

Lite Materialを選び、fShader Inspector上部のEditionをPlusへ変更します。Base/ARMH/Normal、色、共通Mode Propertyを保持し、Plus専用Propertyを既定値で追加します。変更前にMaterialを複製しておくと比較しやすくなります。

## Balancedから始める

PlusはBalancedを基準にします。Screen Refraction、LTCGI、Water 4 Wavesは個別に有効化し、一度に1条件だけ計測します。Showcaseは近景の見本であり、ワールド全体へ一括適用する設定ではありません。

## LTCGI設定

1. 公式LTCGI 1.6.3を導入します。
2. `LTCGI Controller.prefab`をシーンへ配置します。
3. Screen/Emitterを設定します。
4. LTCGIのEditor toolでAffected Renderersを更新します。
5. Plus Water/Glass/Standardの`Lighting / LTCGI`をONにします。
6. Diffuse、Specular、Max Brightnessを調整します。

Glassは結露部分だけDiffuseを増やせます。Standard（不透明PBR）もWater/Glassと同様にLTCGIへ対応します。Iceは1.2.2でもLTCGI非対応です。Controllerが見つからない場合はInspectorが警告します。

## 1.7.xを使わない理由

LTCGI 1.7.xには任意のVRC Light Volumes連携AdapterとUdonSharpProgramAssetが含まれます。VRCLightVolumes本体がないプロジェクトでAdapterのBehaviour定義が条件コンパイルから消えると、関連ProgramAssetエラーが発生する組み合わせを確認しています。fShader 1.2.2はAPI v2を持つLTCGI 1.6.3へ固定します。

## Screen Refraction

Plus Water/Glass、および`Transparent Ice`で使用します。Iceでは先に`Transparent Ice`をONにしてください。ON時はInspectorが共有名付きGrabPassを持つHidden Shaderへ切り替えます。背景を歪められますが、Mirror、透明重なり、広い画面占有でHeavyです。OFFへ戻すと通常1 ForwardBase Shaderへ復帰します。

## 描画とレンダーキュー

Plus Water/Glassも、Coreと同じく「設定」タブの「描画 / レンダーキュー」でカスタムレンダーキュー（0–5000の絶対値指定）と「透過ZWrite（重なり対策）」トグル（既定OFF）を持ちます。重なり合う透明面の前後関係が破綻する場合は、レンダーキューで手動整列するか、Transparent ZWriteをONにして緩和してください。既定のレンダーキューはMode別に自動決定されます（不透明2000 / 透過3000）。詳細はCoreの`Documentation~/USER_MANUAL_JA.md`を参照してください。

## Sample

Package Managerから`Plus Starter Materials`をImportできます。MaterialとGallery Sceneは外部Scriptなしで動作します。Coreの`Gallery Textures`を併用するとMode固有Mapを確認できます。
