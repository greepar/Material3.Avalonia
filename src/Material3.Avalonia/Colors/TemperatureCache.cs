// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities),
// temperature/temperature_cache.ts, Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// Design utilities using color temperature theory.
///
/// Analogous colors, complementary color, and cache to efficiently, lazily,
/// generate data for calculations when needed.
/// </summary>
public sealed class TemperatureCache
{
    public Hct Input { get; }

    private Hct[] _hctsByTempCache = Array.Empty<Hct>();
    private Hct[] _hctsByHueCache = Array.Empty<Hct>();
    private Dictionary<Hct, double> _tempsByHctCache = new();
    private double _inputRelativeTemperatureCache = -1.0;
    private Hct? _complementCache;

    public TemperatureCache(Hct input)
    {
        Input = input;
    }

    public Hct[] HctsByTemp
    {
        get
        {
            if (_hctsByTempCache.Length > 0)
            {
                return _hctsByTempCache;
            }

            var hcts = new List<Hct>(HctsByHue) { Input };
            var temperaturesByHct = TempsByHct;
            // JS Array.sort is stable; List.Sort is not, so use OrderBy (stable).
            var sorted = hcts.OrderBy(h => temperaturesByHct[h]).ToArray();
            _hctsByTempCache = sorted;
            return sorted;
        }
    }

    public Hct Warmest => HctsByTemp[HctsByTemp.Length - 1];

    public Hct Coldest => HctsByTemp[0];

    /// <summary>
    /// A set of colors with differing hues, equidistant in temperature.
    ///
    /// In art, this is usually described as a set of 5 colors on a color wheel
    /// divided into 12 sections. This method allows provision of either of those
    /// values.
    ///
    /// Behavior is undefined when count or divisions is 0.
    /// When divisions &lt; count, colors repeat.
    /// </summary>
    /// <param name="count">The number of colors to return, includes the input color.</param>
    /// <param name="divisions">The number of divisions on the color wheel.</param>
    public List<Hct> Analogous(int count = 5, int divisions = 12)
    {
        // JS Math.round(x) == floor(x + 0.5); hue is non-negative.
        var startHue = (int)Math.Floor(Input.Hue + 0.5);
        var startHct = HctsByHue[startHue];
        var lastTemp = RelativeTemperature(startHct);
        var allColors = new List<Hct> { startHct };

        var absoluteTotalTempDelta = 0.0;
        for (var i = 0; i < 360; i++)
        {
            var hue = MathUtils.SanitizeDegreesInt(startHue + i);
            var hct = HctsByHue[hue];
            var temp = RelativeTemperature(hct);
            var tempDelta = Math.Abs(temp - lastTemp);
            lastTemp = temp;
            absoluteTotalTempDelta += tempDelta;
        }

        var hueAddend = 1;
        var tempStep = absoluteTotalTempDelta / divisions;
        var totalTempDelta = 0.0;
        lastTemp = RelativeTemperature(startHct);
        while (allColors.Count < divisions)
        {
            var hue = MathUtils.SanitizeDegreesInt(startHue + hueAddend);
            var hct = HctsByHue[hue];
            var temp = RelativeTemperature(hct);
            var tempDelta = Math.Abs(temp - lastTemp);
            totalTempDelta += tempDelta;

            var desiredTotalTempDeltaForIndex = allColors.Count * tempStep;
            var indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
            var indexAddend = 1;
            // Keep adding this hue to the answers until its temperature is
            // insufficient. This ensures consistent behavior when there aren't
            // [divisions] discrete steps between 0 and 360 in hue with [tempStep]
            // delta in temperature between them.
            //
            // For example, white and black have no analogues: there are no other
            // colors at T100/T0. Therefore, they should just be added to the array
            // as answers.
            while (indexSatisfied && allColors.Count < divisions)
            {
                allColors.Add(hct);
                desiredTotalTempDeltaForIndex =
                    (allColors.Count + indexAddend) * tempStep;
                indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
                indexAddend++;
            }

            lastTemp = temp;
            hueAddend++;
            if (hueAddend > 360)
            {
                while (allColors.Count < divisions)
                {
                    allColors.Add(hct);
                }
                break;
            }
        }

        var answers = new List<Hct> { Input };

        // First, generate analogues from rotating counter-clockwise.
        var increaseHueCount = (int)Math.Floor((count - 1) / 2.0);
        for (var i = 1; i < increaseHueCount + 1; i++)
        {
            var index = 0 - i;
            while (index < 0)
            {
                index = allColors.Count + index;
            }
            if (index >= allColors.Count)
            {
                index %= allColors.Count;
            }
            answers.Insert(0, allColors[index]);
        }

        // Second, generate analogues from rotating clockwise.
        var decreaseHueCount = count - increaseHueCount - 1;
        for (var i = 1; i < decreaseHueCount + 1; i++)
        {
            var index = i;
            while (index < 0)
            {
                index = allColors.Count + index;
            }
            if (index >= allColors.Count)
            {
                index %= allColors.Count;
            }
            answers.Add(allColors[index]);
        }

        return answers;
    }

    /// <summary>
    /// A color that complements the input color aesthetically.
    ///
    /// In art, this is usually described as being across the color wheel.
    /// History of this shows intent as a color that is just as cool-warm as the
    /// input color is warm-cool.
    /// </summary>
    public Hct Complement
    {
        get
        {
            if (_complementCache != null)
            {
                return _complementCache;
            }

            var coldestHue = Coldest.Hue;
            var coldestTemp = TempsByHct[Coldest];

            var warmestHue = Warmest.Hue;
            var warmestTemp = TempsByHct[Warmest];
            var range = warmestTemp - coldestTemp;
            var startHueIsColdestToWarmest =
                IsBetween(Input.Hue, coldestHue, warmestHue);
            var startHue = startHueIsColdestToWarmest ? warmestHue : coldestHue;
            var endHue = startHueIsColdestToWarmest ? coldestHue : warmestHue;
            const double directionOfRotation = 1.0;
            var smallestError = 1000.0;
            var answer = HctsByHue[(int)Math.Floor(Input.Hue + 0.5)];

            var complementRelativeTemp = 1.0 - InputRelativeTemperature;
            // Find the color in the other section, closest to the inverse percentile
            // of the input color. This is the complement.
            for (var hueAddend = 0.0; hueAddend <= 360.0; hueAddend += 1.0)
            {
                var hue = MathUtils.SanitizeDegreesDouble(
                    startHue + directionOfRotation * hueAddend);
                if (!IsBetween(hue, startHue, endHue))
                {
                    continue;
                }
                var possibleAnswer = HctsByHue[(int)Math.Floor(hue + 0.5)];
                var relativeTemp =
                    (TempsByHct[possibleAnswer] - coldestTemp) / range;
                var error = Math.Abs(complementRelativeTemp - relativeTemp);
                if (error < smallestError)
                {
                    smallestError = error;
                    answer = possibleAnswer;
                }
            }

            _complementCache = answer;
            return _complementCache;
        }
    }

    /// <summary>
    /// Temperature relative to all colors with the same chroma and tone.
    /// Value on a scale from 0 to 1.
    /// </summary>
    public double RelativeTemperature(Hct hct)
    {
        var range = TempsByHct[Warmest] - TempsByHct[Coldest];
        var differenceFromColdest = TempsByHct[hct] - TempsByHct[Coldest];
        // Handle when there's no difference in temperature between warmest and
        // coldest: for example, at T100, only one color is available, white.
        if (range == 0.0)
        {
            return 0.5;
        }
        return differenceFromColdest / range;
    }

    /// <summary>Relative temperature of the input color.</summary>
    public double InputRelativeTemperature
    {
        get
        {
            if (_inputRelativeTemperatureCache >= 0.0)
            {
                return _inputRelativeTemperatureCache;
            }

            _inputRelativeTemperatureCache = RelativeTemperature(Input);
            return _inputRelativeTemperatureCache;
        }
    }

    /// <summary>A map with keys of HCTs in HctsByTemp, values of raw temperature.</summary>
    public Dictionary<Hct, double> TempsByHct
    {
        get
        {
            if (_tempsByHctCache.Count > 0)
            {
                return _tempsByHctCache;
            }

            var allHcts = new List<Hct>(HctsByHue) { Input };
            var temperaturesByHct = new Dictionary<Hct, double>();
            foreach (var e in allHcts)
            {
                temperaturesByHct[e] = RawTemperature(e);
            }
            _tempsByHctCache = temperaturesByHct;
            return temperaturesByHct;
        }
    }

    /// <summary>
    /// HCTs for all hues, with the same chroma/tone as the input.
    /// Sorted ascending, hue 0 to 360.
    /// </summary>
    public Hct[] HctsByHue
    {
        get
        {
            if (_hctsByHueCache.Length > 0)
            {
                return _hctsByHueCache;
            }

            var hcts = new List<Hct>();
            for (var hue = 0.0; hue <= 360.0; hue += 1.0)
            {
                var colorAtHue = Hct.From(hue, Input.Chroma, Input.Tone);
                hcts.Add(colorAtHue);
            }
            _hctsByHueCache = hcts.ToArray();
            return _hctsByHueCache;
        }
    }

    /// <summary>Determines if an angle is between two other angles, rotating clockwise.</summary>
    public static bool IsBetween(double angle, double a, double b)
    {
        if (a < b)
        {
            return a <= angle && angle <= b;
        }
        return a <= angle || angle <= b;
    }

    /// <summary>
    /// Value representing cool-warm factor of a color.
    /// Values below 0 are considered cool, above, warm.
    ///
    /// Color science has researched emotion and harmony, which art uses to select
    /// colors. Warm-cool is the foundation of analogous and complementary colors.
    /// See:
    /// - Li-Chen Ou's Chapter 19 in Handbook of Color Psychology (2015).
    /// - Josef Albers' Interaction of Color chapters 19 and 21.
    ///
    /// Implementation of Ou, Woodcock and Wright's algorithm, which uses
    /// L*a*b* / LCH color space.
    /// Return value has these properties:
    /// - Values below 0 are cool, above 0 are warm.
    /// - Lower bound: -0.52 - (chroma ^ 1.07 / 20). L*a*b* chroma is infinite.
    ///   Assuming max of 130 chroma, -9.66.
    /// - Upper bound: -0.52 + (chroma ^ 1.07 / 20). L*a*b* chroma is infinite.
    ///   Assuming max of 130 chroma, 8.61.
    /// </summary>
    public static double RawTemperature(Hct color)
    {
        var lab = ColorUtils.LabFromArgb(color.ToInt());
        var hue = MathUtils.SanitizeDegreesDouble(
            Math.Atan2(lab[2], lab[1]) * 180.0 / Math.PI);
        var chroma = Math.Sqrt(lab[1] * lab[1] + lab[2] * lab[2]);
        var temperature = -0.5 +
            0.02 * Math.Pow(chroma, 1.07) *
                Math.Cos(
                    MathUtils.SanitizeDegreesDouble(hue - 50.0) * Math.PI / 180.0);
        return temperature;
    }
}
