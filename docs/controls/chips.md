# Chips

Material3.Avalonia provides assist, filter, input, and suggestion chips in `Material3.Avalonia.Controls`:

```xml
xmlns:m3c="using:Material3.Avalonia.Controls"
```

## ChipGroup

`ChipGroup` is a wrapping `ItemsControl`. Its `Spacing` property controls both item and line spacing and defaults to `8`.

```xml
<m3c:ChipGroup Spacing="8">
    <m3c:AssistChip Content="Add to calendar" />
    <m3c:FilterChip Content="Nearby" IsChecked="True" />
    <m3c:InputChip Content="Alex" Removed="OnChipRemoved" />
    <m3c:SuggestionChip Content="Reply: Thanks!" />
</m3c:ChipGroup>
```

The group does not manage selection. Bind each `FilterChip.IsChecked` independently or maintain selection in your view model.

## AssistChip and SuggestionChip

Both derive from `Chip` and support the same two properties:

| Property | Type | Default | Description |
|---|---|---|---|
| `Icon` | `object?` | `null` | Optional 18dp leading content. |
| `IsElevated` | `bool` | `false` | Uses a filled surface and elevation instead of an outline. |

```xml
<m3c:AssistChip Content="Directions" IsElevated="True">
    <m3c:AssistChip.Icon>
        <PathIcon Data="{x:Static icons:MaterialSymbols.Directions}" />
    </m3c:AssistChip.Icon>
</m3c:AssistChip>
```

`Chip`, `AssistChip`, and `SuggestionChip` derive from `Button`. They support `Click`, keyboard activation, `Command`, and `CommandParameter` for actions such as opening directions or applying a suggestion.

## FilterChip

`FilterChip` derives from `ToggleButton`, so it supports `IsChecked`, `Click`, `Command`, and `IsCheckedChanged`. When selected, its optional icon is replaced by a checkmark.

```xml
<m3c:FilterChip Content="Favorites"
                Icon="{x:Static icons:MaterialSymbols.Favorite}"
                IsChecked="{Binding FavoritesOnly}" />
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Icon` | `object?` | `null` | Optional leading content shown while unchecked. |

## InputChip

`InputChip` represents user-supplied input and can show a trailing remove affordance.

```xml
<m3c:InputChip Content="alex@example.com"
               IsRemovable="True"
               Removed="OnChipRemoved" />
```

```csharp
private void OnChipRemoved(object? sender, EventArgs e)
{
    if (sender is InputChip chip)
        Recipients.Remove(chip.Content?.ToString());
}
```

| Member | Type | Default | Description |
|---|---|---|---|
| `Icon` | `object?` | `null` | Optional leading content. |
| `IsRemovable` | `bool` | `true` | Shows the trailing remove affordance. |
| `Removed` | `event EventHandler` | - | Raised when remove is clicked. The chip does not remove itself from a collection. |

::: tip Icons
The examples use the strongly typed icon catalog. Add `xmlns:icons="using:Material3.Avalonia.Icons"`; see [Material Symbols](../guide/icons) for naming and search guidance.
:::
