using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material menu bar that switches directly between open top-level menus on click or hover.
/// </summary>
public class Menu : global::Avalonia.Controls.Menu
{
    private MenuItem? _hoverSwitchedItem;
    private readonly HashSet<MenuItem> _subscribedItems = new();

    public Menu()
    {
        AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnPreviewPointerMoved, RoutingStrategies.Tunnel);
        ContainerPrepared += OnContainerPrepared;
        ContainerClearing += OnContainerClearing;
        AttachedToVisualTree += (_, _) => SubscribeExistingItems();
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is MenuItem item)
        {
            Subscribe(item);
        }
    }

    private void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container is MenuItem item)
        {
            Unsubscribe(item);
        }
    }

    private void SubscribeExistingItems()
    {
        foreach (var item in Items.OfType<MenuItem>())
        {
            Subscribe(item);
        }
    }

    private void Subscribe(MenuItem item)
    {
        if (!_subscribedItems.Add(item))
        {
            return;
        }

        item.PointerEntered += OnTopLevelItemPointerEntered;
        item.AddHandler(PointerPressedEvent, OnTopLevelItemPointerPressed, RoutingStrategies.Tunnel);
        item.AddHandler(PointerReleasedEvent, OnTopLevelItemPointerReleased, RoutingStrategies.Tunnel);
        item.PropertyChanged += OnTopLevelItemPropertyChanged;
    }

    private void Unsubscribe(MenuItem item)
    {
        if (!_subscribedItems.Remove(item))
        {
            return;
        }

        item.PointerEntered -= OnTopLevelItemPointerEntered;
        item.RemoveHandler(PointerPressedEvent, OnTopLevelItemPointerPressed);
        item.RemoveHandler(PointerReleasedEvent, OnTopLevelItemPointerReleased);
        item.PropertyChanged -= OnTopLevelItemPropertyChanged;
    }

    private void OnTopLevelItemPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MenuItem.IsSubMenuOpenProperty
            && sender is MenuItem item
            && ReferenceEquals(_hoverSwitchedItem, item)
            && !item.IsSubMenuOpen)
        {
            // Avalonia's built-in click handler toggles the item after the menu has
            // already switched on hover. Keep the newly selected menu open once.
            _hoverSwitchedItem = null;
            Dispatcher.UIThread.Post(
                () => item.SetCurrentValue(MenuItem.IsSubMenuOpenProperty, true),
                DispatcherPriority.Input);
        }
    }

    private void OnTopLevelItemPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is MenuItem item && SwitchOpenItem(item))
        {
            _hoverSwitchedItem = item;
        }
    }

    private void OnTopLevelItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is MenuItem item
            && e.GetCurrentPoint(item).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
        {
            SwitchOpenItem(item);
        }
    }

    private void OnTopLevelItemPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is MenuItem item && ReferenceEquals(_hoverSwitchedItem, item))
        {
            _hoverSwitchedItem = null;
            Dispatcher.UIThread.Post(
                () => item.SetCurrentValue(MenuItem.IsSubMenuOpenProperty, true),
                DispatcherPriority.Input);
            e.Handled = true;
        }
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed
            && e.Source is Visual visual
            && visual.FindAncestorOfType<MenuItem>(includeSelf: true) is { } target)
        {
            SwitchOpenItem(target);
        }
    }

    private void OnPreviewPointerMoved(object? sender, PointerEventArgs e) => SwitchOpenItem(e.Source);

    private void SwitchOpenItem(object? source)
    {
        if (source is Visual visual
            && visual.FindAncestorOfType<MenuItem>(includeSelf: true) is { } target)
        {
            if (SwitchOpenItem(target))
            {
                _hoverSwitchedItem = target;
            }
        }
    }

    private bool SwitchOpenItem(MenuItem target)
    {
        if (!ReferenceEquals(target.Parent, this) || !target.IsEnabled)
        {
            return false;
        }

        var openItem = Items.OfType<MenuItem>().FirstOrDefault(item => item.IsSubMenuOpen);
        if (openItem is null || ReferenceEquals(openItem, target))
        {
            return false;
        }

        openItem.IsSubMenuOpen = false;
        target.IsSubMenuOpen = true;
        return true;
    }
}
