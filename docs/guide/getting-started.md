# Getting Started

Material3.Avalonia is a **standalone** Material Design 3 theme for Avalonia. It does not depend on `Avalonia.Themes.Fluent` or `Avalonia.Themes.Simple` — it ships its own `ControlTheme` for every built-in Avalonia control.

## Requirements

| | |
|---|---|
| Avalonia | **12.1.1 or later** |
| TFM | **net8.0 or later** (including platform-specific Android, iOS, and Browser TFMs) |
| Extra dependencies | none — only the Avalonia base package |
| AOT / trimming | fully compatible (`IsAotCompatible`, no reflection) |

## Install

Install the preview package:

```bash
dotnet add package Material3.Avalonia --version 0.2.0-preview.1
```

Or add an explicit package reference:

```xml
<ItemGroup>
  <PackageReference Include="Material3.Avalonia" Version="0.2.0-preview.1" />
</ItemGroup>
```

Keep your application's platform package (`Avalonia.Desktop`, `Avalonia.Browser`, `Avalonia.Android`, or `Avalonia.iOS`). The theme package depends only on the Avalonia base package.

## Apply the theme

Add `MaterialTheme` to `Application.Styles` — **instead of** FluentTheme:

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

That's it. Every control in your app now renders with Material 3 visuals, and the whole palette is derived from `SeedColor`.

## Light / dark

The theme populates both variants; switch them the standard Avalonia way:

```csharp
Application.Current.RequestedThemeVariant = ThemeVariant.Dark;  // or Light / Default
```

## First look

```xml
<StackPanel Spacing="12">
    <!-- Button variants via style classes -->
    <Button Content="Filled" />
    <Button Classes="elevated" Content="Elevated" />
    <Button Classes="tonal"    Content="Tonal" />
    <Button Classes="outlined" Content="Outlined" />
    <Button Classes="text"     Content="Text" />

    <!-- M3 type scale via TextBlock classes -->
    <TextBlock Classes="headline-small" Text="Headline" />
    <TextBlock Classes="body-medium"   Text="Body text" />

    <!-- Consume theme colors with DynamicResource -->
    <Border Background="{DynamicResource Md3PrimaryContainerBrush}"
            CornerRadius="{StaticResource Md3CornerMedium}" Padding="16">
        <TextBlock Foreground="{DynamicResource Md3OnPrimaryContainerBrush}"
                   Text="On primary container" />
    </Border>
</StackPanel>
```

::: warning Always use DynamicResource for colors
Color brushes (`Md3*Brush`) are mutated in place when the seed color or variant changes at runtime. Reference them with `{DynamicResource ...}` and never cache or animate the brush instances yourself.
:::

## Run the sample gallery

```bash
git clone https://github.com/greepar/Material3.Avalonia.git
cd Material3.Avalonia
dotnet run --project samples/Material3.Gallery          # desktop
dotnet run --project samples/Material3.Gallery.Browser  # WebAssembly
```

Or try the [hosted WASM demo](../demo/){target="_blank"}.
