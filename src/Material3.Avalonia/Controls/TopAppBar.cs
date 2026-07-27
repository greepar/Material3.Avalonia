// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Material3.Avalonia.Controls;

/// <summary>Layout variants for <see cref="TopAppBar"/>.</summary>
public enum TopAppBarVariant
{
    /// <summary>64dp bar with the title centered between the navigation icon and actions.</summary>
    CenterAligned,
    /// <summary>64dp bar with the title-large title placed after the navigation icon (default).</summary>
    Small,
    /// <summary>112dp bar with a headline-small (24) title on its own bottom row.</summary>
    Medium,
    /// <summary>152dp bar with a headline-medium (28) title on its own bottom row.</summary>
    Large,
}

/// <summary>
/// Material 3 top app bar: Surface container with a leading <see cref="NavigationIcon"/>
/// slot (48dp area), a <see cref="Title"/>, and a trailing <see cref="Actions"/> slot
/// (typically a horizontal StackPanel of <see cref="IconButton"/>s). The
/// <see cref="Variant"/> selects the container height and title placement.
/// </summary>
public class TopAppBar : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TopAppBar, string?>(nameof(Title));

    public static readonly StyledProperty<object?> NavigationIconProperty =
        AvaloniaProperty.Register<TopAppBar, object?>(nameof(NavigationIcon));

    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<TopAppBar, object?>(nameof(Actions));

    public static readonly StyledProperty<TopAppBarVariant> VariantProperty =
        AvaloniaProperty.Register<TopAppBar, TopAppBarVariant>(nameof(Variant), TopAppBarVariant.Small);

    /// <summary>The app bar title text.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional leading navigation control (typically an <see cref="IconButton"/>).</summary>
    public object? NavigationIcon
    {
        get => GetValue(NavigationIconProperty);
        set => SetValue(NavigationIconProperty, value);
    }

    /// <summary>Trailing action content (typically a horizontal StackPanel of <see cref="IconButton"/>s).</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>The layout variant. Defaults to <see cref="TopAppBarVariant.Small"/>.</summary>
    public TopAppBarVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == NavigationIconProperty)
        {
            PseudoClasses.Set(":has-navigation-icon", change.NewValue is not null);
        }
    }
}
