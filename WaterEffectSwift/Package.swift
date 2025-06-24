// swift-tools-version:5.5
import PackageDescription

let package = Package(
    name: "WaterEffectSwift",
    platforms: [
        .iOS(.v13)
    ],
    products: [
        .executable(name: "WaterEffectSwift", targets: ["WaterEffectSwift"])
    ],
    targets: [
        .executableTarget(
            name: "WaterEffectSwift"
        )
    ]
)
