// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Material3.Avalonia.Controls;

/// <summary>Container variants for <see cref="Card"/>.</summary>
public enum CardVariant
{
    /// <summary>SurfaceContainerLow container with a level-1 shadow (default).</summary>
    Elevated,
    /// <summary>SurfaceContainerHighest container, no shadow.</summary>
    Filled,
    /// <summary>Surface container with a 1px OutlineVariant border.</summary>
    Outlined,
}

/// <summary>
/// Material 3 card: a 12dp-rounded content container. <see cref="Variant"/> selects the
/// elevated, filled or outlined container treatment. When <see cref="IsClickable"/> is
/// true the card shows a hover state layer, a ripple, a hand cursor, and raises
/// <see cref="Clicked"/> on pointer release inside its bounds.
/// </summary>
public class Card : ContentControl
{
    public static readonly StyledProperty<CardVariant> VariantProperty =
        AvaloniaProperty.Register<Card, CardVariant>(nameof(Variant), CardVariant.Elevated);

    public static readonly StyledProperty<bool> IsClickableProperty =
        AvaloniaProperty.Register<Card, bool>(nameof(IsClickable));

    /// <summary>The container treatment. Defaults to <see cref="CardVariant.Elevated"/>.</summary>
    public CardVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Whether the card reacts to pointer interaction and raises <see cref="Clicked"/>.</summary>
    public bool IsClickable
    {
        get => GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    /// <summary>Raised when a clickable card is released inside its bounds.</summary>
    public event EventHandler? Clicked;

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (IsClickable && new Rect(Bounds.Size).Contains(e.GetPosition(this)))
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
