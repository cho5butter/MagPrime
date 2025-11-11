# MagPrime for macOS

MagPrime の macOS 版は Swift と AppKit を用いた常駐ユーティリティで、右クリックをフックして稼働中のアプリウィンドウを現在のディスプレイへ移動します。`Package.swift` を含む SwiftPM プロジェクトなので、Xcode もしくは `swift build` でビルドできます。

## 前提条件
- macOS 13 Ventura 以降
- Xcode 15 以降（Swift 5.9）
- システム環境設定 > プライバシーとセキュリティで「アクセシビリティ」権限を付与（ウィンドウ移動に必要）

## 実行方法
```bash
cd mac
swift run MagPrimeMac
```

初回起動時にアクセシビリティ権限の付与ダイアログが表示されるので、MagPrimeMac バイナリを許可してください。

## 主なコンポーネント
- `InputHookService`: `CGEventTap` で右クリックを捕捉
- `WindowCatalogService`: `CGWindowListCopyWindowInfo` でウィンドウ情報を列挙
- `ContextMenuPresenter`: `NSMenu` によるネイティブなポップアップメニュー
- `WindowMoverService`: `AXUIElement` を操作してウィンドウを移動・フォーカス
- `SettingsProvider`: `~/Library/Application Support/MagPrime/settings.json` を読み書き

Windows 実装と同様に `BootstrapService` が各サービスを束ね、イベントドリブンで動作します。
