using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Material3.Avalonia.Colors;

namespace Material3.Avalonia;

/// <summary>
/// Motion scheme selector for the Material 3 theme.
/// </summary>
public enum MotionScheme
{
    Standard,
    Expressive,
}

/// <summary>
/// Density selector for the Material 3 theme.
/// </summary>
public enum MaterialDensity
{
    Comfortable,
    Compact,
}

/// <summary>
/// Material Design 3 theme for Avalonia. Standalone; no Fluent/Simple dependency.
/// Add to <c>Application.Styles</c>:
/// <code>&lt;m3:MaterialTheme SeedColor="#6750A4" /&gt;</code>
/// </summary>
public class MaterialTheme : Styles
{
    public static readonly StyledProperty<Color> SeedColorProperty =
        AvaloniaProperty.Register<MaterialTheme, Color>(nameof(SeedColor), Color.FromRgb(0x67, 0x50, 0xA4));

    public static readonly StyledProperty<SchemeVariant> SchemeVariantProperty =
        AvaloniaProperty.Register<MaterialTheme, SchemeVariant>(nameof(SchemeVariant), SchemeVariant.TonalSpot,
            validate: static value => Enum.IsDefined(value));

    public static readonly StyledProperty<double> ContrastLevelProperty =
        AvaloniaProperty.Register<MaterialTheme, double>(nameof(ContrastLevel), 0.0,
            validate: static value => double.IsFinite(value) && value is >= -1.0 and <= 1.0);

    public static readonly StyledProperty<MotionScheme> MotionSchemeProperty =
        AvaloniaProperty.Register<MaterialTheme, MotionScheme>(nameof(MotionScheme), MotionScheme.Standard);

    public static readonly StyledProperty<bool?> ReduceMotionProperty =
        AvaloniaProperty.Register<MaterialTheme, bool?>(nameof(ReduceMotion), null);

    public static readonly StyledProperty<MaterialDensity> DensityProperty =
        AvaloniaProperty.Register<MaterialTheme, MaterialDensity>(nameof(Density), MaterialDensity.Comfortable);

    private readonly ResourceDictionary _lightColors = new();
    private readonly ResourceDictionary _darkColors = new();

    public MaterialTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);

        var dictionary = (ResourceDictionary)Resources;
        dictionary.ThemeDictionaries[ThemeVariant.Light] = _lightColors;
        dictionary.ThemeDictionaries[ThemeVariant.Dark] = _darkColors;

        RebuildSchemes();
    }

    /// <summary>Source color used to derive the dynamic color scheme.</summary>
    public Color SeedColor
    {
        get => GetValue(SeedColorProperty);
        set => SetValue(SeedColorProperty, value);
    }

    /// <summary>Material Color Utilities scheme variant.</summary>
    public SchemeVariant SchemeVariant
    {
        get => GetValue(SchemeVariantProperty);
        set => SetValue(SchemeVariantProperty, value);
    }

    /// <summary>Contrast level in the range [-1, 1]; 0 is standard, 0.5 medium, 1 high.</summary>
    public double ContrastLevel
    {
        get => GetValue(ContrastLevelProperty);
        set => SetValue(ContrastLevelProperty, value);
    }

    /// <summary>Selects the spring motion scheme used by theme animations.</summary>
    public MotionScheme MotionScheme
    {
        get => GetValue(MotionSchemeProperty);
        set => SetValue(MotionSchemeProperty, value);
    }

    /// <summary>Application override for reduced motion; null follows the platform/default.</summary>
    public bool? ReduceMotion
    {
        get => GetValue(ReduceMotionProperty);
        set => SetValue(ReduceMotionProperty, value);
    }

    /// <summary>Layout density.</summary>
    public MaterialDensity Density
    {
        get => GetValue(DensityProperty);
        set => SetValue(DensityProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SeedColorProperty
            || change.Property == SchemeVariantProperty
            || change.Property == ContrastLevelProperty)
        {
            RebuildSchemes();
        }
        else if (change.Property == MotionSchemeProperty
                 || change.Property == ReduceMotionProperty
                 || change.Property == DensityProperty)
        {
            Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
        }
    }

    private void RebuildSchemes()
    {
        var seed = SeedColor;
        var argb = (uint)((uint)seed.A << 24 | (uint)seed.R << 16 | (uint)seed.G << 8 | seed.B);
        var variant = SchemeVariant;
        var contrast = ContrastLevel;

        Apply(_lightColors, SchemeBuilder.Build(argb, variant, isDark: false, contrast));
        Apply(_darkColors, SchemeBuilder.Build(argb, variant, isDark: true, contrast));

        Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
    }

    private static void Apply(ResourceDictionary target, MaterialColorScheme scheme)
    {
        Set(target, "Primary", scheme.Primary);
        Set(target, "OnPrimary", scheme.OnPrimary);
        Set(target, "PrimaryContainer", scheme.PrimaryContainer);
        Set(target, "OnPrimaryContainer", scheme.OnPrimaryContainer);
        Set(target, "Secondary", scheme.Secondary);
        Set(target, "OnSecondary", scheme.OnSecondary);
        Set(target, "SecondaryContainer", scheme.SecondaryContainer);
        Set(target, "OnSecondaryContainer", scheme.OnSecondaryContainer);
        Set(target, "Tertiary", scheme.Tertiary);
        Set(target, "OnTertiary", scheme.OnTertiary);
        Set(target, "TertiaryContainer", scheme.TertiaryContainer);
        Set(target, "OnTertiaryContainer", scheme.OnTertiaryContainer);
        Set(target, "Error", scheme.Error);
        Set(target, "OnError", scheme.OnError);
        Set(target, "ErrorContainer", scheme.ErrorContainer);
        Set(target, "OnErrorContainer", scheme.OnErrorContainer);
        Set(target, "Surface", scheme.Surface);
        Set(target, "SurfaceDim", scheme.SurfaceDim);
        Set(target, "SurfaceBright", scheme.SurfaceBright);
        Set(target, "SurfaceContainerLowest", scheme.SurfaceContainerLowest);
        Set(target, "SurfaceContainerLow", scheme.SurfaceContainerLow);
        Set(target, "SurfaceContainer", scheme.SurfaceContainer);
        Set(target, "SurfaceContainerHigh", scheme.SurfaceContainerHigh);
        Set(target, "SurfaceContainerHighest", scheme.SurfaceContainerHighest);
        Set(target, "OnSurface", scheme.OnSurface);
        Set(target, "OnSurfaceVariant", scheme.OnSurfaceVariant);
        Set(target, "Outline", scheme.Outline);
        Set(target, "OutlineVariant", scheme.OutlineVariant);
        Set(target, "Shadow", scheme.Shadow);
        Set(target, "Scrim", scheme.Scrim);
        Set(target, "SurfaceTint", scheme.SurfaceTint);
        Set(target, "InverseSurface", scheme.InverseSurface);
        Set(target, "InverseOnSurface", scheme.InverseOnSurface);
        Set(target, "InversePrimary", scheme.InversePrimary);
        Set(target, "PrimaryFixed", scheme.PrimaryFixed);
        Set(target, "PrimaryFixedDim", scheme.PrimaryFixedDim);
        Set(target, "OnPrimaryFixed", scheme.OnPrimaryFixed);
        Set(target, "OnPrimaryFixedVariant", scheme.OnPrimaryFixedVariant);
        Set(target, "SecondaryFixed", scheme.SecondaryFixed);
        Set(target, "SecondaryFixedDim", scheme.SecondaryFixedDim);
        Set(target, "OnSecondaryFixed", scheme.OnSecondaryFixed);
        Set(target, "OnSecondaryFixedVariant", scheme.OnSecondaryFixedVariant);
        Set(target, "TertiaryFixed", scheme.TertiaryFixed);
        Set(target, "TertiaryFixedDim", scheme.TertiaryFixedDim);
        Set(target, "OnTertiaryFixed", scheme.OnTertiaryFixed);
        Set(target, "OnTertiaryFixedVariant", scheme.OnTertiaryFixedVariant);
    }

    private static void Set(ResourceDictionary target, string role, uint argb)
    {
        var color = Color.FromUInt32(argb);
        target["Md3" + role] = color;
        if (target.TryGetValue("Md3" + role + "Brush", out var existing) && existing is SolidColorBrush brush)
        {
            brush.Color = color;
        }
        else
        {
            target["Md3" + role + "Brush"] = new SolidColorBrush(color);
        }
    }
}
