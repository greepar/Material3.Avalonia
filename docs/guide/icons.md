# Material Symbols

Material3.Avalonia includes the Google Material Symbols Rounded 400 catalog as strongly typed Avalonia geometries. The catalog contains 4,007 icon names in both regular and filled variants, using paths optically designed for 24dp rendering.

## XAML

Add the icon namespace and pass a property directly to `PathIcon.Data` or `Path.Data`:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:icons="using:Material3.Avalonia.Icons">
  <StackPanel Orientation="Horizontal" Spacing="12">
    <PathIcon Width="24" Height="24"
              Data="{x:Static icons:MaterialSymbols.Home}" />

    <PathIcon Width="24" Height="24"
              Data="{x:Static icons:MaterialSymbolsFilled.Favorite}" />

    <Path Width="24" Height="24"
          Data="{x:Static icons:MaterialSymbols.Search}"
          Fill="{DynamicResource Md3PrimaryBrush}"
          Stretch="Uniform" />
  </StackPanel>
</UserControl>
```

## C#

Each property returns an Avalonia `Geometry`:

```csharp
using Material3.Avalonia.Icons;

pathIcon.Data = MaterialSymbols.Settings;
path.Data = MaterialSymbolsFilled.Star;
```

Geometry data is parsed only on first access and then cached. Accessing `MaterialSymbols.Home` repeatedly returns the same instance.

## Naming

SVG snake-case names are converted to PascalCase C# properties:

| Google name | Property |
| --- | --- |
| `home` | `MaterialSymbols.Home` |
| `arrow_back` | `MaterialSymbols.ArrowBack` |
| `account_circle` | `MaterialSymbols.AccountCircle` |
| `360` | `MaterialSymbols.Icon360` |

Use `MaterialSymbols` for regular icons and `MaterialSymbolsFilled` for filled icons.

## Catalog And Virtualized Gallery

For runtime icon browsers, `MaterialSymbolCatalog.All` exposes the names without
parsing any geometry. Retrieve a cached geometry by catalog index and variant:

```csharp
foreach (var symbol in MaterialSymbolCatalog.All)
{
    var geometry = MaterialSymbolCatalog.GetGeometry(symbol.Index, filled: false);
}
```

The Gallery's `Material Symbols` section uses `Avalonia.Controls.ItemsRepeater`
with `UniformGridLayout`. Search and variant changes replace only the lightweight
item source; Geometry is resolved when a realized card binds to it.

## Updating The Catalog

The generated sources are based on `@iconify-json/material-symbols@1.2.87`. To regenerate from an extracted package:

```bash
node tools/generate-material-symbols.mjs \
  /path/to/package/icons.json \
  src/Material3.Avalonia/Icons/Generated
```

The generator verifies that every icon has both variants and rejects invalid or colliding C# identifiers.

Material Symbols are provided by Google under the Apache License 2.0. See the repository `NOTICE` file for attribution.
