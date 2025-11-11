import AppKit
import CoreGraphics
import Foundation

final class ContextMenuPresenter {
    private let settingsProvider: SettingsProvider

    init(settingsProvider: SettingsProvider) {
        self.settingsProvider = settingsProvider
    }

    func requestSelection(request: MenuRequest, windows: [WindowDescriptor]) async -> WindowDescriptor? {
        guard !windows.isEmpty else { return nil }
        return await MainActor.run {
            let limited = Array(windows.prefix(settingsProvider.current.maxMenuItems))
            return presentMenu(request: request, windows: limited)
        }
    }

    @MainActor
    private func presentMenu(request: MenuRequest, windows: [WindowDescriptor]) -> WindowDescriptor? {
        let menu = NSMenu(title: "MagPrime")
        for (index, descriptor) in windows.enumerated() {
            let label = "\(descriptor.ownerName) | \(descriptor.title)"
            let item = NSMenuItem(title: label, action: nil, keyEquivalent: "")
            item.tag = index
            menu.addItem(item)
        }

        let point = request.cursorPosition
        let selectedItem = menu.popUp(positioning: nil, at: point, in: nil)
        guard let item = selectedItem, item.tag >= 0, item.tag < windows.count else {
            return nil
        }

        return windows[item.tag]
    }
}
