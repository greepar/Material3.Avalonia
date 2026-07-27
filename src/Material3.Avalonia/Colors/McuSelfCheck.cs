// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities), Apache-2.0.
// Self-check assertions for the C# port. Not executed automatically; call Verify() from tests.

namespace Material3.Avalonia.Colors;

internal static class McuSelfCheck
{
    /// <summary>
    /// Runs a set of sanity assertions against known material-color-utilities values.
    /// Throws <see cref="InvalidOperationException"/> on failure.
    /// </summary>
    internal static void Verify()
    {
        // 1. White has L* ~= 100, black has L* ~= 0.
        AssertNear(ColorUtils.LstarFromArgb(0xFFFFFFFF), 100.0, 0.01, "LstarFromArgb(white)");
        AssertNear(ColorUtils.LstarFromArgb(0xFF000000), 0.0, 0.01, "LstarFromArgb(black)");

        // 2. ARGB channel round-trip.
        Assert(ColorUtils.ArgbFromRgb(255, 0, 0) == 0xFFFF0000, "ArgbFromRgb red");
        Assert(ColorUtils.RedFromArgb(0xFF123456) == 0x12, "RedFromArgb");
        Assert(ColorUtils.GreenFromArgb(0xFF123456) == 0x34, "GreenFromArgb");
        Assert(ColorUtils.BlueFromArgb(0xFF123456) == 0x56, "BlueFromArgb");
        Assert(ColorUtils.AlphaFromArgb(0xFF123456) == 0xFF, "AlphaFromArgb");

        // 3. Hct.From(25, 84, 40): red-ish; tone must round-trip within 2.
        var red = Hct.From(25, 84, 40);
        AssertNear(red.Tone, 40, 2.0, "Hct.From(25,84,40).Tone");
        AssertNear(red.Hue, 25, 2.0, "Hct.From(25,84,40).Hue");
        var redArgb = red.ToInt();
        Assert(ColorUtils.AlphaFromArgb(redArgb) == 255, "red argb opaque");
        Assert(
            ColorUtils.RedFromArgb(redArgb) > ColorUtils.BlueFromArgb(redArgb) &&
            ColorUtils.RedFromArgb(redArgb) > ColorUtils.GreenFromArgb(redArgb),
            "Hct.From(25,84,40) should be red-dominant");

        // 4. Hct round-trip from an ARGB value.
        var blue = Hct.FromInt(0xFF0000FF);
        var blueRoundTrip = Hct.From(blue.Hue, blue.Chroma, blue.Tone);
        Assert(blueRoundTrip.ToInt() == 0xFF0000FF, "Hct round-trip of pure blue");

        // 5. TonalPalette.FromHueAndChroma(258, 36).Tone(40): blue-purple.
        var palette = TonalPalette.FromHueAndChroma(258, 36);
        var t40 = palette.Tone(40);
        Assert(ColorUtils.AlphaFromArgb(t40) == 255, "palette tone 40 opaque");
        AssertNear(ColorUtils.LstarFromArgb(t40), 40, 2.0, "palette tone 40 L*");
        Assert(
            ColorUtils.BlueFromArgb(t40) > ColorUtils.RedFromArgb(t40) &&
            ColorUtils.BlueFromArgb(t40) > ColorUtils.GreenFromArgb(t40),
            "palette(258, 36).Tone(40) should be blue-dominant");
        // Tone(0) is black, Tone(100) is white.
        Assert(palette.Tone(0) == 0xFF000000, "palette tone 0 is black");
        Assert(palette.Tone(100) == 0xFFFFFFFF, "palette tone 100 is white");
        // Cache returns identical values.
        Assert(palette.Tone(40) == t40, "palette tone cache");

        // 6. Y/L* round-trip.
        AssertNear(ColorUtils.LstarFromY(ColorUtils.YFromLstar(50.0)), 50.0, 1e-9, "Y/L* round-trip");

        // 7. Cam16 UCS round-trip: distance to itself is 0.
        var cam = Cam16.FromInt(0xFF6750A4);
        Assert(cam.Distance(Cam16.FromUcs(cam.Jstar, cam.Astar, cam.Bstar)) < 0.1, "Cam16 UCS round-trip");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"McuSelfCheck failed: {message}");
        }
    }

    private static void AssertNear(double actual, double expected, double tolerance, string message)
    {
        if (Math.Abs(actual - expected) > tolerance)
        {
            throw new InvalidOperationException(
                $"McuSelfCheck failed: {message}: expected {expected} ± {tolerance}, got {actual}");
        }
    }
}
