// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities), Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// Utility methods for mathematical operations.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// The signum function.
    /// </summary>
    /// <returns>1 if num &gt; 0, -1 if num &lt; 0, and 0 if num = 0</returns>
    public static int Signum(double num)
    {
        if (num < 0)
        {
            return -1;
        }
        else if (num == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    /// <summary>
    /// The linear interpolation function.
    /// </summary>
    /// <returns>start if amount = 0 and stop if amount = 1</returns>
    public static double Lerp(double start, double stop, double amount)
    {
        return (1.0 - amount) * start + amount * stop;
    }

    /// <summary>
    /// Clamps an integer between two integers.
    /// </summary>
    public static int ClampInt(int min, int max, int input)
    {
        if (input < min)
        {
            return min;
        }
        else if (input > max)
        {
            return max;
        }

        return input;
    }

    /// <summary>
    /// Clamps a floating-point number between two floating-point numbers.
    /// </summary>
    public static double ClampDouble(double min, double max, double input)
    {
        if (input < min)
        {
            return min;
        }
        else if (input > max)
        {
            return max;
        }

        return input;
    }

    /// <summary>
    /// Sanitizes a degree measure as an integer.
    /// </summary>
    /// <returns>a degree measure between 0 (inclusive) and 360 (exclusive).</returns>
    public static int SanitizeDegreesInt(int degrees)
    {
        degrees = degrees % 360;
        if (degrees < 0)
        {
            degrees = degrees + 360;
        }
        return degrees;
    }

    /// <summary>
    /// Sanitizes a degree measure as a floating-point number.
    /// </summary>
    /// <returns>a degree measure between 0.0 (inclusive) and 360.0 (exclusive).</returns>
    public static double SanitizeDegreesDouble(double degrees)
    {
        degrees = degrees % 360.0;
        if (degrees < 0)
        {
            degrees = degrees + 360.0;
        }
        return degrees;
    }

    /// <summary>
    /// Sign of direction change needed to travel from one angle to another.
    /// </summary>
    /// <returns>
    /// -1 if decreasing from leads to the shortest travel distance,
    /// 1 if increasing from leads to the shortest travel distance.
    /// </returns>
    public static double RotationDirection(double from, double to)
    {
        var increasingDifference = SanitizeDegreesDouble(to - from);
        return increasingDifference <= 180.0 ? 1.0 : -1.0;
    }

    /// <summary>
    /// Distance of two points on a circle, represented using degrees.
    /// </summary>
    public static double DifferenceDegrees(double a, double b)
    {
        return 180.0 - Math.Abs(Math.Abs(a - b) - 180.0);
    }

    /// <summary>
    /// Multiplies a 1x3 row vector with a 3x3 matrix.
    /// </summary>
    public static double[] MatrixMultiply(double[] row, double[][] matrix)
    {
        var a =
            row[0] * matrix[0][0] + row[1] * matrix[0][1] + row[2] * matrix[0][2];
        var b =
            row[0] * matrix[1][0] + row[1] * matrix[1][1] + row[2] * matrix[1][2];
        var c =
            row[0] * matrix[2][0] + row[1] * matrix[2][1] + row[2] * matrix[2][2];
        return new[] { a, b, c };
    }
}
