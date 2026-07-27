// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 input chip: represents a discrete piece of user input such as a recipient
/// or tag. Supports an optional 18dp leading <see cref="Icon"/> and, when
/// <see cref="IsRemovable"/> is true, a trailing remove affordance that raises
/// <see cref="Removed"/> when activated.
/// </summary>
[TemplatePart(PartRemoveButton, typeof(Button))]
public class InputChip : ContentControl
{
    public const string PartRemoveButton = "PART_RemoveButton";

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<InputChip, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsRemovableProperty =
        AvaloniaProperty.Register<InputChip, bool>(nameof(IsRemovable), true);

    private Button? _removeButton;

    /// <summary>Optional leading graphic, displayed at 18x18 before the label.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether the trailing remove affordance is shown. Defaults to true.</summary>
    public bool IsRemovable
    {
        get => GetValue(IsRemovableProperty);
        set => SetValue(IsRemovableProperty, value);
    }

    /// <summary>Raised when the trailing remove affordance is activated.</summary>
    public event EventHandler? Removed;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_removeButton is not null)
        {
            _removeButton.Click -= OnRemoveClick;
        }

        _removeButton = e.NameScope.Find<Button>(PartRemoveButton);
        if (_removeButton is not null)
        {
            _removeButton.Click += OnRemoveClick;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            PseudoClasses.Set(":with-icon", change.NewValue is not null);
        }
    }

    private void OnRemoveClick(object? sender, RoutedEventArgs e)
    {
        Removed?.Invoke(this, EventArgs.Empty);
    }
}
