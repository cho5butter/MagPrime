import Foundation

final class SettingsProvider {
    private let settingsURL: URL
    private let queue = DispatchQueue(label: "dev.codex.magprime.settings", attributes: .concurrent)
    private var cachedSettings = AppSettings()

    init(fileManager: FileManager = .default) {
        let base = fileManager.urls(for: .applicationSupportDirectory, in: .userDomainMask).first ?? URL(fileURLWithPath: NSHomeDirectory())
        let directory = base.appendingPathComponent("MagPrime", isDirectory: true)
        try? fileManager.createDirectory(at: directory, withIntermediateDirectories: true)
        settingsURL = directory.appendingPathComponent("settings.json")
        loadFromDisk()
    }

    var current: AppSettings {
        queue.sync { cachedSettings }
    }

    func reload() {
        queue.sync(flags: .barrier) {
            loadFromDisk()
        }
    }

    private func loadFromDisk() {
        do {
            let data = try Data(contentsOf: settingsURL)
            let decoded = try JSONDecoder().decode(AppSettings.self, from: data)
            cachedSettings = decoded
        } catch {
            cachedSettings = AppSettings()
            persistDefaults()
        }
    }

    private func persistDefaults() {
        do {
            let encoder = JSONEncoder()
            encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
            let data = try encoder.encode(cachedSettings)
            try data.write(to: settingsURL, options: .atomic)
        } catch {
            Log.error("Failed to persist default settings: \(error)")
        }
    }
}
