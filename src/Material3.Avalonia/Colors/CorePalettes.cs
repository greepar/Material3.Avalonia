// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities),
// dynamiccolor/dynamic_scheme.ts (DynamicSchemePalettesDelegateImpl2021, the 2021 spec) and
// scheme/scheme_*.ts, Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// The core tonal palettes of a Material 3 dynamic scheme, constructed from a
/// source color and a <see cref="SchemeVariant"/> per the official
/// material-color-utilities 2021 spec palette rules.
/// </summary>
public sealed class CorePalettes
{
    public TonalPalette Primary { get; }
    public TonalPalette Secondary { get; }
    public TonalPalette Tertiary { get; }
    public TonalPalette Neutral { get; }
    public TonalPalette NeutralVariant { get; }
    public TonalPalette Error { get; }

    private CorePalettes(
        TonalPalette primary,
        TonalPalette secondary,
        TonalPalette tertiary,
        TonalPalette neutral,
        TonalPalette neutralVariant,
        TonalPalette error)
    {
        Primary = primary;
        Secondary = secondary;
        Tertiary = tertiary;
        Neutral = neutral;
        NeutralVariant = neutralVariant;
        Error = error;
    }

    /// <summary>
    /// Builds the core palettes for a source color and scheme variant, following
    /// DynamicSchemePalettesDelegateImpl2021 in dynamiccolor/dynamic_scheme.ts.
    /// </summary>
    public static CorePalettes Of(Hct sourceHct, SchemeVariant variant)
    {
        return new CorePalettes(
            GetPrimaryPalette(variant, sourceHct),
            GetSecondaryPalette(variant, sourceHct),
            GetTertiaryPalette(variant, sourceHct),
            GetNeutralPalette(variant, sourceHct),
            GetNeutralVariantPalette(variant, sourceHct),
            // In MCU, the error palette defaults to hue 25, chroma 84 for the 2021 spec.
            TonalPalette.FromHueAndChroma(25.0, 84.0));
    }

    /// <summary>
    /// Returns a shifted hue based on a piecewise function and input color hue.
    /// Port of DynamicScheme.getRotatedHue from dynamiccolor/dynamic_scheme.ts.
    /// </summary>
    /// <param name="sourceColorHct">The source color of the theme, in HCT.</param>
    /// <param name="hueBreakpoints">The hues at which a rotation should be applied.
    /// No default lower or upper bounds are assumed.</param>
    /// <param name="rotations">The rotation that should be applied when source color's
    /// hue is &gt;= the same index in the breakpoints array, and &lt; the hue at the
    /// next index. Otherwise, the source color's hue is returned.</param>
    public static double GetRotatedHue(
        Hct sourceColorHct, double[] hueBreakpoints, double[] rotations)
    {
        var rotation = GetPiecewiseHue(sourceColorHct, hueBreakpoints, rotations);
        if (Math.Min(hueBreakpoints.Length - 1, rotations.Length) <= 0)
        {
            // No condition matched, return the source hue.
            rotation = 0;
        }
        return MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + rotation);
    }

    /// <summary>
    /// Port of DynamicScheme.getPiecewiseHue from dynamiccolor/dynamic_scheme.ts.
    /// </summary>
    private static double GetPiecewiseHue(
        Hct sourceColorHct, double[] hueBreakpoints, double[] hues)
    {
        var size = Math.Min(hueBreakpoints.Length - 1, hues.Length);
        var sourceHue = sourceColorHct.Hue;
        for (var i = 0; i < size; i++)
        {
            if (sourceHue >= hueBreakpoints[i] && sourceHue < hueBreakpoints[i + 1])
            {
                return MathUtils.SanitizeDegreesDouble(hues[i]);
            }
        }
        // No condition matched, return the source hue.
        return sourceHue;
    }

    private static TonalPalette GetPrimaryPalette(SchemeVariant variant, Hct sourceColorHct)
    {
        return variant switch
        {
            SchemeVariant.Content or SchemeVariant.Fidelity =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, sourceColorHct.Chroma),
            SchemeVariant.FruitSalad =>
                TonalPalette.FromHueAndChroma(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue - 50.0), 48.0),
            SchemeVariant.Monochrome =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            SchemeVariant.Neutral =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 12.0),
            SchemeVariant.Rainbow =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 48.0),
            SchemeVariant.TonalSpot =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 36.0),
            SchemeVariant.Expressive =>
                TonalPalette.FromHueAndChroma(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 240), 40),
            SchemeVariant.Vibrant =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 200.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
        };
    }

    private static TonalPalette GetSecondaryPalette(SchemeVariant variant, Hct sourceColorHct)
    {
        return variant switch
        {
            SchemeVariant.Content or SchemeVariant.Fidelity =>
                TonalPalette.FromHueAndChroma(
                    sourceColorHct.Hue,
                    Math.Max(sourceColorHct.Chroma - 32.0, sourceColorHct.Chroma * 0.5)),
            SchemeVariant.FruitSalad =>
                TonalPalette.FromHueAndChroma(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue - 50.0), 36.0),
            SchemeVariant.Monochrome =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            SchemeVariant.Neutral =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 8.0),
            SchemeVariant.Rainbow =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            SchemeVariant.TonalSpot =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            SchemeVariant.Expressive =>
                TonalPalette.FromHueAndChroma(
                    GetRotatedHue(
                        sourceColorHct,
                        new double[] { 0, 21, 51, 121, 151, 191, 271, 321, 360 },
                        new double[] { 45, 95, 45, 20, 45, 90, 45, 45, 45 }),
                    24.0),
            SchemeVariant.Vibrant =>
                TonalPalette.FromHueAndChroma(
                    GetRotatedHue(
                        sourceColorHct,
                        new double[] { 0, 41, 61, 101, 131, 181, 251, 301, 360 },
                        new double[] { 18, 15, 10, 12, 15, 18, 15, 12, 12 }),
                    24.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
        };
    }

    private static TonalPalette GetTertiaryPalette(SchemeVariant variant, Hct sourceColorHct)
    {
        return variant switch
        {
            SchemeVariant.Content =>
                TonalPalette.FromHct(DislikeAnalyzer.FixIfDisliked(
                    new TemperatureCache(sourceColorHct)
                        .Analogous(count: 3, divisions: 6)[2])),
            SchemeVariant.Fidelity =>
                TonalPalette.FromHct(DislikeAnalyzer.FixIfDisliked(
                    new TemperatureCache(sourceColorHct).Complement)),
            SchemeVariant.FruitSalad =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 36.0),
            SchemeVariant.Monochrome =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            SchemeVariant.Neutral =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            SchemeVariant.Rainbow or SchemeVariant.TonalSpot =>
                TonalPalette.FromHueAndChroma(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 60.0), 24.0),
            SchemeVariant.Expressive =>
                TonalPalette.FromHueAndChroma(
                    GetRotatedHue(
                        sourceColorHct,
                        new double[] { 0, 21, 51, 121, 151, 191, 271, 321, 360 },
                        new double[] { 120, 120, 20, 45, 20, 15, 20, 120, 120 }),
                    32.0),
            SchemeVariant.Vibrant =>
                TonalPalette.FromHueAndChroma(
                    GetRotatedHue(
                        sourceColorHct,
                        new double[] { 0, 41, 61, 101, 131, 181, 251, 301, 360 },
                        new double[] { 35, 30, 20, 25, 30, 35, 30, 25, 25 }),
                    32.0),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
        };
    }

    private static TonalPalette GetNeutralPalette(SchemeVariant variant, Hct sourceColorHct)
    {
        return variant switch
        {
            SchemeVariant.Content or SchemeVariant.Fidelity =>
                TonalPalette.FromHueAndChroma(
                    sourceColorHct.Hue, sourceColorHct.Chroma / 8.0),
            SchemeVariant.FruitSalad =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 10.0),
            SchemeVariant.Monochrome =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            SchemeVariant.Neutral =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 2.0),
            SchemeVariant.Rainbow =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            SchemeVariant.TonalSpot =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 6.0),
            SchemeVariant.Expressive =>
                TonalPalette.FromHueAndChroma(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 15), 8),
            SchemeVariant.Vibrant =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 10),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
        };
    }

    private static TonalPalette GetNeutralVariantPalette(SchemeVariant variant, Hct sourceColorHct)
    {
        return variant switch
        {
            SchemeVariant.Content or SchemeVariant.Fidelity =>
                TonalPalette.FromHueAndChroma(
                    sourceColorHct.Hue, sourceColorHct.Chroma / 8.0 + 4.0),
            SchemeVariant.FruitSalad =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            SchemeVariant.Monochrome =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            SchemeVariant.Neutral =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 2.0),
            SchemeVariant.Rainbow =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 0.0),
            SchemeVariant.TonalSpot =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 8.0),
            SchemeVariant.Expressive =>
                TonalPalette.FromHueAndChroma(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 15), 12),
            SchemeVariant.Vibrant =>
                TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 12),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
        };
    }
}
