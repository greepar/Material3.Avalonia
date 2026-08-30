// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 avatar: a fixed-size PrimaryContainer tile showing either an image
/// (<see cref="Source"/>, filling the container) or the first letter of
/// <see cref="Text"/>. <see cref="Variant"/> selects a circular, 12dp-rounded or
/// 4dp-rounded container shape; <see cref="Size"/> sets the edge length (default 40).
/// </summary>
public class Avatar : TemplatedControl
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Avatar, IImage?>(nameof(Source));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Avatar, string?>(nameof(Text));

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Avatar, double>(nameof(Size), 40.0,
            validate: static value => double.IsFinite(value) && value > 0);

    public static readonly StyledProperty<AvatarVariant> VariantProperty =
        AvaloniaProperty.Register<Avatar, AvatarVariant>(nameof(Variant));

    public static readonly DirectProperty<Avatar, string> DisplayInitialProperty =
        AvaloniaProperty.RegisterDirect<Avatar, string>(nameof(DisplayInitial), o => o.DisplayInitial);

    private string _displayInitial = "";

    public Avatar()
    {
        UpdateMetrics();
    }

    /// <summary>Avatar image; shown filling the container when non-null.</summary>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Fallback text; its first letter is displayed when <see cref="Source"/> is null.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Edge length of the square container, in logical pixels. Defaults to 40.</summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Container shape. Defaults to <see cref="AvatarVariant.Circle"/>.</summary>
    public AvatarVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>The uppercased first letter of <see cref="Text"/>, or an empty string.</summary>
    public string DisplayInitial
    {
        get => _displayInitial;
        private set => SetAndRaise(DisplayInitialProperty, ref _displayInitial, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SizeProperty || change.Property == VariantProperty)
        {
            UpdateMetrics();
        }
        else if (change.Property == SourceProperty)
        {
            PseudoClasses.Set(":has-image", change.NewValue is not null);
        }
        else if (change.Property == TextProperty)
        {
            var text = change.GetNewValue<string?>();
            if (string.IsNullOrEmpty(text))
            {
                DisplayInitial = "";
            }
            else
            {
                // Keep surrogate pairs (e.g. emoji) intact.
                var length = char.IsHighSurrogate(text[0]) && text.Length > 1 ? 2 : 1;
                DisplayInitial = text[..length].ToUpperInvariant();
            }
        }
    }

    private void UpdateMetrics()
    {
        var size = Size;
        SetCurrentValue(WidthProperty, size);
        SetCurrentValue(HeightProperty, size);
        // Initial label scales with the container (16 at the default 40dp size).
        SetCurrentValue(FontSizeProperty, size * 0.4);
        SetCurrentValue(CornerRadiusProperty, Variant switch
        {
            AvatarVariant.Rounded => new CornerRadius(12),
            AvatarVariant.Square => new CornerRadius(4),
            _ => new CornerRadius(size / 2),
        });
    }
}
