using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 Expressive linear wavy progress indicator. Self-drawn: the active portion is a
/// scrolling sine wave (<see cref="TemplatedControl.Foreground"/>), the remaining track is a
/// straight line (<see cref="TemplatedControl.Background"/>) with a stop dot at the end.
/// Supports determinate (smooth value animation, wave flattens near completion) and
/// indeterminate modes.
/// </summary>
public class WavyProgressBar : RangeBase
{
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<WavyProgressBar, bool>(nameof(IsIndeterminate));

    public static readonly StyledProperty<double> AmplitudeProperty =
        AvaloniaProperty.Register<WavyProgressBar, double>(nameof(Amplitude), 3.0,
            validate: static value => double.IsFinite(value) && value >= 0);

    public static readonly StyledProperty<double> WavelengthProperty =
        AvaloniaProperty.Register<WavyProgressBar, double>(nameof(Wavelength), 40.0,
            validate: static value => double.IsFinite(value) && value > 0);

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<WavyProgressBar, double>(nameof(StrokeWidth), 4.0,
            validate: static value => double.IsFinite(value) && value > 0);

    public static readonly StyledProperty<double> TrackGapProperty =
        AvaloniaProperty.Register<WavyProgressBar, double>(nameof(TrackGap), 4.0,
            validate: static value => double.IsFinite(value) && value >= 0);

    // Motion (implementation choices, visually calibrated):
    // - value changes animate over ~0.5s with decelerate easing;
    // - wave phase scrolls at ~40 px/s while progress is in flight;
    // - indeterminate: a wave segment (~40% of the width) sweeps left-to-right on a 2s cycle;
    // - amplitude fades linearly to 0 above 90% so the bar flattens at completion.
    private const double ValueAnimationSeconds = 0.5;
    private const double WaveSpeedPxPerSecond = 40.0;
    private const double IndeterminateCycleSeconds = 2.0;
    private const double IndeterminateSegmentFraction = 0.4;
    private const double FlattenStartFraction = 0.9;
    private const double StopDotRadius = 2.0;
    private const double SampleStepPx = 2.5;

    private bool _attached;
    private bool _frameRequested;
    private long _clockStart;

    private bool _valueAnimating;
    private double _animFrom;
    private double _animTo;
    private long _animStart;

    private IPen? _activePen;
    private IBrush? _activePenBrush;
    private double _activePenWidth;
    private IPen? _trackPen;
    private IBrush? _trackPenBrush;
    private double _trackPenWidth;

    static WavyProgressBar()
    {
        AffectsRender<WavyProgressBar>(
            IsIndeterminateProperty, AmplitudeProperty, WavelengthProperty,
            StrokeWidthProperty, TrackGapProperty,
            ForegroundProperty, BackgroundProperty,
            MinimumProperty, MaximumProperty, ValueProperty);
    }

    /// <summary>When true, shows the looping sweep animation instead of the value wave.</summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Peak vertical displacement of the wave from the centerline.</summary>
    public double Amplitude
    {
        get => GetValue(AmplitudeProperty);
        set => SetValue(AmplitudeProperty, value);
    }

    /// <summary>Horizontal length of one full wave cycle.</summary>
    public double Wavelength
    {
        get => GetValue(WavelengthProperty);
        set => SetValue(WavelengthProperty, value);
    }

    /// <summary>Stroke thickness of the wave and track.</summary>
    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    /// <summary>Horizontal gap between the active wave end and the track.</summary>
    public double TrackGap
    {
        get => GetValue(TrackGapProperty);
        set => SetValue(TrackGapProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        _clockStart = Stopwatch.GetTimestamp();
        _valueAnimating = false;
        _animTo = Value;
        if (IsIndeterminate)
            RequestFrame();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _attached = false;
        _valueAnimating = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            var newValue = change.GetNewValue<double>();
            if (_attached)
            {
                var now = Stopwatch.GetTimestamp();
                _animFrom = GetAnimatedValue(now);
                _animTo = newValue;
                _animStart = now;
                _valueAnimating = true;
                RequestFrame();
            }
            else
            {
                _animTo = newValue;
            }
        }
        else if (change.Property == IsIndeterminateProperty ||
                 (change.Property == IsVisibleProperty && change.GetNewValue<bool>()))
        {
            RequestFrame();
        }
    }

    private double GetAnimatedValue(long now)
    {
        if (!_valueAnimating)
            return _animTo;

        var t = (now - _animStart) / (double)Stopwatch.Frequency / ValueAnimationSeconds;
        if (t >= 1)
        {
            _valueAnimating = false;
            return _animTo;
        }

        var eased = 1 - Math.Pow(1 - t, 3); // decelerate (cubic ease-out)
        return _animFrom + (_animTo - _animFrom) * eased;
    }

    private void RequestFrame()
    {
        if (_frameRequested || !_attached)
            return;
        _frameRequested = true;
        Dispatcher.UIThread.Post(OnFrame, DispatcherPriority.Render);
    }

    private void OnFrame()
    {
        _frameRequested = false;
        if (!_attached || VisualRoot is null)
            return;

        if (!IsEffectivelyVisible)
        {
            if (!IsIndeterminate)
                return;

            // Hidden (e.g. a collapsed ancestor): Render is not invoked for hidden visuals,
            // which would kill the frame loop. Poll cheaply until shown again, then resume.
            _frameRequested = true;
            DispatcherTimer.RunOnce(() =>
            {
                _frameRequested = false;
                RequestFrame();
            }, TimeSpan.FromMilliseconds(100));
            return;
        }

        InvalidateVisual();
    }

    private IPen? GetActivePen()
    {
        var brush = Foreground;
        if (brush is null)
            return null;
        var width = StrokeWidth;
        if (_activePen is null || !ReferenceEquals(_activePenBrush, brush) || _activePenWidth != width)
        {
            _activePen = new Pen(brush, width, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
            _activePenBrush = brush;
            _activePenWidth = width;
        }

        return _activePen;
    }

    private IPen? GetTrackPen()
    {
        var brush = Background;
        if (brush is null)
            return null;
        var width = StrokeWidth;
        if (_trackPen is null || !ReferenceEquals(_trackPenBrush, brush) || _trackPenWidth != width)
        {
            _trackPen = new Pen(brush, width, lineCap: PenLineCap.Round);
            _trackPenBrush = brush;
            _trackPenWidth = width;
        }

        return _trackPen;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Height fits the wave (2*amplitude) plus the stroke; width stretches.
        var height = 2 * Amplitude + StrokeWidth;
        var width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
        return new Size(width, height);
    }

    public override void Render(DrawingContext context)
    {
        // Always draw when asked: skipping while transiently hidden would cache an empty
        // frame in the compositor and the control would appear blank when shown again.
        var bounds = Bounds;
        var stroke = StrokeWidth;
        var inset = stroke / 2;
        var left = inset;
        var right = bounds.Width - inset;
        var width = right - left;
        if (width <= 0 || bounds.Height <= 0)
            return;

        var centerY = bounds.Height / 2;
        var now = Stopwatch.GetTimestamp();
        var t = (now - _clockStart) / (double)Stopwatch.Frequency;
        var phase = t * WaveSpeedPxPerSecond * 2 * Math.PI / Math.Max(1.0, Wavelength);

        var activePen = GetActivePen();
        var trackPen = GetTrackPen();
        var dotBrush = Foreground;

        // Reserve space at the right edge for the stop dot.
        var dotX = right - StopDotRadius;
        var trackEnd = dotX - StopDotRadius - TrackGap;

        bool animating;

        if (IsIndeterminate)
        {
            animating = true;

            // A wave segment sweeping left-to-right, wrapping around (implementation choice).
            var segLen = width * IndeterminateSegmentFraction;
            var cycle = t / IndeterminateCycleSeconds;
            var travel = (cycle - Math.Floor(cycle)) * (width + segLen);
            var segStart = left + travel - segLen;
            var segEnd = left + travel;

            var clampedStart = Math.Max(left, segStart);
            var clampedEnd = Math.Min(right, segEnd);
            var hasSegment = clampedEnd - clampedStart > 1;

            // Track on both sides of the wave segment, separated by TrackGap
            // (never drawn behind the wave itself).
            if (trackPen is not null)
            {
                if (hasSegment)
                {
                    var leftTrackEnd = Math.Min(clampedStart - inset - TrackGap, trackEnd);
                    if (leftTrackEnd - left > 0)
                        context.DrawLine(trackPen, new Point(left, centerY), new Point(leftTrackEnd, centerY));

                    var rightTrackStart = clampedEnd + inset + TrackGap;
                    if (trackEnd - rightTrackStart > 0)
                        context.DrawLine(trackPen, new Point(rightTrackStart, centerY), new Point(trackEnd, centerY));
                }
                else
                {
                    context.DrawLine(trackPen, new Point(left, centerY), new Point(trackEnd, centerY));
                }
            }

            if (activePen is not null && hasSegment)
                DrawWave(context, activePen, clampedStart, clampedEnd, centerY, Amplitude, phase);
        }
        else
        {
            var value = GetAnimatedValue(now);
            animating = _valueAnimating;

            var range = Maximum - Minimum;
            var fraction = range > 0 ? (value - Minimum) / range : 0.0;
            fraction = Math.Clamp(fraction, 0.0, 1.0);

            // Flatten near completion: amplitude fades linearly to 0 above the threshold.
            var amplitude = Amplitude;
            if (fraction > FlattenStartFraction)
                amplitude *= (1.0 - fraction) / (1.0 - FlattenStartFraction);

            // Wave keeps flowing while progress is in flight (M3 wavy characteristic).
            if (fraction > 0 && fraction < 1 && amplitude > 0.01)
                animating = true;

            var activeEnd = left + width * fraction;

            if (activePen is not null && fraction > 0)
            {
                if (amplitude > 0.01 && activeEnd - left > 1)
                    DrawWave(context, activePen, left, activeEnd, centerY, amplitude, phase);
                else
                    context.DrawLine(activePen, new Point(left, centerY), new Point(activeEnd, centerY));
            }

            // Track from active end + gap to the stop dot.
            var trackStart = fraction > 0 ? activeEnd + inset + TrackGap : left;
            if (trackPen is not null && fraction < 1 && trackEnd - trackStart > 0)
                context.DrawLine(trackPen, new Point(trackStart, centerY), new Point(trackEnd, centerY));

            // Stop dot at the right edge (M3 2024 style).
            if (dotBrush is not null && fraction < 1)
                context.DrawEllipse(dotBrush, null, new Point(dotX, centerY), StopDotRadius, StopDotRadius);
        }

        if (animating)
            RequestFrame();
    }

    private void DrawWave(DrawingContext context, IPen pen, double x0, double x1,
        double centerY, double amplitude, double phase)
    {
        var wavelength = Math.Max(1.0, Wavelength);
        var k = 2 * Math.PI / wavelength;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(x0, centerY + amplitude * Math.Sin(k * x0 - phase)), false);
            for (var x = x0 + SampleStepPx; x < x1; x += SampleStepPx)
                ctx.LineTo(new Point(x, centerY + amplitude * Math.Sin(k * x - phase)));
            ctx.LineTo(new Point(x1, centerY + amplitude * Math.Sin(k * x1 - phase)));
            ctx.EndFigure(false);
        }

        context.DrawGeometry(null, pen, geometry);
    }
}
