# fShader

fShaderは、VRChat World向けUnity Built-in Render Pipeline専用のWater・Ice・Glassシェーダーファミリーです。軽量な`fShader Lite (Core)`と、LTCGI・追加表現を備える`fShader Plus`を提供します。

## 最新版 1.0.1

- Unity 2022.3.22f1
- VRChat SDK Worlds 3.10.4
- Windows PC / Desktop / PC VR
- Built-in Render Pipeline / Forward / Linear Color Space
- PlusのLTCGI依存: `at.pimaker.ltcgi >=1.6.3 <1.7.0`
- Quest/Android、URP/HDRP、Deferredは対象外

Unity 2022.3.22f1 + VRChat SDK 3.10.4でfShader 1.0.0はEditMode 73/73件に合格し、GitHub Public ReleaseとVPM一覧を公開済みです。1.0.1ではIce透過を追加しています。

## 導入

公開VCCリポジトリURL:

`https://fuyutsukinatsuki.github.io/fShader/index.json`

VCCのSettings > Packages > Add Repositoryへ上記URLを追加してください。公開Release ZIPと`index.json`は外部URLから取得確認済みです。

Liteは`com.fshader.core`、Plusは`com.fshader.plus`です。PlusはCoreとLTCGI 1.6.xを必要とし、VPM経由では依存関係を自動解決します。

配布形式は、BoothのUnityPackage版と、GitHub Pagesを一覧元にするVPM版です。依存関係を自動管理できるVPM版を推奨します。Booth版とVPM版を同じプロジェクトへ重複導入しないでください。Booth版でPlusを使う場合は、LTCGI 1.6.3、Lite、Plusの順で導入します。

詳しい導入、推奨設定、既知制約は[日本語User Manual](Packages/com.fshader.core/Documentation~/USER_MANUAL_JA.md)、Shader Property契約は[Property Reference](Packages/com.fshader.core/Documentation~/PROPERTY_REFERENCE.md)を参照してください。

## リリース生成

PowerShell 7またはWindows PowerShellで次を実行します。

```powershell
./Tools/Release/Build-Packages.ps1
```

`Release/`へVPM用Core/Plus ZIP、Booth用UnityPackageと配布ZIP、SHA-256一覧、ローカル検証用`index.json`を生成します。GitHubでは`Build Release` ActionがVPMパッケージを別Releaseとして公開し、`Build VPM Listing` ActionがGitHub Pages用一覧を構築します。Booth用成果物は`Build Booth Package` Actionまたはローカル生成物から手動でBoothへ登録します。

## ライセンス

fShaderは[fShader License 1.0](fSHaderLicense.md)で、個人・法人、非商用・商用を問わず無料で利用できます。通常のWorld・Avatar・ゲーム・画像・動画への利用ではクレジットは不要です。fShader本体を再配布する場合は`fSHaderLicense.md`を同梱し、改変版を再配布する場合はfShaderを改変元として明記してください。LTCGI等の帰属は[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)に記載しています。
