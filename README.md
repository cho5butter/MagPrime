# MagPrime

マルチディスプレイ環境で右クリックメニューから起動中ウィンドウを一覧し、選択したウィンドウを右クリック位置へ瞬時に移動させる常駐型ユーティリティです。

## 概要

MagPrimeは、画面の一部が他の用途で占有されている場合でも、見えなくなったアプリケーションウィンドウを素早く操作可能にするツールです。右クリックから直感的に全ウィンドウを俯瞰し、任意のディスプレイ上へ再配置できます。

### 主な特徴

- 🖱️ **右クリックメニュー統合** - 任意の場所で右クリックするだけでウィンドウ一覧を表示
- 🪟 **ウィンドウ移動** - 選択したウィンドウを右クリック位置のディスプレイへ即座に移動
- 🎨 **モダンUI** - Fluent Design準拠のWinUI 3ベースの管理画面
- ⚡ **高性能** - メモリ使用量30MB以内、待機時CPU使用率1%未満
- 🔧 **柔軟な設定** - 除外プロセス、最大表示件数、トースト通知などをカスタマイズ可能

## 対応プラットフォーム

- **Windows**: Windows 11/10 (64bit) - WinUI 3 / Windows App SDK
- **macOS**: macOS 13 Ventura以降 - Swift / AppKit

## システム要件

### Windows版

- Windows 11 または Windows 10 (64bit)
- .NET 8 Runtime
- Windows App SDK 1.5以降

### macOS版

- macOS 13 Ventura以降
- Xcodeコマンドラインツール（ビルド時）
- アクセシビリティ権限の付与が必要

## インストール

### Windows

1. [Releases](https://github.com/cho5butter/MagPrime/releases)から最新版をダウンロード
2. インストーラーを実行
3. アプリケーションを起動すると、自動的にバックグラウンドで常駐開始

### macOS

1. [Releases](https://github.com/cho5butter/MagPrime/releases)から最新版をダウンロード
2. アプリケーションを起動
3. 初回起動時にアクセシビリティ権限の付与ダイアログが表示されるので、許可してください

## 使用方法

### 基本的な使い方

1. **MagPrimeを起動** - アプリケーションが起動すると、グローバル右クリックフックが有効化されます
2. **右クリック** - デスクトップ上の任意の場所で右クリック
3. **ウィンドウを選択** - コンテキストメニューに表示される起動中ウィンドウ一覧から移動したいウィンドウを選択
4. **自動移動** - 選択したウィンドウが右クリック位置のディスプレイ中央付近へ移動し、最前面化されます

### 管理画面

MagPrimeのメインウィンドウでは以下の操作が可能です：

- **サービスのON/OFF** - グローバルフックの有効/無効を切り替え
- **ウィンドウ一覧の表示** - 現在監視中のウィンドウを確認
- **最新の状態を取得** - ボタンクリックでウィンドウ情報を更新

## 設定

設定ファイルは `%AppData%\MagPrime\settings.json` に保存されます。

### 設定項目

```json
{
  "LaunchOnStartup": true,
  "MaxMenuItems": 12,
  "SortMode": "Recent",
  "ExcludedProcesses": [],
  "EnableRestore": false,
  "ShowToast": true
}
```

| 項目 | 説明 | デフォルト |
|------|------|-----------|
| `LaunchOnStartup` | Windows起動時に自動起動 | `true` |
| `MaxMenuItems` | 右クリックメニューに表示する最大ウィンドウ数 | `12` |
| `SortMode` | ウィンドウの並び順 (`Recent` または `Alphabetic`) | `"Recent"` |
| `ExcludedProcesses` | 除外するプロセス名のリスト | `[]` |
| `EnableRestore` | 元位置への復元機能を有効化（将来の機能） | `false` |
| `ShowToast` | トースト通知を表示 | `true` |

### 特定のアプリケーションを除外する

```json
{
  "ExcludedProcesses": ["explorer", "taskmgr", "SystemSettings"]
}
```

## アーキテクチャ

### 技術スタック

- **フレームワーク**: .NET 8 + WinUI 3 (Windows App SDK 1.5)
- **依存性注入**: Microsoft.Extensions.DependencyInjection
- **ロギング**: Microsoft.Extensions.Logging
- **UIツールキット**: CommunityToolkit.WinUI

### 主要コンポーネント

```
MagPrime/
├── Services/
│   ├── InputHookService       # グローバル右クリックフック
│   ├── ContextMenuPresenter   # コンテキストメニュー表示
│   ├── WindowCatalogService   # ウィンドウ列挙・管理
│   ├── WindowMoverService     # ウィンドウ移動・最前面化
│   ├── ToastNotificationService # トースト通知
│   └── BootstrapService       # サービスの統合・起動管理
├── ViewModels/
│   └── MainWindowViewModel    # メインウィンドウのビジネスロジック
├── Configuration/
│   └── SettingsProvider       # 設定ファイル管理
├── Interop/
│   └── NativeMethods          # Win32 API呼び出し
└── Models/
    ├── WindowDescriptor       # ウィンドウ情報
    ├── MenuRequest            # メニュー要求
    └── AppSettings            # アプリ設定
```

### データフロー

```
右クリック入力
    ↓
InputHookService (低レベルマウスフック)
    ↓
BootstrapService (イベント処理)
    ↓
WindowCatalogService (ウィンドウ列挙)
    ↓
ContextMenuPresenter (メニュー表示)
    ↓
WindowMoverService (ウィンドウ移動)
    ↓
ToastNotificationService (通知)
```

## 開発

### Windows版

#### 前提条件

- Visual Studio 2022 (17.8以降)
- .NET 8 SDK
- Windows App SDK 1.5

#### ビルド

```bash
cd win
dotnet build MagPrime.sln
```

#### デバッグ実行

```bash
dotnet run --project MagPrime/MagPrime.csproj
```

### macOS版

#### 前提条件

- macOS 13 Ventura以降
- Xcode 15以降（Swift 5.9）
- Xcodeコマンドラインツール

#### ビルド

```bash
cd mac
swift build
```

#### デバッグ実行

```bash
cd mac
swift run MagPrimeMac
```

### プロジェクト構成

プロジェクトはMVVMパターンとクリーンアーキテクチャの原則に従っています：

- **依存性注入**: すべてのサービスはインターフェースを通じて注入
- **関心の分離**: UI、ビジネスロジック、データアクセスを明確に分離
- **イミュータブルモデル**: `record`型を使用したイミュータブルなデータモデル
- **非同期プログラミング**: すべてのI/O操作は非同期で実装

## トラブルシューティング

### 右クリックメニューが表示されない

1. 管理画面でサービスが有効になっているか確認
2. セキュリティソフトがフックをブロックしていないか確認
3. アプリケーションを管理者として実行してみる

### 特定のウィンドウが移動できない

管理者権限で実行されているウィンドウは、MagPrimeも管理者権限で起動する必要があります。

### パフォーマンスが低下する

1. `MaxMenuItems`の値を減らす
2. `ExcludedProcesses`に不要なプロセスを追加
3. ログファイルのサイズを確認（`%AppData%\MagPrime\logs`）

## ロードマップ

- [ ] 元位置復元機能の実装
- [ ] ホットキーサポート
- [ ] カスタムメニューアイコン
- [x] macOS版の開発
- [ ] 自動更新機能

## ライセンス

このプロジェクトのライセンス情報は[LICENSE](LICENSE)ファイルを参照してください。

## コントリビューション

プルリクエストや Issue の投稿を歓迎します。大きな変更を行う場合は、まず Issue を作成して変更内容について議論してください。

## 作者

cho5butter

## 謝辞

- Microsoft - Windows App SDK / WinUI 3
- CommunityToolkit - WinUI コンポーネント
