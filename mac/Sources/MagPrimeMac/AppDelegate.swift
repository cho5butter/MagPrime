import AppKit

final class AppDelegate: NSObject, NSApplicationDelegate {
    private var context: AppContext?
    private var statusItem: NSStatusItem?
    private var statusWindow: StatusWindow?

    func applicationDidFinishLaunching(_ notification: Notification) {
        context = AppContext()
        context?.start()
        setupMenuBar()
    }

    func applicationWillTerminate(_ notification: Notification) {
        context?.stop()
    }

    private func setupMenuBar() {
        // Create status bar item with modern icon
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.squareLength)

        if let button = statusItem?.button {
            button.image = NSImage(systemSymbolName: "rectangle.on.rectangle.angled", accessibilityDescription: "MagPrime")
            button.action = #selector(toggleStatusWindow)
            button.target = self
        }

        // Create menu for right-click
        let menu = NSMenu()
        menu.addItem(NSMenuItem(title: "コントロールセンターを開く", action: #selector(showStatusWindow), keyEquivalent: ""))
        menu.addItem(NSMenuItem.separator())
        menu.addItem(NSMenuItem(title: "終了", action: #selector(quitApp), keyEquivalent: "q"))

        statusItem?.menu = menu
    }

    @objc private func toggleStatusWindow() {
        if statusWindow?.isVisible == true {
            statusWindow?.orderOut(nil)
        } else {
            showStatusWindow()
        }
    }

    @objc private func showStatusWindow() {
        if statusWindow == nil {
            let viewModel = StatusViewModel(catalogService: context?.getCatalogService())
            statusWindow = StatusWindow(viewModel: viewModel)
        }

        statusWindow?.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }

    @objc private func quitApp() {
        NSApp.terminate(nil)
    }
}
