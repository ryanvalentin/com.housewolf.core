using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public static class UnityMathematicsPackageExtensions
{
    public static readonly float RadToDeg = 360f / (math.PI * 2);

    /// <summary>
    /// A cheaper implementation of Vector3.angle
    /// </summary>
    public static float Angle2(this float3 a, float3 b)
    {
        var abm = a * Magnitude(b);
        var bam = b * Magnitude(a);
        return 2 * math.atan2(Magnitude(abm - bam), Magnitude(abm + bam)) * RadToDeg;
    }

    /// <summary>
    /// Finds a point directly between two positions at a specified distance from the target.
    /// </summary>
    public static float3 PointBetween(this float3 startPosition, float3 targetPosition, float distanceFromTarget)
    {
        return targetPosition + (distanceFromTarget * math.normalizesafe(startPosition - targetPosition));
    }

    /// <summary>
    /// Returns the <see cref="TransformAccess"/> equivalent of transform.up
    /// </summary>
    public static float3 ToUp(this TransformAccess transform)
    {
        return transform.rotation * math.up();
    }

    /// <summary>
    /// Returns the <see cref="TransformAccess"/> equivalent of transform.forward
    /// </summary>
    public static float3 ToForward(this TransformAccess transform)
    {
        return transform.rotation * math.forward();
    }

    /// <summary>
    /// Returns the <see cref="TransformAccess"/> equivalent of transform.right
    /// </summary>
    public static float3 ToRight(this TransformAccess transform)
    {
        return transform.rotation * math.right();
    }

    /// <summary>
    /// Returns the <see cref="TransformAccess"/> equivalent of transform.down
    /// </summary>
    public static float3 ToDown(this TransformAccess transform)
    {
        return transform.rotation * math.down();
    }

    /// <summary>
    /// Returns the <see cref="TransformAccess"/> equivalent of transform.back
    /// </summary>
    public static float3 ToBack(this TransformAccess transform)
    {
        return transform.rotation * math.back();
    }

    /// <summary>
    /// Returns the <see cref="TransformAccess"/> equivalent of transform.left
    /// </summary>
    public static float3 ToLeft(this TransformAccess transform)
    {
        return transform.rotation * math.left();
    }

    public static float3 ToLocalFromWorldPosition(this float4x4 transform, float3 point)
    {
        return math.transform(math.inverse(transform), point);
    }

    public static float3 ToWorldFromLocalPosition(this float4x4 transform, float3 point)
    {
        return math.transform(transform, point);
    }

    public static float3 ToLocalFromWorldDirection(this Matrix4x4 transform, float3 direction)
    {
        return transform.inverse.MultiplyVector(direction);
    }

    public static float3 ToWorldFromLocalDirection(this Matrix4x4 transform, float3 direction)
    {
        return transform.MultiplyVector(direction);
    }

    /// <summary>
    /// Sets the y component of a vector to 0.
    /// </summary>
    public static float3 ToFlattened(this float3 vector)
    {
        return new float3(vector.x, 0, vector.z);
    }

    /// <summary>
    /// Normalizes a <see cref="ToFlattened(float3)"/> output.
    /// </summary>
    public static float3 ToFlattenedNormalized(this float3 vector)
    {
        return math.normalizesafe(new float3(vector.x, 0, vector.z));
    }

    public static float3 ToFloat3(this Vector3 vector)
    {
        return new float3(vector.x, vector.y, vector.z);
    }

    public static float ToInverseLerpClamped(this float value, float a, float b) => math.clamp((value - a) / (b - a), 0, 1);

    /// <summary>
    /// Mathematical representation of an inverse lerp, which is like "8 is 60% of the way from 5 to 10".
    /// </summary>
    public static float ToInverseLerp(this float value, float min, float max)
    {
        if (math.abs(max - min) < math.EPSILON)
            return min;

        return (value - min) / (max - min);
    }

    public static float Magnitude(float3 val)
    {
        return val.ToMagnitude();
    }

    public static float ToMagnitude(this float3 value)
    {
        return math.sqrt(value.x * value.x + value.y * value.y + value.z * value.z);
    }
}
