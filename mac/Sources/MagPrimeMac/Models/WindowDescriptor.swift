import CoreGraphics
import Foundation

struct WindowDescriptor: Identifiable {
    let id: CGWindowID
    let processIdentifier: pid_t
    let ownerName: String
    let title: String
    let bounds: CGRect
    let isOnScreen: Bool

    var size: CGSize { bounds.size }
}
