// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;

namespace Material3.Avalonia.Controls;

/// <summary>Layout variants for <see cref="ButtonGroup"/>.</summary>
public enum ButtonGroupVariant
{
    /// <summary>Independent buttons in a row with an 8dp gap.</summary>
    Standard,
    /// <summary>Connected row with a 2dp gap: outer edges fully rounded, inner corners 8dp.</summary>
    Connected,
}

/// <summary>
/// Material 3 button group: arranges buttons in a horizontal row. The
/// <see cref="ButtonGroupVariant.Connected"/> variant tightens the gap to 2dp and
/// reshapes the child buttons so only the outer edges keep the full rounding while
/// inner corners use 8dp and the pressed item becomes fully rounded. The
/// <see cref="ButtonGroupVariant.Standard"/> variant uses the M3 Expressive press
/// interaction: a pressed (or checked) button grows 15% wider
/// while its direct neighbours are squeezed to absorb the growth, animated with a
/// springy overshoot.
/// </summary>
public class ButtonGroup : ItemsControl
{
    public static readonly StyledProperty<ButtonGroupVariant> VariantProperty =
        AvaloniaProperty.Register<ButtonGroup, ButtonGroupVariant>(nameof(Variant));

    private const double InnerCorner = 8;

    // M3 Expressive press interaction (spec ported from m3fx): the pressed button's
    // width grows by 15%; only the direct neighbours absorb the growth (half each,
    // or all of it when the pressed button sits at an edge); squeezed buttons never
    // go below 48px. Progress is time-driven: 350ms with an overshooting
    // cubic-bezier(0.42, 1.67, 0.21, 0.90).
    private const double PressGrowthFraction = 0.15;
    private const double MinSqueezedWidth = 48;
    private const double PressAnimationSeconds = 0.35;

    private sealed class PressAnim
    {
        public double Current;
        public double From;
        public double Target;
        public long Start;
        public bool Animating;
    }

    private readonly Dictionary<Control, PressAnim> _anims = new();

    // Natural widths snapshotted right before the interaction forces explicit Width
    // values. Driving Width (instead of custom arrange math) lets the regular measure
    // pass re-flow the button content, so labels stay centered while squeezing.
    private readonly Dictionary<Control, double> _naturalWidths = new();
    private readonly Dictionary<Control, Thickness> _naturalPaddings = new();
    private readonly Dictionary<Control, IDisposable> _widthOverrides = new();
    private readonly Dictionary<Button, IDisposable> _paddingOverrides = new();
    private readonly Dictionary<Button, IDisposable> _cornerOverrides = new();
    private bool _widthsForced;
    private bool _frameRequested;
    private StackPanel? _itemsPanel;

    public ButtonGroup()
    {
        // The panel is built in code so its Spacing can follow Variant (8dp standard,
        // 2dp connected) without a theme-side panel swap.
        ItemsPanel = new FuncTemplate<Panel?>(CreateItemsPanel);

        // Implementation choice: connected corner shapes are applied directly to the
        // child Buttons in C# (style classes on containers cannot target the buttons
        // from a ControlTheme). ContainerPrepared/Clearing/IndexChanged keep the
        // shapes correct as items change.
        ContainerPrepared += OnContainerPrepared;
        ContainerClearing += OnContainerClearing;
        ContainerIndexChanged += (_, _) => UpdateShapes();
    }

    /// <summary>The layout variant. Defaults to <see cref="ButtonGroupVariant.Standard"/>.</summary>
    public ButtonGroupVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VariantProperty)
        {
            if (_itemsPanel is not null)
            {
                _itemsPanel.Spacing = Variant == ButtonGroupVariant.Connected ? 2.0 : 8.0;
            }

            UpdateShapes();
            RefreshAllPressTargets();
        }
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is Button button)
        {
            // AOT-safe: strongly typed property-change subscription, no reflection.
            button.PropertyChanged += OnChildPropertyChanged;
            button.DetachedFromVisualTree += OnChildDetachedFromVisualTree;
            UpdatePressTarget(button);
            ApplyShape(button, e.Index, ItemCount);
        }

        UpdateShapes();
    }

    private void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        // Stop the current group-wide width morph before ItemsControl changes the
        // realized container set. Otherwise a queued animation frame can reapply a
        // stale override to the button after ContainerClearing released it.
        ReleaseInteractionOverrides();

        if (e.Container is Button button)
        {
            button.PropertyChanged -= OnChildPropertyChanged;
            button.DetachedFromVisualTree -= OnChildDetachedFromVisualTree;
            ClearOverride(_cornerOverrides, button);
            ClearOverride(_paddingOverrides, button);
        }

        ClearOverride(_widthOverrides, e.Container);

        _anims.Remove(e.Container);
        _naturalWidths.Remove(e.Container);
        _naturalPaddings.Remove(e.Container);

        // ContainerClearing is raised before ItemsControl removes the container. Updating
        // shapes here would immediately reapply animation-priority values to the button
        // that is being cleared; ContainerIndexChanged updates the remaining buttons.
    }

    private void OnChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if ((e.Property == Button.IsPressedProperty || e.Property == ToggleButton.IsCheckedProperty) &&
            sender is Button button)
        {
            UpdatePressTarget(button);
            EnsureCornerTransition(button);
            UpdateShapes();
        }
    }

    private void OnChildDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Button button)
            return;

        button.PropertyChanged -= OnChildPropertyChanged;
        button.DetachedFromVisualTree -= OnChildDetachedFromVisualTree;
        ClearOverride(_widthOverrides, button);
        ClearOverride(_paddingOverrides, button);
        ClearOverride(_cornerOverrides, button);
        _anims.Remove(button);
        _naturalWidths.Remove(button);
        _naturalPaddings.Remove(button);
    }

    private static void EnsureCornerTransition(Button button)
    {
        if (button.Transitions?.OfType<CornerRadiusTransition>()
            .Any(transition => transition.Property == Button.CornerRadiusProperty) == true)
        {
            return;
        }

        var transitions = new Transitions();
        if (button.Transitions is { } existing)
        {
            foreach (var transition in existing)
            {
                transitions.Add(transition);
            }
        }

        transitions.Add(new CornerRadiusTransition
        {
            Property = Button.CornerRadiusProperty,
            Duration = TimeSpan.FromMilliseconds(200),
            Easing = new CubicEaseOut(),
        });
        button.Transitions = transitions;
    }

    private void UpdatePressTarget(Button button)
    {
        // Width morphing is for Standard groups. Connected groups keep stable widths
        // and express the active item by changing its inner corners instead.
        var expanded = Variant == ButtonGroupVariant.Standard &&
                       (button.IsPressed || button is ToggleButton { IsChecked: true });
        var target = expanded ? 1.0 : 0.0;

        if (!_anims.TryGetValue(button, out var anim))
        {
            if (target == 0.0)
            {
                return;
            }

            anim = new PressAnim();
            _anims[button] = anim;
        }

        if (anim.Target == target)
        {
            return;
        }

        // Snapshot natural widths before the first forced Width takes effect.
        if (!_widthsForced)
        {
            SnapshotNaturalWidths();
        }

        anim.From = anim.Current;
        anim.Target = target;
        anim.Start = Stopwatch.GetTimestamp();
        anim.Animating = true;
        RequestFrame();
    }

    private void SnapshotNaturalWidths()
    {
        _naturalWidths.Clear();
        var count = ItemCount;
        for (var i = 0; i < count; i++)
        {
            if (ContainerFromIndex(i) is { } child)
            {
                var w = child.Bounds.Width;
                if (w <= 0)
                {
                    w = child.DesiredSize.Width;
                }

                _naturalWidths[child] = w;
                _naturalPaddings[child] = child is Button button ? button.Padding : default;
            }
        }
    }

    private void RefreshAllPressTargets()
    {
        var count = ItemCount;
        for (var i = 0; i < count; i++)
        {
            if (ContainerFromIndex(i) is Button button)
            {
                UpdatePressTarget(button);
            }
        }
    }

    private void RequestFrame()
    {
        if (_frameRequested)
        {
            return;
        }

        _frameRequested = true;
        Dispatcher.UIThread.Post(OnFrame, DispatcherPriority.Render);
    }

    private void OnFrame()
    {
        _frameRequested = false;
        var now = Stopwatch.GetTimestamp();
        var anyAnimating = false;
        foreach (var anim in _anims.Values)
        {
            if (!anim.Animating)
            {
                continue;
            }

            var t = (now - anim.Start) / (double)Stopwatch.Frequency / PressAnimationSeconds;
            if (t >= 1)
            {
                anim.Current = anim.Target;
                anim.Animating = false;
            }
            else
            {
                anim.Current = anim.From + (anim.Target - anim.From) * EaseSpring(t);
                anyAnimating = true;
            }
        }

        ApplyWidths();
        if (anyAnimating)
        {
            _frameRequested = true;
            DispatcherTimer.RunOnce(OnFrame, TimeSpan.FromMilliseconds(16));
        }
    }

    /// <summary>
    /// Applies the interaction widths by driving each child's Width property, letting
    /// the normal layout pass re-center content. Releases the overrides once every
    /// animation has settled back to rest.
    /// </summary>
    private void ApplyWidths()
    {
        var count = ItemCount;
        if (count == 0)
        {
            return;
        }

        var atRest = true;
        foreach (var anim in _anims.Values)
        {
            if (anim.Animating || anim.Current > 0.001)
            {
                atRest = false;
                break;
            }
        }

        if (atRest)
        {
            if (_widthsForced)
            {
                for (var i = 0; i < count; i++)
                {
                    if (ContainerFromIndex(i) is { } child)
                    {
                        ClearOverride(_widthOverrides, child);
                        if (child is Button button)
                        {
                            ClearOverride(_paddingOverrides, button);
                        }
                    }
                }

                _widthsForced = false;
                _naturalWidths.Clear();
                _naturalPaddings.Clear();
            }

            return;
        }

        // Compute target widths from the natural snapshot.
        var children = new Control[count];
        var widths = new double[count];
        for (var i = 0; i < count; i++)
        {
            var child = ContainerFromIndex(i);
            if (child is null)
            {
                return;
            }

            children[i] = child;
            widths[i] = _naturalWidths.TryGetValue(child, out var w) ? w : child.Bounds.Width;
        }

        for (var i = 0; i < count; i++)
        {
            var progress = _anims.TryGetValue(children[i], out var anim) ? anim.Current : 0.0;
            var baseWidth = _naturalWidths.TryGetValue(children[i], out var nat) ? nat : children[i].Bounds.Width;
            var growth = baseWidth * PressGrowthFraction * progress;
            if (growth <= 0)
            {
                continue;
            }

            widths[i] += growth;
            var left = i - 1;
            var right = i + 1;
            if (left >= 0 && right < count)
            {
                widths[left] -= growth / 2;
                widths[right] -= growth / 2;
            }
            else if (left >= 0)
            {
                widths[left] -= growth;
            }
            else if (right < count)
            {
                widths[right] -= growth;
            }
        }

        for (var i = 0; i < count; i++)
        {
            var baseWidth = _naturalWidths.TryGetValue(children[i], out var nat) ? nat : widths[i];
            var w = Math.Max(widths[i], Math.Min(baseWidth, MinSqueezedWidth));
            SetOverride(_widthOverrides, children[i], Control.WidthProperty, w, BindingPriority.Animation);
            if (children[i] is Button button
                && _naturalPaddings.TryGetValue(button, out var padding))
            {
                var shrink = Math.Max(0, baseWidth - w);
                var left = Math.Max(8, padding.Left - shrink / 2);
                var right = Math.Max(8, padding.Right - shrink / 2);
                SetOverride(_paddingOverrides, button, Button.PaddingProperty,
                    new Thickness(left, padding.Top, right, padding.Bottom), BindingPriority.Animation);
            }
        }

        _widthsForced = true;
    }

    /// <summary>Overshooting spring approximation: cubic-bezier(0.42, 1.67, 0.21, 0.90).</summary>
    private static double EaseSpring(double x)
    {
        if (x <= 0)
        {
            return 0;
        }

        if (x >= 1)
        {
            return 1;
        }

        const double x1 = 0.42, y1 = 1.67, x2 = 0.21, y2 = 0.90;

        // Solve the parametric t for the given x (Newton; x(t) is monotonic here).
        var t = x;
        for (var i = 0; i < 8; i++)
        {
            var error = BezierComponent(t, x1, x2) - x;
            var derivative = BezierDerivative(t, x1, x2);
            if (Math.Abs(derivative) < 1e-6)
            {
                break;
            }

            t = Math.Clamp(t - error / derivative, 0, 1);
        }

        return BezierComponent(t, y1, y2);
    }

    private static double BezierComponent(double t, double p1, double p2)
    {
        var mt = 1 - t;
        return 3 * mt * mt * t * p1 + 3 * mt * t * t * p2 + t * t * t;
    }

    private static double BezierDerivative(double t, double p1, double p2)
    {
        var mt = 1 - t;
        return 3 * mt * mt * p1 + 6 * mt * t * (p2 - p1) + 3 * t * t * (1 - p2);
    }

    private Panel CreateItemsPanel()
    {
        var panel = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = Variant == ButtonGroupVariant.Connected ? 2.0 : 8.0,
        };
        _itemsPanel = panel;
        return panel;
    }

    private void UpdateShapes()
    {
        var count = ItemCount;
        for (var i = 0; i < count; i++)
        {
            if (ContainerFromIndex(i) is not Button button)
            {
                continue;
            }

            ApplyShape(button, i, count);
        }
    }

    private void ApplyShape(Button button, int index, int count)
    {
        if (Variant != ButtonGroupVariant.Connected)
        {
            ClearOverride(_cornerOverrides, button);
            return;
        }

        if (button.IsPressed || button is ToggleButton { IsChecked: true })
        {
            var activeRadius = GetFullRadius(button);
            SetOverride(_cornerOverrides, button, Button.CornerRadiusProperty,
                new CornerRadius(activeRadius), BindingPriority.Animation);
            return;
        }

        var fullRadius = GetFullRadius(button);
        var left = index == 0 ? fullRadius : InnerCorner;
        var right = index == count - 1 ? fullRadius : InnerCorner;
        SetOverride(_cornerOverrides, button, Button.CornerRadiusProperty,
            new CornerRadius(left, right, right, left), BindingPriority.Animation);
    }

    private static void SetOverride<TKey, TValue>(Dictionary<TKey, IDisposable> overrides, TKey key,
        StyledProperty<TValue> property, TValue value, BindingPriority priority)
        where TKey : AvaloniaObject
    {
        ClearOverride(overrides, key);
        overrides[key] = key.SetValue(property, value, priority)!;
    }

    private static void ClearOverride<TKey>(Dictionary<TKey, IDisposable> overrides, TKey key)
        where TKey : notnull
    {
        if (overrides.Remove(key, out var disposable))
        {
            disposable.Dispose();
        }
    }

    private void ReleaseInteractionOverrides()
    {
        foreach (var disposable in _widthOverrides.Values)
            disposable.Dispose();
        foreach (var disposable in _paddingOverrides.Values)
            disposable.Dispose();

        _widthOverrides.Clear();
        _paddingOverrides.Clear();
        _widthsForced = false;
        _naturalWidths.Clear();
        _naturalPaddings.Clear();

        foreach (var anim in _anims.Values)
        {
            anim.Current = 0;
            anim.From = 0;
            anim.Target = 0;
            anim.Animating = false;
        }
    }

    private static double GetFullRadius(Button button) =>
        button.Bounds.Height > 0 ? button.Bounds.Height / 2 : 20;
}
