# Sheets, Scrim, and Dialogs

These controls use `xmlns:m3c="using:Material3.Avalonia.Controls"`.

::: warning In-place overlays
`BottomSheet`, `SideSheet`, `Scrim`, and `DialogPane` are not popups and do not move themselves to a top-level overlay. Place them after page content in a shared `Panel`; that panel's bounds define the covered region.
:::

## BottomSheet

```xml
<Panel>
    <Grid>
        <Button Content="Open sheet" Command="{Binding OpenSheetCommand}" />
    </Grid>

    <m3c:BottomSheet IsOpen="{Binding IsSheetOpen}"
                     Closed="OnSheetClosed">
        <StackPanel Margin="24,8,24,24" Spacing="8">
            <TextBlock Classes="title-medium" Text="Bottom sheet" />
            <TextBlock Text="Tap the scrim or drag down to dismiss." />
        </StackPanel>
    </m3c:BottomSheet>
</Panel>
```

| Member | Type | Default | Description |
|---|---|---|---|
| `IsOpen` | `bool` | `false` | Two-way by default. |
| `ShowDragHandle` | `bool` | `true` | Shows the handle and its drag interaction area. |
| `Closed` | `event EventHandler` | - | Raised when `IsOpen` changes to `false`. |

Dragging starts from the handle. Releasing after the dismiss threshold closes the sheet; otherwise it returns to the open position. Pressing the built-in scrim also closes it.

## SideSheet

```xml
<Panel>
    <ContentControl Content="{Binding CurrentPage}" />

    <m3c:SideSheet IsOpen="{Binding IsDetailsOpen}">
        <StackPanel Margin="24" Spacing="12">
            <TextBlock Classes="title-large" Text="Details" />
            <TextBlock Text="Supporting information" />
        </StackPanel>
    </m3c:SideSheet>
</Panel>
```

`SideSheet` exposes `IsOpen` and `Closed`. It supports scrim and Escape-key dismissal but does not expose a drag handle or drag gesture.

## Scrim

Use standalone `Scrim` when composing your own overlay. It raises `Dismissed` but does not close itself.

```xml
<m3c:Scrim IsOpen="{Binding IsDialogOpen}"
            Dismissed="OnScrimDismissed" />
```

```csharp
private void OnScrimDismissed(object? sender, EventArgs e)
{
    IsDialogOpen = false;
}
```

## DialogPane

`DialogPane` is the visual dialog surface. It does not create a `Window` or `Popup`, trap focus, provide modality, or return a result.

```xml
<Panel>
    <ContentControl Content="{Binding CurrentPage}" />
    <m3c:Scrim IsOpen="{Binding IsDialogOpen}" />

    <m3c:DialogPane Title="Delete item?"
                    IsVisible="{Binding IsDialogOpen}"
                    HorizontalAlignment="Center"
                    VerticalAlignment="Center">
        <TextBlock Text="This operation cannot be undone." />

        <m3c:DialogPane.Buttons>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <Button Classes="text" Content="Cancel"
                        Command="{Binding CancelCommand}" />
                <Button Classes="text danger" Content="Delete"
                        Command="{Binding DeleteCommand}" />
            </StackPanel>
        </m3c:DialogPane.Buttons>
    </m3c:DialogPane>
</Panel>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Optional headline. |
| `Icon` | `object?` | `null` | Optional centered hero icon. |
| `Buttons` | `object?` | `null` | Action slot; wrap multiple buttons in a panel. |
| `Content` | inherited | `null` | Dialog body. |

Applications remain responsible for focus management, Escape handling, results, and modal behavior.
