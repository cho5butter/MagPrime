import CoreGraphics
import Foundation

struct MenuRequest {
    let timestamp: Date
    let cursorPosition: CGPoint
    let eventWindowNumber: CGWindowID?
    let processIdentifier: pid_t?
}
