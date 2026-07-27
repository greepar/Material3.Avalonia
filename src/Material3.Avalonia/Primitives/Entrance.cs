using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Material3.Avalonia.Primitives;

/// <summary>Entrance animation kinds used by popup containers.</summary>
public enum EntranceAnimationKind
{
    None,
    /// <summary>Grow downward from the top edge (menus, dropdowns).</summary>
    GrowDown,
    /// <summary>Grow rightward from the leading edge (submenus).</summary>
    GrowRight,
    /// <summary>Plain fade.</summary>
    Fade,
}

/// <summary>
/// Attached behavior that replays an M3 decelerate entrance animation every time the
/// target element is attached to the visual tree. Style-defined animations only run when
/// the style first applies, so reopened popups (which may reuse their content) would not
/// animate again; hooking AttachedToVisualTree replays reliably on every open.
/// </summary>
public static class Entrance
{
    public static readonly AttachedProperty<EntranceAnimationKind> AnimationProperty =
        AvaloniaProperty.RegisterAttached<Control, EntranceAnimationKind>(
            "Animation", typeof(Entrance));

    // M3 emphasized-decelerate.
    private static readonly Easing s_easing = new SplineEasing(0.05, 0.7, 0.1, 1);

    static Entrance()
    {
        AnimationProperty.Changed.AddClassHandler<Control>(OnAnimationChanged);
    }

    public static EntranceAnimationKind GetAnimation(Control control)
        => control.GetValue(AnimationProperty);

    public static void SetAnimation(Control control, EntranceAnimationKind value)
        => control.SetValue(AnimationProperty, value);

    private static void OnAnimationChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        control.AttachedToVisualTree -= OnAttachedToVisualTree;
        if (e.GetNewValue<EntranceAnimationKind>() != EntranceAnimationKind.None)
        {
            control.AttachedToVisualTree += OnAttachedToVisualTree;
            // The property is usually set from the template before the first attach,
            // but guard for elements that are already in the tree.
            if (((Visual)control).IsAttachedToVisualTree())
                Play(control);
        }
    }

    private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
            Play(control);
    }

    private static void Play(Control control)
    {
        var kind = GetAnimation(control);
        if (kind == EntranceAnimationKind.None)
            return;

        var from = new KeyFrame { Cue = new Cue(0.0) };
        var to = new KeyFrame { Cue = new Cue(1.0) };

        from.Setters.Add(new Setter(Visual.OpacityProperty, 0.0));
        to.Setters.Add(new Setter(Visual.OpacityProperty, 1.0));

        switch (kind)
        {
            case EntranceAnimationKind.GrowDown:
                from.Setters.Add(new Setter(ScaleTransform.ScaleYProperty, 0.9));
                from.Setters.Add(new Setter(TranslateTransform.YProperty, -4.0));
                to.Setters.Add(new Setter(ScaleTransform.ScaleYProperty, 1.0));
                to.Setters.Add(new Setter(TranslateTransform.YProperty, 0.0));
                break;
            case EntranceAnimationKind.GrowRight:
                from.Setters.Add(new Setter(ScaleTransform.ScaleXProperty, 0.9));
                from.Setters.Add(new Setter(TranslateTransform.XProperty, -4.0));
                to.Setters.Add(new Setter(ScaleTransform.ScaleXProperty, 1.0));
                to.Setters.Add(new Setter(TranslateTransform.XProperty, 0.0));
                break;
        }

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(250),
            Easing = s_easing,
            FillMode = FillMode.Forward,
            Children = { from, to },
        };

        _ = animation.RunAsync(control);
    }
}
