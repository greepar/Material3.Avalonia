using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace Material3.Avalonia.Controls;

/// <summary>Which unit the <see cref="TimePickerDial"/> is currently selecting.</summary>
public enum TimePickerDialMode
{
    Hours,
    Minutes,
}

/// <summary>
/// Material 3 time picker clock dial (Android style). Self-drawn: a circular
/// <see cref="TemplatedControl.Background"/> dial with 12 labels, a Primary selector
/// (center dot, 2px track line and 48px selection circle) and optional 24-hour double ring.
/// Pointer press/drag selects; arrow keys nudge the value; <see cref="SelectionCommitted"/>
/// fires on pointer release.
/// </summary>
public class TimePickerDial : TemplatedControl
{
    public static readonly StyledProperty<TimePickerDialMode> ModeProperty =
        AvaloniaProperty.Register<TimePickerDial, TimePickerDialMode>(nameof(Mode));

    public static readonly StyledProperty<bool> Is24HourProperty =
        AvaloniaProperty.Register<TimePickerDial, bool>(nameof(Is24Hour));

    public static readonly StyledProperty<int> SelectedHourProperty =
        AvaloniaProperty.Register<TimePickerDial, int>(nameof(SelectedHour),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: static (_, v) => Math.Clamp(v, 0, 23));

    public static readonly StyledProperty<int> SelectedMinuteProperty =
        AvaloniaProperty.Register<TimePickerDial, int>(nameof(SelectedMinute),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: static (_, v) => Math.Clamp(v, 0, 59));

    public static readonly StyledProperty<IBrush?> SelectorBrushProperty =
        AvaloniaProperty.Register<TimePickerDial, IBrush?>(nameof(SelectorBrush));

    public static readonly StyledProperty<IBrush?> SelectedForegroundProperty =
        AvaloniaProperty.Register<TimePickerDial, IBrush?>(nameof(SelectedForeground));

    public static readonly StyledProperty<IBrush?> SecondaryForegroundProperty =
        AvaloniaProperty.Register<TimePickerDial, IBrush?>(nameof(SecondaryForeground));

    // Geometry (M3 spec-ish, visually calibrated against the Android dial at 256dp).
    private const double DefaultDiameter = 256.0;
    private const double SelectionRadius = 24.0; // 48px selection circle
    private const double CenterDotRadius = 4.0; // 8px center dot
    private const double MinuteDotRadius = 2.0; // 4px off-tick minute dot
    private const double TrackInset = 28.0; // label ring center distance from dial edge
    private const double InnerRingInset = 44.0; // inner (13-24/00) ring offset from outer ring

    // Motion: short decelerate on click/keyboard jumps; drags follow the pointer directly.
    private const double AngleAnimationSeconds = 0.15;

    private bool _attached;
    private bool _frameRequested;
    private bool _dragging;

    // Raw pointer angle/ring while dragging: the selector follows the finger
    // continuously and only snaps (with animation) on release.
    private double _dragAngle;
    private double _dragRing;

    // Selector animation state: angle in degrees clockwise from 12 o'clock,
    // ring interpolator 0 = outer ring, 1 = inner (24h) ring.
    private bool _animating;
    private long _animStart;
    private double _animFromAngle;
    private double _animToAngle;
    private double _animFromRing;
    private double _animToRing;

    // Cached pen (selector line + focus ring) and typeface.
    private IPen? _linePen;
    private IBrush? _linePenBrush;
    private Typeface _typeface = Typeface.Default;
    private FontFamily? _typefaceFamily;

    static TimePickerDial()
    {
        AffectsRender<TimePickerDial>(
            ModeProperty, Is24HourProperty, SelectedHourProperty, SelectedMinuteProperty,
            SelectorBrushProperty, SelectedForegroundProperty, SecondaryForegroundProperty,
            ForegroundProperty, BackgroundProperty, FontSizeProperty, FontFamilyProperty,
            IsFocusedProperty);
        FocusableProperty.OverrideDefaultValue<TimePickerDial>(true);
    }

    /// <summary>Whether the dial selects hours or minutes.</summary>
    public TimePickerDialMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>When true, hours mode shows a double ring: 1-12 outside, 13-24/00 inside.</summary>
    public bool Is24Hour
    {
        get => GetValue(Is24HourProperty);
        set => SetValue(Is24HourProperty, value);
    }

    /// <summary>Selected hour, 0-23. In 12-hour mode picking a number keeps the current half-day.</summary>
    public int SelectedHour
    {
        get => GetValue(SelectedHourProperty);
        set => SetValue(SelectedHourProperty, value);
    }

    /// <summary>Selected minute, 0-59.</summary>
    public int SelectedMinute
    {
        get => GetValue(SelectedMinuteProperty);
        set => SetValue(SelectedMinuteProperty, value);
    }

    /// <summary>Brush for the selector (center dot, track line, selection circle). Defaults to Primary.</summary>
    public IBrush? SelectorBrush
    {
        get => GetValue(SelectorBrushProperty);
        set => SetValue(SelectorBrushProperty, value);
    }

    /// <summary>Brush for the label under the selection circle. Defaults to OnPrimary.</summary>
    public IBrush? SelectedForeground
    {
        get => GetValue(SelectedForegroundProperty);
        set => SetValue(SelectedForegroundProperty, value);
    }

    /// <summary>Brush for inner-ring (24h) labels. Defaults to OnSurfaceVariant.</summary>
    public IBrush? SecondaryForeground
    {
        get => GetValue(SecondaryForegroundProperty);
        set => SetValue(SecondaryForegroundProperty, value);
    }

    /// <summary>Raised whenever <see cref="SelectedHour"/> or <see cref="SelectedMinute"/> changes.</summary>
    public event EventHandler? SelectedTimeChanged;

    /// <summary>Raised when the user releases the pointer after making a selection.</summary>
    public event EventHandler? SelectionCommitted;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        // Snap to the current value on attach; no animation from stale state.
        _animating = false;
        (_animToAngle, _animToRing) = GetTargetAngleRing();
        InvalidateVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _attached = false;
        _animating = false;
        _dragging = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedHourProperty || change.Property == SelectedMinuteProperty)
        {
            OnTargetChanged(animate: !_dragging);
            SelectedTimeChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (change.Property == ModeProperty || change.Property == Is24HourProperty)
        {
            OnTargetChanged(animate: !_dragging);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
        => new(DefaultDiameter, DefaultDiameter);

    protected override AutomationPeer OnCreateAutomationPeer()
        => new ControlAutomationPeer(this);

    // ---- Interaction ----

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        Focus();
        // Press starts a drag: the selector tracks the finger continuously and
        // only snaps to the nearest tick (with a short animation) on release.
        _dragging = true;
        ApplyPoint(e.GetPosition(this));
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging)
        {
            // Dragging follows the pointer directly (no animation, no tick snapping).
            ApplyPoint(e.GetPosition(this));
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_dragging)
        {
            EndDrag();
            e.Handled = true;
            SelectionCommitted?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_dragging)
            EndDrag();
    }

    /// <summary>Ends a drag: animates the selector from the raw pointer angle to the snapped value.</summary>
    private void EndDrag()
    {
        _dragging = false;
        // Seed the animation state with the raw drag position so the settle
        // animation starts from under the finger instead of jumping.
        _animating = false;
        _animToAngle = _dragAngle;
        _animToRing = _dragRing;
        OnTargetChanged(animate: true);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var delta = e.Key switch
        {
            Key.Up or Key.Right => 1,
            Key.Down or Key.Left => -1,
            _ => 0,
        };
        if (delta == 0)
            return;

        if (Mode == TimePickerDialMode.Hours)
            SetCurrentValue(SelectedHourProperty, ((SelectedHour + delta) % 24 + 24) % 24);
        else
            SetCurrentValue(SelectedMinuteProperty, ((SelectedMinute + delta) % 60 + 60) % 60);
        e.Handled = true;
    }

    /// <summary>
    /// Converts a pointer position to a dial value and applies it. While dragging the raw
    /// (unsnapped) angle is also recorded so the selector can follow the finger continuously.
    /// </summary>
    private void ApplyPoint(Point p)
    {
        var bounds = Bounds;
        var dx = p.X - bounds.Width / 2;
        var dy = p.Y - bounds.Height / 2;
        // Degrees clockwise from 12 o'clock.
        var angle = (Math.Atan2(dy, dx) * 180.0 / Math.PI + 90.0 + 360.0) % 360.0;
        GetRadii(bounds, out var outerRadius, out var innerRadius);

        _dragAngle = angle;
        _dragRing = 0.0;

        if (Mode == TimePickerDialMode.Hours)
        {
            var index = (int)Math.Round(angle / 30.0) % 12; // 0 = top
            int hour;
            if (Is24Hour)
            {
                var distance = Math.Sqrt(dx * dx + dy * dy);
                var inner = distance < (outerRadius + innerRadius) / 2;
                _dragRing = inner ? 1.0 : 0.0;
                hour = inner
                    ? index == 0 ? 0 : index + 12 // inner ring: 00 at top, then 13-23
                    : index == 0 ? 12 : index; // outer ring: 12 at top, then 1-11
            }
            else
            {
                var number = index == 0 ? 12 : index; // 1-12
                hour = number % 12 + (SelectedHour >= 12 ? 12 : 0); // keep current half-day
            }

            SetCurrentValue(SelectedHourProperty, hour);
        }
        else
        {
            SetCurrentValue(SelectedMinuteProperty, (int)Math.Round(angle / 6.0) % 60);
        }

        if (_dragging)
            InvalidateVisual();
    }

    // ---- Selector animation ----

    private void OnTargetChanged(bool animate)
    {
        var (angle, ring) = GetTargetAngleRing();
        if (_attached && animate)
        {
            var now = Stopwatch.GetTimestamp();
            var (currentAngle, currentRing) = GetAnimated(now);
            // Shortest arc path.
            var delta = ((angle - currentAngle) % 360.0 + 540.0) % 360.0 - 180.0;
            if (Math.Abs(delta) < 0.001 && Math.Abs(ring - currentRing) < 0.001)
            {
                _animating = false;
                _animToAngle = angle;
                _animToRing = ring;
                return;
            }

            _animFromAngle = currentAngle;
            _animToAngle = currentAngle + delta;
            _animFromRing = currentRing;
            _animToRing = ring;
            _animStart = now;
            _animating = true;
            RequestFrame();
        }
        else
        {
            _animating = false;
            _animToAngle = angle;
            _animToRing = ring;
        }
    }

    private (double Angle, double Ring) GetTargetAngleRing()
    {
        if (Mode == TimePickerDialMode.Hours)
        {
            var hour = SelectedHour;
            var ring = Is24Hour && (hour == 0 || hour > 12) ? 1.0 : 0.0;
            return (hour % 12 * 30.0, ring);
        }

        return (SelectedMinute * 6.0, 0.0);
    }

    private (double Angle, double Ring) GetAnimated(long now)
    {
        if (!_animating)
            return (_animToAngle, _animToRing);

        var t = (now - _animStart) / (double)Stopwatch.Frequency / AngleAnimationSeconds;
        if (t >= 1)
        {
            _animating = false;
            return (_animToAngle, _animToRing);
        }

        var eased = 1 - Math.Pow(1 - t, 3); // decelerate (cubic ease-out)
        return (_animFromAngle + (_animToAngle - _animFromAngle) * eased,
            _animFromRing + (_animToRing - _animFromRing) * eased);
    }

    private void RequestFrame()
    {
        if (_frameRequested || !_attached)
            return;
        _frameRequested = true;
        Dispatcher.UIThread.Post(() =>
        {
            _frameRequested = false;
            if (_attached && VisualRoot is not null)
                InvalidateVisual();
        }, DispatcherPriority.Render);
    }

    // ---- Rendering ----

    private static void GetRadii(Rect bounds, out double outerRadius, out double innerRadius)
    {
        var radius = Math.Min(bounds.Width, bounds.Height) / 2;
        outerRadius = radius - TrackInset;
        innerRadius = Math.Max(outerRadius - InnerRingInset, outerRadius * 0.5);
    }

    private IPen? GetLinePen()
    {
        var brush = SelectorBrush;
        if (brush is null)
            return null;
        if (_linePen is null || !ReferenceEquals(_linePenBrush, brush))
        {
            _linePen = new Pen(brush, 2.0);
            _linePenBrush = brush;
        }

        return _linePen;
    }

    private Typeface GetTypeface()
    {
        var family = FontFamily;
        if (_typefaceFamily is null || !ReferenceEquals(_typefaceFamily, family))
        {
            _typeface = new Typeface(family);
            _typefaceFamily = family;
        }

        return _typeface;
    }

    private static Point PointOnDial(Point center, double radius, double angleDeg)
    {
        // angleDeg is clockwise from 12 o'clock.
        var rad = (angleDeg - 90.0) * Math.PI / 180.0;
        return new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
    }

    public override void Render(DrawingContext context)
    {
        // Always draw when asked: skipping while transiently hidden would cache an empty
        // frame in the compositor and the control would appear blank when shown again.
        var bounds = Bounds;
        var radius = Math.Min(bounds.Width, bounds.Height) / 2;
        if (radius <= TrackInset)
            return;

        var center = new Point(bounds.Width / 2, bounds.Height / 2);
        GetRadii(bounds, out var outerRadius, out var innerRadius);

        // Dial background.
        if (Background is { } background)
            context.DrawEllipse(background, null, center, radius, radius);

        var now = Stopwatch.GetTimestamp();
        double angle, ring;
        bool animating;
        if (_dragging)
        {
            // Follow the finger directly while dragging.
            (angle, ring) = (_dragAngle, _dragRing);
            animating = false;
        }
        else
        {
            (angle, ring) = GetAnimated(now);
            animating = _animating;
        }

        var selectionRadius = outerRadius + (innerRadius - outerRadius) * ring;
        var selectionCenter = PointOnDial(center, selectionRadius, angle);

        // Selector: track line, selection circle, center dot, focus ring.
        if (SelectorBrush is { } selector)
        {
            if (GetLinePen() is { } linePen)
            {
                context.DrawLine(linePen, center, selectionCenter);
                if (IsFocused)
                    context.DrawEllipse(null, linePen, center, CenterDotRadius + 5, CenterDotRadius + 5);
            }

            context.DrawEllipse(selector, null, selectionCenter, SelectionRadius, SelectionRadius);
            context.DrawEllipse(selector, null, center, CenterDotRadius, CenterDotRadius);
        }

        DrawLabels(context, center, outerRadius, innerRadius);

        // Off-tick minute: small dot marks the exact position inside the selection circle.
        if (Mode == TimePickerDialMode.Minutes && SelectedMinute % 5 != 0 && SelectedForeground is { } dotBrush)
            context.DrawEllipse(dotBrush, null, selectionCenter, MinuteDotRadius, MinuteDotRadius);

        if (animating)
            RequestFrame();
    }

    private void DrawLabels(DrawingContext context, Point center, double outerRadius, double innerRadius)
    {
        var typeface = GetTypeface();
        var fontSize = FontSize;
        var culture = CultureInfo.CurrentCulture;
        var normal = Foreground;
        var secondary = SecondaryForeground ?? normal;
        var selected = SelectedForeground ?? normal;
        var hours = Mode == TimePickerDialMode.Hours;
        var is24 = Is24Hour;

        for (var i = 0; i < 12; i++)
        {
            var angle = i * 30.0;

            if (hours)
            {
                var outerHour = i == 0 ? 12 : i;
                bool isSelected;
                if (is24)
                {
                    isSelected = SelectedHour == outerHour;
                }
                else
                {
                    isSelected = SelectedHour % 12 == outerHour % 12;
                }

                DrawLabel(context, outerHour.ToString(culture), typeface, fontSize, culture,
                    isSelected ? selected : normal, PointOnDial(center, outerRadius, angle));

                if (is24)
                {
                    var innerHour = i == 0 ? 0 : i + 12;
                    var text = innerHour == 0 ? "00" : innerHour.ToString(culture);
                    var innerSelected = SelectedHour == innerHour;
                    DrawLabel(context, text, typeface, Math.Max(fontSize - 2, 8), culture,
                        innerSelected ? selected : secondary, PointOnDial(center, innerRadius, angle));
                }
            }
            else
            {
                var minute = i * 5;
                var isSelected = SelectedMinute == minute;
                DrawLabel(context, minute.ToString("00", culture), typeface, fontSize, culture,
                    isSelected ? selected : normal, PointOnDial(center, outerRadius, angle));
            }
        }
    }

    private static void DrawLabel(DrawingContext context, string text, Typeface typeface,
        double fontSize, CultureInfo culture, IBrush? brush, Point position)
    {
        if (brush is null)
            return;

        var formatted = new FormattedText(text, culture, FlowDirection.LeftToRight, typeface, fontSize, brush);
        context.DrawText(formatted,
            new Point(position.X - formatted.Width / 2, position.Y - formatted.Height / 2));
    }
}
