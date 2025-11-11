import Foundation

final class BootstrapService: NSObject {
    private let hookService: InputHookService
    private let windowCatalog: WindowCatalogService
    private let menuPresenter: ContextMenuPresenter
    private let windowMover: WindowMoverService
    private let toastService: ToastNotificationService
    private let settingsProvider: SettingsProvider
    private var isRunning = false

    init(
        hookService: InputHookService,
        windowCatalog: WindowCatalogService,
        menuPresenter: ContextMenuPresenter,
        windowMover: WindowMoverService,
        toastService: ToastNotificationService,
        settingsProvider: SettingsProvider)
    {
        self.hookService = hookService
        self.windowCatalog = windowCatalog
        self.menuPresenter = menuPresenter
        self.windowMover = windowMover
        self.toastService = toastService
        self.settingsProvider = settingsProvider
    }

    func start() {
        guard !isRunning else { return }
        hookService.delegate = self
        hookService.start()
        isRunning = true
        Log.info("BootstrapService started.")
    }

    func stop() {
        guard isRunning else { return }
        hookService.stop()
        hookService.delegate = nil
        isRunning = false
        Log.info("BootstrapService stopped.")
    }
}

extension BootstrapService: InputHookServiceDelegate {
    func inputHookService(_ service: InputHookService, didReceive request: MenuRequest) {
        Task.detached { [weak self] in
            await self?.handleMenuRequest(request)
        }
    }

    private func handleMenuRequest(_ request: MenuRequest) async {
        let windows = await windowCatalog.getWindows()
        guard !windows.isEmpty else {
            await toastService.show(title: "MagPrime", body: "移動可能なウィンドウが見つかりませんでした。")
            return
        }

        guard let selection = await menuPresenter.requestSelection(request: request, windows: windows) else {
            Log.info("User dismissed menu or no selection was made.")
            return
        }

        do {
            try await windowMover.move(window: selection, request: request)
            await toastService.show(title: "MagPrime", body: "\(selection.title) を現在の画面へ移動しました。")
        } catch WindowMoverError.accessibilityDisabled {
            await toastService.show(title: "MagPrime", body: "アクセシビリティ権限を付与してください。")
        } catch {
            Log.error("Failed to move window: \(error)")
            await toastService.show(title: "MagPrime", body: "ウィンドウ移動に失敗しました。")
        }
    }
}
