# App Bars and Toolbar

Use `xmlns:m3c="using:Material3.Avalonia.Controls"` for all controls on this page.

## TopAppBar

`TopAppBar` provides a navigation slot, title, and action slot.

```xml
<m3c:TopAppBar Title="Messages" Variant="Small">
    <m3c:TopAppBar.NavigationIcon>
        <m3c:IconButton Command="{Binding OpenMenuCommand}">
            <PathIcon Data="{x:Static icons:MaterialSymbols.Menu}" />
        </m3c:IconButton>
    </m3c:TopAppBar.NavigationIcon>

    <m3c:TopAppBar.Actions>
        <StackPanel Orientation="Horizontal" Spacing="4">
            <m3c:IconButton Command="{Binding SearchCommand}">
                <PathIcon Data="{x:Static icons:MaterialSymbols.Search}" />
            </m3c:IconButton>
            <m3c:IconButton Command="{Binding MoreCommand}">
                <PathIcon Data="{x:Static icons:MaterialSymbols.MoreVert}" />
            </m3c:IconButton>
        </StackPanel>
    </m3c:TopAppBar.Actions>
</m3c:TopAppBar>
```

Add `xmlns:icons="using:Material3.Avalonia.Icons"` when using the icon catalog.

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | App bar title. |
| `NavigationIcon` | `object?` | `null` | Usually an `IconButton`. |
| `Actions` | `object?` | `null` | One content slot; wrap multiple controls in a panel. |
| `Variant` | `TopAppBarVariant` | `Small` | `CenterAligned`, `Small`, `Medium`, or `Large`. |

`CenterAligned` and `Small` are 64dp high. `Medium` and `Large` use taller layouts with the title on the lower row. The app bar does not handle navigation itself; commands belong on controls in its slots.

## BottomAppBar

`BottomAppBar` hosts actions and an optional trailing FAB. It is for commands, unlike `NavigationBar`, which represents destinations.

```xml
<m3c:BottomAppBar>
    <m3c:BottomAppBar.Actions>
        <StackPanel Orientation="Horizontal" Spacing="4">
            <m3c:IconButton Command="{Binding AttachCommand}">
                <PathIcon Data="{x:Static icons:MaterialSymbols.AttachFile}" />
            </m3c:IconButton>
        </StackPanel>
    </m3c:BottomAppBar.Actions>

    <m3c:BottomAppBar.FloatingActionButton>
        <m3c:FloatingActionButton Size="Small" Command="{Binding CreateCommand}">
            <PathIcon Data="{x:Static icons:MaterialSymbols.Add}" />
        </m3c:FloatingActionButton>
    </m3c:BottomAppBar.FloatingActionButton>
</m3c:BottomAppBar>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Actions` | `object?` | `null` | Leading action content. |
| `FloatingActionButton` | `object?` | `null` | Optional trailing content, normally a FAB. |

## Toolbar

`Toolbar` is a 64dp container for a related row of actions. Its direct XAML child becomes `Content`.

```xml
<m3c:Toolbar Variant="Floating">
    <StackPanel Orientation="Horizontal" Spacing="4">
        <m3c:IconButton Command="{Binding UndoCommand}" />
        <m3c:IconButton Command="{Binding RedoCommand}" />
    </StackPanel>
</m3c:Toolbar>
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` | `object?` | `null` | Toolbar contents. |
| `Variant` | `ToolbarVariant` | `Docked` | `Docked` stretches edge-to-edge; `Floating` is centered, rounded, and elevated. |

Floating toolbar shadows draw outside the control. Keep `ClipToBounds="False"` on surrounding containers when the shadow is important.
