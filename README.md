# WaterEffect

WaterEffect is a simple iOS sample that demonstrates a dynamic water ripple effect. Originally written in C# using Xamarin.iOS, the repository now also contains a Swift implementation that can be opened directly in Xcode.

## Prerequisites

- Xcode 13 or newer.
- An iOS device or simulator to run the application.

## Building and Running (Swift Version)

1. Navigate to the `WaterEffectSwift` folder.
2. Open `Package.swift` in Xcode (for example by running `open Package.swift`).
3. Build and run the app on a simulator or device.

The Swift version replicates the ripple effect using `UIKit` without external dependencies.

## Building and Running (Original Xamarin Version)

1. Open `WaterEffect.sln` in Visual Studio.
2. Restore NuGet packages if prompted.
3. Build the `WaterEffect` project.
4. Deploy to an iOS simulator or a connected device.

Running either version will display a view where tapping or dragging generates water ripples.
