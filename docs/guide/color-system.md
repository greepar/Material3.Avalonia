# Color System (HCT / Material Color Utilities)

The palette engine is a C# port of Google's [material-color-utilities](https://github.com/material-foundation/material-color-utilities) (Apache-2.0), namespace `Material3.Avalonia.Colors`. You can use it directly — independent of the XAML theme — to generate schemes, tonal palettes, or do HCT color math.

Colors are represented as `uint` ARGB (`0xFFRRGGBB`). Convert to Avalonia with `Color.FromUInt32(argb)`.

## Build a full scheme

```csharp
using Material3.Avalonia.Colors;

MaterialColorScheme scheme = SchemeBuilder.Build(
    seedArgb:      0xFF6750A4,
    variant:       SchemeVariant.TonalSpot,
    isDark:        false,
    contrastLevel: 0.0);

uint primary   = scheme.Primary;            // 0xFF6750A4-derived P40
uint container = scheme.PrimaryContainer;
var  avalonia  = Color.FromUInt32(scheme.Surface);
```

`MaterialColorScheme` is a record with all 45 role properties — the exact set the theme injects as `Md3*` resources.

## HCT color space

HCT (Hue / Chroma / Tone) is the perceptual color space M3 is built on:

```csharp
var hct = Hct.FromInt(0xFF6750A4);
double hue    = hct.Hue;     // 0–360
double chroma = hct.Chroma;  // 0–~150
double tone   = hct.Tone;    // 0–100 (L*)

hct.Tone = 80;               // mutate; re-solves to nearest displayable color
uint argb = hct.ToInt();

var custom = Hct.From(hue: 25, chroma: 84, tone: 40);  // ≈ M3 error-40
```

## Tonal palettes

A `TonalPalette` fixes hue+chroma and exposes any tone:

```csharp
var palette = TonalPalette.FromInt(0xFF6750A4);
uint t40 = palette.Tone(40);   // key tone used for light primary
uint t80 = palette.Tone(80);   // dark primary
uint t90 = palette.Tone(90);   // light container
```

`CorePalettes.Of(sourceHct, variant)` gives you the six palettes (primary, secondary, tertiary, neutral, neutral-variant, error) a variant is made of.

## Utility helpers

- `DislikeAnalyzer.FixIfDisliked(hct)` — moves universally-disliked (bile-like) colors to a pleasant tone
- `TemperatureCache(hct).Analogous(...)` / `.Complement` — warm/cool color relations
- `ColorUtils`, `Cam16`, `ViewingConditions`, `HctSolver` — low-level CAM16 math, same API shape as the Google library

::: info Known deviations from upstream MCU
- Tone mapping uses the fixed 2021 M3 baseline (no `ContrastCurve`/`ToneDeltaPair` solver)
- `ContrastLevel` applies a linear interpolation approximation
- `Fidelity`/`Content` skip the container tone fine-tuning step

These are documented implementation choices; visual output matches the reference for standard contrast.
:::
