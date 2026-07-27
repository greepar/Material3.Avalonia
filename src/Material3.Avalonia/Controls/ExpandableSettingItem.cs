// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

namespace Material3.Avalonia.Controls;

/// <summary>
/// A settings row that expands to reveal <see cref="ContentControl.Content"/> below it.
/// The header row shows icon/headline/supporting text and a rotating chevron.
/// </summary>
[TemplatePart(PartHeader, typeof(Border))]
public class ExpandableSettingItem : ContentControl
{
    public const string PartHeader = "PART_Header";

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<ExpandableSettingItem, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> HeadlineProperty =
        AvaloniaProperty.Register<ExpandableSettingItem, string?>(nameof(Headline));

    public static readonly StyledProperty<string?> SupportingTextProperty =
        AvaloniaProperty.Register<ExpandableSettingItem, string?>(nameof(SupportingText));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<ExpandableSettingItem, bool>(nameof(IsExpanded),
            defaultBindingMode: BindingMode.TwoWay);

    private Border? _header;

    /// <summary>Leading 24dp icon slot.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Primary line.</summary>
    public string? Headline
    {
        get => GetValue(HeadlineProperty);
        set => SetValue(HeadlineProperty, value);
    }

    /// <summary>Secondary line.</summary>
    public string? SupportingText
    {
        get => GetValue(SupportingTextProperty);
        set => SetValue(SupportingTextProperty, value);
    }

    /// <summary>Whether the detail content is visible.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_header is not null)
            _header.PointerReleased -= OnHeaderReleased;
        _header = e.NameScope.Find<Border>(PartHeader);
        if (_header is not null)
            _header.PointerReleased += OnHeaderReleased;
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsExpandedProperty)
            UpdatePseudoClasses();
        else if (change.Property == IconProperty)
            PseudoClasses.Set(":has-icon", change.NewValue is not null);
        else if (change.Property == SupportingTextProperty)
            PseudoClasses.Set(":has-supporting", change.NewValue is not null);
    }

    private void UpdatePseudoClasses() => PseudoClasses.Set(":expanded", IsExpanded);

    private void OnHeaderReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.InitialPressMouseButton == MouseButton.Left)
        {
            SetCurrentValue(IsExpandedProperty, !IsExpanded);
            e.Handled = true;
        }
    }
}
