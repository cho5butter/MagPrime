import AppKit
import ApplicationServices

final class AppContext {
    private let settingsProvider = SettingsProvider()
    private lazy var catalogService = WindowCatalogService(settingsProvider: settingsProvider)
    private lazy var menuPresenter = ContextMenuPresenter(settingsProvider: settingsProvider)
    private lazy var moverService = WindowMoverService()
    private lazy var toastService = ToastNotificationService(settingsProvider: settingsProvider)
    private lazy var hookService = InputHookService()
    private lazy var bootstrap = BootstrapService(
        hookService: hookService,
        windowCatalog: catalogService,
        menuPresenter: menuPresenter,
        windowMover: moverService,
        toastService: toastService,
        settingsProvider: settingsProvider)

    func start() {
        requestAccessibilityTrustIfNeeded()
        bootstrap.start()
    }

    func stop() {
        bootstrap.stop()
    }

    func getCatalogService() -> WindowCatalogService {
        return catalogService
    }

    private func requestAccessibilityTrustIfNeeded() {
        let trusted = AXIsProcessTrusted()
        if !trusted {
            let options = [kAXTrustedCheckOptionPrompt.takeUnretainedValue() as String: true] as CFDictionary
            AXIsProcessTrustedWithOptions(options)
        }
    }
}
