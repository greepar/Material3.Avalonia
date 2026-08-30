# Buttons, FABs, and Groups

These controls use `xmlns:m3c="using:Material3.Avalonia.Controls"`. Icon examples also use `xmlns:icons="using:Material3.Avalonia.Icons"`.

## Icon buttons

`IconButton` derives from `Button`; `IconToggleButton` derives from `ToggleButton`. Put the icon in `Content`.

```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <m3c:IconButton Variant="Standard" Command="{Binding SearchCommand}">
        <PathIcon Data="{x:Static icons:MaterialSymbols.Search}" />
    </m3c:IconButton>

    <m3c:IconToggleButton Variant="Tonal" IsChecked="{Binding IsFavorite}">
        <PathIcon Data="{x:Static icons:MaterialSymbols.Favorite}" />
    </m3c:IconToggleButton>
</StackPanel>
```

`Variant` defaults to `Standard` and accepts `Standard`, `Filled`, `Tonal`, or `Outlined`. Both controls are 40dp circular buttons with 24dp content. The toggle button changes its color and shape when checked.

## FloatingActionButton

```xml
<m3c:FloatingActionButton Size="Medium"
                          Color="PrimaryContainer"
                          Command="{Binding CreateCommand}">
    <PathIcon Data="{x:Static icons:MaterialSymbols.Add}" />
</m3c:FloatingActionButton>
```

| Property | Type | Default | Values |
|---|---|---|---|
| `Size` | `FabSize` | `Medium` | `Small`, `Medium`, `Large` |
| `Color` | `FabColor` | `PrimaryContainer` | `PrimaryContainer`, `Surface`, `Secondary`, `Tertiary` |

Sizes are 40dp, 56dp, and 96dp respectively. Shadows and focus rings draw outside the control, so avoid clipping ancestors.

## ExtendedFloatingActionButton

The extended FAB combines a leading icon and label:

```xml
<m3c:ExtendedFloatingActionButton Content="Compose"
                                  Color="Secondary"
                                  Command="{Binding ComposeCommand}">
    <m3c:ExtendedFloatingActionButton.Icon>
        <PathIcon Data="{x:Static icons:MaterialSymbols.Edit}" />
    </m3c:ExtendedFloatingActionButton.Icon>
</m3c:ExtendedFloatingActionButton>
```

Its `Icon` defaults to `null`; `Color` uses the same `FabColor` values as `FloatingActionButton`.

## FabMenu

`FabMenu` stacks extended FAB actions above a primary FAB. Clicking the primary FAB toggles `IsOpen`; clicking an action or pressing Escape closes it.

```xml
<m3c:FabMenu IsOpen="{Binding IsCreateMenuOpen}" Color="Surface">
    <m3c:FabMenu.Icon>
        <PathIcon Data="{x:Static icons:MaterialSymbols.Add}" />
    </m3c:FabMenu.Icon>

    <m3c:ExtendedFloatingActionButton Content="Document"
                                      Command="{Binding NewDocumentCommand}" />
    <m3c:ExtendedFloatingActionButton Content="Message"
                                      Command="{Binding NewMessageCommand}" />
</m3c:FabMenu>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `IsOpen` | `bool` | `false` | Two-way by default. |
| `Icon` | `object?` | `null` | Primary FAB icon. |
| `Color` | `FabColor` | `PrimaryContainer` | Inherited by the primary FAB and menu actions. |

## ButtonGroup

`ButtonGroup` arranges ordinary Avalonia buttons horizontally.

```xml
<m3c:ButtonGroup Variant="Connected">
    <Button Content="Day" Command="{Binding DayCommand}" />
    <Button Content="Week" Command="{Binding WeekCommand}" />
    <Button Content="Month" Command="{Binding MonthCommand}" />
</m3c:ButtonGroup>
```

`Variant="Standard"` uses separated expressive buttons that grow while pressed. `Connected` keeps stable widths and joins button shapes. Only realized items that derive from `Button` receive group behavior.

## SegmentedButtonGroup

Use segmented buttons for persistent selection rather than immediate commands.

```xml
<m3c:SegmentedButtonGroup SelectionMode="Single">
    <m3c:SegmentedButton Content="Day" IsChecked="True" />
    <m3c:SegmentedButton Content="Week" />
    <m3c:SegmentedButton Content="Month" />
</m3c:SegmentedButtonGroup>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `SelectionMode` | `SegmentedSelectionMode` | `Single` | `Single` unchecks the other segments; `Multiple` leaves them independent. |
| `SegmentedButton.Icon` | `object?` | `null` | Optional leading content; selected segments show a checkmark. |

Single selection permits zero selected items: users can toggle the current segment off. Bind each segment's inherited `IsChecked` property when selection must live in a view model.
