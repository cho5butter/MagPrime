// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "MagPrimeMac",
    platforms: [
        .macOS(.v13)
    ],
    products: [
        .executable(name: "MagPrimeMac", targets: ["MagPrimeMac"])
    ],
    targets: [
        .executableTarget(
            name: "MagPrimeMac",
            dependencies: [],
            path: "Sources/MagPrimeMac",
            swiftSettings: [
                .enableExperimentalFeature("StrictConcurrency")
            ]
        )
    ]
)
