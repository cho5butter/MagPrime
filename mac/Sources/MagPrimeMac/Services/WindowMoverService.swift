import AppKit
import ApplicationServices
import CoreGraphics
import Foundation

enum WindowMoverError: Error {
    case accessibilityDisabled
    case windowNotFound
}

final class WindowMoverService {
    private let moveQueue = DispatchQueue(label: "dev.codex.magprime.mover", qos: .userInitiated)

    func move(window descriptor: WindowDescriptor, request: MenuRequest) async throws {
        try await withCheckedThrowingContinuation { continuation in
            moveQueue.async {
                do {
                    try self.performMove(window: descriptor, request: request)
                    continuation.resume()
                } catch {
                    continuation.resume(throwing: error)
                }
            }
        }
    }

    private func performMove(window descriptor: WindowDescriptor, request: MenuRequest) throws {
        guard AXIsProcessTrusted() else {
            throw WindowMoverError.accessibilityDisabled
        }

        guard let targetScreen = NSScreen.screen(containing: request.cursorPosition) ?? NSScreen.main else {
            throw WindowMoverError.windowNotFound
        }

        let visibleFrame = targetScreen.visibleFrame
        let width = descriptor.bounds.width > 0 ? descriptor.bounds.width : 800
        let height = descriptor.bounds.height > 0 ? descriptor.bounds.height : 600
        let targetPoint = CGPoint(
            x: visibleFrame.midX - width / 2,
            y: visibleFrame.midY - height / 2)
        let targetSize = CGSize(width: width, height: height)

        guard let axWindow = accessibilityWindow(for: descriptor) else {
            throw WindowMoverError.windowNotFound
        }

        setAXValue(axWindow, attribute: kAXPositionAttribute, point: targetPoint)
        setAXValue(axWindow, attribute: kAXSizeAttribute, size: targetSize)

        if let app = NSRunningApplication(processIdentifier: descriptor.processIdentifier) {
            app.activate(options: [.activateIgnoringOtherApps])
        }
    }

    private func accessibilityWindow(for descriptor: WindowDescriptor) -> AXUIElement? {
        let appElement = AXUIElementCreateApplication(descriptor.processIdentifier)
        var windowListRef: CFTypeRef?
        guard AXUIElementCopyAttributeValue(appElement, kAXWindowsAttribute as CFString, &windowListRef) == .success,
              let windowArray = windowListRef as? [AXUIElement] else {
            return nil
        }

        for element in windowArray {
            if matches(element, descriptor: descriptor) {
                return element
            }
        }

        return nil
    }

    private func matches(_ element: AXUIElement, descriptor: WindowDescriptor) -> Bool {
        if let title = copyStringAttribute(element, attribute: kAXTitleAttribute), !title.isEmpty {
            if title == descriptor.title {
                return true
            }
        }

        var position = CGPoint.zero
        var size = CGSize.zero
        let hasPosition = copyPointAttribute(element, attribute: kAXPositionAttribute, into: &position)
        let hasSize = copySizeAttribute(element, attribute: kAXSizeAttribute, into: &size)

        if hasPosition && hasSize {
            let currentRect = CGRect(origin: position, size: size)
            if currentRect.center.distance(to: descriptor.bounds.center) < 10 {
                return true
            }
        }

        return false
    }

    private func copyStringAttribute(_ element: AXUIElement, attribute: String) -> String? {
        var value: CFTypeRef?
        guard AXUIElementCopyAttributeValue(element, attribute as CFString, &value) == .success else {
            return nil
        }
        return value as? String
    }

    private func copyPointAttribute(_ element: AXUIElement, attribute: String, into point: inout CGPoint) -> Bool {
        var value: CFTypeRef?
        guard AXUIElementCopyAttributeValue(element, attribute as CFString, &value) == .success,
              let axValue = value as? AXValue,
              AXValueGetType(axValue) == .cgPoint,
              AXValueGetValue(axValue, .cgPoint, &point) else {
            return false
        }
        return true
    }

    private func copySizeAttribute(_ element: AXUIElement, attribute: String, into size: inout CGSize) -> Bool {
        var value: CFTypeRef?
        guard AXUIElementCopyAttributeValue(element, attribute as CFString, &value) == .success,
              let axValue = value as? AXValue,
              AXValueGetType(axValue) == .cgSize,
              AXValueGetValue(axValue, .cgSize, &size) else {
            return false
        }
        return true
    }

    private func setAXValue(_ element: AXUIElement, attribute: String, point: CGPoint) {
        var mutablePoint = point
        if let axValue = AXValueCreate(.cgPoint, &mutablePoint) {
            AXUIElementSetAttributeValue(element, attribute as CFString, axValue)
        }
    }

    private func setAXValue(_ element: AXUIElement, attribute: String, size: CGSize) {
        var mutableSize = size
        if let axValue = AXValueCreate(.cgSize, &mutableSize) {
            AXUIElementSetAttributeValue(element, attribute as CFString, axValue)
        }
    }
}

private extension CGRect {
    var center: CGPoint { CGPoint(x: midX, y: midY) }
}

private extension CGPoint {
    func distance(to other: CGPoint) -> CGFloat {
        hypot(other.x - x, other.y - y)
    }
}

private extension NSScreen {
    static func screen(containing point: CGPoint) -> NSScreen? {
        for screen in NSScreen.screens {
            if screen.frame.contains(point) {
                return screen
            }
        }
        return nil
    }
}
