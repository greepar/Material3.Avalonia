// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>Selection behaviour for <see cref="SegmentedButtonGroup"/>.</summary>
public enum SegmentedSelectionMode
{
    /// <summary>At most one segment is checked; checking one unchecks the others.</summary>
    Single,
    /// <summary>Any number of segments may be checked independently.</summary>
    Multiple,
}

/// <summary>
/// Material 3 segmented button container: lays out <see cref="SegmentedButton"/>
/// segments in a horizontal row with shared outlines. The first and last segments
/// receive the outer 20dp rounded corners (via the "first"/"last" style classes),
/// and in <see cref="SegmentedSelectionMode.Single"/> mode checking a segment
/// unchecks the rest.
/// </summary>
public class SegmentedButtonGroup : ItemsControl
{
    public static readonly StyledProperty<SegmentedSelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<SegmentedButtonGroup, SegmentedSelectionMode>(nameof(SelectionMode));

    private bool _syncingSelection;

    public SegmentedButtonGroup()
    {
        // Implementation choice: first/last classes are maintained from the
        // ContainerPrepared / ContainerClearing / ContainerIndexChanged events, which
        // together cover realization, removal and reordering without a LayoutUpdated
        // recomputation pass.
        ContainerPrepared += OnContainerPrepared;
        ContainerClearing += OnContainerClearing;
        ContainerIndexChanged += (_, _) => UpdateEdgeClasses();
    }

    /// <summary>Selection behaviour. Defaults to <see cref="SegmentedSelectionMode.Single"/>.</summary>
    public SegmentedSelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is SegmentedButton segment)
        {
            segment.IsCheckedChanged += OnSegmentCheckedChanged;
        }

        UpdateEdgeClasses();
    }

    private void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container is SegmentedButton segment)
        {
            segment.IsCheckedChanged -= OnSegmentCheckedChanged;
        }

        e.Container.Classes.Remove("first");
        e.Container.Classes.Remove("last");
        UpdateEdgeClasses();
    }

    private void UpdateEdgeClasses()
    {
        var count = ItemCount;
        for (var i = 0; i < count; i++)
        {
            if (ContainerFromIndex(i) is not { } container)
            {
                continue;
            }

            SetClass(container, "first", i == 0);
            SetClass(container, "last", i == count - 1);
        }
    }

    private static void SetClass(Control control, string name, bool present)
    {
        if (present)
        {
            if (!control.Classes.Contains(name))
            {
                control.Classes.Add(name);
            }
        }
        else
        {
            control.Classes.Remove(name);
        }
    }

    private void OnSegmentCheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_syncingSelection ||
            SelectionMode != SegmentedSelectionMode.Single ||
            sender is not SegmentedButton { IsChecked: true } checkedSegment)
        {
            return;
        }

        _syncingSelection = true;
        try
        {
            var count = ItemCount;
            for (var i = 0; i < count; i++)
            {
                if (ContainerFromIndex(i) is SegmentedButton other &&
                    other != checkedSegment &&
                    other.IsChecked == true)
                {
                    other.SetCurrentValue(SegmentedButton.IsCheckedProperty, false);
                }
            }
        }
        finally
        {
            _syncingSelection = false;
        }
    }
}
