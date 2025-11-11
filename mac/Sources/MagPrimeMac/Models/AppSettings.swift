import Foundation

struct AppSettings: Codable {
    var launchOnStartup: Bool = true
    var maxMenuItems: Int = 12
    var sortMode: String = "Recent" // "Alphabetic" supported
    var excludedApps: [String] = []
    var enableRestore: Bool = false
    var showToast: Bool = true
}
