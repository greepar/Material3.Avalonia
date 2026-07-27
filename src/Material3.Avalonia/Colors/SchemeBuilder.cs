// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities),
// dynamiccolor/color_spec_2021.ts (MaterialDynamicColors, 2021 spec, standard-contrast tones)
// and scheme/scheme_*.ts, Apache-2.0.
//
// Implementation choices (deviations from the full MCU dynamic color engine):
// - Role tones are the fixed standard-contrast tone mappings from the 2021 spec;
//   the full ContrastCurve / ToneDeltaPair machinery is not ported.
// - Contrast adjustment is a simplified linear interpolation of tones toward
//   their extremes (see AdjustedTone below), not the official contrast curves.
// - Fidelity/Content container tone tweaks (source-color tone matching) are not
//   applied; those variants differ from MCU only in palette construction here.

namespace Material3.Avalonia.Colors;

/// <summary>
/// Builds a full <see cref="MaterialColorScheme"/> from a seed color, scheme
/// variant, dark/light mode and contrast level.
/// </summary>
public static class SchemeBuilder
{
    /// <summary>
    /// Builds a Material 3 color scheme.
    /// </summary>
    /// <param name="seedArgb">Seed color, ARGB.</param>
    /// <param name="variant">Scheme variant.</param>
    /// <param name="isDark">Whether to build the dark scheme.</param>
    /// <param name="contrastLevel">-1 (reduced) to 1 (high). Clamped. 0 is standard.
    /// Implementation choice: values below 0 are treated as standard contrast;
    /// values above 0 linearly move foreground/outline/accent tones toward their
    /// higher-contrast extremes.</param>
    public static MaterialColorScheme Build(
        uint seedArgb, SchemeVariant variant, bool isDark, double contrastLevel)
    {
        var c = MathUtils.ClampDouble(-1.0, 1.0, contrastLevel);
        // Implementation choice: only positive contrast levels adjust tones.
        var k = Math.Max(0.0, c);

        var sourceHct = Hct.FromInt(seedArgb);
        var palettes = CorePalettes.Of(sourceHct, variant);
        var p = palettes.Primary;
        var s = palettes.Secondary;
        var t = palettes.Tertiary;
        var n = palettes.Neutral;
        var nv = palettes.NeutralVariant;
        var e = palettes.Error;

        var mono = variant == SchemeVariant.Monochrome;

        // Local helpers.
        // Tone(palette, standardTone, highContrastTone): lerp toward the
        // high-contrast tone by the (positive) contrast amount.
        uint Tone(TonalPalette palette, double standard, double highContrast) =>
            palette.Tone(RoundTone(MathUtils.Lerp(standard, highContrast, k)));
        uint Fixed(TonalPalette palette, double tone) => palette.Tone(RoundTone(tone));

        if (!isDark)
        {
            return new MaterialColorScheme
            {
                // Accents. Monochrome tones follow MaterialDynamicColors'
                // isMonochrome branches in color_spec_2021.ts.
                Primary = mono ? Fixed(p, 0) : Tone(p, 40, 30),
                OnPrimary = mono ? Fixed(p, 90) : Fixed(p, 100),
                PrimaryContainer = mono ? Fixed(p, 25) : Fixed(p, 90),
                OnPrimaryContainer = mono ? Fixed(p, 100) : Tone(p, 30, 20),
                Secondary = Tone(s, 40, 30),
                OnSecondary = Fixed(s, 100),
                SecondaryContainer = mono ? Fixed(s, 85) : Fixed(s, 90),
                OnSecondaryContainer = mono ? Tone(s, 10, 0) : Tone(s, 30, 20),
                Tertiary = mono ? Tone(t, 25, 15) : Tone(t, 40, 30),
                OnTertiary = mono ? Fixed(t, 90) : Fixed(t, 100),
                TertiaryContainer = mono ? Fixed(t, 49) : Fixed(t, 90),
                OnTertiaryContainer = mono ? Fixed(t, 100) : Tone(t, 30, 20),
                Error = Tone(e, 40, 30),
                OnError = Fixed(e, 100),
                ErrorContainer = Fixed(e, 90),
                OnErrorContainer = mono ? Tone(e, 10, 0) : Tone(e, 30, 20),

                // Surfaces.
                Surface = Fixed(n, 98),
                SurfaceDim = Fixed(n, 87),
                SurfaceBright = Fixed(n, 98),
                SurfaceContainerLowest = Fixed(n, 100),
                SurfaceContainerLow = Fixed(n, 96),
                SurfaceContainer = Fixed(n, 94),
                SurfaceContainerHigh = Fixed(n, 92),
                SurfaceContainerHighest = Fixed(n, 90),
                OnSurface = Tone(n, 10, 0),
                OnSurfaceVariant = Tone(nv, 30, 20),
                Outline = Tone(nv, 50, 30),
                OutlineVariant = Tone(nv, 80, 60),
                Shadow = Fixed(n, 0),
                Scrim = Fixed(n, 0),
                SurfaceTint = mono ? Fixed(p, 0) : Tone(p, 40, 30),
                InverseSurface = Fixed(n, 20),
                InverseOnSurface = Fixed(n, 95),
                InversePrimary = Fixed(p, 80),

                // Fixed roles (identical in light and dark).
                PrimaryFixed = mono ? Fixed(p, 40) : Fixed(p, 90),
                PrimaryFixedDim = mono ? Fixed(p, 30) : Fixed(p, 80),
                OnPrimaryFixed = mono ? Fixed(p, 100) : Fixed(p, 10),
                OnPrimaryFixedVariant = mono ? Fixed(p, 90) : Fixed(p, 30),
                SecondaryFixed = mono ? Fixed(s, 80) : Fixed(s, 90),
                SecondaryFixedDim = mono ? Fixed(s, 70) : Fixed(s, 80),
                OnSecondaryFixed = Fixed(s, 10),
                OnSecondaryFixedVariant = mono ? Fixed(s, 25) : Fixed(s, 30),
                TertiaryFixed = mono ? Fixed(t, 40) : Fixed(t, 90),
                TertiaryFixedDim = mono ? Fixed(t, 30) : Fixed(t, 80),
                OnTertiaryFixed = mono ? Fixed(t, 100) : Fixed(t, 10),
                OnTertiaryFixedVariant = mono ? Fixed(t, 90) : Fixed(t, 30),
            };
        }

        return new MaterialColorScheme
        {
            // Accents (dark). Monochrome tones follow MaterialDynamicColors'
            // isMonochrome branches in color_spec_2021.ts.
            Primary = mono ? Fixed(p, 100) : Tone(p, 80, 90),
            OnPrimary = mono ? Fixed(p, 10) : Fixed(p, 20),
            PrimaryContainer = mono ? Fixed(p, 85) : Fixed(p, 30),
            OnPrimaryContainer = mono ? Fixed(p, 0) : Tone(p, 90, 100),
            Secondary = Tone(s, 80, 90),
            OnSecondary = mono ? Fixed(s, 10) : Fixed(s, 20),
            SecondaryContainer = Fixed(s, 30),
            OnSecondaryContainer = Tone(s, 90, 100),
            Tertiary = mono ? Tone(t, 90, 100) : Tone(t, 80, 90),
            OnTertiary = mono ? Fixed(t, 10) : Fixed(t, 20),
            TertiaryContainer = mono ? Fixed(t, 60) : Fixed(t, 30),
            OnTertiaryContainer = mono ? Fixed(t, 0) : Tone(t, 90, 100),
            Error = Tone(e, 80, 90),
            OnError = Fixed(e, 20),
            ErrorContainer = Fixed(e, 30),
            OnErrorContainer = Tone(e, 90, 100),

            // Surfaces (dark).
            Surface = Fixed(n, 6),
            SurfaceDim = Fixed(n, 6),
            SurfaceBright = Fixed(n, 24),
            SurfaceContainerLowest = Fixed(n, 4),
            SurfaceContainerLow = Fixed(n, 10),
            SurfaceContainer = Fixed(n, 12),
            SurfaceContainerHigh = Fixed(n, 17),
            SurfaceContainerHighest = Fixed(n, 22),
            OnSurface = Tone(n, 90, 100),
            OnSurfaceVariant = Tone(nv, 80, 90),
            Outline = Tone(nv, 60, 80),
            OutlineVariant = Tone(nv, 30, 50),
            Shadow = Fixed(n, 0),
            Scrim = Fixed(n, 0),
            SurfaceTint = mono ? Fixed(p, 100) : Tone(p, 80, 90),
            InverseSurface = Fixed(n, 90),
            InverseOnSurface = Fixed(n, 20),
            InversePrimary = Fixed(p, 40),

            // Fixed roles (identical in light and dark).
            PrimaryFixed = mono ? Fixed(p, 40) : Fixed(p, 90),
            PrimaryFixedDim = mono ? Fixed(p, 30) : Fixed(p, 80),
            OnPrimaryFixed = mono ? Fixed(p, 100) : Fixed(p, 10),
            OnPrimaryFixedVariant = mono ? Fixed(p, 90) : Fixed(p, 30),
            SecondaryFixed = mono ? Fixed(s, 80) : Fixed(s, 90),
            SecondaryFixedDim = mono ? Fixed(s, 70) : Fixed(s, 80),
            OnSecondaryFixed = Fixed(s, 10),
            OnSecondaryFixedVariant = mono ? Fixed(s, 25) : Fixed(s, 30),
            TertiaryFixed = mono ? Fixed(t, 40) : Fixed(t, 90),
            TertiaryFixedDim = mono ? Fixed(t, 30) : Fixed(t, 80),
            OnTertiaryFixed = mono ? Fixed(t, 100) : Fixed(t, 10),
            OnTertiaryFixedVariant = mono ? Fixed(t, 90) : Fixed(t, 30),
        };
    }

    private static int RoundTone(double tone)
    {
        return MathUtils.ClampInt(0, 100, (int)Math.Floor(tone + 0.5));
    }
}
