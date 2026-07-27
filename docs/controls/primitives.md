# Ripple & Entrance Animations

`Material3.Avalonia.Primitives` (`xmlns:m3p="using:Material3.Avalonia.Primitives"`) exposes the building blocks the theme itself uses — handy when writing custom control templates that should feel native to the theme.

## RippleHost

Pointer-accurate M3 ripple. Drop it inside any control template, above the background and below the content; it automatically attaches to its templated parent (or the nearest `InputElement` ancestor) for input:

```xml
<ControlTemplate>
    <Border Background="{TemplateBinding Background}"
            CornerRadius="{TemplateBinding CornerRadius}">
        <Panel>
            <m3p:RippleHost RippleBrush="{TemplateBinding Foreground}"
                            RippleOpacity="{StaticResource Md3StatePressedOpacity}"
                            RippleClipRadius="{TemplateBinding CornerRadius}" />
            <ContentPresenter Content="{TemplateBinding Content}" />
        </Panel>
    </Border>
</ControlTemplate>
```

| Property | Type | Default | |
|---|---|---|---|
| `RippleBrush` | `IBrush?` | `null` | Usually the component's on-color |
| `RippleOpacity` | `double` | `0.10` | Peak opacity (pressed state layer) |
| `RippleClipRadius` | `CornerRadius` | `0` | Clips to the component shape; oversized values (the `Full` token) are clamped automatically |
| `IsRippleEnabled` | `bool` | `true` | |

Behavior: expands from the press point (keyboard Space/Enter ripples from center), holds while pressed, fades on release, and **cancels instantly when a scroll gesture steals the pointer** — no stuck ripples in touch lists. It never hit-tests and draws nothing when idle.

## Entrance (attached animation)

Replays an M3 emphasized-decelerate entrance (250 ms fade + subtle grow) **every time** the element is attached to the visual tree — which is exactly when a popup opens. Style-based animations only run once per style application; this behavior fixes the "animation only plays the first time" popup problem.

```xml
<Popup>
    <Border m3p:Entrance.Animation="GrowDown"
            RenderTransformOrigin="50%,0%"
            Background="{DynamicResource Md3SurfaceContainerBrush}"
            CornerRadius="{StaticResource Md3CornerSmall}"
            BoxShadow="{StaticResource Md3Elevation2}">
        <!-- flyout content -->
    </Border>
</Popup>
```

`EntranceAnimationKind`: `None` · `GrowDown` (menus, dropdowns) · `GrowRight` (submenus) · `Fade`.

Set `RenderTransformOrigin` to the anchoring edge (`50%,0%` for below-anchor popups, `0%,0%` for right-of-anchor submenus).

## BoxShadow tip

Avalonia clips `BoxShadow` at `ClipToBounds` boundaries and popup edges. When a shadowed element sits inside a popup, reserve margin for the shadow (the theme uses `Margin="10,8,10,14"` for elevation 2, `12,8,12,16` for elevation 3) and offset the popup back with `HorizontalOffset`/`VerticalOffset`.
