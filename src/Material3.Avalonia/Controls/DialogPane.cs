// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 basic dialog surface: a SurfaceContainerHigh, 28dp-rounded, level-3
/// elevated pane laying out an optional centered <see cref="Icon"/>, a headline-small
/// <see cref="Title"/>, the body content and right-aligned <see cref="Buttons"/>.
/// Purely visual; host it yourself in an overlay or window.
/// </summary>
public class DialogPane : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DialogPane, string?>(nameof(Title));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<DialogPane, object?>(nameof(Icon));

    public static readonly StyledProperty<object?> ButtonsProperty =
        AvaloniaProperty.Register<DialogPane, object?>(nameof(Buttons));

    /// <summary>The headline text.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional hero icon shown centered above the title, in the Secondary color.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Trailing action area content (typically a horizontal StackPanel of text buttons).</summary>
    public object? Buttons
    {
        get => GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            PseudoClasses.Set(":with-icon", change.NewValue is not null);
        }
        else if (change.Property == TitleProperty)
        {
            PseudoClasses.Set(":with-title", !string.IsNullOrEmpty(change.NewValue as string));
        }
        else if (change.Property == ButtonsProperty)
        {
            PseudoClasses.Set(":with-buttons", change.NewValue is not null);
        }
    }
}
