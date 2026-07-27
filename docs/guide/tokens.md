# Typography & Design Tokens

All static tokens live in `Themes/Tokens.axaml` and are available app-wide once the theme is applied. Reference structural tokens with `StaticResource`, colors with `DynamicResource`.

## Type scale (TextBlock classes)

Fifteen `TextBlock` style classes map 1:1 to the M3 type scale:

```xml
<TextBlock Classes="display-large"  Text="57/64" />
<TextBlock Classes="headline-medium" Text="28/36" />
<TextBlock Classes="title-large"   Text="22/28" />
<TextBlock Classes="body-medium"   Text="14/20" />
<TextBlock Classes="label-small"   Text="11/16" />
```

`display|headline|title|body|label` × `large|medium|small`. Font sizes / line heights are also exposed as doubles: `Md3BodyLargeSize`, `Md3BodyLargeLineHeight`, etc. Font family: `Md3FontFamilyPlain` / `Md3FontFamilyBrand`.

## Shape

| Key | Radius |
|---|---|
| `Md3CornerNone` | 0 |
| `Md3CornerExtraSmall` | 4 |
| `Md3CornerSmall` | 8 |
| `Md3CornerMedium` | 12 |
| `Md3CornerLarge` | 16 |
| `Md3CornerLargeIncreased` | 20 |
| `Md3CornerExtraLarge` | 28 |
| `Md3CornerExtraLargeIncreased` | 32 |
| `Md3CornerExtraExtraLarge` | 48 |
| `Md3CornerFull` | 9999 (pill — Avalonia clamps to bounds) |

## State layers & elevation

```
Md3StateHoverOpacity    0.08      Md3Elevation0 … Md3Elevation5   (BoxShadows)
Md3StateFocusOpacity    0.10
Md3StatePressedOpacity  0.10      Md3DisabledContentOpacity 0.38
Md3StateDraggedOpacity  0.16      Md3DisabledContainerOpacity 0.12
```

Elevation levels are two-layer `BoxShadows`; level 0 keeps the two-layer structure so `BoxShadowsTransition` interpolates smoothly.

## Motion durations

`Md3DurationShort1–4` (50–200 ms) · `Md3DurationMedium1–4` (250–400 ms) · `Md3DurationLong1–4` (450–600 ms), all `x:TimeSpan`.

## Component metrics

`Md3ButtonHeight` 40 · `Md3MinTouchTarget` 48 · `Md3TextFieldHeight` 56 · `Md3ListItemHeight` 48 · `Md3MenuItemHeight` 48 · `Md3IconButtonSize` 40 · `Md3FocusRingThickness` 2

## Overriding tokens

Standard Avalonia resource shadowing works — declare the same key closer to your control:

```xml
<Window.Resources>
    <!-- Squarer buttons app-wide -->
    <CornerRadius x:Key="Md3CornerFull">12</CornerRadius>
</Window.Resources>
```

Many sub-component `ControlTheme`s are also exposed by key (e.g. `Md3SliderThumb`, `Md3TimePickerDisplayChip`, `Md3CalendarNavButton`) so you can re-style internals without redefining whole templates.
