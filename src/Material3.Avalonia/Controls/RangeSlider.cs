// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Globalization;

namespace Material3.Avalonia.Controls;

/// <summary>Controls when a <see cref="RangeSlider"/> displays its value indicators.</summary>
public enum RangeSliderValueIndicatorMode
{
    /// <summary>Never display value indicators.</summary>
    Never,
    /// <summary>Display the indicator for the handle currently being dragged or focused.</summary>
    OnInteraction,
    /// <summary>Always display indicators for both handles.</summary>
    Always,
}

/// <summary>Provides old and new values for a <see cref="RangeSlider"/> handle.</summary>
public sealed class RangeSliderValueChangedEventArgs : EventArgs
{
    internal RangeSliderValueChangedEventArgs(double oldValue, double newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>Gets the value before the change.</summary>
    public double OldValue { get; }

    /// <summary>Gets the value after the change.</summary>
    public double NewValue { get; }
}

/// <summary>
/// Material 3 (2024 expressive) range slider with two handles selecting a
/// [<see cref="LowerValue"/>, <see cref="UpperValue"/>] interval. Renders three 16dp-thick
/// track segments (SecondaryContainer outside, Primary between the handles) separated from
/// the 4x44 bar handles by a 6dp gap. The template positions everything on a Canvas; this
/// class computes the segment/handle geometry from the values and control width.
/// </summary>
[TemplatePart("PART_Track", typeof(Border))]
[TemplatePart("PART_LowerThumb", typeof(Thumb))]
[TemplatePart("PART_UpperThumb", typeof(Thumb))]
[TemplatePart("PART_LowerIndicator", typeof(Border))]
[TemplatePart("PART_UpperIndicator", typeof(Border))]
public class RangeSlider : TemplatedControl
{
    /// <summary>Hit-test width of each thumb (the visible bar is 4dp wide, centred).</summary>
    private const double ThumbHitWidth = 20;

    /// <summary>Horizontal padding so handle centres stay inside the control.</summary>
    private const double EdgePadding = 8;

    /// <summary>Distance from a handle centre to the adjacent track end: 6dp gap + 2dp half handle.</summary>
    private const double SegmentInset = 8;

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(Minimum), 0.0,
            validate: double.IsFinite);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(Maximum), 100.0,
            validate: double.IsFinite);

    public static readonly StyledProperty<double> LowerValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(LowerValue), 20.0,
            defaultBindingMode: BindingMode.TwoWay, validate: double.IsFinite, coerce: CoerceLowerValue);

    public static readonly StyledProperty<double> UpperValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(UpperValue), 80.0,
            defaultBindingMode: BindingMode.TwoWay, validate: double.IsFinite, coerce: CoerceUpperValue);

    public static readonly StyledProperty<double> SmallChangeProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(SmallChange), 1.0,
            validate: static value => double.IsFinite(value) && value > 0);

    public static readonly StyledProperty<double> LargeChangeProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(LargeChange), 10.0,
            validate: static value => double.IsFinite(value) && value > 0);

    public static readonly StyledProperty<double> TickFrequencyProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(TickFrequency), 1.0,
            validate: static value => double.IsFinite(value) && value > 0);

    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AvaloniaProperty.Register<RangeSlider, bool>(nameof(IsSnapToTickEnabled));

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        AvaloniaProperty.Register<RangeSlider, bool>(nameof(IsDirectionReversed));

    public static readonly StyledProperty<RangeSliderValueIndicatorMode> ValueIndicatorModeProperty =
        AvaloniaProperty.Register<RangeSlider, RangeSliderValueIndicatorMode>(
            nameof(ValueIndicatorMode), RangeSliderValueIndicatorMode.OnInteraction);

    public static readonly StyledProperty<string> ValueFormatProperty =
        AvaloniaProperty.Register<RangeSlider, string>(nameof(ValueFormat), "0.##");

    private Border? _track;
    private Canvas? _canvas;
    private Border? _startSegment;
    private Border? _midSegment;
    private Border? _endSegment;
    private Thumb? _lowerThumb;
    private Thumb? _upperThumb;
    private Border? _lowerIndicator;
    private Border? _upperIndicator;
    private TextBlock? _lowerIndicatorText;
    private TextBlock? _upperIndicatorText;
    private bool _isLowerInteracting;
    private bool _isUpperInteracting;

    /// <summary>Raised after <see cref="LowerValue"/> changes.</summary>
    public event EventHandler<RangeSliderValueChangedEventArgs>? LowerValueChanged;

    /// <summary>Raised after <see cref="UpperValue"/> changes.</summary>
    public event EventHandler<RangeSliderValueChangedEventArgs>? UpperValueChanged;

    /// <summary>Lower bound of the selectable range.</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Upper bound of the selectable range.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Value of the left handle; coerced into [<see cref="Minimum"/>, <see cref="UpperValue"/>].</summary>
    public double LowerValue
    {
        get => GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    /// <summary>Value of the right handle; coerced into [<see cref="LowerValue"/>, <see cref="Maximum"/>].</summary>
    public double UpperValue
    {
        get => GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    /// <summary>Amount applied by arrow-key input. Defaults to 1.</summary>
    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    /// <summary>Amount applied by PageUp and PageDown. Defaults to 10.</summary>
    public double LargeChange
    {
        get => GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    /// <summary>Distance between snap points when <see cref="IsSnapToTickEnabled"/> is true.</summary>
    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    /// <summary>Whether handle values snap to multiples of <see cref="TickFrequency"/> from <see cref="Minimum"/>.</summary>
    public bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    /// <summary>Whether minimum is rendered at the right edge and maximum at the left edge.</summary>
    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    /// <summary>Controls when formatted handle values are displayed above the handles.</summary>
    public RangeSliderValueIndicatorMode ValueIndicatorMode
    {
        get => GetValue(ValueIndicatorModeProperty);
        set => SetValue(ValueIndicatorModeProperty, value);
    }

    /// <summary>.NET numeric format string used by value indicators. Defaults to <c>0.##</c>.</summary>
    public string ValueFormat
    {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    private static double CoerceLowerValue(AvaloniaObject sender, double value)
    {
        var slider = (RangeSlider)sender;
        value = slider.Snap(value);
        var min = slider.Minimum;
        var max = Math.Min(slider.Maximum, slider.UpperValue);
        return Math.Clamp(value, min, Math.Max(min, max));
    }

    private static double CoerceUpperValue(AvaloniaObject sender, double value)
    {
        var slider = (RangeSlider)sender;
        value = slider.Snap(value);
        var max = slider.Maximum;
        var min = Math.Max(slider.Minimum, slider.LowerValue);
        return Math.Clamp(value, Math.Min(min, max), max);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_lowerThumb is not null)
        {
            _lowerThumb.DragDelta -= OnLowerThumbDragDelta;
            _lowerThumb.DragStarted -= OnLowerThumbDragStarted;
            _lowerThumb.DragCompleted -= OnLowerThumbDragCompleted;
            _lowerThumb.GotFocus -= OnLowerThumbGotFocus;
            _lowerThumb.LostFocus -= OnLowerThumbLostFocus;
            _lowerThumb.KeyDown -= OnLowerThumbKeyDown;
        }
        if (_upperThumb is not null)
        {
            _upperThumb.DragDelta -= OnUpperThumbDragDelta;
            _upperThumb.DragStarted -= OnUpperThumbDragStarted;
            _upperThumb.DragCompleted -= OnUpperThumbDragCompleted;
            _upperThumb.GotFocus -= OnUpperThumbGotFocus;
            _upperThumb.LostFocus -= OnUpperThumbLostFocus;
            _upperThumb.KeyDown -= OnUpperThumbKeyDown;
        }
        if (_track is not null)
        {
            _track.PointerPressed -= OnTrackPointerPressed;
        }

        _track = e.NameScope.Find<Border>("PART_Track");
        _canvas = e.NameScope.Find<Canvas>("PART_Canvas");
        _startSegment = e.NameScope.Find<Border>("PART_StartSegment");
        _midSegment = e.NameScope.Find<Border>("PART_MidSegment");
        _endSegment = e.NameScope.Find<Border>("PART_EndSegment");
        _lowerThumb = e.NameScope.Find<Thumb>("PART_LowerThumb");
        _upperThumb = e.NameScope.Find<Thumb>("PART_UpperThumb");
        _lowerIndicator = e.NameScope.Find<Border>("PART_LowerIndicator");
        _upperIndicator = e.NameScope.Find<Border>("PART_UpperIndicator");
        _lowerIndicatorText = e.NameScope.Find<TextBlock>("PART_LowerIndicatorText");
        _upperIndicatorText = e.NameScope.Find<TextBlock>("PART_UpperIndicatorText");

        if (_lowerThumb is not null)
        {
            _lowerThumb.DragDelta += OnLowerThumbDragDelta;
            _lowerThumb.DragStarted += OnLowerThumbDragStarted;
            _lowerThumb.DragCompleted += OnLowerThumbDragCompleted;
            _lowerThumb.GotFocus += OnLowerThumbGotFocus;
            _lowerThumb.LostFocus += OnLowerThumbLostFocus;
            _lowerThumb.KeyDown += OnLowerThumbKeyDown;
        }
        if (_upperThumb is not null)
        {
            _upperThumb.DragDelta += OnUpperThumbDragDelta;
            _upperThumb.DragStarted += OnUpperThumbDragStarted;
            _upperThumb.DragCompleted += OnUpperThumbDragCompleted;
            _upperThumb.GotFocus += OnUpperThumbGotFocus;
            _upperThumb.LostFocus += OnUpperThumbLostFocus;
            _upperThumb.KeyDown += OnUpperThumbKeyDown;
        }
        if (_track is not null)
        {
            _track.PointerPressed += OnTrackPointerPressed;
        }

        UpdateVisuals();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateVisuals();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MinimumProperty || change.Property == MaximumProperty)
        {
            if (change.Property == MinimumProperty && Minimum > Maximum)
                SetCurrentValue(MaximumProperty, Minimum);
            else if (change.Property == MaximumProperty && Maximum < Minimum)
                SetCurrentValue(MinimumProperty, Maximum);

            CoerceValue(LowerValueProperty);
            CoerceValue(UpperValueProperty);
            UpdateVisuals();
        }
        else if (change.Property == LowerValueProperty)
        {
            CoerceValue(UpperValueProperty);
            UpdateVisuals();
            LowerValueChanged?.Invoke(this, new RangeSliderValueChangedEventArgs(
                change.GetOldValue<double>(), change.GetNewValue<double>()));
        }
        else if (change.Property == UpperValueProperty)
        {
            CoerceValue(LowerValueProperty);
            UpdateVisuals();
            UpperValueChanged?.Invoke(this, new RangeSliderValueChangedEventArgs(
                change.GetOldValue<double>(), change.GetNewValue<double>()));
        }
        else if (change.Property == IsDirectionReversedProperty ||
                 change.Property == ValueIndicatorModeProperty ||
                 change.Property == ValueFormatProperty ||
                 change.Property == IsSnapToTickEnabledProperty ||
                 change.Property == TickFrequencyProperty)
        {
            if (change.Property == IsSnapToTickEnabledProperty || change.Property == TickFrequencyProperty)
            {
                CoerceValue(LowerValueProperty);
                CoerceValue(UpperValueProperty);
            }
            UpdateVisuals();
        }
    }

    private double CanvasWidth => _canvas?.Bounds.Width is > 0 and var w && !double.IsNaN(w) ? w : Bounds.Width;

    private double Fraction(double value)
    {
        var range = Maximum - Minimum;
        var fraction = range <= 0 ? 0 : Math.Clamp((value - Minimum) / range, 0, 1);
        return IsDirectionReversed ? 1 - fraction : fraction;
    }

    private void UpdateVisuals()
    {
        if (_startSegment is null || _midSegment is null || _endSegment is null
            || _lowerThumb is null || _upperThumb is null)
        {
            return;
        }

        var width = CanvasWidth;
        if (width <= 0 || double.IsNaN(width))
        {
            return;
        }

        var usable = Math.Max(0, width - 2 * EdgePadding);
        var lowerX = EdgePadding + Fraction(LowerValue) * usable;
        var upperX = EdgePadding + Fraction(UpperValue) * usable;
        var leftX = Math.Min(lowerX, upperX);
        var rightX = Math.Max(lowerX, upperX);

        SetSegment(_startSegment, 0, leftX - SegmentInset);
        SetSegment(_midSegment, leftX + SegmentInset, rightX - leftX - 2 * SegmentInset);
        SetSegment(_endSegment, rightX + SegmentInset, width - rightX - SegmentInset);

        Canvas.SetLeft(_lowerThumb, lowerX - ThumbHitWidth / 2);
        Canvas.SetLeft(_upperThumb, upperX - ThumbHitWidth / 2);
        PositionIndicator(_lowerIndicator, lowerX);
        PositionIndicator(_upperIndicator, upperX);
        if (_lowerIndicatorText is not null)
            _lowerIndicatorText.Text = FormatValue(LowerValue);
        if (_upperIndicatorText is not null)
            _upperIndicatorText.Text = FormatValue(UpperValue);
        UpdateIndicatorVisibility();
    }

    private static void SetSegment(Border segment, double left, double width)
    {
        if (width <= 0)
        {
            segment.IsVisible = false;
            return;
        }

        segment.IsVisible = true;
        Canvas.SetLeft(segment, left);
        segment.Width = width;
    }

    private double PixelsToValueDelta(double pixels)
    {
        var usable = Math.Max(1, CanvasWidth - 2 * EdgePadding);
        var delta = pixels / usable * (Maximum - Minimum);
        return IsDirectionReversed ? -delta : delta;
    }

    private void OnLowerThumbDragDelta(object? sender, VectorEventArgs e)
    {
        SetCurrentValue(LowerValueProperty, LowerValue + PixelsToValueDelta(e.Vector.X));
    }

    private void OnUpperThumbDragDelta(object? sender, VectorEventArgs e)
    {
        SetCurrentValue(UpperValueProperty, UpperValue + PixelsToValueDelta(e.Vector.X));
    }

    private void OnTrackPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Clicks on the thumbs are handled by the thumbs themselves.
        if (e.Source is Visual source && (_lowerThumb?.IsVisualAncestorOf(source) == true
                                          || _upperThumb?.IsVisualAncestorOf(source) == true
                                          || source == _lowerThumb || source == _upperThumb))
        {
            return;
        }

        var reference = (Visual?)_canvas ?? this;
        var x = e.GetPosition(reference).X;
        var usable = Math.Max(1, CanvasWidth - 2 * EdgePadding);
        var fraction = Math.Clamp((x - EdgePadding) / usable, 0, 1);
        if (IsDirectionReversed)
            fraction = 1 - fraction;
        var value = Minimum + fraction * (Maximum - Minimum);

        // Move whichever handle is closest to the pressed position.
        if (Math.Abs(value - LowerValue) <= Math.Abs(value - UpperValue))
        {
            SetCurrentValue(LowerValueProperty, value);
        }
        else
        {
            SetCurrentValue(UpperValueProperty, value);
        }

        e.Handled = true;
    }

    private double Snap(double value)
    {
        if (!IsSnapToTickEnabled)
            return value;
        return Minimum + Math.Round((value - Minimum) / TickFrequency) * TickFrequency;
    }

    private static void PositionIndicator(Border? indicator, double centerX)
    {
        if (indicator is not null)
            Canvas.SetLeft(indicator, centerX - 28);
    }

    private string FormatValue(double value)
    {
        try
        {
            return value.ToString(ValueFormat, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }
    }

    private void UpdateIndicatorVisibility()
    {
        if (_lowerIndicator is not null)
            _lowerIndicator.IsVisible = ValueIndicatorMode == RangeSliderValueIndicatorMode.Always ||
                                        ValueIndicatorMode == RangeSliderValueIndicatorMode.OnInteraction && _isLowerInteracting;
        if (_upperIndicator is not null)
            _upperIndicator.IsVisible = ValueIndicatorMode == RangeSliderValueIndicatorMode.Always ||
                                        ValueIndicatorMode == RangeSliderValueIndicatorMode.OnInteraction && _isUpperInteracting;
    }

    private void OnLowerThumbDragStarted(object? sender, VectorEventArgs e) => SetLowerInteraction(true);
    private void OnLowerThumbDragCompleted(object? sender, VectorEventArgs e) => SetLowerInteraction(_lowerThumb?.IsFocused == true);
    private void OnUpperThumbDragStarted(object? sender, VectorEventArgs e) => SetUpperInteraction(true);
    private void OnUpperThumbDragCompleted(object? sender, VectorEventArgs e) => SetUpperInteraction(_upperThumb?.IsFocused == true);
    private void OnLowerThumbGotFocus(object? sender, RoutedEventArgs e) => SetLowerInteraction(true);
    private void OnLowerThumbLostFocus(object? sender, RoutedEventArgs e) => SetLowerInteraction(false);
    private void OnUpperThumbGotFocus(object? sender, RoutedEventArgs e) => SetUpperInteraction(true);
    private void OnUpperThumbLostFocus(object? sender, RoutedEventArgs e) => SetUpperInteraction(false);

    private void SetLowerInteraction(bool value)
    {
        _isLowerInteracting = value;
        UpdateIndicatorVisibility();
    }

    private void SetUpperInteraction(bool value)
    {
        _isUpperInteracting = value;
        UpdateIndicatorVisibility();
    }

    private void OnLowerThumbKeyDown(object? sender, KeyEventArgs e) =>
        HandleKey(e, LowerValue, Minimum, UpperValue, value => SetCurrentValue(LowerValueProperty, value));

    private void OnUpperThumbKeyDown(object? sender, KeyEventArgs e) =>
        HandleKey(e, UpperValue, LowerValue, Maximum, value => SetCurrentValue(UpperValueProperty, value));

    private void HandleKey(KeyEventArgs e, double current, double minimum, double maximum, Action<double> setValue)
    {
        var direction = IsDirectionReversed ? -1 : 1;
        double? value = e.Key switch
        {
            Key.Left or Key.Down => current - direction * Math.Abs(SmallChange),
            Key.Right or Key.Up => current + direction * Math.Abs(SmallChange),
            Key.PageDown => current - direction * Math.Abs(LargeChange),
            Key.PageUp => current + direction * Math.Abs(LargeChange),
            Key.Home => minimum,
            Key.End => maximum,
            _ => null,
        };

        if (value is null)
            return;

        setValue(Math.Clamp(value.Value, minimum, maximum));
        e.Handled = true;
    }
}
