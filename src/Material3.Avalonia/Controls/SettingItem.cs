// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 list-style settings row: optional leading icon, headline + supporting text,
/// and a trailing slot (switch, chevron, value label…).
/// </summary>
public class SettingItem : ContentControl
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SettingItem, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> HeadlineProperty =
        AvaloniaProperty.Register<SettingItem, string?>(nameof(Headline));

    public static readonly StyledProperty<string?> SupportingTextProperty =
        AvaloniaProperty.Register<SettingItem, string?>(nameof(SupportingText));

    public static readonly StyledProperty<object?> TrailingProperty =
        AvaloniaProperty.Register<SettingItem, object?>(nameof(Trailing));

    /// <summary>Leading 24dp icon slot.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Primary line (body-large).</summary>
    public string? Headline
    {
        get => GetValue(HeadlineProperty);
        set => SetValue(HeadlineProperty, value);
    }

    /// <summary>Secondary line (body-medium, OnSurfaceVariant).</summary>
    public string? SupportingText
    {
        get => GetValue(SupportingTextProperty);
        set => SetValue(SupportingTextProperty, value);
    }

    /// <summary>Trailing slot; typically a switch, checkbox or value text.</summary>
    public object? Trailing
    {
        get => GetValue(TrailingProperty);
        set => SetValue(TrailingProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
            PseudoClasses.Set(":has-icon", change.NewValue is not null);
        else if (change.Property == SupportingTextProperty)
            PseudoClasses.Set(":has-supporting", change.NewValue is not null);
    }
}
