// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 snackbar: an InverseSurface, 4dp-rounded, level-3 elevated bar showing a
/// short <see cref="Message"/> and an optional text action. The action button is shown
/// only when <see cref="ActionText"/> is set and raises <see cref="ActionClicked"/>.
/// </summary>
[TemplatePart(PartActionButton, typeof(Button))]
public class Snackbar : TemplatedControl
{
    public const string PartActionButton = "PART_ActionButton";

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<Snackbar, string?>(nameof(Message));

    public static readonly StyledProperty<string?> ActionTextProperty =
        AvaloniaProperty.Register<Snackbar, string?>(nameof(ActionText));

    private Button? _actionButton;

    /// <summary>The snackbar message text.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Label of the optional trailing text action; null/empty hides the action.</summary>
    public string? ActionText
    {
        get => GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    /// <summary>Raised when the action button is clicked.</summary>
    public event EventHandler? ActionClicked;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_actionButton is not null)
        {
            _actionButton.Click -= OnActionClick;
        }

        _actionButton = e.NameScope.Find<Button>(PartActionButton);
        if (_actionButton is not null)
        {
            _actionButton.Click += OnActionClick;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActionTextProperty)
        {
            PseudoClasses.Set(":has-action", !string.IsNullOrEmpty(change.NewValue as string));
        }
    }

    private void OnActionClick(object? sender, RoutedEventArgs e)
    {
        ActionClicked?.Invoke(this, EventArgs.Empty);
    }
}
