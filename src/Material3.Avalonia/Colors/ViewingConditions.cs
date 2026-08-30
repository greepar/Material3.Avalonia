// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities), Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// In traditional color spaces, a color can be identified solely by the
/// observer's measurement of the color. Color appearance models such as CAM16
/// also use information about the environment where the color was
/// observed, known as the viewing conditions.
///
/// For example, white under the traditional assumption of a midday sun white
/// point is accurately measured as a slightly chromatic blue by CAM16. (roughly,
/// hue 203, chroma 3, lightness 100)
///
/// This class caches intermediate values of the CAM16 conversion process that
/// depend only on viewing conditions, enabling speed ups.
/// </summary>
public sealed class ViewingConditions
{
    private readonly double[] _rgbD;

    /// <summary>sRGB-like viewing conditions.</summary>
    public static readonly ViewingConditions Default = Make();

    public double N { get; }
    public double Aw { get; }
    public double Nbb { get; }
    public double Ncb { get; }
    public double C { get; }
    public double Nc { get; }
    public double[] RgbD => (double[])_rgbD.Clone();
    public double Fl { get; }
    public double FLRoot { get; }
    public double Z { get; }

    /// <summary>
    /// Create ViewingConditions from a simple, physically relevant, set of
    /// parameters.
    /// </summary>
    /// <param name="whitePoint">White point, measured in the XYZ color space.
    ///     default = D65, or sunny day afternoon</param>
    /// <param name="adaptingLuminance">The luminance of the adapting field. Informally,
    ///     how bright it is in the room where the color is viewed. Can be
    ///     calculated from lux by multiplying lux by 0.0586. default = 11.72,
    ///     or 200 lux.</param>
    /// <param name="backgroundLstar">The lightness of the area surrounding the color.
    ///     measured by L* in L*a*b*. default = 50.0</param>
    /// <param name="surround">A general description of the lighting surrounding the
    ///     color. 0 is pitch dark, like watching a movie in a theater. 1.0 is a
    ///     dimly light room, like watching TV at home at night. 2.0 means there
    ///     is no difference between the lighting on the color and around it.
    ///     default = 2.0</param>
    /// <param name="discountingIlluminant">Whether the eye accounts for the tint of the
    ///     ambient lighting, such as knowing an apple is still red in green light.
    ///     default = false, the eye does not perform this process on
    ///     self-luminous objects like displays.</param>
    public static ViewingConditions Make(
        double[]? whitePoint = null,
        double? adaptingLuminance = null,
        double backgroundLstar = 50.0,
        double surround = 2.0,
        bool discountingIlluminant = false)
    {
        whitePoint = whitePoint is null
            ? ColorUtils.WhitePointD65()
            : (double[])whitePoint.Clone();
        var adaptingLuminanceValue =
            adaptingLuminance ?? (200.0 / Math.PI) * ColorUtils.YFromLstar(50.0) / 100.0;

        var xyz = whitePoint;
        var rW = xyz[0] * 0.401288 + xyz[1] * 0.650173 + xyz[2] * -0.051461;
        var gW = xyz[0] * -0.250268 + xyz[1] * 1.204414 + xyz[2] * 0.045854;
        var bW = xyz[0] * -0.002079 + xyz[1] * 0.048952 + xyz[2] * 0.953127;
        var f = 0.8 + surround / 10.0;
        var c = f >= 0.9
            ? MathUtils.Lerp(0.59, 0.69, (f - 0.9) * 10.0)
            : MathUtils.Lerp(0.525, 0.59, (f - 0.8) * 10.0);
        var d = discountingIlluminant
            ? 1.0
            : f * (1.0 - (1.0 / 3.6) * Math.Exp((-adaptingLuminanceValue - 42.0) / 92.0));
        d = d > 1.0 ? 1.0 : d < 0.0 ? 0.0 : d;
        var nc = f;
        var rgbD = new[]
        {
            d * (100.0 / rW) + 1.0 - d,
            d * (100.0 / gW) + 1.0 - d,
            d * (100.0 / bW) + 1.0 - d,
        };
        var k = 1.0 / (5.0 * adaptingLuminanceValue + 1.0);
        var k4 = k * k * k * k;
        var k4F = 1.0 - k4;
        var fl = k4 * adaptingLuminanceValue +
            0.1 * k4F * k4F * Math.Cbrt(5.0 * adaptingLuminanceValue);
        var n = ColorUtils.YFromLstar(backgroundLstar) / whitePoint[1];
        var z = 1.48 + Math.Sqrt(n);
        var nbb = 0.725 / Math.Pow(n, 0.2);
        var ncb = nbb;
        var rgbAFactors = new[]
        {
            Math.Pow((fl * rgbD[0] * rW) / 100.0, 0.42),
            Math.Pow((fl * rgbD[1] * gW) / 100.0, 0.42),
            Math.Pow((fl * rgbD[2] * bW) / 100.0, 0.42),
        };
        var rgbA = new[]
        {
            (400.0 * rgbAFactors[0]) / (rgbAFactors[0] + 27.13),
            (400.0 * rgbAFactors[1]) / (rgbAFactors[1] + 27.13),
            (400.0 * rgbAFactors[2]) / (rgbAFactors[2] + 27.13),
        };
        var aw = (2.0 * rgbA[0] + rgbA[1] + 0.05 * rgbA[2]) * nbb;
        return new ViewingConditions(
            n, aw, nbb, ncb, c, nc, rgbD, fl, Math.Pow(fl, 0.25), z);
    }

    /// <summary>
    /// Parameters are intermediate values of the CAM16 conversion process. Their
    /// names are shorthand for technical color science terminology, this class
    /// would not benefit from documenting them individually. A brief overview
    /// is available in the CAM16 specification, and a complete overview requires
    /// a color science textbook, such as Fairchild's Color Appearance Models.
    /// </summary>
    private ViewingConditions(
        double n, double aw, double nbb, double ncb, double c, double nc,
        double[] rgbD, double fl, double fLRoot, double z)
    {
        N = n;
        Aw = aw;
        Nbb = nbb;
        Ncb = ncb;
        C = c;
        Nc = nc;
        _rgbD = (double[])rgbD.Clone();
        Fl = fl;
        FLRoot = fLRoot;
        Z = z;
    }

    internal double GetRgbD(int index) => _rgbD[index];
}
