// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities),
// dislike/dislike_analyzer.ts, Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// Check and/or fix universally disliked colors.
/// Color science studies of color preference indicate universal distaste for
/// dark yellow-greens, and also show this is correlated to distaste for
/// biological waste and rotting food.
///
/// See Palmer and Schloss, 2010 or Schloss and Palmer's Chapter 21 in Handbook
/// of Color Psychology (2015).
/// </summary>
public static class DislikeAnalyzer
{
    /// <summary>
    /// Returns true if a color is disliked.
    /// Disliked is defined as a dark yellow-green that is not neutral.
    /// </summary>
    public static bool IsDisliked(Hct hct)
    {
        // JS Math.round(x) == floor(x + 0.5).
        var huePasses = Math.Floor(hct.Hue + 0.5) >= 90.0 &&
                        Math.Floor(hct.Hue + 0.5) <= 111.0;
        var chromaPasses = Math.Floor(hct.Chroma + 0.5) > 16.0;
        var tonePasses = Math.Floor(hct.Tone + 0.5) < 65.0;

        return huePasses && chromaPasses && tonePasses;
    }

    /// <summary>
    /// If a color is disliked, lighten it to make it likable.
    /// </summary>
    public static Hct FixIfDisliked(Hct hct)
    {
        if (IsDisliked(hct))
        {
            return Hct.From(hct.Hue, hct.Chroma, 70.0);
        }

        return hct;
    }
}
