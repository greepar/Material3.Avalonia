# Navigation

Material3.Avalonia provides Material navigation containers and items in `Material3.Avalonia.Controls`:

```xml
xmlns:m3c="using:Material3.Avalonia.Controls"
xmlns:icons="using:Material3.Avalonia.Icons"
```

Navigation controls expose selection; they do not switch pages automatically. Handle inherited `SelectionChanged`, or bind `SelectedIndex`/`SelectedItem` and update your application's content.

## NavigationBar

Use a bottom navigation bar for three to five top-level destinations.

```xml
<m3c:NavigationBar SelectedIndex="0"
                   SelectionChanged="OnDestinationChanged">
    <m3c:NavigationBarItem Label="Home">
        <m3c:NavigationBarItem.Icon>
            <PathIcon Data="{x:Static icons:MaterialSymbols.Home}" />
        </m3c:NavigationBarItem.Icon>
    </m3c:NavigationBarItem>
    <m3c:NavigationBarItem Label="Search">
        <m3c:NavigationBarItem.Icon>
            <PathIcon Data="{x:Static icons:MaterialSymbols.Search}" />
        </m3c:NavigationBarItem.Icon>
    </m3c:NavigationBarItem>
</m3c:NavigationBar>
```

`NavigationBar` derives from `ListBox`; each destination is given equal width. It intentionally has no internal scrolling.

## NavigationRail

Use a rail in medium and wide layouts. Its optional `Header` can host a menu button or FAB.

```xml
<m3c:NavigationRail SelectedIndex="0"
                     SelectionChanged="OnDestinationChanged">
    <m3c:NavigationRail.Header>
        <m3c:IconButton Command="{Binding OpenMenuCommand}">
            <PathIcon Data="{x:Static icons:MaterialSymbols.Menu}" />
        </m3c:IconButton>
    </m3c:NavigationRail.Header>

    <m3c:NavigationRailItem Label="Home">
        <m3c:NavigationRailItem.Icon>
            <PathIcon Data="{x:Static icons:MaterialSymbols.Home}" />
        </m3c:NavigationRailItem.Icon>
    </m3c:NavigationRailItem>
</m3c:NavigationRail>
```

`NavigationRail` derives from `ListBox`, defaults to an 80dp width, and does not contain a `ScrollViewer`. Wrap it in one when the destination list can exceed the available height.

## Handling selection

```csharp
private void OnDestinationChanged(object? sender, SelectionChangedEventArgs e)
{
    if (sender is ListBox navigation)
        CurrentPageIndex = navigation.SelectedIndex;
}
```

For MVVM, bind selection instead:

```xml
<m3c:NavigationBar SelectedIndex="{Binding CurrentPageIndex}">
    ...
</m3c:NavigationBar>
```

`NavigationBarItem` and `NavigationRailItem` expose `Icon` and `Label`. Their default templates do not use inherited `Content` as the label, so explicit item containers are the simplest XAML pattern.

## NavigationDrawerItem

There is no separate `NavigationDrawer` control. Place drawer items in an ordinary `ListBox`, usually inside `SplitView.Pane`.

```xml
<SplitView IsPaneOpen="{Binding IsDrawerOpen}"
           DisplayMode="Overlay"
           OpenPaneLength="320">
    <SplitView.Pane>
        <ListBox SelectedIndex="{Binding CurrentPageIndex}">
            <m3c:NavigationDrawerItem Content="Inbox" BadgeText="12">
                <m3c:NavigationDrawerItem.Icon>
                    <PathIcon Data="{x:Static icons:MaterialSymbols.Inbox}" />
                </m3c:NavigationDrawerItem.Icon>
            </m3c:NavigationDrawerItem>
            <m3c:NavigationDrawerItem Content="Sent" />
        </ListBox>
    </SplitView.Pane>

    <ContentControl Content="{Binding CurrentPage}" />
</SplitView>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Icon` | `object?` | `null` | Optional leading icon. |
| `BadgeText` | `string?` | `null` | Optional trailing text. Use `null`, not an empty string, to hide it. |
| `Content` | inherited | `null` | Main drawer label or content. |

::: tip Responsive shells
The controls do not choose bar, rail, or drawer automatically. Use container queries or application layout state to select the appropriate navigation pattern for the available width.
:::
