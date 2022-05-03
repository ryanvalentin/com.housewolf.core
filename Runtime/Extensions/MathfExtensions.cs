using UnityEngine;

public static class MathfExtensions
{
    /// <summary>
    /// Converts a value to a percentage between 0.0 and 1.0 for given min/max values.
    /// </summary>
    public static float ToDecimalPercentage(this float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    /// <summary>
    /// Converts a value between 0.0 and 1.0 to a percentage string.
    /// </summary>
    public static string ToPercentageString(this float value)
    {
        return $"{Mathf.Clamp(Mathf.RoundToInt(value * 100f), 0, 100)}%";
    }

    /// <summary>
    /// Ensures a given value will be positive.
    /// </summary>
    public static float ToPositiveNumber(this float value)
    {
        if (value > 0)
            return value;

        return -value;
    }

    /// <summary>
    /// Randomly converts a number to either be positive or negative
    /// </summary>
    public static float ToEitherPositiveOrNegative(this float value)
    {
        return Random.value < 0.5 ? -value : value;
    }

    public static float ToNormalized(this float value, float min, float max)
    {
        var zeroValue = (min + max) / 2;
        if (value > zeroValue)
            return value.ToDecimalPercentage(zeroValue, max);
        else
            return value.ToDecimalPercentage(min, zeroValue);
    }
}
