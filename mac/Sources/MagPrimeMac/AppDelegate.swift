import AppKit

final class AppDelegate: NSObject, NSApplicationDelegate {
    private var context: AppContext?

    func applicationDidFinishLaunching(_ notification: Notification) {
        context = AppContext()
        context?.start()
    }

    func applicationWillTerminate(_ notification: Notification) {
        context?.stop()
    }
}
