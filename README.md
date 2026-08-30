# Material3.Avalonia

A standalone Material Design 3 theme and component library for Avalonia 12.1 on .NET 8 or later. It does not depend on FluentTheme or SimpleTheme and is designed for desktop, Android, iOS, WebAssembly, trimming, and NativeAOT.

[Documentation](https://greepar.github.io/Material3.Avalonia/) | [WASM gallery](https://greepar.github.io/Material3.Avalonia/demo/) | [Releases](https://github.com/greepar/Material3.Avalonia/releases)

## Features

- HCT/MCU dynamic color with light/dark schemes, contrast levels, and nine scheme variants.
- Material 3 themes for 80+ built-in Avalonia controls.
- Material-specific controls including chips, FAB menus, button groups, sheets, search, time picker, wavy progress, loading morphs, and range sliders.
- 4,007 Google Material Symbols Rounded 400 icons, optically sized for 24dp, in regular and filled variants as cached Avalonia `Geometry` objects.
- Pointer-centered ripples, state layers, elevation, expressive shape transitions, and responsive Gallery layouts.
- Desktop Gallery tray integration with a compact native menu.
- AOT/trim analyzers enabled in the library and shared Gallery UI.

## Requirements

- .NET 8 or later
- Avalonia 12.1.1 or later

## Install

Install the preview package:

```bash
dotnet add package Material3.Avalonia --version 0.2.0-preview.1
```

Or add an explicit package reference:

```xml
<PackageReference Include="Material3.Avalonia" Version="0.2.0-preview.1" />
```

Keep the Avalonia platform package used by your application, such as `Avalonia.Desktop`, `Avalonia.Browser`, `Avalonia.Android`, or `Avalonia.iOS`. Then replace FluentTheme with `MaterialTheme`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:m3="using:Material3.Avalonia"
             x:Class="MyApp.App">
  <Application.Styles>
    <m3:MaterialTheme SeedColor="#6750A4" />
  </Application.Styles>
</Application>
```

Material-specific controls use `xmlns:m3c="using:Material3.Avalonia.Controls"`. See the [controls overview](https://greepar.github.io/Material3.Avalonia/controls/overview.html) for chips, FABs, navigation, sheets, settings rows, progress indicators, and other components.

For example, add the namespace to a view and use the controls directly:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:m3c="using:Material3.Avalonia.Controls"
             x:Class="MyApp.Views.HomeView">
  <StackPanel Spacing="12">
    <Button Content="Standard Avalonia button, automatically themed" />
    <m3c:AssistChip Content="Material-specific control" />
    <m3c:RangeSlider Minimum="0" Maximum="100"
                     LowerValue="25" UpperValue="75" />
  </StackPanel>
</UserControl>
```

See [Getting Started](https://greepar.github.io/Material3.Avalonia/guide/getting-started.html) for FluentTheme migration, Central Package Management, platform packages, namespaces, and troubleshooting.

## RangeSlider

`RangeSlider` exposes independently bindable lower and upper values, optional indicators, snapping, direction reversal, keyboard input, and change events:

```xml
<m3c:RangeSlider Minimum="-100"
                 Maximum="100"
                 LowerValue="-25"
                 UpperValue="40"
                 TickFrequency="5"
                 IsSnapToTickEnabled="True"
                 ValueIndicatorMode="Always"
                 ValueFormat="0" />
```

See the [slider documentation](https://greepar.github.io/Material3.Avalonia/controls/sliders.html) for the complete API.

## Material Symbols

Use the strongly typed regular or filled icon catalogs directly with `PathIcon.Data` or `Path.Data`:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:icons="using:Material3.Avalonia.Icons">
  <StackPanel Orientation="Horizontal" Spacing="12">
    <PathIcon Data="{x:Static icons:MaterialSymbols.Home}" />
    <PathIcon Data="{x:Static icons:MaterialSymbolsFilled.Favorite}" />
    <Path Data="{x:Static icons:MaterialSymbols.Search}"
          Fill="Black"
          Stretch="Uniform" />
  </StackPanel>
</UserControl>
```

The properties return lazily parsed, cached Avalonia `Geometry` instances, so unused icons have no parsing cost. See the [icon documentation](https://greepar.github.io/Material3.Avalonia/guide/icons.html) for C# usage and naming rules.

## Run

```bash
dotnet run --project samples/Material3.Gallery
dotnet run --project samples/Material3.Gallery.Browser
dotnet run --project tests/Material3.Screenshots
```

## NativeAOT

Tagged releases publish native artifacts for Windows x64, Linux x64, and macOS arm64. To publish locally:

```bash
dotnet publish samples/Material3.Gallery -c Release -r osx-arm64 --self-contained
```

Use `win-x64` or `linux-x64` for the other platforms.

## License

Apache-2.0. Components whose APIs were modeled after [m3fx](https://github.com/Glavo/m3fx) retain attribution in source comments.
