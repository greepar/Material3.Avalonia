using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Material3.Avalonia.Primitives;

/// <summary>
/// Renders Material pressed ripples. Place inside a control template, clipped by the
/// component shape. It attaches to its templated parent (or a named source) for input.
/// Does not hit-test and does not appear in the automation tree.
/// </summary>
public sealed class RippleHost : Control
{
    public static readonly StyledProperty<IBrush?> RippleBrushProperty =
        AvaloniaProperty.Register<RippleHost, IBrush?>(nameof(RippleBrush));

    public static readonly StyledProperty<double> RippleOpacityProperty =
        AvaloniaProperty.Register<RippleHost, double>(nameof(RippleOpacity), 0.10,
            validate: static value => double.IsFinite(value) && value is >= 0.0 and <= 1.0);

    public static readonly StyledProperty<bool> IsRippleEnabledProperty =
        AvaloniaProperty.Register<RippleHost, bool>(nameof(IsRippleEnabled), true);

    public static readonly StyledProperty<CornerRadius> RippleClipRadiusProperty =
        AvaloniaProperty.Register<RippleHost, CornerRadius>(nameof(RippleClipRadius));

    private readonly List<Ripple> _ripples = new();
    private readonly HashSet<Key> _pressedKeys = new();
    private InputElement? _source;
    private bool _frameRequested;

    static RippleHost()
    {
        IsHitTestVisibleProperty.OverrideDefaultValue<RippleHost>(false);
        AffectsRender<RippleHost>(RippleBrushProperty, RippleOpacityProperty, RippleClipRadiusProperty);
    }

    /// <summary>Brush used for ripples; should be the component's on-color/state color.</summary>
    public IBrush? RippleBrush
    {
        get => GetValue(RippleBrushProperty);
        set => SetValue(RippleBrushProperty, value);
    }

    /// <summary>Peak ripple opacity (pressed state layer opacity).</summary>
    public double RippleOpacity
    {
        get => GetValue(RippleOpacityProperty);
        set => SetValue(RippleOpacityProperty, value);
    }

    public bool IsRippleEnabled
    {
        get => GetValue(IsRippleEnabledProperty);
        set => SetValue(IsRippleEnabledProperty, value);
    }

    /// <summary>Corner radius used to clip ripples to the component shape.</summary>
    public CornerRadius RippleClipRadius
    {
        get => GetValue(RippleClipRadiusProperty);
        set => SetValue(RippleClipRadiusProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _source = FindSource();
        if (_source is not null)
        {
            _source.AddHandler(PointerPressedEvent, OnSourcePointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
            _source.AddHandler(PointerReleasedEvent, OnSourcePointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
            // PointerCaptureLost is a Direct-routed event; registering with Bubble would
            // never fire, leaving ripples stuck at peak opacity when a scroll gesture
            // steals the pointer (the common touch-scroll path on mobile).
            _source.AddHandler(PointerCaptureLostEvent, OnSourceCaptureLost, RoutingStrategies.Direct, handledEventsToo: true);
            // Touch capture may be held by a child of the source; the Direct capture-lost
            // then never reaches us. Scroll gesture events bubble through the source, so
            // use them as the release signal when a pan takes over.
            _source.AddHandler(ScrollGestureEvent, OnSourceScrollGesture, RoutingStrategies.Bubble, handledEventsToo: true);
            _source.AddHandler(KeyDownEvent, OnSourceKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
            _source.AddHandler(KeyUpEvent, OnSourceKeyUp, RoutingStrategies.Tunnel, handledEventsToo: true);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_source is not null)
        {
            _source.RemoveHandler(PointerPressedEvent, OnSourcePointerPressed);
            _source.RemoveHandler(PointerReleasedEvent, OnSourcePointerReleased);
            _source.RemoveHandler(PointerCaptureLostEvent, OnSourceCaptureLost);
            _source.RemoveHandler(ScrollGestureEvent, OnSourceScrollGesture);
            _source.RemoveHandler(KeyDownEvent, OnSourceKeyDown);
            _source.RemoveHandler(KeyUpEvent, OnSourceKeyUp);
            _source = null;
        }

        _ripples.Clear();
        _pressedKeys.Clear();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsRippleEnabledProperty && !change.GetNewValue<bool>())
        {
            _pressedKeys.Clear();
            ReleaseAll();
        }
    }

    private InputElement? FindSource()
    {
        if (TemplatedParent is InputElement tp)
            return tp;
        return this.FindAncestorOfType<InputElement>();
    }

    private void OnSourcePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsRippleEnabled || _source is not { IsEffectivelyEnabled: true })
            return;
        if (!e.GetCurrentPoint(_source).Properties.IsLeftButtonPressed)
            return;

        // Tunnel handlers also fire for presses on nested interactive children
        // (e.g. child TreeViewItems below this item's header). Only ripple when
        // the press actually lands inside this host's own bounds.
        var origin = e.GetPosition(this);
        if (origin.X < 0 || origin.Y < 0 || origin.X > Bounds.Width || origin.Y > Bounds.Height)
            return;

        StartRipple(origin, fromKeyboard: false);
    }

    private void OnSourcePointerReleased(object? sender, PointerReleasedEventArgs e) => ReleaseAll();

    private void OnSourceCaptureLost(object? sender, PointerCaptureLostEventArgs e) => ReleaseAll();

    private void OnSourceScrollGesture(object? sender, ScrollGestureEventArgs e) => ReleaseAll();

    private void OnSourceKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsRippleEnabled || _source is not { IsEffectivelyEnabled: true })
            return;
        if (e.Key is Key.Space or Key.Enter && _pressedKeys.Add(e.Key))
        {
            StartRipple(new Point(Bounds.Width / 2, Bounds.Height / 2), fromKeyboard: true);
        }
    }

    private void OnSourceKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Space or Key.Enter)
        {
            _pressedKeys.Remove(e.Key);
            ReleaseAll();
        }
    }

    internal void StartRipple(Point origin, bool fromKeyboard)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        // Radius reaching the farthest corner.
        var dx = Math.Max(origin.X, w - origin.X);
        var dy = Math.Max(origin.Y, h - origin.Y);
        var radius = Math.Sqrt(dx * dx + dy * dy);

        _ripples.Add(new Ripple
        {
            Origin = origin,
            TargetRadius = radius,
            Started = Stopwatch.GetTimestamp(),
            Released = null,
        });

        // Cap concurrent ripples to avoid unbounded growth on rapid clicks.
        if (_ripples.Count > 8)
            _ripples.RemoveAt(0);

        RequestFrame();
    }

    private void ReleaseAll()
    {
        var now = Stopwatch.GetTimestamp();
        foreach (var r in _ripples)
            r.Released ??= now;
        RequestFrame();
    }

    private void RequestFrame()
    {
        if (_frameRequested)
            return;
        _frameRequested = true;
        Dispatcher.UIThread.Post(() =>
        {
            _frameRequested = false;
            if (_ripples.Count > 0 && VisualRoot is not null)
            {
                InvalidateVisual();
            }
        }, DispatcherPriority.Render);
    }

    public override void Render(DrawingContext context)
    {
        if (_ripples.Count == 0)
            return;

        var brush = RippleBrush;
        if (brush is null)
            return;

        // Motion timings: implementation tokens, visually calibrated (M3 has no public ripple values).
        const double expandSeconds = 0.45;
        const double fadeInSeconds = 0.075;
        const double fadeOutSeconds = 0.3;

        var now = Stopwatch.GetTimestamp();
        var peakOpacity = RippleOpacity;
        var rect = new Rect(Bounds.Size);

        // Clamp corner radii to half the bounds; oversized values (e.g. the "Full" 9999 token)
        // would otherwise produce an invalid rounded-rect clip.
        var radius = RippleClipRadius;
        var maxRadius = Math.Min(rect.Width, rect.Height) / 2;
        var clip = new RoundedRect(rect,
            ClampCorner(radius.TopLeft, maxRadius),
            ClampCorner(radius.TopRight, maxRadius),
            ClampCorner(radius.BottomRight, maxRadius),
            ClampCorner(radius.BottomLeft, maxRadius));

        using var _ = context.PushClip(clip);

        for (var i = _ripples.Count - 1; i >= 0; i--)
        {
            var ripple = _ripples[i];
            var elapsed = (now - ripple.Started) / (double)Stopwatch.Frequency;
            var expand = Math.Min(1.0, elapsed / expandSeconds);
            // Decelerate easing for the spatial expansion.
            var eased = 1 - Math.Pow(1 - expand, 3);
            var currentRadius = ripple.TargetRadius * (0.15 + 0.85 * eased);

            // Short fade-in so the ripple does not pop in at full strength.
            var opacity = peakOpacity * Math.Min(1.0, elapsed / fadeInSeconds);
            if (ripple.Released is { } released)
            {
                var fadeElapsed = (now - released) / (double)Stopwatch.Frequency;
                var fade = Math.Min(1.0, fadeElapsed / fadeOutSeconds);
                // Fade out from the opacity the ripple actually reached at release time.
                var releaseElapsed = (released - ripple.Started) / (double)Stopwatch.Frequency;
                var opacityAtRelease = peakOpacity * Math.Min(1.0, releaseElapsed / fadeInSeconds);
                opacity = opacityAtRelease * (1 - fade);
                if (fade >= 1)
                {
                    _ripples.RemoveAt(i);
                    continue;
                }
            }

            if (brush is ISolidColorBrush scb)
            {
                var c = scb.Color;
                var faded = new SolidColorBrush(c, opacity * scb.Opacity);
                context.DrawEllipse(faded, null, ripple.Origin, currentRadius, currentRadius);
            }
            else
            {
                using var op = context.PushOpacity(opacity);
                context.DrawEllipse(brush, null, ripple.Origin, currentRadius, currentRadius);
            }
        }

        if (_ripples.Count > 0)
            RequestFrame();
    }

    private static Vector ClampCorner(double r, double max)
    {
        var v = Math.Min(Math.Max(r, 0), max);
        return new Vector(v, v);
    }

    private sealed class Ripple
    {
        public Point Origin;
        public double TargetRadius;
        public long Started;
        public long? Released;
    }
}
