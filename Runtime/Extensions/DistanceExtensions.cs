using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Extension class to convert raw distances to strings.
/// </summary>
public static class DistanceExtensions
{
    private const float METRIC_DETAIL_THRESHOLD = 1000f; // 1km
    private const float IMPERIAL_DETAIL_THRESHOLD = 800f; // 0.5 miles

    private static Dictionary<float, string> _cachedValues = new Dictionary<float, string>();

    /// <summary>
    /// Converts a float value to a distance string.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="isMetric">Whether to use metric (or imperial if false).</param>
    /// <returns></returns>
    public static string ToDistanceString(this float value, bool isMetric = false)
    {
        // Round value so we can make faster dictionary lookups
        var roundedValue = Mathf.Round(value);

        if (isMetric)
        {
            if (!_cachedValues.TryGetValue(roundedValue, out string distanceStr))
            {
                if (roundedValue > METRIC_DETAIL_THRESHOLD)
                    distanceStr = String.Format("{0} km", (roundedValue / 1000).ToString("0.0"));
                else
                    distanceStr = String.Format("{0} m", roundedValue);

                _cachedValues[roundedValue] = distanceStr;
            }

            return distanceStr;
        }
        else
        {
            if (!_cachedValues.TryGetValue(roundedValue, out string distanceStr))
            {
                if (roundedValue > IMPERIAL_DETAIL_THRESHOLD)
                    distanceStr = String.Format("{0} mi", (value / ConversionRatios.M_MI_CONVERSION).ToString("0.0"));
                else
                    distanceStr = String.Format("{0} ft", Mathf.Round(value * ConversionRatios.M_FT_CONVERSION));

                _cachedValues[roundedValue] = distanceStr;
            }

            return distanceStr;
        }
    }
}
