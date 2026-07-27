// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities), Apache-2.0.

using System.Globalization;

namespace Material3.Avalonia.Colors;

/// <summary>
/// HCT, hue, chroma, and tone. A color system that provides a perceptually
/// accurate color measurement system that can also accurately render what colors
/// will appear as in different lighting environments.
///
/// A color system built using CAM16 hue and chroma, and L* from L*a*b*.
///
/// Using L* creates a link between the color system, contrast, and thus
/// accessibility. Contrast ratio depends on relative luminance, or Y in the XYZ
/// color space. L*, or perceptual luminance can be calculated from Y.
///
/// Unlike Y, L* is linear to human perception, allowing trivial creation of
/// accurate color tones.
///
/// Unlike contrast ratio, measuring contrast in L* is linear, and simple to
/// calculate. A difference of 40 in HCT tone guarantees a contrast ratio >= 3.0,
/// and a difference of 50 guarantees a contrast ratio >= 4.5.
/// </summary>
public sealed class Hct
{
    private uint _argb;
    private double _internalHue;
    private double _internalChroma;
    private double _internalTone;

    /// <summary>
    /// </summary>
    /// <param name="hue">0 &lt;= hue &lt; 360; invalid values are corrected.</param>
    /// <param name="chroma">0 &lt;= chroma &lt; ?; Informally, colorfulness. The color
    ///     returned may be lower than the requested chroma. Chroma has a different
    ///     maximum for any given hue and tone.</param>
    /// <param name="tone">0 &lt;= tone &lt;= 100; invalid values are corrected.</param>
    /// <returns>HCT representation of a color in default viewing conditions.</returns>
    public static Hct From(double hue, double chroma, double tone)
    {
        return new Hct(HctSolver.SolveToInt(hue, chroma, tone));
    }

    /// <summary>
    /// </summary>
    /// <param name="argb">ARGB representation of a color.</param>
    /// <returns>HCT representation of a color in default viewing conditions</returns>
    public static Hct FromInt(uint argb)
    {
        return new Hct(argb);
    }

    public uint ToInt()
    {
        return _argb;
    }

    /// <summary>
    /// A number, in degrees, representing ex. red, orange, yellow, etc.
    /// Ranges from 0 &lt;= hue &lt; 360.
    ///
    /// When set: 0 &lt;= value &lt; 360; invalid values are corrected.
    /// Chroma may decrease because chroma has a different maximum for any given
    /// hue and tone.
    /// </summary>
    public double Hue
    {
        get => _internalHue;
        set => SetInternalState(
            HctSolver.SolveToInt(value, _internalChroma, _internalTone));
    }

    /// <summary>
    /// Informally, colorfulness.
    ///
    /// When set: 0 &lt;= value &lt; ?
    /// Chroma may decrease because chroma has a different maximum for any given
    /// hue and tone.
    /// </summary>
    public double Chroma
    {
        get => _internalChroma;
        set => SetInternalState(
            HctSolver.SolveToInt(_internalHue, value, _internalTone));
    }

    /// <summary>
    /// Lightness. Ranges from 0 to 100.
    ///
    /// When set: 0 &lt;= value &lt;= 100; invalid values are corrected.
    /// Chroma may decrease because chroma has a different maximum for any given
    /// hue and tone.
    /// </summary>
    public double Tone
    {
        get => _internalTone;
        set => SetInternalState(
            HctSolver.SolveToInt(_internalHue, _internalChroma, value));
    }

    public override string ToString()
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"HCT({Hue:F0}, {Chroma:F0}, {Tone:F0})");
    }

    public static bool IsBlue(double hue)
    {
        return hue >= 250 && hue < 270;
    }

    public static bool IsYellow(double hue)
    {
        return hue >= 105 && hue < 125;
    }

    public static bool IsCyan(double hue)
    {
        return hue >= 170 && hue < 207;
    }

    private Hct(uint argb)
    {
        SetInternalState(argb);
    }

    private void SetInternalState(uint argb)
    {
        var cam = Cam16.FromInt(argb);
        _internalHue = cam.Hue;
        _internalChroma = cam.Chroma;
        _internalTone = ColorUtils.LstarFromArgb(argb);
        _argb = argb;
    }

    /// <summary>
    /// Translates a color into different ViewingConditions.
    ///
    /// Colors change appearance. They look different with lights on versus off,
    /// the same color, as in hex code, on white looks different when on black.
    /// This is called color relativity, most famously explicated by Josef Albers
    /// in Interaction of Color.
    ///
    /// In color science, color appearance models can account for this and
    /// calculate the appearance of a color in different settings. HCT is based on
    /// CAM16, a color appearance model, and uses it to make these calculations.
    ///
    /// See ViewingConditions.Make for parameters affecting color appearance.
    /// </summary>
    public Hct InViewingConditions(ViewingConditions vc)
    {
        // 1. Use CAM16 to find XYZ coordinates of color in specified VC.
        var cam = Cam16.FromInt(ToInt());
        var viewedInVc = cam.XyzInViewingConditions(vc);

        // 2. Create CAM16 of those XYZ coordinates in default VC.
        var recastInVc = Cam16.FromXyzInViewingConditions(
            viewedInVc[0],
            viewedInVc[1],
            viewedInVc[2],
            ViewingConditions.Make());

        // 3. Create HCT from:
        // - CAM16 using default VC with XYZ coordinates in specified VC.
        // - L* converted from Y in XYZ coordinates in specified VC.
        var recastHct = From(
            recastInVc.Hue,
            recastInVc.Chroma,
            ColorUtils.LstarFromY(viewedInVc[1]));
        return recastHct;
    }
}
