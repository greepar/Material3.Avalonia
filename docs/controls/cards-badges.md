# Cards, Surfaces, Avatars, and Badges

Use `xmlns:m3c="using:Material3.Avalonia.Controls"`.

## Card

```xml
<m3c:Card Variant="Outlined" Padding="16">
    <StackPanel Spacing="4">
        <TextBlock Classes="title-medium" Text="Card title" />
        <TextBlock Text="Supporting content" />
    </StackPanel>
</m3c:Card>
```

| Member | Type | Default | Description |
|---|---|---|---|
| `Variant` | `CardVariant` | `Elevated` | `Elevated`, `Filled`, or `Outlined`. |
| `IsClickable` | `bool` | `false` | Enables hand cursor, state layer, ripple, and `Clicked`. |
| `Clicked` | `event EventHandler` | - | Raised after a primary press/release inside, or Enter/Space, on a clickable card. |

```xml
<m3c:Card IsClickable="True"
            Clicked="OnCardClicked"
            Padding="16">
    <TextBlock Text="Open details" />
</m3c:Card>
```

`Clicked` is a CLR event, not a routed event or command property. For command-first MVVM interactions, place a `Button` in the card or bind through your preferred behavior library.

Clickable cards participate in keyboard focus and show a visible focus ring. Secondary-button clicks do not activate them.

## Surface

`Surface` is a simpler content container whose background and shadow follow `Elevation`.

```xml
<m3c:Surface Elevation="2" CornerRadius="12" Padding="16">
    <TextBlock Text="Surface content" />
</m3c:Surface>
```

`Elevation` defaults to `0` and is clamped to `0` through `5`. Higher levels use progressively stronger shadows and surface-container colors. Avoid clipping parents if the shadow must remain visible.

## Avatar

```xml
<StackPanel Orientation="Horizontal" Spacing="12">
    <m3c:Avatar Text="Alex" />
    <m3c:Avatar Text="Bea" Size="48" Variant="Rounded" />
    <m3c:Avatar Source="{Binding ProfileImage}"
                 Text="{Binding DisplayName}"
                 Variant="Circle" />
</StackPanel>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Source` | `IImage?` | `null` | Image displayed with `UniformToFill`. |
| `Text` | `string?` | `null` | Fallback text; the first text element becomes the initial. |
| `Size` | `double` | `40` | Width and height. |
| `Variant` | `AvatarVariant` | `Circle` | `Circle`, `Rounded`, or `Square`. |
| `DisplayInitial` | `string` | `""` | Read-only derived initial. |

## Badge

`Badge` supports an unlabeled dot or a numeric pill.

```xml
<m3c:Badge IsDotBadge="True" />
<m3c:Badge Value="150" MaxValue="99" />
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `int` | `0` | Current count. |
| `MaxValue` | `int` | `99` | Larger values display as `MaxValue+`. |
| `IsDotBadge` | `bool` | `false` | Uses the 6dp dot form. |
| `DisplayText` | `string` | `"0"` | Read-only rendered label. |

## BadgedBox

Use `BadgedBox` to anchor a badge at the top-right of another control:

```xml
<m3c:BadgedBox>
    <m3c:BadgedBox.Badge>
        <m3c:Badge Value="12" />
    </m3c:BadgedBox.Badge>

    <m3c:IconButton>
        <PathIcon Data="{x:Static icons:MaterialSymbols.Notifications}" />
    </m3c:IconButton>
</m3c:BadgedBox>
```

The `Badge` property accepts any object but normally contains `Badge`. The overlay extends beyond the main content and is not hit-testable; avoid clipping ancestors.
