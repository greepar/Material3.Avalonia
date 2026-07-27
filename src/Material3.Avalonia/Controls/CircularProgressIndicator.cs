using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 circular progress indicator (2024 style). Self-drawn: the active arc uses
/// <see cref="TemplatedControl.Foreground"/>, the track uses <see cref="TemplatedControl.Background"/>,
/// with a gap between active arc and track. Supports determinate (with smooth value animation)
/// and indeterminate modes.
/// </summary>
public class CircularProgressIndicator : RangeBase
{
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<CircularProgressIndicator, bool>(nameof(IsIndeterminate));

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<CircularProgressIndicator, double>(nameof(StrokeWidth), 4.0);

    public static readonly StyledProperty<double> TrackGapProperty =
        AvaloniaProperty.Register<CircularProgressIndicator, double>(nameof(TrackGap), 4.0);

    public static readonly StyledProperty<bool> IsWavyProperty =
        AvaloniaProperty.Register<CircularProgressIndicator, bool>(nameof(IsWavy));

    public static readonly StyledProperty<double> AmplitudeProperty =
        AvaloniaProperty.Register<CircularProgressIndicator, double>(nameof(Amplitude), 2.0);

    public static readonly StyledProperty<double> WaveCountProperty =
        AvaloniaProperty.Register<CircularProgressIndicator, double>(nameof(WaveCount), 12.0);

    // Motion: M3 progress value animation, ~0.5s decelerate (implementation choice, visually calibrated).
    private const double ValueAnimationSeconds = 0.5;

    // Wavy motion (implementation choices, visually calibrated against M3 Expressive):
    // phase scrolls at ~1.5 rad/s while progress is in flight; amplitude fades linearly
    // to 0 above 90% so the ring smooths out at completion (same as WavyProgressBar).
    private const double WavePhaseRadPerSecond = 1.5;
    private const double FlattenStartFraction = 0.9;
    private const double WavySampleStepDeg = 2.0;

    // Indeterminate motion constants (implementation choice, visually calibrated against Android M3):
    // linear whole rotation ~1330ms; arc grows/shrinks between ~10 and ~270 degrees per ~1333ms cycle,
    // advancing its start angle each half-cycle so the head keeps moving forward.
    private const double RotationSeconds = 1.33;
    private const double CycleSeconds = 1.333;
    private const double MinSweepDeg = 10.0;
    private const double MaxSweepDeg = 270.0;

    private bool _attached;
    private bool _frameRequested;
    private long _clockStart;

    // Smooth value interpolation state.
    private bool _valueAnimating;
    private double _animFrom;
    private double _animTo;
    private long _animStart;

    // Cached pens (rebuilt only when brush/width change).
    private IPen? _activePen;
    private IBrush? _activePenBrush;
    private double _activePenWidth;
    private IPen? _trackPen;
    private IBrush? _trackPenBrush;
    private double _trackPenWidth;

    static CircularProgressIndicator()
    {
        AffectsRender<CircularProgressIndicator>(
            IsIndeterminateProperty, StrokeWidthProperty, TrackGapProperty,
            IsWavyProperty, AmplitudeProperty, WaveCountProperty,
            ForegroundProperty, BackgroundProperty,
            MinimumProperty, MaximumProperty, ValueProperty);
    }

    /// <summary>When true, shows the looping indeterminate spinner instead of the value arc.</summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Stroke thickness of the active and track arcs.</summary>
    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    /// <summary>Gap (in pixels along the arc) between the active arc and the track.</summary>
    public double TrackGap
    {
        get => GetValue(TrackGapProperty);
        set => SetValue(TrackGapProperty, value);
    }

    /// <summary>When true, the active arc is drawn as a rolling sine wave (M3 Expressive wavy style).</summary>
    public bool IsWavy
    {
        get => GetValue(IsWavyProperty);
        set => SetValue(IsWavyProperty, value);
    }

    /// <summary>Peak radial displacement of the wave from the ring radius, in pixels.</summary>
    public double Amplitude
    {
        get => GetValue(AmplitudeProperty);
        set => SetValue(AmplitudeProperty, value);
    }

    /// <summary>Number of full wave cycles around the whole circle.</summary>
    public double WaveCount
    {
        get => GetValue(WaveCountProperty);
        set => SetValue(WaveCountProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        _clockStart = Stopwatch.GetTimestamp();
        // Snap to current value on attach; no animation from stale state.
        _valueAnimating = false;
        _animTo = Value;
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
                 change.Property == IsWavyProperty ||
                 (change.Property == IsVisibleProperty && change.GetNewValue<bool>()))
        {
            // Restart the frame loop; it self-terminates while the control is not rendered.
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

    public override void Render(DrawingContext context)
    {
        // Always draw when asked: skipping while transiently hidden would cache an empty
        // frame in the compositor and the control would appear blank when shown again.
        var bounds = Bounds;
        var stroke = StrokeWidth;
        var size = Math.Min(bounds.Width, bounds.Height);
        var radius = (size - stroke) / 2;
        // Inset the ring so wave peaks (R + Amplitude) stay inside the bounds.
        if (IsWavy)
            radius -= Math.Max(0, Amplitude);
        if (radius <= 0)
            return;

        var center = new Point(bounds.Width / 2, bounds.Height / 2);
        var now = Stopwatch.GetTimestamp();
        var animating = false;
        var wavy = IsWavy;
        var elapsed = (now - _clockStart) / (double)Stopwatch.Frequency;
        var phase = elapsed * WavePhaseRadPerSecond;

        if (IsIndeterminate)
        {
            var t = elapsed;

            // Linear whole rotation.
            var rotation = t / RotationSeconds * 360.0;

            // Arc expand/contract cycle; the start angle advances during contraction so the
            // head keeps travelling forward (visual approximation of the Android behavior).
            var cycle = t / CycleSeconds;
            var frac = cycle - Math.Floor(cycle);
            var grow = MaxSweepDeg - MinSweepDeg;
            double sweep, startOffset;
            if (frac < 0.5)
            {
                sweep = MinSweepDeg + grow * EaseInOut(frac * 2);
                startOffset = 0;
            }
            else
            {
                var p = EaseInOut((frac - 0.5) * 2);
                sweep = MaxSweepDeg - grow * p;
                startOffset = grow * p;
            }

            var start = -90.0 + rotation + Math.Floor(cycle) * grow + startOffset;
            if (GetActivePen() is { } pen)
            {
                if (wavy && Amplitude > 0.01)
                    DrawWavyArc(context, pen, center, radius, start, sweep, Amplitude, WaveCount, phase);
                else
                    DrawArc(context, pen, center, radius, start, sweep);
            }

            animating = true;
        }
        else
        {
            var value = GetAnimatedValue(now);
            animating = _valueAnimating;

            var range = Maximum - Minimum;
            var fraction = range > 0 ? (value - Minimum) / range : 0.0;
            fraction = Math.Clamp(fraction, 0.0, 1.0);

            var sweep = fraction * 360.0;
            var circumference = 2 * Math.PI * radius;
            // Convert pixel gap to degrees; round caps extend by stroke/2 on each side.
            var gapDeg = circumference > 0 ? (TrackGap + stroke) / circumference * 360.0 : 0;

            // Flatten near completion: amplitude fades linearly to 0 above the threshold.
            var amplitude = wavy ? Amplitude : 0.0;
            if (fraction > FlattenStartFraction)
                amplitude *= (1.0 - fraction) / (1.0 - FlattenStartFraction);

            // Wave keeps flowing while progress is in flight (M3 wavy characteristic).
            if (wavy && fraction > 0 && fraction < 1 && amplitude > 0.01)
                animating = true;

            var activePen = GetActivePen();
            var trackPen = GetTrackPen();

            if (fraction >= 1.0)
            {
                // Full active ring, no track.
                if (activePen is not null)
                    context.DrawEllipse(null, activePen, center, radius, radius);
            }
            else if (fraction <= 0.0)
            {
                // Track only, full ring.
                if (trackPen is not null)
                    context.DrawEllipse(null, trackPen, center, radius, radius);
            }
            else
            {
                if (activePen is not null)
                {
                    if (amplitude > 0.01)
                        DrawWavyArc(context, activePen, center, radius, -90.0, sweep, amplitude, WaveCount, phase);
                    else
                        DrawArc(context, activePen, center, radius, -90.0, sweep);
                }

                var trackSweep = 360.0 - sweep - 2 * gapDeg;
                if (trackPen is not null && trackSweep > 0)
                    DrawArc(context, trackPen, center, radius, -90.0 + sweep + gapDeg, trackSweep);
            }
        }

        if (animating)
            RequestFrame();
    }

    private static double EaseInOut(double t)
    {
        // Cubic ease-in-out.
        return t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    }

    private static void DrawArc(DrawingContext context, IPen pen, Point center, double radius,
        double startDeg, double sweepDeg)
    {
        if (sweepDeg <= 0)
            return;
        if (sweepDeg >= 360)
        {
            context.DrawEllipse(null, pen, center, radius, radius);
            return;
        }

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(PointOnCircle(center, radius, startDeg), false);
            ctx.ArcTo(
                PointOnCircle(center, radius, startDeg + sweepDeg),
                new Size(radius, radius),
                0,
                sweepDeg > 180,
                SweepDirection.Clockwise);
            ctx.EndFigure(false);
        }

        context.DrawGeometry(null, pen, geometry);
    }

    private static void DrawWavyArc(DrawingContext context, IPen pen, Point center, double radius,
        double startDeg, double sweepDeg, double amplitude, double waveCount, double phase)
    {
        if (sweepDeg <= 0)
            return;

        // Polyline sampled every WavySampleStepDeg: r(θ) = R + A·sin(waveCount·θ + phase).
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(WavyPoint(center, radius, startDeg, amplitude, waveCount, phase), false);
            for (var deg = startDeg + WavySampleStepDeg; deg < startDeg + sweepDeg; deg += WavySampleStepDeg)
                ctx.LineTo(WavyPoint(center, radius, deg, amplitude, waveCount, phase));
            ctx.LineTo(WavyPoint(center, radius, startDeg + sweepDeg, amplitude, waveCount, phase));
            ctx.EndFigure(false);
        }

        context.DrawGeometry(null, pen, geometry);
    }

    private static Point WavyPoint(Point center, double radius, double angleDeg,
        double amplitude, double waveCount, double phase)
    {
        var rad = angleDeg * Math.PI / 180.0;
        var r = radius + amplitude * Math.Sin(waveCount * rad + phase);
        return new Point(center.X + r * Math.Cos(rad), center.Y + r * Math.Sin(rad));
    }

    private static Point PointOnCircle(Point center, double radius, double angleDeg)
    {
        var rad = angleDeg * Math.PI / 180.0;
        return new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
    }
}
