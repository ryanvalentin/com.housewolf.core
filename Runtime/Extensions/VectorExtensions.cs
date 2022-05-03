using UnityEngine;

namespace WarpilotII
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a comma-separated Vector3 string into a Vector3
        /// </summary>
        public static Vector3 ToVector3(this string vectorString)
        {
            const char delimeter = ',';

            var splits = vectorString.Split(delimeter);

            return new Vector3(int.Parse(splits[0].Trim()), int.Parse(splits[1].Trim()), int.Parse(splits[2].Trim()));
        }

        /// <summary>
        /// Finds a random point within the provided bounds.
        /// </summary>
        public static Vector3 ToRandomPoint(this Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        public static Vector3 ToPointBetween(this Transform sourceObject, Transform targetObject, float distanceFromTarget)
        {
            return ToPointBetween(sourceObject.position, targetObject.position, distanceFromTarget);
        }

        public static Vector3 ToPointBetween(this Vector3 sourcePosition, Vector3 targetPosition, float distanceFromTarget)
        {
            // Subtract both vectors to get a vector pointing from target to source object
            var pointingVector = sourcePosition - targetPosition;

            // Normalize the vector so it has a length of 1
            var normalizedVector = pointingVector.normalized;

            // Scale the vector to find a point between A and B, so we ultimately are "distanceFromTarget" units from "targetObject"
            var targetLocation = targetPosition + (distanceFromTarget * normalizedVector);

            return targetLocation;
        }

        /// <summary>
        /// Calculates the forward angle of the source to the target transform. Positive numbers up to 1f
        /// mean facing, -1f means away from.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float ToAngleToTarget(this Transform source, Transform target, Vector3 direction)
        {
            return ToAngleToTarget(source, target.position, direction);
        }

        /// <summary>
        /// Calculates the forward angle of the source to the target transform. Positive numbers up to 1f
        /// mean facing, -1f means away from.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float ToAngleToTarget(this Transform source, Vector3 target, Vector3 direction)
        {
            if (!source)
                return 0f;

            Vector3 forward = source.TransformDirection(direction);
            Vector3 toOther = target - source.position;
            float dot = Vector3.Dot(forward.normalized, toOther.normalized);

            return dot;
        }

        public static Vector3 ToCalculatedLead(this Rigidbody targetRigidbody, Transform targetTransform, Transform currentTransform, float projectileSpeed, float leadMultiplier)
        {
            var distance = Vector3.Distance(currentTransform.position, targetTransform.position);
            var velocityPosition1 = targetTransform.position + ((targetRigidbody.velocity * leadMultiplier) * (distance / projectileSpeed));
            var velocityDist1 = Vector3.Distance(velocityPosition1, currentTransform.position);
            // second calc., using distance from first calc.
            var velocityPosition2 = targetTransform.position + ((targetRigidbody.velocity * leadMultiplier) * (velocityDist1 / projectileSpeed));
            var velocityDist2 = Vector3.Distance(velocityPosition2, currentTransform.position);
            // third calc., using distance from second calc.
            var targetInterceptPosition = targetTransform.position + ((targetRigidbody.velocity * leadMultiplier) * (velocityDist2 / projectileSpeed));

            return targetInterceptPosition;
        }

        public static float ToRelativeDirection(this Transform source, Vector3 targetForward)
        {
            return Vector3.Angle(source.forward, targetForward).ToNormalized(0, 180);
        }

        /// <summary>
        /// Adapted from Source: https://forum.unity.com/threads/scale-around-point-similar-to-rotate-around.232768/#post-5505829
        /// Scales the target around an arbitrary point by scaleFactor.
        /// This is relative scaling, meaning using  scale Factor of Vector3.one
        /// will not change anything and new Vector3(0.5f,0.5f,0.5f) will reduce
        /// the object size by half.
        /// The pivot is assumed to be the position in the space of the target.
        /// Scaling is applied to localScale of target.
        /// </summary>
        /// <param name="target">The object to scale.</param>
        /// <param name="pivot">The point to scale around in space of target.</param>
        /// <param name="scaleFactor">The factor with which the current localScale of the target will be multiplied with.</param>
        public static void ScaleAroundRelative(this Transform target, Vector3 pivot, Vector3 scaleFactor)
        {
            // pivot
            var pivotDelta = target.localPosition - pivot;
            pivotDelta.Scale(scaleFactor);
            target.localPosition = pivot + pivotDelta;

            // scale
            var finalScale = target.localScale;
            finalScale.Scale(scaleFactor);
            target.localScale = finalScale;
        }

        /// <summary>
        /// Adapted from Source: https://forum.unity.com/threads/scale-around-point-similar-to-rotate-around.232768/#post-5505829
        /// Scales the target around an arbitrary pivot.
        /// This is absolute scaling, meaning using for example a scale factor of
        /// Vector3.one will set the localScale of target to x=1, y=1 and z=1.
        /// The pivot is assumed to be the position in the space of the target.
        /// Scaling is applied to localScale of target.
        /// </summary>
        /// <param name="target">The object to scale.</param>
        /// <param name="pivot">The point to scale around in the space of target.</param>
        /// <param name="scaleFactor">The new localScale the target object will have after scaling.</param>
        public static void ScaleAround(this Transform target, Vector3 pivot, Vector3 newScale)
        {
            // pivot
            Vector3 pivotDelta = target.localPosition - pivot; // diff from object pivot to desired pivot/origin
            Vector3 scaleFactor = new Vector3(
                newScale.x / target.localScale.x,
                newScale.y / target.localScale.y,
                newScale.z / target.localScale.z);
            pivotDelta.Scale(scaleFactor);
            target.localPosition = pivot + pivotDelta;

            //scale
            target.localScale = newScale;
        }
    }
}
