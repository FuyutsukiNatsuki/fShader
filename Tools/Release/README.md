# fShader Release Tools

`Build-Packages.ps1`はCore/Plusのversion、作者メール、ライセンス、Core依存、LTCGI範囲を検証し、VPM配布用ZIPとSHA-256、ローカル`index.json`を生成します。

`Build-BoothPackages.ps1`は全Unityアセットの`.meta`とGUIDを検証し、Lite/PlusのUnityPackage、Booth配布ZIP、SHA-256を生成します。

```powershell
./Tools/Release/Build-Packages.ps1 -ExpectedVersion 1.2.3
```

Boothへは`Release/fShader-1.2.3-Booth.zip`を登録し、`BOOTH_DESCRIPTION_JA.md`を商品説明の原稿として使います。GitHubで公開する前に、`RELEASE_CHECKLIST_JA.md`、fShader License 1.0、GitHub Pages設定、両パッケージのRelease生成結果を確認してください。
