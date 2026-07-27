// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 FAB menu: a primary <see cref="FloatingActionButton"/> that toggles a
/// vertical column of action items (typically small FABs or extended FABs) above it.
/// Clicking the primary FAB toggles <see cref="IsOpen"/>; while open, the primary
/// FAB's icon container rotates 45 degrees (a "+" icon supplied via <see cref="Icon"/>
/// visually becomes an X) and the items fade/slide in.
/// </summary>
[TemplatePart("PART_PrimaryFab", typeof(FloatingActionButton))]
public class FabMenu : ItemsControl
{
    public static readonly StyledProperty<FabColor> ColorProperty =
        FloatingActionButton.ColorProperty.AddOwner<FabMenu>();

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FabMenu, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<FabMenu, object?>(nameof(Icon));

    private FloatingActionButton? _primaryFab;

    public FabMenu()
    {
        AddHandler(Button.ClickEvent, OnAnyButtonClick, RoutingStrategies.Bubble);
        ContainerPrepared += OnContainerPrepared;
        ContainerClearing += OnContainerClearing;
    }

    /// <summary>
    /// Color scheme inherited by the primary FAB and menu items that do not set their
    /// own <see cref="FloatingActionButton.Color"/> value.
    /// </summary>
    public FabColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Whether the action items are shown. Toggled by the primary FAB.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The primary FAB's graphic, typically a "+" path (reads as an X while open).</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ApplyItemClasses();
        if (_primaryFab is not null)
        {
            _primaryFab.Click -= OnPrimaryFabClick;
        }

        _primaryFab = e.NameScope.Find<FloatingActionButton>("PART_PrimaryFab");
        if (_primaryFab is not null)
        {
            _primaryFab.Click += OnPrimaryFabClick;
        }
    }

    private void OnPrimaryFabClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsOpenProperty, !IsOpen);
    }

    private void ApplyItemClasses()
    {
        foreach (var item in Items)
        {
            if (item is ExtendedFloatingActionButton extendedFab)
            {
                extendedFab.Classes.Add("fab-menu-item");
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            SetCurrentValue(IsOpenProperty, false);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnAnyButtonClick(object? sender, RoutedEventArgs e)
    {
        if (IsOpen && e.Source is Button button && !ReferenceEquals(button, _primaryFab))
        {
            SetCurrentValue(IsOpenProperty, false);
        }
    }

    private static void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (GetExtendedFab(sender, e.Container, e.Index) is { } item)
        {
            item.Classes.Add("fab-menu-item");
        }
    }

    private static void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (GetExtendedFab(sender, e.Container, -1) is { } item)
        {
            item.Classes.Remove("fab-menu-item");
        }
    }

    private static ExtendedFloatingActionButton? GetExtendedFab(object? sender, Control container, int index) =>
        container as ExtendedFloatingActionButton ??
        (container as ContentPresenter)?.Content as ExtendedFloatingActionButton ??
        (index >= 0 ? (sender as FabMenu)?.Items[index] : null) as ExtendedFloatingActionButton;
}
