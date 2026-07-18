# fShader 1.0.0 リリースチェックリスト

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

- [ ] `main`へpushした
- [ ] Repository Actionsを有効にした
- [ ] Settings > Pages > SourceをGitHub Actionsにした
- [ ] `Build Release`を実行し、Core/Plusの別Releaseを確認した
- [ ] VPM用ZIPとpackage.jsonが各Releaseにある
- [ ] `Build VPM Listing`成功後、`https://fuyutsukinatsuki.github.io/fShader/index.json`を開ける
- [ ] VCCへ一覧URLを追加し、新規Worldプロジェクトへ導入できる

## 公開判定

Privateのままでは一般VCCからGitHub ReleaseとPagesへアクセスできません。公開配布時はリポジトリ、Release、Pagesの到達性、ライセンス条件と同梱ファイルを必ず再確認してください。
## ローカルRC検証記録（2026-07-18）

- Core ZIP単体: Unity exit 0、C# / Shader error 0。
- Core + Plus ZIP、VRCSDK 3.10.4、LTCGI 1.6.3 full package: fShader 73 / 73 Passed。
- 削除確認: fShader package directory 0、fShader由来自動生成Assets 0、2回目再読込でC# / Missing Script error 0。
- Core SHA-256: `3d96b02de8f80bc4dc4cd4b2829abafbf33fc147d1d7191f98cae589da720185`
- Plus SHA-256: `d124e2c27a3858175b55c88300a5653c30d98dddca7aa16797cb0c29df2795e2`
