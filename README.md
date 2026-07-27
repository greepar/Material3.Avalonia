# Material3.Avalonia

A standalone Material Design 3 theme and component library for Avalonia 12.1 on .NET 10. It does not depend on FluentTheme or SimpleTheme and is designed for desktop, Android, iOS, WebAssembly, trimming, and NativeAOT.

[Documentation](https://greepar.github.io/Material3.Avalonia/) | [WASM gallery](https://greepar.github.io/Material3.Avalonia/demo/) | [Releases](https://github.com/greepar/Material3.Avalonia/releases)

## Features

- HCT/MCU dynamic color with light/dark schemes, contrast levels, and nine scheme variants.
- Material 3 themes for 80+ built-in Avalonia controls.
- Material-specific controls including chips, FAB menus, button groups, sheets, search, time picker, wavy progress, loading morphs, and range sliders.
- 4,007 Google Material Symbols Rounded 400 icons, optically sized for 24dp, in regular and filled variants as cached Avalonia `Geometry` objects.
- Pointer-centered ripples, state layers, elevation, expressive shape transitions, and responsive Gallery layouts.
- Desktop Gallery tray integration with a native tray entry and a custom Material background panel.
- AOT/trim analyzers enabled in the library and shared Gallery UI.

## Requirements

- .NET SDK 10
- Avalonia 12.1.0

## Install

Until the NuGet package is published, reference `src/Material3.Avalonia/Material3.Avalonia.csproj`. Then replace FluentTheme with `MaterialTheme`:

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
