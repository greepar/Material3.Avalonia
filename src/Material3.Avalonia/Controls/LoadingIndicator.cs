// Geometry and motion spec ported from m3fx (https://github.com/Glavo/m3fx), Apache-2.0.
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace Material3.Avalonia.Controls;

/// <summary>Visual variants of <see cref="LoadingIndicator"/>.</summary>
public enum LoadingIndicatorVariant
{
    /// <summary>Shape only, no container.</summary>
    Default,
    /// <summary>Shape drawn on a filled circular container.</summary>
    Contained,
}

/// <summary>
/// Material 3 Expressive loading indicator: a self-drawn polygonal "petal" shape that
/// continuously morphs through a sequence of seven rounded polygons while rotating.
///
/// Implementation note: instead of porting the full AndroidX <c>Morph</c> cubic-matching
/// algorithm, each shape is pre-sampled as a polar radius function r(θ) on a fixed grid of
/// <see cref="SampleCount"/> angles. Corner rounding is approximated by circular moving-average
/// smoothing of the sharp polygon's r(θ) (window proportional to the rounding value); the pill
/// and oval have closed-form r(θ). Morphing is then a per-sample lerp between two radius arrays
/// (the grids are naturally aligned), which reproduces the petal-morph look at a fraction of
/// the complexity.
/// </summary>
public class LoadingIndicator : Control
{
    public static readonly StyledProperty<LoadingIndicatorVariant> VariantProperty =
        AvaloniaProperty.Register<LoadingIndicator, LoadingIndicatorVariant>(nameof(Variant));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TemplatedControl.ForegroundProperty.AddOwner<LoadingIndicator>();

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        TemplatedControl.BackgroundProperty.AddOwner<LoadingIndicator>();

    /// <summary>
    /// Test hook: when non-null, freezes the animation. The integer part selects the morph
    /// segment (0-based, wraps modulo 7) and the fractional part the progress within the
    /// segment's active phase (e.g. 2.5 = segment 2 at 50% morph).
    /// </summary>
    public static readonly StyledProperty<double?> DebugSegmentOverrideProperty =
        AvaloniaProperty.Register<LoadingIndicator, double?>(nameof(DebugSegmentOverride));

    // Motion constants from m3fx M3LoadingIndicatorSkin.
    private const double SegmentMs = 650.0;            // one morph segment
    private const double ActiveMs = SegmentMs * 0.72;  // active (lerping) part of a segment
    private const double GlobalRotationMs = 4666.0;    // one full linear rotation
    private const double SegmentRotationDeg = 90.0;    // extra rotation added per segment
    private const double ScaleAmplitude = 0.12;        // scale pulse amplitude

    private const double ContainerSize = 48.0;
    private const double ShapeSize = 38.0;

    private const int SampleCount = 360; // one radius sample per degree
    private const int ShapeCount = 7;

    private static readonly Lazy<double[][]> Shapes = new(BuildShapes);

    private bool _attached;
    private bool _frameRequested;
    private long _clockStart;

    static LoadingIndicator()
    {
        AffectsRender<LoadingIndicator>(
            VariantProperty, ForegroundProperty, BackgroundProperty, DebugSegmentOverrideProperty);
    }

    /// <summary>Selects between the bare shape and the contained (circle background) style.</summary>
    public LoadingIndicatorVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Fill brush of the morphing active shape.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>Fill brush of the circular container (used by <see cref="LoadingIndicatorVariant.Contained"/>).</summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <inheritdoc cref="DebugSegmentOverrideProperty"/>
    public double? DebugSegmentOverride
    {
        get => GetValue(DebugSegmentOverrideProperty);
        set => SetValue(DebugSegmentOverrideProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        _clockStart = Stopwatch.GetTimestamp();
        RequestFrame();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _attached = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DebugSegmentOverrideProperty ||
            (change.Property == IsVisibleProperty && change.GetNewValue<bool>()))
        {
            // Restart the frame loop; it self-terminates while the control is not rendered.
            RequestFrame();
        }
    }

    protected override Size MeasureOverride(Size availableSize) => new(ContainerSize, ContainerSize);

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

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        var size = Math.Min(bounds.Width, bounds.Height);
        if (size <= 0)
            return;

        var center = new Point(bounds.Width / 2, bounds.Height / 2);
        var pixelsPerUnit = size / ContainerSize;

        double elapsedMs;
        bool animating;
        if (DebugSegmentOverride is { } dbg)
        {
            var seg = Math.Floor(dbg);
            elapsedMs = seg * SegmentMs + (dbg - seg) * ActiveMs;
            animating = false;
        }
        else
        {
            elapsedMs = (Stopwatch.GetTimestamp() - _clockStart) / (double)Stopwatch.Frequency * 1000.0;
            animating = true;
        }

        var segIndex = (int)(elapsedMs / SegmentMs);
        var segT = elapsedMs - segIndex * SegmentMs;
        var progress = Math.Min(segT / ActiveMs, 1.0);
        var eased = MorphEase(progress);

        if (Variant == LoadingIndicatorVariant.Contained && Background is { } containerBrush)
            context.DrawEllipse(containerBrush, null, center, size / 2, size / 2);

        if (Foreground is not { } shapeBrush)
        {
            if (animating)
                RequestFrame();
            return;
        }

        var shapes = Shapes.Value;
        var from = shapes[segIndex % ShapeCount];
        var to = shapes[(segIndex + 1) % ShapeCount];

        // Scale pulse over the segment plus per-segment and continuous global rotation.
        var pulse = 1.0 + ScaleAmplitude * Math.Pow(Math.Sin(Math.PI * Math.Clamp(eased, 0.0, 1.0)), 2);
        var rotationDeg = elapsedMs / GlobalRotationMs * 360.0 + (segIndex + eased) * SegmentRotationDeg;

        // Radius arrays are normalized so the shape's bounding box max dimension is 1;
        // multiplying by ShapeSize (in 48-unit space) yields the 38px drawing area.
        var radiusScale = ShapeSize * pixelsPerUnit * pulse;

        var geometry = new StreamGeometry();
        using (var g = geometry.Open())
        {
            g.BeginFigure(MorphPoint(0), true);
            for (var i = 1; i < SampleCount; i++)
                g.LineTo(MorphPoint(i));
            g.EndFigure(true);
        }

        var rotation = Matrix.CreateTranslation(-center.X, -center.Y)
                       * Matrix.CreateRotation(rotationDeg * Math.PI / 180.0)
                       * Matrix.CreateTranslation(center.X, center.Y);
        using (context.PushTransform(rotation))
        {
            context.DrawGeometry(shapeBrush, null, geometry);
        }

        if (animating)
            RequestFrame();

        Point MorphPoint(int i)
        {
            // Eased progress may overshoot 1 (bezier y1 = 1.21); lerp extrapolates, which is
            // the intended expressive overshoot.
            var r = (from[i] + (to[i] - from[i]) * eased) * radiusScale;
            var theta = 2.0 * Math.PI * i / SampleCount;
            return new Point(center.X + r * Math.Cos(theta), center.Y + r * Math.Sin(theta));
        }
    }

    /// <summary>Cubic-bezier(0.38, 1.21, 0.22, 1.0) easing, solved by bisection on x.</summary>
    private static double MorphEase(double t)
    {
        if (t <= 0)
            return 0;
        if (t >= 1)
            return 1;

        const double p1x = 0.38, p1y = 1.21, p2x = 0.22, p2y = 1.0;
        double lo = 0, hi = 1;
        for (var i = 0; i < 32; i++)
        {
            var mid = (lo + hi) / 2;
            if (Bezier(mid, p1x, p2x) < t)
                lo = mid;
            else
                hi = mid;
        }

        return Bezier((lo + hi) / 2, p1y, p2y);

        static double Bezier(double u, double c1, double c2)
        {
            var v = 1 - u;
            return 3 * v * v * u * c1 + 3 * v * u * u * c2 + u * u * u;
        }
    }

    // ---- Shape construction (runs once, lazily) ----

    private static double[][] BuildShapes()
    {
        return new[]
        {
            // softBurst: 10-point star, inner 0.65, rounding 0.1, rotated +18°.
            Normalize(Shift(Smooth(SampleSharp(Star(10, 1.0, 0.65)), Window(0.1, 20)), 18)),
            // cookie9: 9-point star, inner 0.8, rounding 0.5, rotated -90°.
            Normalize(Shift(Smooth(SampleSharp(Star(9, 1.0, 0.8)), Window(0.5, 18)), -90)),
            // pentagon: regular pentagon, rounding 0.3, rotated -18°.
            Normalize(Shift(Smooth(SampleSharp(Regular(5, 1.0)), Window(0.3, 5)), -18)),
            // pill: 1.25 x 1.0 stadium, rotated -45° (closed-form r(θ)).
            Normalize(Shift(PillRadii(), -45)),
            // sunny: 8-point star, inner 0.8, rounding 0.15, no rotation.
            Normalize(Smooth(SampleSharp(Star(8, 1.0, 0.8)), Window(0.15, 16))),
            // cookie4: 4-point star, inner 0.5, rounding 0.3, rotated -45°.
            Normalize(Shift(Smooth(SampleSharp(Star(4, 1.0, 0.5)), Window(0.3, 8)), -45)),
            // oval: fully rounded octagon squashed to 0.7 => ellipse, rotated -45° (closed-form).
            Normalize(Shift(OvalRadii(), -45)),
        };
    }

    /// <summary>Smoothing window (samples) approximating a corner rounding value: rounding × K / cornerCount.</summary>
    private static int Window(double rounding, int corners) =>
        Math.Max(2, (int)Math.Round(rounding * SampleCount / corners));

    /// <summary>Star polygon: 2n vertices alternating outer/inner radius, vertex i at angle π/n × i.</summary>
    private static Point[] Star(int n, double outer, double inner)
    {
        var points = new Point[2 * n];
        for (var i = 0; i < 2 * n; i++)
        {
            var radius = i % 2 == 0 ? outer : inner;
            var angle = Math.PI / n * i;
            points[i] = new Point(radius * Math.Cos(angle), radius * Math.Sin(angle));
        }

        return points;
    }

    /// <summary>Regular polygon: n vertices, vertex i at angle 2π/n × i.</summary>
    private static Point[] Regular(int n, double radius)
    {
        var points = new Point[n];
        for (var i = 0; i < n; i++)
        {
            var angle = 2.0 * Math.PI / n * i;
            points[i] = new Point(radius * Math.Cos(angle), radius * Math.Sin(angle));
        }

        return points;
    }

    /// <summary>
    /// Samples the sharp polygon boundary as r(θ) by casting a ray from the origin at each grid
    /// angle. All shapes used here are star-shaped with respect to the origin, so each ray hits
    /// the boundary exactly once.
    /// </summary>
    private static double[] SampleSharp(Point[] vertices)
    {
        var result = new double[SampleCount];
        for (var i = 0; i < SampleCount; i++)
        {
            var theta = 2.0 * Math.PI * i / SampleCount;
            var dx = Math.Cos(theta);
            var dy = Math.Sin(theta);

            var best = 0.0;
            for (var v = 0; v < vertices.Length; v++)
            {
                var a = vertices[v];
                var b = vertices[(v + 1) % vertices.Length];
                var ex = b.X - a.X;
                var ey = b.Y - a.Y;
                var denom = dx * ey - dy * ex;
                if (Math.Abs(denom) < 1e-12)
                    continue;

                var t = (a.X * ey - a.Y * ex) / denom; // distance along the ray
                var s = (a.X * dy - a.Y * dx) / denom; // position along the segment
                if (t > 0 && s >= -1e-9 && s <= 1 + 1e-9 && t > best)
                    best = t;
            }

            result[i] = best;
        }

        return result;
    }

    /// <summary>
    /// Circular moving average over the radius array (two passes ≈ triangular kernel). Smoothing
    /// r(θ) in the angle domain shaves convex corners and fills concave ones — a visually adequate
    /// stand-in for arc-based corner rounding.
    /// </summary>
    private static double[] Smooth(double[] radii, int window)
    {
        var half = Math.Max(1, window / 2);
        var current = radii;
        for (var pass = 0; pass < 2; pass++)
        {
            var next = new double[SampleCount];
            var span = 2 * half + 1;
            for (var i = 0; i < SampleCount; i++)
            {
                var sum = 0.0;
                for (var j = -half; j <= half; j++)
                    sum += current[(i + j + SampleCount) % SampleCount];
                next[i] = sum / span;
            }

            current = next;
        }

        return current;
    }

    /// <summary>Rotates the shape by circularly shifting the radius array (1 sample = 1 degree).</summary>
    private static double[] Shift(double[] radii, int degrees)
    {
        var result = new double[SampleCount];
        for (var i = 0; i < SampleCount; i++)
            result[i] = radii[((i - degrees) % SampleCount + SampleCount) % SampleCount];
        return result;
    }

    /// <summary>Scales the array so the shape's bounding box max dimension is exactly 1.</summary>
    private static double[] Normalize(double[] radii)
    {
        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        for (var i = 0; i < SampleCount; i++)
        {
            var theta = 2.0 * Math.PI * i / SampleCount;
            var x = radii[i] * Math.Cos(theta);
            var y = radii[i] * Math.Sin(theta);
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        var scale = 1.0 / Math.Max(maxX - minX, maxY - minY);
        for (var i = 0; i < SampleCount; i++)
            radii[i] *= scale;
        return radii;
    }

    /// <summary>
    /// Closed-form r(θ) of a stadium (capsule) of width 1.25, height 1.0, cap radius 0.5:
    /// a 0.25-long rectangle joined to two semicircle caps centered at x = ±0.125.
    /// </summary>
    private static double[] PillRadii()
    {
        const double capRadius = 0.5;
        const double capCenter = 0.125;

        var result = new double[SampleCount];
        for (var i = 0; i < SampleCount; i++)
        {
            var theta = 2.0 * Math.PI * i / SampleCount;
            var cos = Math.Cos(theta);
            var sin = Math.Sin(theta);

            // Flat top/bottom: r = 0.5 / |sin θ|, valid while the hit lands within |x| <= 0.125.
            var flat = Math.Abs(sin) < 1e-9 ? double.PositiveInfinity : capRadius / Math.Abs(sin);
            if (flat * Math.Abs(cos) <= capCenter + 1e-9)
            {
                result[i] = flat;
                continue;
            }

            // Cap circle centered at (±0.125, 0): r² - 2 r c cosθ + c² - R² = 0.
            var c = cos >= 0 ? capCenter : -capCenter;
            result[i] = c * cos + Math.Sqrt(capRadius * capRadius - c * c * sin * sin);
        }

        return result;
    }

    /// <summary>
    /// Closed-form r(θ) of the oval: an 8-vertex fully rounded polygon is visually a circle;
    /// squashing y by 0.7 yields an ellipse with axis ratio 1 : 0.7.
    /// </summary>
    private static double[] OvalRadii()
    {
        const double a = 1.0;
        const double b = 0.7;

        var result = new double[SampleCount];
        for (var i = 0; i < SampleCount; i++)
        {
            var theta = 2.0 * Math.PI * i / SampleCount;
            result[i] = a * b / Math.Sqrt(
                b * b * Math.Cos(theta) * Math.Cos(theta) + a * a * Math.Sin(theta) * Math.Sin(theta));
        }

        return result;
    }
}
