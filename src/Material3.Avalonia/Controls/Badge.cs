// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 badge: a small Error-colored indicator. The dot form
/// (<see cref="IsDotBadge"/>) is a 6dp circle; the large form is a 16dp-high pill
/// showing <see cref="Value"/>, capped at <see cref="MaxValue"/> (e.g. "99+").
/// </summary>
public class Badge : TemplatedControl
{
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<Badge, int>(nameof(Value), coerce: static (_, value) => Math.Max(0, value));

    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<Badge, int>(nameof(MaxValue), 99,
            coerce: static (_, value) => Math.Max(0, value));

    public static readonly StyledProperty<bool> IsDotBadgeProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsDotBadge));

    public static readonly DirectProperty<Badge, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<Badge, string>(nameof(DisplayText), o => o.DisplayText);

    private string _displayText = "0";

    /// <summary>The count displayed by the large badge. Defaults to 0.</summary>
    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Maximum displayed count; larger values render as "MaxValue+". Defaults to 99.</summary>
    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>When true, renders the small 6dp dot form without a label.</summary>
    public bool IsDotBadge
    {
        get => GetValue(IsDotBadgeProperty);
        set => SetValue(IsDotBadgeProperty, value);
    }

    /// <summary>The label shown by the large badge ("Value" or "MaxValue+").</summary>
    public string DisplayText
    {
        get => _displayText;
        private set => SetAndRaise(DisplayTextProperty, ref _displayText, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == MaxValueProperty)
        {
            var value = Value;
            var max = MaxValue;
            DisplayText = value > max ? max + "+" : value.ToString();
        }
    }
}
