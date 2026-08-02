# Ark

Unity 向けの汎用ゲーム開発基盤ライブラリです。プロジェクトをまたいで使い回せる、小さくテスト済みのプリミティブを集めています。

## インストール

対象プロジェクトの `Packages/manifest.json` の `dependencies` に追記してください。

```json
"com.harumas.ark": "https://github.com/harumas/Ark.git?path=Assets/Ark"
```

または Unity エディタの **Package Manager > `+` > Add package from git URL** から同じ URL を指定しても構いません。

### 要件

- Unity 6000.5 以降
- Input System (`com.unity.inputsystem`)
- uGUI (`com.unity.ugui`)

依存パッケージは `package.json` に明記済みなので、インストール時に自動で解決されます。

## 構成

| モジュール | 内容 |
|---|---|
| `Ark.Basis` | `Event` / `Timer` / `SingletonMonoBehaviour` など、他の全モジュールが土台にする最小限のプリミティブ |
| `Ark.Input` | Input System の `InputAction` を Guid ベースで扱うラッパーと、ゲームパッド振動の管理 |
| `Ark.Scenes` | フェード演出つきのシーン遷移 |
| `Ark.GUI` | 画面（Canvas）の開閉・重ね順・カーソル表示をまとめて管理するシングルトン |
| `Ark.GR` | プロジェクト内のシングルトンへの参照を1箇所に集約するクラス (Global Reference) |
| `Ark.Mathematics` | Unity の型に依存しない、自作のベクトル・行列などの数学ライブラリ |

## 使い方

### Event — 安全に購読解除できるイベント

`event` と違い、購読の追加・解除で確保が発生しません。1人の購読者が例外を投げても他の購読者には伝わりません。

```csharp
using Ark;

private readonly Event<int> _OnDamaged = new();
public IReadOnlyEvent<int> OnDamaged => _OnDamaged;

_OnDamaged.Subscribe(damage => Debug.Log(damage)).AddTo(this); // GameObject破棄で自動解除
_OnDamaged.Execute(10);
```

### Timer — 確保を伴わない値型タイマー

```csharp
using Ark;

private Timer _Timer = new(3f, autoReset: true);

void Update()
{
    _Timer.Tick(Time.deltaTime);

    if (_Timer.IsOverLimit) Spawn();
}
```

### Ark.Input — Guid ベースの入力

Project-wide Input Actions のアクション名をリネームしても壊れないよう、文字列ではなく Guid で引きます（`InputSystem/GenerateActionName` メニューで自動生成）。

```csharp
using Ark.Input;

var jump = InputProvider.CreateEvent(ActionGuid.Player.Jump);
jump.Performed.Subscribe(_ => Jump()).AddTo(this);
```

### Ark.Scenes — フェード付きシーン遷移

```csharp
using Ark.Scenes;

await SceneLoader.LoadAsync("Battle");
```

### Ark.GUI / Ark.GR — 画面管理

```csharp
using Ark;
using Ark.GUI;

GR.GUIManager.Open(ScreenLayer.Popup, _SettingsPanel);
GR.GUIManager.Close(_SettingsPanel);
```

`GUIManager` はシングルトンです。シーン内のいずれかの GameObject にアタッチしておく必要があります。

### Ark.Mathematics — 自作の数学ライブラリ

```csharp
using Ark.Mathematics;

var reflected = Vec3.Reflect(velocity, normal);
```

## テスト

- `Ark.Mathematics` は Unity Test Framework（EditMode）でテストされています。`Window > General > Test Runner` から実行できます
- リポジトリ直下の `MathTests/` は、Unity を開かずに同じテストを実行するための別プロジェクトです（`dotnet run --project MathTests`）
