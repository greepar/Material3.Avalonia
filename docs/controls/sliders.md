# Sliders

Material3.Avalonia themes the built-in Avalonia `Slider` and provides `RangeSlider` for selecting a lower/upper interval.

## Slider

The standard `Slider` keeps Avalonia's complete value, keyboard, tick, orientation, and direction API while rendering the Material 3 Expressive 16dp track, 4x44 handle, handle gaps, and maximum stop indicator.

```xml
<Slider Minimum="0" Maximum="100" Value="30" />
<Slider Minimum="0" Maximum="100"
        Value="70"
        TickFrequency="10"
        IsSnapToTickEnabled="True" />
```

## RangeSlider

```xml
<m3c:RangeSlider xmlns:m3c="using:Material3.Avalonia.Controls"
                 Minimum="-100"
                 Maximum="100"
                 LowerValue="-25"
                 UpperValue="40"
                 TickFrequency="5"
                 IsSnapToTickEnabled="True"
                 ValueIndicatorMode="Always"
                 ValueFormat="0" />
```

The selected interval uses `Md3PrimaryBrush`; the two outside tracks use `Md3SecondaryContainerBrush`. Both bounds have endpoint dots and each handle maintains the Material 3 six-dp track gap.

### Properties

| Property | Default | Description |
|---|---:|---|
| `Minimum` | `0` | Lowest permitted value. |
| `Maximum` | `100` | Highest permitted value. |
| `LowerValue` | `20` | Lower selected value. Two-way by default and clamped below `UpperValue`. |
| `UpperValue` | `80` | Upper selected value. Two-way by default and clamped above `LowerValue`. |
| `SmallChange` | `1` | Arrow-key step. |
| `LargeChange` | `10` | PageUp/PageDown step. |
| `TickFrequency` | `1` | Distance between snap points. |
| `IsSnapToTickEnabled` | `false` | Snaps both values to ticks measured from `Minimum`. |
| `IsDirectionReversed` | `false` | Renders minimum on the right and reverses directional keyboard/drag input. |
| `ValueIndicatorMode` | `OnInteraction` | `Never`, `OnInteraction`, or `Always`. |
| `ValueFormat` | `0.##` | .NET numeric format string for indicator text. |

### Events

- `LowerValueChanged`
- `UpperValueChanged`

Both use `RangeSliderValueChangedEventArgs`, which exposes `OldValue` and `NewValue`.

### Keyboard

Tab focuses each handle independently. Arrow keys apply `SmallChange`; PageUp/PageDown apply `LargeChange`; Home/End move the focused handle to its allowed boundary.

### Binding

```xml
<m3c:RangeSlider LowerValue="{Binding MinimumPrice}"
                 UpperValue="{Binding MaximumPrice}" />
```

`LowerValue` and `UpperValue` use two-way binding by default.
