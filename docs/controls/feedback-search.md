# Snackbar, Tooltip, Search, and Divider

Use `xmlns:m3c="using:Material3.Avalonia.Controls"`.

## Snackbar

Render a single snackbar directly when you manage its lifetime yourself:

```xml
<m3c:Snackbar Message="Photo saved to album"
              ActionText="UNDO"
              ActionClicked="OnUndo" />
```

| Member | Type | Default | Description |
|---|---|---|---|
| `Message` | `string?` | `null` | Snackbar message. |
| `ActionText` | `string?` | `null` | Optional action label; null or empty hides it. |
| `ActionClicked` | `event EventHandler` | - | Raised when the action is clicked. |

`Snackbar` does not dismiss itself or start a timer.

## SnackbarHost

Wrap the UI region over which transient snackbars should appear. Calls made before the host is attached are queued and displayed after its template is applied.

```xml
<m3c:SnackbarHost x:Name="SnackbarHost">
    <Grid>
        <!-- Page UI -->
    </Grid>
</m3c:SnackbarHost>
```

```csharp
SnackbarHost.Show(
    message: "Saved",
    actionText: "UNDO",
    duration: TimeSpan.FromSeconds(4),
    onAction: UndoSave);
```

```csharp
public void Show(
    string message,
    string? actionText = null,
    TimeSpan? duration = null,
    Action? onAction = null)
```

The default duration is four seconds. A new call replaces the current snackbar; clicking its action invokes the callback and dismisses it. The host has no public `Dismiss` method. Its own bounds define the overlay region, so stretch it to cover the intended content.

## RichTooltip

`RichTooltip` supplies structured tooltip content; Avalonia's `ToolTip` infrastructure still controls opening and placement.

```xml
<Button Content="Help">
    <ToolTip.Tip>
        <m3c:RichTooltip Subhead="Keyboard shortcuts">
            <TextBlock Text="Press Ctrl+K to open search."
                       TextWrapping="Wrap" />
            <m3c:RichTooltip.Actions>
                <Button Classes="text" Content="Learn more" />
            </m3c:RichTooltip.Actions>
        </m3c:RichTooltip>
    </ToolTip.Tip>
</Button>
```

`Subhead` and `Actions` both default to `null`; direct content is the tooltip body.

## SearchBar

```xml
<m3c:SearchBar Text="{Binding Query}"
               Watermark="Search components"
               QuerySubmitted="OnQuerySubmitted" />
```

```csharp
private void OnQuerySubmitted(object? sender, string query)
{
    Search(query);
}
```

| Member | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | Query text; two-way by default. |
| `Watermark` | `string?` | `"Search"` | Placeholder text. |
| `LeadingIcon` | `object?` | `null` | Replaces the built-in search icon. |
| `TrailingContent` | `object?` | `null` | Optional avatar, microphone, clear button, or other content. |
| `QuerySubmitted` | `event EventHandler<string>` | - | Raised with the current text when Enter is pressed. |

## Divider

```xml
<m3c:Divider Margin="0,8" />

<Grid ColumnDefinitions="*,Auto,*" Height="48">
    <TextBlock Grid.Column="0" Text="Left" />
    <m3c:Divider Grid.Column="1"
                   Orientation="Vertical"
                   Margin="12,0" />
    <TextBlock Grid.Column="2" Text="Right" />
</Grid>
```

`Orientation` defaults to `Horizontal`. A vertical divider needs a parent that gives it a usable height.
