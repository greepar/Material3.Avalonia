// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities), Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// Color science utilities.
///
/// Utility methods for color science constants and color space
/// conversions that aren't HCT or CAM16.
/// </summary>
public static class ColorUtils
{
    private static readonly double[][] SrgbToXyz =
    {
        new[] { 0.41233895, 0.35762064, 0.18051042 },
        new[] { 0.2126, 0.7152, 0.0722 },
        new[] { 0.01932141, 0.11916382, 0.95034478 },
    };

    private static readonly double[][] XyzToSrgb =
    {
        new[]
        {
            3.2413774792388685,
            -1.5376652402851851,
            -0.49885366846268053,
        },
        new[]
        {
            -0.9691452513005321,
            1.8758853451067872,
            0.04156585616912061,
        },
        new[]
        {
            0.05562093689691305,
            -0.20395524564742123,
            1.0571799111220335,
        },
    };

    private static readonly double[] WhitePointD65Values = { 95.047, 100.0, 108.883 };

    /// <summary>
    /// Converts a color from RGB components to ARGB format.
    /// </summary>
    public static uint ArgbFromRgb(int red, int green, int blue)
    {
        return (uint)(255 << 24 | (red & 255) << 16 | (green & 255) << 8 | (blue & 255));
    }

    /// <summary>
    /// Converts a color from linear RGB components to ARGB format.
    /// </summary>
    public static uint ArgbFromLinrgb(double[] linrgb)
    {
        var r = Delinearized(linrgb[0]);
        var g = Delinearized(linrgb[1]);
        var b = Delinearized(linrgb[2]);
        return ArgbFromRgb(r, g, b);
    }

    /// <summary>
    /// Returns the alpha component of a color in ARGB format.
    /// </summary>
    public static int AlphaFromArgb(uint argb)
    {
        return (int)(argb >> 24 & 255);
    }

    /// <summary>
    /// Returns the red component of a color in ARGB format.
    /// </summary>
    public static int RedFromArgb(uint argb)
    {
        return (int)(argb >> 16 & 255);
    }

    /// <summary>
    /// Returns the green component of a color in ARGB format.
    /// </summary>
    public static int GreenFromArgb(uint argb)
    {
        return (int)(argb >> 8 & 255);
    }

    /// <summary>
    /// Returns the blue component of a color in ARGB format.
    /// </summary>
    public static int BlueFromArgb(uint argb)
    {
        return (int)(argb & 255);
    }

    /// <summary>
    /// Returns whether a color in ARGB format is opaque.
    /// </summary>
    public static bool IsOpaque(uint argb)
    {
        return AlphaFromArgb(argb) >= 255;
    }

    /// <summary>
    /// Converts a color from XYZ to ARGB.
    /// </summary>
    public static uint ArgbFromXyz(double x, double y, double z)
    {
        var matrix = XyzToSrgb;
        var linearR = matrix[0][0] * x + matrix[0][1] * y + matrix[0][2] * z;
        var linearG = matrix[1][0] * x + matrix[1][1] * y + matrix[1][2] * z;
        var linearB = matrix[2][0] * x + matrix[2][1] * y + matrix[2][2] * z;
        var r = Delinearized(linearR);
        var g = Delinearized(linearG);
        var b = Delinearized(linearB);
        return ArgbFromRgb(r, g, b);
    }

    /// <summary>
    /// Converts a color from ARGB to XYZ.
    /// </summary>
    public static double[] XyzFromArgb(uint argb)
    {
        var r = Linearized(RedFromArgb(argb));
        var g = Linearized(GreenFromArgb(argb));
        var b = Linearized(BlueFromArgb(argb));
        return MathUtils.MatrixMultiply(new[] { r, g, b }, SrgbToXyz);
    }

    /// <summary>
    /// Converts a color represented in Lab color space into an ARGB integer.
    /// </summary>
    public static uint ArgbFromLab(double l, double a, double b)
    {
        var whitePoint = WhitePointD65Values;
        var fy = (l + 16.0) / 116.0;
        var fx = a / 500.0 + fy;
        var fz = fy - b / 200.0;
        var xNormalized = LabInvf(fx);
        var yNormalized = LabInvf(fy);
        var zNormalized = LabInvf(fz);
        var x = xNormalized * whitePoint[0];
        var y = yNormalized * whitePoint[1];
        var z = zNormalized * whitePoint[2];
        return ArgbFromXyz(x, y, z);
    }

    /// <summary>
    /// Converts a color from ARGB representation to L*a*b* representation.
    /// </summary>
    /// <param name="argb">the ARGB representation of a color</param>
    /// <returns>an array of [L*, a*, b*] representing the color</returns>
    public static double[] LabFromArgb(uint argb)
    {
        var linearR = Linearized(RedFromArgb(argb));
        var linearG = Linearized(GreenFromArgb(argb));
        var linearB = Linearized(BlueFromArgb(argb));
        var matrix = SrgbToXyz;
        var x =
            matrix[0][0] * linearR + matrix[0][1] * linearG + matrix[0][2] * linearB;
        var y =
            matrix[1][0] * linearR + matrix[1][1] * linearG + matrix[1][2] * linearB;
        var z =
            matrix[2][0] * linearR + matrix[2][1] * linearG + matrix[2][2] * linearB;
        var whitePoint = WhitePointD65Values;
        var xNormalized = x / whitePoint[0];
        var yNormalized = y / whitePoint[1];
        var zNormalized = z / whitePoint[2];
        var fx = LabF(xNormalized);
        var fy = LabF(yNormalized);
        var fz = LabF(zNormalized);
        var l = 116.0 * fy - 16;
        var a = 500.0 * (fx - fy);
        var b = 200.0 * (fy - fz);
        return new[] { l, a, b };
    }

    /// <summary>
    /// Converts an L* value to an ARGB representation.
    /// </summary>
    /// <param name="lstar">L* in L*a*b*</param>
    /// <returns>ARGB representation of grayscale color with lightness matching L*</returns>
    public static uint ArgbFromLstar(double lstar)
    {
        var y = YFromLstar(lstar);
        var component = Delinearized(y);
        return ArgbFromRgb(component, component, component);
    }

    /// <summary>
    /// Computes the L* value of a color in ARGB representation.
    /// </summary>
    /// <param name="argb">ARGB representation of a color</param>
    /// <returns>L*, from L*a*b*, coordinate of the color</returns>
    public static double LstarFromArgb(uint argb)
    {
        var y = XyzFromArgb(argb)[1];
        return 116.0 * LabF(y / 100.0) - 16.0;
    }

    /// <summary>
    /// Converts an L* value to a Y value.
    ///
    /// L* in L*a*b* and Y in XYZ measure the same quantity, luminance.
    /// </summary>
    /// <param name="lstar">L* in L*a*b*</param>
    /// <returns>Y in XYZ</returns>
    public static double YFromLstar(double lstar)
    {
        return 100.0 * LabInvf((lstar + 16.0) / 116.0);
    }

    /// <summary>
    /// Converts a Y value to an L* value.
    /// </summary>
    /// <param name="y">Y in XYZ</param>
    /// <returns>L* in L*a*b*</returns>
    public static double LstarFromY(double y)
    {
        return LabF(y / 100.0) * 116.0 - 16.0;
    }

    /// <summary>
    /// Linearizes an RGB component.
    /// </summary>
    /// <param name="rgbComponent">0 &lt;= rgb_component &lt;= 255, represents R/G/B channel</param>
    /// <returns>0.0 &lt;= output &lt;= 100.0, color channel converted to linear RGB space</returns>
    public static double Linearized(int rgbComponent)
    {
        var normalized = rgbComponent / 255.0;
        if (normalized <= 0.040449936)
        {
            return normalized / 12.92 * 100.0;
        }
        else
        {
            return Math.Pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
        }
    }

    /// <summary>
    /// Delinearizes an RGB component.
    /// </summary>
    /// <param name="rgbComponent">0.0 &lt;= rgb_component &lt;= 100.0, represents linear R/G/B channel</param>
    /// <returns>0 &lt;= output &lt;= 255, color channel converted to regular RGB space</returns>
    public static int Delinearized(double rgbComponent)
    {
        var normalized = rgbComponent / 100.0;
        var delinearized = 0.0;
        if (normalized <= 0.0031308)
        {
            delinearized = normalized * 12.92;
        }
        else
        {
            delinearized = 1.055 * Math.Pow(normalized, 1.0 / 2.4) - 0.055;
        }
        // JS Math.round(x) == floor(x + 0.5); C# Math.Round uses banker's rounding.
        return MathUtils.ClampInt(0, 255, (int)Math.Floor(delinearized * 255.0 + 0.5));
    }

    /// <summary>
    /// Returns the standard white point; white on a sunny day.
    /// </summary>
    public static double[] WhitePointD65()
    {
        return WhitePointD65Values;
    }

    private static double LabF(double t)
    {
        var e = 216.0 / 24389.0;
        var kappa = 24389.0 / 27.0;
        if (t > e)
        {
            return Math.Pow(t, 1.0 / 3.0);
        }
        else
        {
            return (kappa * t + 16) / 116;
        }
    }

    private static double LabInvf(double ft)
    {
        var e = 216.0 / 24389.0;
        var kappa = 24389.0 / 27.0;
        var ft3 = ft * ft * ft;
        if (ft3 > e)
        {
            return ft3;
        }
        else
        {
            return (116 * ft - 16) / kappa;
        }
    }
}
