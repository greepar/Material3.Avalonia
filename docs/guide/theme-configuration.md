# Theme Configuration

All configuration lives on the `MaterialTheme` instance in `Application.Styles`. Every property is a `StyledProperty` and can be changed **at runtime** — the whole color scheme rebuilds instantly.

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `SeedColor` | `Color` | `#6750A4` | Source color the entire palette is derived from |
| `SchemeVariant` | `SchemeVariant` | `TonalSpot` | Palette recipe (see below) |
| `ContrastLevel` | `double` | `0.0` | `0` standard · `0.5` medium · `1.0` high contrast |

```xml
<m3:MaterialTheme SeedColor="#006A6A"
                  SchemeVariant="Vibrant"
                  ContrastLevel="0.5" />
```

## Runtime switching

Grab the theme instance and set properties — no restart needed:

```csharp
using Material3.Avalonia;
using Material3.Avalonia.Colors;

var theme = Application.Current!.Styles.OfType<MaterialTheme>().First();

theme.SeedColor     = Colors.Teal;              // re-derives everything
theme.SchemeVariant = SchemeVariant.Expressive;
theme.ContrastLevel = 1.0;
```

## Scheme variants

`Material3.Avalonia.Colors.SchemeVariant` — the same nine variants as Android / Material Color Utilities:

| Variant | Character |
|---|---|
| `TonalSpot` | Default. Balanced, muted chroma (Android 12+ default) |
| `Neutral` | Near-grayscale with a hint of the seed |
| `Vibrant` | Maximum chroma, colorful secondaries |
| `Expressive` | Playful hue rotations, unexpected accents |
| `Fidelity` | Stays faithful to the exact seed color |
| `Content` | Like Fidelity, tuned for content-sourced seeds (album art…) |
| `Monochrome` | Pure grayscale |
| `Rainbow` | Colorful primary, neutral surfaces |
| `FruitSalad` | Shifted-hue primary/secondary pairing |

## What the theme injects

For each of the 45 M3 color roles the theme writes **two resources** per theme variant:

- `Md3{Role}` — `Color`
- `Md3{Role}Brush` — `SolidColorBrush` (mutated in place on rebuild)

Roles: `Primary`, `OnPrimary`, `PrimaryContainer`, `OnPrimaryContainer`, `Secondary…`, `Tertiary…`, `Error…`, `Surface`, `SurfaceDim`, `SurfaceBright`, `SurfaceContainerLowest/Low/­/High/Highest`, `OnSurface`, `OnSurfaceVariant`, `Outline`, `OutlineVariant`, `Shadow`, `Scrim`, `SurfaceTint`, `InverseSurface`, `InverseOnSurface`, `InversePrimary`, and the `Fixed` family (`PrimaryFixed`, `PrimaryFixedDim`, `OnPrimaryFixed`, `OnPrimaryFixedVariant`, same for Secondary/Tertiary).

```xml
<Border Background="{DynamicResource Md3SurfaceContainerBrush}">
    <TextBlock Foreground="{DynamicResource Md3OnSurfaceBrush}" />
</Border>
```

::: info Reserved properties
`MotionScheme`, `ReduceMotion` and `Density` exist on `MaterialTheme` but are **not consumed by the theme yet** — they are reserved for a future release.
:::
