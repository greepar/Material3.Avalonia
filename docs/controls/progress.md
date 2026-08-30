# Progress Indicators

Besides the themed linear `ProgressBar`, the library ships two Material-specific indicators in `Material3.Avalonia.Controls` (`xmlns:m3c="using:Material3.Avalonia.Controls"`).

## LoadingIndicator

`LoadingIndicator` is the Material 3 Expressive indeterminate loader. Its polygonal shape continuously morphs and rotates; its natural size is 48dp.

```xml
<StackPanel Orientation="Horizontal" Spacing="16">
    <m3c:LoadingIndicator />
    <m3c:LoadingIndicator Variant="Contained" />
</StackPanel>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Variant` | `LoadingIndicatorVariant` | `Default` | `Default` draws only the shape; `Contained` adds a circular container. |
| `Foreground` | `IBrush?` | themed | Morphing shape fill. |
| `Background` | `IBrush?` | themed | Container fill for the contained variant. |

The animation suspends while detached and polls cheaply while hidden, then resumes when visible. `DebugSegmentOverride` is a public visual-test hook; applications should normally leave it `null`.

## CircularProgressIndicator

M3 circular progress with track gap and round caps. Inherits `RangeBase` (`Minimum`/`Maximum`/`Value`).

```xml
<m3c:CircularProgressIndicator Value="65" Maximum="100" />
<m3c:CircularProgressIndicator IsIndeterminate="True" />
<m3c:CircularProgressIndicator Value="30" Width="64" Height="64" StrokeWidth="6" />
```

| Property | Type | Default | |
|---|---|---|---|
| `IsIndeterminate` | `bool` | `false` | Spinning arc animation |
| `StrokeWidth` | `double` | `4` | Arc thickness |
| `TrackGap` | `double` | `4` | Gap between active arc and track (M3 2024 style) |
| `Foreground` / `Background` | `IBrush` | Primary / SecondaryContainer | Active arc / track |

Value changes animate smoothly (0.5 s decelerate). Animations pause automatically while the control is hidden or detached.

## WavyProgressBar

M3 Expressive linear progress — the active track is a **flowing sine wave**:

```xml
<m3c:WavyProgressBar Value="60" Maximum="100" />
<m3c:WavyProgressBar IsIndeterminate="True" />
<m3c:WavyProgressBar Value="60" Amplitude="5" Wavelength="30" />
```

| Property | Type | Default | |
|---|---|---|---|
| `IsIndeterminate` | `bool` | `false` | Sweeping wave segment |
| `Amplitude` | `double` | `3` | Wave height (control height auto-sizes to fit) |
| `Wavelength` | `double` | `40` | Wave period in px |
| `StrokeWidth` | `double` | `4` | |
| `TrackGap` | `double` | `4` | Gap around the wave head |

Details matching the Android implementation: the wave phase scrolls continuously while in progress, a stop dot marks the track end, and the wave flattens to a straight line as the value approaches 100 %.

## Linear ProgressBar

The built-in control, restyled: 4dp rounded tracks, `:indeterminate` sweep animation, horizontal & vertical.

```xml
<ProgressBar Value="40" Maximum="100" />
<ProgressBar Value="40" ShowProgressText="True" ProgressTextFormat="\{1:0\}%" />
<ProgressBar IsIndeterminate="True" />
<ProgressBar Orientation="Vertical" Height="120" Value="45" Maximum="100"
             ShowProgressText="True" ProgressTextFormat="\{1:0\}%" />
```

`ShowProgressText` displays Avalonia's calculated progress text in both orientations. `ProgressTextFormat` uses `{0}` for `Value`, `{1}` for percentage, `{2}` for `Minimum`, and `{3}` for `Maximum`.
