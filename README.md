# WaterEffect

WaterEffect is a simple iOS sample that demonstrates a dynamic water ripple effect. It is written in C# using Xamarin.iOS. The sample includes an `EnhancedRippleViewController` that renders ripples with SkiaSharp and an `OpenGLWaterEffectViewController` that renders a basic effect with OpenGL.

## Prerequisites

- macOS with [Xamarin.iOS](https://learn.microsoft.com/xamarin/ios/) installed. The easiest way to get started is using **Visual Studio for Mac** or **Visual Studio** on Windows paired with a Mac build host.
- An iOS device or simulator to run the application.

## Building and Running

1. Clone this repository.
2. Open `WaterEffect.sln` in Visual Studio.
3. Restore NuGet packages if prompted.
4. Build the `WaterEffect` project.
5. Deploy to an iOS simulator or a connected device.

Running the application will display a view where tapping or dragging generates water ripples.

## Splashing Water

The enhanced ripple controller now tracks velocity for each point on the water surface.
Touches inject momentum into the velocity maps so quick taps create splash-like effects.
A small particle sprite (`Resources/Particles.png`) is drawn whenever the velocity is
high enough, giving visual feedback of splashing water.

