# fShader 1.2.3 リリースチェックリスト

## 必須

- [x] Core / Plusの`package.json`が同じSemVerである
- [x] Plusが同じ版のCoreと`at.pimaker.ltcgi >=1.6.3 <1.7.0`へ依存する
- [x] Unity 2022.3.22f1、VRCSDK Worlds 3.10.4、Linear、BRP Forwardを確認した
- [x] fShader EditMode testsが全成功した（73 / 73）
- [x] クリーンプロジェクトでCoreのみ、Core+Plusの順にImportできた
- [x] パッケージ削除後にC# compile error、Missing Script、自動生成Assetsが残らない
- [x] Desktop Build & TestとPico 4の実測結果を確認した
- [x] `fSHaderLicense.md`をfShader License 1.0として確定し、Core / Plusへ同梱した
- [ ] SPSI / Multi Pass左右眼、Photo Camera、CPU render、batches、SetPass、overdrawの残件を完了またはRelease Notesへ明記した
- [x] `THIRD_PARTY_NOTICES.md`とPlus内のnoticeを同梱した
- [x] 公開候補シーンにWorld Blueprint IDや認証情報が残っていない

## GitHub

- [x] `main`へpushした
- [x] Repository Actionsを有効にした
- [x] Settings > Pages > SourceをGitHub Actionsにした
- [x] `Build Release`を実行し、Core/Plusの別Releaseを確認した
- [x] VPM用ZIPとpackage.jsonが各Releaseにある
- [x] `Build VPM Listing`成功後、`https://fuyutsukinatsuki.github.io/fShader/index.json`を開ける
- [ ] VCCへ一覧URLを追加し、新規Worldプロジェクトへ導入できる

## Booth

- [x] `Build-BoothPackages.ps1`でLite / Plus UnityPackageとBooth ZIPを生成した
- [x] Booth ZIPのライセンス、第三者通知、日本語READMEを確認した
- [x] Lite単体、LTCGI 1.6.3 → Lite → Plusの実Importに成功した
- [ ] Boothへ商品登録し、ダウンロード物を再確認した

## 公開判定

Public repository、Core / Plus Release、GitHub Pagesの到達性は確認済み。残る公開ゲートはVCC UI実導入とBooth登録・再ダウンロード確認です。

## ローカルRC検証記録（2026-07-18）

- Core ZIP単体: Unity exit 0、C# / Shader error 0。
- Core + Plus ZIP、VRCSDK 3.10.4、LTCGI 1.6.3 full package: fShader 73 / 73 Passed。
- 削除確認: fShader package directory 0、fShader由来自動生成Assets 0、2回目再読込でC# / Missing Script error 0。
- Core SHA-256: `3d96b02de8f80bc4dc4cd4b2829abafbf33fc147d1d7191f98cae589da720185`
- Plus SHA-256: `d124e2c27a3858175b55c88300a5653c30d98dddca7aa16797cb0c29df2795e2`

## Public Release検証記録（2026-07-18）

- Core公開ZIP SHA-256: `24aa93626689cc7482ee6f7da78cec4158c5ac7d8eee77a598c4bfdf8380cda3`
- Plus公開ZIP SHA-256: `b2a35a36d03c77f115a2970b57a368db629a000b30d50f7a5b166eeabbcd35bf`
- VPM `index.json`: HTTP 200、Core / Plus 1.2.3、依存関係、URL、SHA-256確認済み。
