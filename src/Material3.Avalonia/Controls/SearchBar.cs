// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 search bar: a 56dp-high, fully rounded SurfaceContainerHigh field with an
/// optional leading icon (magnifying glass by default), a borderless inner text input and
/// an optional trailing slot. Raises <see cref="QuerySubmitted"/> when Enter is pressed.
/// </summary>
[TemplatePart("PART_TextBox", typeof(TextBox))]
public class SearchBar : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SearchBar, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<SearchBar, string?>(nameof(Watermark), "Search");

    public static readonly StyledProperty<object?> LeadingIconProperty =
        AvaloniaProperty.Register<SearchBar, object?>(nameof(LeadingIcon));

    public static readonly StyledProperty<object?> TrailingContentProperty =
        AvaloniaProperty.Register<SearchBar, object?>(nameof(TrailingContent));

    private TextBox? _textBox;

    /// <summary>The current query text (two-way).</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Placeholder shown while the field is empty.</summary>
    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <summary>Custom leading graphic; when null a magnifying-glass icon is shown.</summary>
    public object? LeadingIcon
    {
        get => GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }

    /// <summary>Optional trailing slot (avatar, mic button, ...).</summary>
    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    /// <summary>Raised when the user presses Enter in the text field.</summary>
    public event EventHandler<string>? QuerySubmitted;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_textBox is not null)
        {
            _textBox.KeyDown -= OnTextBoxKeyDown;
        }

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");

        if (_textBox is not null)
        {
            _textBox.KeyDown += OnTextBoxKeyDown;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LeadingIconProperty)
        {
            PseudoClasses.Set(":custom-leading", change.NewValue is not null);
        }
        else if (change.Property == TrailingContentProperty)
        {
            PseudoClasses.Set(":with-trailing", change.NewValue is not null);
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.Source == this)
        {
            _textBox?.Focus();
        }
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return && !e.Handled)
        {
            e.Handled = true;
            QuerySubmitted?.Invoke(this, Text ?? string.Empty);
        }
    }
}
