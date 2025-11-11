import CoreGraphics
import Foundation

final class WindowCatalogService {
    private let settingsProvider: SettingsProvider
    private let enumerateQueue = DispatchQueue(label: "dev.codex.magprime.catalog", qos: .userInitiated)

    init(settingsProvider: SettingsProvider) {
        self.settingsProvider = settingsProvider
    }

    func getWindows() async -> [WindowDescriptor] {
        await withCheckedContinuation { continuation in
            enumerateQueue.async {
                continuation.resume(returning: self.enumerate())
            }
        }
    }

    private func enumerate() -> [WindowDescriptor] {
        let options: CGWindowListOption = [.optionOnScreenOnly, .excludeDesktopElements]
        guard let infoList = CGWindowListCopyWindowInfo(options, kCGNullWindowID) as? [[String: Any]] else {
            return []
        }

        let excluded = Set(settingsProvider.current.excludedApps.map { $0.lowercased() })
        var descriptors: [WindowDescriptor] = []

        for info in infoList {
            guard
                let ownerName = info[kCGWindowOwnerName as String] as? String,
                let pidValue = info[kCGWindowOwnerPID as String],
                let windowNumberValue = info[kCGWindowNumber as String],
                let boundsDict = info[kCGWindowBounds as String] as? [String: Double]
            else {
                continue
            }

            let pid: pid_t
            if let typed = pidValue as? pid_t {
                pid = typed
            } else if let intVal = pidValue as? Int {
                pid = pid_t(intVal)
            } else {
                continue
            }

            let windowNumber: UInt32
            if let typed = windowNumberValue as? UInt32 {
                windowNumber = typed
            } else if let intVal = windowNumberValue as? Int {
                windowNumber = UInt32(intVal)
            } else {
                continue
            }

            if excluded.contains(ownerName.lowercased()) {
                continue
            }

            let layer = info[kCGWindowLayer as String] as? Int ?? 0
            if layer != 0 {
                continue
            }

            let title = (info[kCGWindowName as String] as? String)?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
            if title.isEmpty {
                continue
            }

            let bounds = CGRect(
                x: boundsDict["X"] ?? 0,
                y: boundsDict["Y"] ?? 0,
                width: boundsDict["Width"] ?? 0,
                height: boundsDict["Height"] ?? 0)

            descriptors.append(
                WindowDescriptor(
                    id: CGWindowID(windowNumber),
                    processIdentifier: pid,
                    ownerName: ownerName,
                    title: title,
                    bounds: bounds,
                    isOnScreen: (info[kCGWindowIsOnscreen as String] as? Bool) ?? true)
            )
        }

        return applySort(descriptors)
    }

    private func applySort(_ windows: [WindowDescriptor]) -> [WindowDescriptor] {
        switch settingsProvider.current.sortMode.lowercased() {
        case "alphabetic":
            return windows.sorted { $0.title.localizedCaseInsensitiveCompare($1.title) == .orderedAscending }
        default:
            return windows
        }
    }
}
