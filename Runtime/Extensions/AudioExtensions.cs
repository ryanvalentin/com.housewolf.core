using UnityEngine;

public static class AudioExtensions
{
    private const float _muteValue = -80f;
    private const float _minValue = -30f;
    private const float _maxValue = 0f;

    /// <summary>
    /// Returns a scaled audio volume given a normalized input (0, 1)
    /// </summary>
    public static float LerpAudio(float value)
    {
        value = Mathf.Clamp01(value);

        if (value < 0.01f)
            return _muteValue;
        return Mathf.Lerp(_minValue, _maxValue, value);
    }

    public static float ToNormalizedVolume(this float value)
    {
        return LerpAudio(value);
    }

    public static float FromNormalizedVolume(this float value)
    {
        if (value <= _muteValue)
            return 0f;

        return Mathf.InverseLerp(_minValue, _maxValue, value);
    }
}
