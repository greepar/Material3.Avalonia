// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities), Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// A convenience class for retrieving colors that are constant in hue and
/// chroma, but vary in tone.
/// </summary>
public sealed class TonalPalette
{
    private readonly Dictionary<int, uint> _cache = new();

    public double Hue { get; }
    public double Chroma { get; }
    public Hct KeyColor { get; }

    /// <summary>
    /// </summary>
    /// <param name="argb">ARGB representation of a color</param>
    /// <returns>Tones matching that color's hue and chroma.</returns>
    public static TonalPalette FromInt(uint argb)
    {
        var hct = Hct.FromInt(argb);
        return FromHct(hct);
    }

    /// <summary>
    /// </summary>
    /// <param name="hct">Hct</param>
    /// <returns>Tones matching that color's hue and chroma.</returns>
    public static TonalPalette FromHct(Hct hct)
    {
        return new TonalPalette(hct.Hue, hct.Chroma, hct);
    }

    /// <summary>
    /// </summary>
    /// <param name="hue">HCT hue</param>
    /// <param name="chroma">HCT chroma</param>
    /// <returns>Tones matching hue and chroma.</returns>
    public static TonalPalette FromHueAndChroma(double hue, double chroma)
    {
        var keyColor = new KeyColorGenerator(hue, chroma).Create();
        return new TonalPalette(hue, chroma, keyColor);
    }

    private TonalPalette(double hue, double chroma, Hct keyColor)
    {
        Hue = hue;
        Chroma = chroma;
        KeyColor = keyColor;
    }

    /// <summary>
    /// </summary>
    /// <param name="tone">HCT tone, measured from 0 to 100.</param>
    /// <returns>ARGB representation of a color with that tone.</returns>
    public uint Tone(int tone)
    {
        if (!_cache.TryGetValue(tone, out var argb))
        {
            if (tone == 99 && Hct.IsYellow(Hue))
            {
                argb = AverageArgb(Tone(98), Tone(100));
            }
            else
            {
                argb = Hct.From(Hue, Chroma, tone).ToInt();
            }
            _cache[tone] = argb;
        }
        return argb;
    }

    /// <summary>
    /// </summary>
    /// <param name="tone">HCT tone.</param>
    /// <returns>HCT representation of a color with that tone.</returns>
    public Hct GetHct(int tone)
    {
        return Hct.FromInt(Tone(tone));
    }

    private static uint AverageArgb(uint argb1, uint argb2)
    {
        var red1 = (int)((argb1 >> 16) & 0xff);
        var green1 = (int)((argb1 >> 8) & 0xff);
        var blue1 = (int)(argb1 & 0xff);
        var red2 = (int)((argb2 >> 16) & 0xff);
        var green2 = (int)((argb2 >> 8) & 0xff);
        var blue2 = (int)(argb2 & 0xff);
        // JS Math.round(x) == floor(x + 0.5)
        var red = (int)Math.Floor((red1 + red2) / 2.0 + 0.5);
        var green = (int)Math.Floor((green1 + green2) / 2.0 + 0.5);
        var blue = (int)Math.Floor((blue1 + blue2) / 2.0 + 0.5);
        return (uint)(255 << 24 | (red & 255) << 16 | (green & 255) << 8 | (blue & 255));
    }

    /// <summary>
    /// Key color is a color that represents the hue and chroma of a tonal palette.
    /// (Named KeyColor in the TypeScript source; renamed to avoid clashing with the
    /// KeyColor property.)
    /// </summary>
    private sealed class KeyColorGenerator
    {
        // Cache that maps tone to max chroma to avoid duplicated HCT calculation.
        private readonly Dictionary<int, double> _chromaCache = new();
        private const double MaxChromaValue = 200.0;

        private readonly double _hue;
        private readonly double _requestedChroma;

        public KeyColorGenerator(double hue, double requestedChroma)
        {
            _hue = hue;
            _requestedChroma = requestedChroma;
        }

        /// <summary>
        /// Creates a key color from a hue and a chroma.
        /// The key color is the first tone, starting from T50, matching the given hue
        /// and chroma.
        /// </summary>
        /// <returns>Key color Hct</returns>
        public Hct Create()
        {
            // Pivot around T50 because T50 has the most chroma available, on
            // average. Thus it is most likely to have a direct answer.
            const int pivotTone = 50;
            const int toneStepSize = 1;
            // Epsilon to accept values slightly higher than the requested chroma.
            const double epsilon = 0.01;

            // Binary search to find the tone that can provide a chroma that is closest
            // to the requested chroma.
            var lowerTone = 0;
            var upperTone = 100;
            while (lowerTone < upperTone)
            {
                var midTone = (lowerTone + upperTone) / 2;
                var isAscending =
                    MaxChroma(midTone) < MaxChroma(midTone + toneStepSize);
                var sufficientChroma =
                    MaxChroma(midTone) >= _requestedChroma - epsilon;

                if (sufficientChroma)
                {
                    // Either range [lowerTone, midTone] or [midTone, upperTone] has
                    // the answer, so search in the range that is closer the pivot tone.
                    if (Math.Abs(lowerTone - pivotTone) < Math.Abs(upperTone - pivotTone))
                    {
                        upperTone = midTone;
                    }
                    else
                    {
                        if (lowerTone == midTone)
                        {
                            return Hct.From(_hue, _requestedChroma, lowerTone);
                        }
                        lowerTone = midTone;
                    }
                }
                else
                {
                    // As there is no sufficient chroma in the midTone, follow the direction
                    // to the chroma peak.
                    if (isAscending)
                    {
                        lowerTone = midTone + toneStepSize;
                    }
                    else
                    {
                        // Keep midTone for potential chroma peak.
                        upperTone = midTone;
                    }
                }
            }

            return Hct.From(_hue, _requestedChroma, lowerTone);
        }

        // Find the maximum chroma for a given tone
        private double MaxChroma(int tone)
        {
            if (_chromaCache.TryGetValue(tone, out var cached))
            {
                return cached;
            }
            var chroma = Hct.From(_hue, MaxChromaValue, tone).Chroma;
            _chromaCache[tone] = chroma;
            return chroma;
        }
    }
}
