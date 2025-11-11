import Foundation
import os.log

enum Log {
    private static let subsystem = "dev.codex.magprime"

    static func info(_ message: String) {
        os_log("%@", log: OSLog(subsystem: subsystem, category: "info"), type: .info, message)
    }

    static func warning(_ message: String) {
        os_log("%@", log: OSLog(subsystem: subsystem, category: "warning"), type: .default, message)
    }

    static func error(_ message: String) {
        os_log("%@", log: OSLog(subsystem: subsystem, category: "error"), type: .error, message)
    }
}
