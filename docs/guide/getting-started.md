# Getting Started

Material3.Avalonia is a **standalone** Material Design 3 theme for Avalonia. It does not depend on `Avalonia.Themes.Fluent` or `Avalonia.Themes.Simple` — it ships its own `ControlTheme` for every built-in Avalonia control.

## Requirements

| | |
|---|---|
| Avalonia | **12.1.1 or later** |
| TFM | **net8.0 or later** (including platform-specific Android, iOS, and Browser TFMs) |
| Extra dependencies | none — only the Avalonia base package |
| AOT / trimming | fully compatible (`IsAotCompatible`, no reflection) |

## Add it to an existing Avalonia project

The following steps apply to Desktop, Browser, Android, and iOS applications. Material3.Avalonia replaces the control theme; it does not replace your Avalonia platform package or application startup code.

### 1. Install the package

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

If your repository uses NuGet Central Package Management, put the version in `Directory.Packages.props`:

```xml
<ItemGroup>
  <PackageVersion Include="Material3.Avalonia" Version="0.2.0-preview.1" />
</ItemGroup>
```

Then add an unversioned reference to your application project:

```xml
<ItemGroup>
  <PackageReference Include="Material3.Avalonia" />
</ItemGroup>
```

Keep the platform package already used by your application:

```xml
<!-- Desktop example: keep this package -->
<PackageReference Include="Avalonia.Desktop" />
```

The equivalent package may be `Avalonia.Browser`, `Avalonia.Android`, or `Avalonia.iOS`. Material3.Avalonia depends only on the Avalonia base package and does not configure a platform backend.

### 2. Replace the existing theme in App.axaml

Open `App.axaml`. Remove `<FluentTheme />`, `<SimpleTheme />`, or the corresponding `StyleInclude`, then add the Material namespace and `MaterialTheme`:

```xml
<!-- Before -->
<Application.Styles>
    <FluentTheme />
</Application.Styles>
```

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

Do not load FluentTheme and MaterialTheme together. Both provide themes for built-in Avalonia controls, so mixing them creates ambiguous resources and inconsistent visuals.

### 3. Use standard and Material-specific controls

Standard Avalonia controls require no new namespace and are restyled automatically:

```xml
<StackPanel Spacing="12">
    <Button Content="Save" />
    <TextBox Watermark="Name" />
    <CheckBox Content="Remember me" />
</StackPanel>
```

For controls supplied by this package, add the controls namespace to your `Window`, `UserControl`, or view:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:m3c="using:Material3.Avalonia.Controls"
             xmlns:icons="using:Material3.Avalonia.Icons"
             x:Class="MyApp.Views.HomeView">
    <StackPanel Margin="24" Spacing="16">
        <m3c:AssistChip Content="Create"
                        Command="{Binding CreateCommand}">
            <m3c:AssistChip.Icon>
                <PathIcon Data="{x:Static icons:MaterialSymbols.Add}" />
            </m3c:AssistChip.Icon>
        </m3c:AssistChip>

        <m3c:RangeSlider Minimum="0"
                         Maximum="100"
                         LowerValue="25"
                         UpperValue="75" />
    </StackPanel>
</UserControl>
```

Use `Material3.Avalonia.Controls` for package controls, `Material3.Avalonia.Icons` for Material Symbols, and `Material3.Avalonia.Colors` when calling the color engine from C#.

### 4. Build and run

```bash
dotnet restore
dotnet run --project path/to/MyApp.csproj
```

If the application starts and the ordinary `Button` uses Material styling, the theme is installed correctly. The complete palette is derived from `SeedColor`.

::: warning Common mistakes
- Do not remove `Avalonia.Desktop`, `Avalonia.Browser`, `Avalonia.Android`, or `Avalonia.iOS` from the application project.
- Do not keep `<FluentTheme />` or `<SimpleTheme />` beside `<m3:MaterialTheme />`.
- Put `MaterialTheme` in `Application.Styles`, not inside a page's visual content.
- The package is currently a preview, so specify `0.2.0-preview.1` explicitly.
:::

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
