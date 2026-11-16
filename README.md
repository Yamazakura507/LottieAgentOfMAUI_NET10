# LottieAgent

**LottieAgent** is a .NET MAUI control library for displaying and controlling Lottie animations in JSON format. Rendering is powered by **SkiaSharp.Views.Maui** and the **SkiaSharp.Skottie** animation engine.

In addition to playback, the library can modify individual elements of a Lottie composition before it is loaded by Skottie. Supported modifications include replacing colors and text, and changing opacity, position, scale, and rotation.

## Features

- Display Lottie JSON animations in MAUI using **LottieView**.
- Control playback with play, pause, and stop operations.
- Enable looping and configure playback speed.
- Get and set the current animation position (**Progress**, from **0** to **1**).
- Modify animation content through the strongly typed **LottieModifierCollection**.
- Handle loading errors and receive notifications when a modifier cannot be applied.
- Replace the JSON loader and modifier processor through interfaces and Dependency Injection.

## Technology Stack

| Component | Purpose |
|---|---|
| .NET 10 | Project target framework |
| .NET MAUI | UI control and bindable properties |
| SkiaSharp.Views.Maui.Controls 4.151.2 | Rendering surface based on SKCanvasView |
| SkiaSharp.Skottie 4.151.2 | Lottie animation loading and rendering |
| System.Text.Json | Reading and modifying Lottie JSON |
| Microsoft.Extensions.DependencyInjection | Service and handler registration |

> **Current platform configuration:** The project file currently targets only **net10.0-windows10.0.19041.0**. The minimum Windows version specified in the project settings is **10.0.17763.0**. Using MAUI and SkiaSharp does not by itself mean that Android, iOS, or macOS are included in the current build configuration.

## Repository Structure

~~~text
LottieAgentOfMAUI_NET10/
├── LICENSE
├── README.md
└── LottieAgent/
    ├── Controls/
    │   ├── LottieView.xaml
    │   └── LottieView.xaml.cs
    ├── Enums/
    │   ├── LottieModifierOperation.cs
    │   └── LottieTransformProperty.cs
    ├── Extensions/
    │   ├── LottieAgentServiceCollectionExtensions.cs
    │   └── LottieJsonOperations.cs
    ├── Handlers/
    │   ├── Abstract/
    │   ├── OpacityModifierHandler.cs
    │   ├── RotationModifierHandler.cs
    │   ├── TransformModifierHandler.cs
    │   └── ValueTypeModifierHandler.cs
    ├── Interfaces/
    ├── Models/
    ├── Services/
    │   ├── LottieJsonLoader.cs
    │   └── LottieModifierProcessor.cs
    └── LottieAgent.csproj
~~~

## Adding LottieAgent to a MAUI Application

Add a project reference to the library in your application project file:

~~~xml
<ItemGroup>
    <ProjectReference Include="../LottieAgent/LottieAgent.csproj" />
</ItemGroup>
~~~

Register LottieAgent services in **MauiProgram.cs**. Configure SkiaSharp for the MAUI application as well:

~~~csharp
using LottieAgent.Extensions;
using SkiaSharp.Views.Maui.Controls.Hosting;

builder
    .UseMauiApp<App>()
    .UseSkiaSharp();

builder.Services.AddLottieAgent();
~~~

The **AddLottieAgent()** method registers the default JSON loader, modifier processor, built-in modifier handlers, and **LottieView** with a transient lifetime. The control can also be created directly from XAML without DI; in that case, its built-in service implementations are used.

## Loading a Lottie Animation

When using the default **MauiLottieJsonLoader**, the JSON file must be included in the application package as a MAUI asset. For example, place the file in the following directory:

~~~text
Resources/
└── Raw/
    └── Animations/
        └── loading.json
~~~

Set the **Source** property to the asset path relative to **Resources/Raw**:

~~~xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:lottie="clr-namespace:LottieAgent.Controls;assembly=LottieAgent"
    x:Class="MyApp.MainPage">

    <lottie:LottieView
        x:Name="LoadingAnimation"
        Source="Animations/loading.json"
        IsPlaying="True"
        IsLooping="True"
        Speed="1" />

</ContentPage>
~~~

The default loader uses **FileSystem.OpenAppPackageFileAsync**, so **Source** must be a path to a file packaged with the application. It is not a URL or an arbitrary file-system path. To load JSON from another source, implement **ILottieJsonLoader** and provide your implementation to the control or through DI.

## Playback Control

**LottieView** provides the following methods:

~~~csharp
LoadingAnimation.Play();   // Start playback
LoadingAnimation.Pause();  // Pause playback
LoadingAnimation.Stop();   // Stop playback and reset Progress to 0
~~~

The control exposes these main properties:

| Property | Type | Description |
|---|---|---|
| **Source** | **string?** | Path to the Lottie JSON asset in the application package |
| **IsPlaying** | **bool** | Playback state; defaults to **false** |
| **IsLooping** | **bool** | Enables looping; defaults to **true** |
| **Speed** | **double** | Playback speed multiplier; must be greater than zero |
| **Progress** | **double** | Animation position from **0** to **1**; supports TwoWay binding |
| **Modifiers** | **LottieModifierCollection** | Collection of modifiers applied to the source JSON |
| **JsonLoader** | **ILottieJsonLoader** | Source JSON loader |
| **ModifierProcessor** | **ILottieModifierProcessor** | Modifier processor |

Playback is automatically paused when the control is unloaded (**Unloaded**).

## Modifying a Lottie Animation

Modifications are defined through **LottieModifierCollection**. Each modifier targets a Lottie element by its **nm** field in the animation JSON. Name matching is case-insensitive. When multiple elements have the same name, the modifier is applied to every matching element.

Example:

~~~csharp
using LottieAgent.Models;
using Microsoft.Maui.Graphics;

LoadingAnimation.Modifiers = new LottieModifierCollection()
    .SetColor("AccentShape", Color.FromArgb("#FF5A36"))
    .SetText("Title", "Hello, MAUI!")
    .SetOpacity("Icon", 0.8)
    .MultiplyOpacity("Shadow", 0.5)
    .SetPosition("Icon", new Point(100, 200))
    .OffsetPosition("Icon", new Point(10, -5))
    .SetScale("Icon", 1.2)
    .MultiplyScale("Icon", new Point(1.1, 0.9))
    .SetRotation("Icon", 45)
    .OffsetRotation("Icon", 15);
~~~

The collection provides the following operations:

| Method | Description |
|---|---|
| **SetColor(name, color)** | Replaces fill and stroke colors within the target element |
| **SetText(name, text)** | Replaces text content |
| **SetOpacity(name, opacity)** | Sets opacity to a value between **0** and **1** |
| **MultiplyOpacity(name, factor)** | Multiplies existing opacity values by the specified factor |
| **SetPosition(name, point)** | Sets the position |
| **OffsetPosition(name, offset)** | Offsets the current position |
| **SetScale(name, scale)** | Sets the scale |
| **MultiplyScale(name, factor)** | Multiplies the current scale by the specified factor |
| **SetRotation(name, degrees)** | Sets the rotation angle in degrees |
| **OffsetRotation(name, degrees)** | Adds the specified number of degrees to the current rotation angle |

**SetScale** and **MultiplyScale** support both uniform scaling using a **double** value and non-uniform scaling using a **Point** with separate X and Y values. For example, a scale of **1.2** represents 120%, while **1.0** represents 100%.

The collection is observable. Adding, removing, or changing its contents triggers reprocessing of the original JSON and recreation of the animation. The original JSON is retained separately, so applying modifiers again does not accumulate changes on top of a previously modified version.

## Events

**LottieView** exposes events for monitoring animation loading and modifier application:

~~~csharp
LoadingAnimation.AnimationLoaded += (_, _) =>
{
    Debug.WriteLine("Lottie animation loaded.");
};

LoadingAnimation.AnimationFailed += exception =>
{
    Debug.WriteLine($"Lottie animation error: {exception}");
};

LoadingAnimation.ModifierNotApplied += targetName =>
{
    Debug.WriteLine($"Could not apply modifier to target: {targetName}");
};
~~~

- **AnimationLoaded** — the animation was successfully created or recreated.
- **AnimationFailed** — an error occurred while loading the JSON or creating the animation.
- **ModifierNotApplied** — the modifier could not be applied to the specified target. The element may not exist, may not contain the required property, or may not have a registered handler for its value type.

## Building

Building the project requires a Windows environment with the .NET 10 SDK and the .NET MAUI workload installed.

Install the MAUI workload if necessary:

~~~powershell
dotnet workload install maui
~~~

Restore dependencies and build the project from the repository root:

~~~powershell
dotnet restore ./LottieAgent/LottieAgent.csproj
dotnet build ./LottieAgent/LottieAgent.csproj --configuration Release --framework net10.0-windows10.0.19041.0
~~~

## License

This project is licensed under the [MIT License](LICENSE).
