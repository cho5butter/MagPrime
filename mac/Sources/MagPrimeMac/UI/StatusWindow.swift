import AppKit
import SwiftUI

/// Modern status window with Liquid Glass design
final class StatusWindow: NSPanel {
    private let viewModel: StatusViewModel

    init(viewModel: StatusViewModel) {
        self.viewModel = viewModel

        // Modern window styling
        super.init(
            contentRect: NSRect(x: 0, y: 0, width: 380, height: 520),
            styleMask: [.titled, .closable, .fullSizeContentView, .nonactivatingPanel],
            backing: .buffered,
            defer: false
        )

        setupWindow()
        setupVisualEffect()
        setupContent()
    }

    private func setupWindow() {
        // Window properties
        titlebarAppearsTransparent = true
        titleVisibility = .hidden
        isMovableByWindowBackground = true
        level = .floating

        // Modern rounded corners
        if #available(macOS 13.0, *) {
            // Enable modern window appearance
            standardWindowButton(.closeButton)?.isHidden = false
            standardWindowButton(.miniaturizeButton)?.isHidden = true
            standardWindowButton(.zoomButton)?.isHidden = true
        }

        // Center on screen
        center()
    }

    private func setupVisualEffect() {
        // Liquid Glass effect - Modern macOS design
        let visualEffect = NSVisualEffectView()
        visualEffect.material = .hudWindow
        visualEffect.state = .active
        visualEffect.blendingMode = .behindWindow

        // Add subtle vibrancy
        if #available(macOS 14.0, *) {
            visualEffect.material = .sidebar
        }

        contentView = visualEffect
    }

    private func setupContent() {
        let hostingView = NSHostingView(rootView: StatusContentView(viewModel: viewModel))
        hostingView.translatesAutoresizingMaskIntoConstraints = false

        contentView?.addSubview(hostingView)

        NSLayoutConstraint.activate([
            hostingView.topAnchor.constraint(equalTo: contentView!.topAnchor),
            hostingView.leadingAnchor.constraint(equalTo: contentView!.leadingAnchor),
            hostingView.trailingAnchor.constraint(equalTo: contentView!.trailingAnchor),
            hostingView.bottomAnchor.constraint(equalTo: contentView!.bottomAnchor)
        ])
    }
}

// MARK: - SwiftUI Content View with Modern Design

struct StatusContentView: View {
    @ObservedObject var viewModel: StatusViewModel

    var body: some View {
        VStack(spacing: 0) {
            // Modern Header with Glow
            headerView
                .padding(.top, 52) // Title bar space
                .padding(.horizontal, 24)
                .padding(.bottom, 20)

            // Content Area
            ScrollView {
                VStack(spacing: 16) {
                    // Service Control Card
                    serviceControlCard

                    // Status Info Card
                    statusInfoCard

                    // Window List
                    windowListCard
                }
                .padding(.horizontal, 24)
                .padding(.bottom, 24)
            }
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }

    // MARK: - Header

    private var headerView: some View {
        HStack(spacing: 16) {
            // App Icon with Gradient Background
            ZStack {
                RoundedRectangle(cornerRadius: 16)
                    .fill(
                        LinearGradient(
                            colors: [.accentColor, .accentColor.opacity(0.7)],
                            startPoint: .topLeading,
                            endPoint: .bottomTrailing
                        )
                    )
                    .frame(width: 56, height: 56)
                    .shadow(color: .accentColor.opacity(0.5), radius: 12, x: 0, y: 4)

                Image(systemName: "rectangle.on.rectangle.angled")
                    .font(.system(size: 24, weight: .semibold))
                    .foregroundColor(.white)
            }

            // Title Section
            VStack(alignment: .leading, spacing: 4) {
                Text("MagPrime Control Center")
                    .font(.title2)
                    .fontWeight(.semibold)

                Text("ウィンドウサーフェスの管理")
                    .font(.subheadline)
                    .foregroundColor(.secondary)
            }

            Spacer()
        }
    }

    // MARK: - Service Control Card

    private var serviceControlCard: some View {
        modernCard {
            HStack {
                VStack(alignment: .leading, spacing: 4) {
                    Text("常駐サービス")
                        .font(.headline)
                        .fontWeight(.semibold)

                    Text("グローバル右クリックフックとウィンドウ移動を制御")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }

                Spacer()

                Toggle("", isOn: $viewModel.serviceActive)
                    .toggleStyle(.switch)
                    .labelsHidden()
            }
            .padding(20)
        }
    }

    // MARK: - Status Info Card

    private var statusInfoCard: some View {
        modernCard {
            HStack(spacing: 12) {
                Image(systemName: "info.circle.fill")
                    .font(.title2)
                    .foregroundColor(.accentColor)

                VStack(alignment: .leading, spacing: 2) {
                    Text("サービス状態")
                        .font(.subheadline)
                        .fontWeight(.semibold)

                    Text(viewModel.statusMessage)
                        .font(.caption)
                        .foregroundColor(.secondary)
                }

                Spacer()
            }
            .padding(16)
        }
        .background(
            RoundedRectangle(cornerRadius: 12)
                .fill(.blue.opacity(0.1))
        )
    }

    // MARK: - Window List Card

    private var windowListCard: some View {
        VStack(spacing: 12) {
            // Header
            HStack {
                Text("監視中のウィンドウ")
                    .font(.headline)
                    .fontWeight(.semibold)

                if viewModel.windowCount > 0 {
                    Text("\(viewModel.windowCount)")
                        .font(.caption)
                        .fontWeight(.bold)
                        .padding(.horizontal, 8)
                        .padding(.vertical, 4)
                        .background(
                            Capsule()
                                .fill(.accentColor)
                        )
                        .foregroundColor(.white)
                }

                Spacer()

                Button(action: viewModel.refresh) {
                    Label("更新", systemImage: "arrow.clockwise")
                        .font(.subheadline)
                }
                .buttonStyle(.borderedProminent)
                .controlSize(.small)
            }

            // Window Items
            modernCard {
                if viewModel.windows.isEmpty {
                    emptyStateView
                } else {
                    VStack(spacing: 8) {
                        ForEach(viewModel.windows) { window in
                            windowRow(window)
                        }
                    }
                    .padding(12)
                }
            }
        }
    }

    private func windowRow(_ window: WindowItem) -> some View {
        HStack(spacing: 12) {
            // Icon
            ZStack {
                RoundedRectangle(cornerRadius: 8)
                    .fill(.accentColor.opacity(0.2))
                    .frame(width: 40, height: 40)

                Image(systemName: window.icon)
                    .font(.system(size: 16))
                    .foregroundColor(.accentColor)
            }

            // Title and subtitle
            VStack(alignment: .leading, spacing: 2) {
                Text(window.title)
                    .font(.subheadline)
                    .fontWeight(.medium)
                    .lineLimit(1)

                Text(window.processName)
                    .font(.caption)
                    .foregroundColor(.secondary)
            }

            Spacer()

            // Dimensions badge
            Text(window.dimensions)
                .font(.caption2)
                .foregroundColor(.secondary)
                .padding(.horizontal, 8)
                .padding(.vertical, 4)
                .background(
                    Capsule()
                        .fill(.secondary.opacity(0.2))
                )
        }
        .padding(12)
        .background(
            RoundedRectangle(cornerRadius: 10)
                .fill(.primary.opacity(0.03))
        )
        .contentShape(RoundedRectangle(cornerRadius: 10))
        .onHover { hovering in
            // Hover effect handled by macOS
        }
    }

    private var emptyStateView: some View {
        VStack(spacing: 12) {
            Image(systemName: "rectangle.on.rectangle.slash")
                .font(.system(size: 48))
                .foregroundColor(.secondary)

            Text("ウィンドウが見つかりません")
                .font(.subheadline)
                .foregroundColor(.secondary)
        }
        .frame(maxWidth: .infinity)
        .padding(40)
    }

    // MARK: - Modern Card Helper

    private func modernCard<Content: View>(@ViewBuilder content: () -> Content) -> some View {
        content()
            .background(
                RoundedRectangle(cornerRadius: 12)
                    .fill(.background)
                    .shadow(color: .black.opacity(0.1), radius: 8, x: 0, y: 2)
            )
    }
}

// MARK: - View Model

final class StatusViewModel: ObservableObject {
    @Published var serviceActive: Bool = false
    @Published var statusMessage: String = "サービスは停止中です"
    @Published var windowCount: Int = 0
    @Published var windows: [WindowItem] = []

    private let catalogService: WindowCatalogService?

    init(catalogService: WindowCatalogService? = nil) {
        self.catalogService = catalogService
        refresh()
    }

    func refresh() {
        guard let catalogService = catalogService else { return }

        windows = catalogService.getVisibleWindows().map { descriptor in
            WindowItem(
                id: descriptor.windowId,
                title: descriptor.title,
                processName: descriptor.processName,
                icon: "app.window.document.fill",
                dimensions: "\(Int(descriptor.bounds.width))×\(Int(descriptor.bounds.height))"
            )
        }

        windowCount = windows.count
        statusMessage = serviceActive ? "サービスは実行中です" : "サービスは停止中です"
    }
}

// MARK: - Window Item Model

struct WindowItem: Identifiable {
    let id: Int
    let title: String
    let processName: String
    let icon: String
    let dimensions: String
}
