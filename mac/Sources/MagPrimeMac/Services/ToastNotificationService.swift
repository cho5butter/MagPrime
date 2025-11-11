import AppKit
import Foundation

final class ToastNotificationService {
    private let settingsProvider: SettingsProvider

    init(settingsProvider: SettingsProvider) {
        self.settingsProvider = settingsProvider
    }

    func show(title: String, body: String) async {
        guard settingsProvider.current.showToast else { return }
        await MainActor.run {
            let notification = NSUserNotification()
            notification.title = title
            notification.informativeText = body
            notification.soundName = NSUserNotificationDefaultSoundName
            NSUserNotificationCenter.default.deliver(notification)
        }
    }
}
