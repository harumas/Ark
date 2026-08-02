# Ark

Unity 向けの汎用ゲーム開発基盤ライブラリです。プロジェクトをまたいで使い回せる、小さくテスト済みのプリミティブを集めています。

このリポジトリは Ark を開発・検証するための Unity プロジェクトそのものです。パッケージ本体は [`Assets/Ark`](Assets/Ark) 配下にあり、そこだけを別プロジェクトから参照して使います。

## インストール

対象プロジェクトの `Packages/manifest.json` の `dependencies` に追記してください。

```json
"com.harumas.ark": "https://github.com/harumas/Ark.git?path=Assets/Ark"
```

または Unity エディタの **Package Manager > `+` > Add package from git URL** から同じ URL を指定しても構いません。

モジュール一覧・使用例・テストの実行方法など詳しいドキュメントは [`Assets/Ark/README.md`](Assets/Ark/README.md) を参照してください。

## リポジトリの構成

| パス | 内容 |
|---|---|
| `Assets/Ark/` | Ark パッケージ本体。別プロジェクトからはここだけを参照します |
| `Assets/Game/` | Ark の動作確認用に作った2Dミニゲーム |
| `MathTests/` | `Ark.Mathematics` を Unity を開かずに実行するための、独立した dotnet プロジェクト |
