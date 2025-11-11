import ApplicationServices
import CoreGraphics
import Foundation

protocol InputHookServiceDelegate: AnyObject {
    func inputHookService(_ service: InputHookService, didReceive request: MenuRequest)
}

final class InputHookService {
    weak var delegate: InputHookServiceDelegate?

    private var eventTap: CFMachPort?
    private var runLoopSource: CFRunLoopSource?
    private let queue = DispatchQueue(label: "dev.codex.magprime.hooks")
    private var backgroundRunLoop: CFRunLoop?

    func start() {
        guard eventTap == nil else { return }

        let mask = (1 << CGEventType.rightMouseUp.rawValue)
        guard let tap = CGEvent.tapCreate(
            tap: .cghidEventTap,
            place: .headInsertEventTap,
            options: .defaultTap,
            eventsOfInterest: CGEventMask(mask),
            callback: { _, type, event, refcon in
                guard let refcon else { return Unmanaged.passUnretained(event) }
                let service = Unmanaged<InputHookService>.fromOpaque(refcon).takeUnretainedValue()
                service.handle(event: event, type: type)
                return Unmanaged.passUnretained(event)
            },
            userInfo: UnsafeMutableRawPointer(Unmanaged.passUnretained(self).toOpaque())) else {
            Log.error("Failed to create CGEventTap. Ensure the app has the required privileges.")
            return
        }

        eventTap = tap
        runLoopSource = CFMachPortCreateRunLoopSource(kCFAllocatorDefault, tap, 0)

        queue.async { [weak self] in
            guard let self, let source = self.runLoopSource else { return }
            let loop = CFRunLoopGetCurrent()
            self.backgroundRunLoop = loop
            CFRunLoopAddSource(loop, source, .commonModes)
            CGEvent.tapEnable(tap: tap, enable: true)
            CFRunLoopRun()
            CFRunLoopRemoveSource(loop, source, .commonModes)
        }

        Log.info("InputHookService started.")
    }

    func stop() {
        guard let tap = eventTap else { return }
        CGEvent.tapEnable(tap: tap, enable: false)
        if let loop = backgroundRunLoop {
            CFRunLoopStop(loop)
            backgroundRunLoop = nil
        }
        eventTap = nil
        runLoopSource = nil
        Log.info("InputHookService stopped.")
    }

    private func handle(event: CGEvent?, type: CGEventType) {
        guard type == .rightMouseUp, let event else { return }
        let location = event.location
        let pid = pid_t(event.getIntegerValueField(.eventTargetUnixProcessID))
        let windowNumber = CGWindowID(event.getIntegerValueField(.eventTargetWindowNumber))
        let request = MenuRequest(
            timestamp: Date(),
            cursorPosition: location,
            eventWindowNumber: windowNumber == 0 ? nil : windowNumber,
            processIdentifier: pid == 0 ? nil : pid)

        Task { @MainActor [weak self] in
            guard let self else { return }
            self.delegate?.inputHookService(self, didReceive: request)
        }
    }
}
