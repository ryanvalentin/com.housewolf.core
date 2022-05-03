using UnityEngine;

public static class ColliderUtils
{
    public static bool CanCheckClosestPoint(this Collider collider)
    {
        if (collider is BoxCollider)
            return true;

        if (collider is CapsuleCollider)
            return true;

        if (collider is SphereCollider)
            return true;

        if (collider is MeshCollider meshCollider)
            return meshCollider.convex;

        return false;
    }

    /// <summary>
    /// Finds closest point possible. If it's a non-convex mesh collider, the bounds will be used. Otherwise it'll
    /// find the point on the collider itself.
    /// </summary>
    public static Vector3 FindClosestPoint(this Collider collider, Vector3 point)
    {
        if (collider == null)
            return point;

        if (CanCheckClosestPoint(collider))
            return collider.ClosestPoint(point);
        else
            return collider.ClosestPointOnBounds(point);
    }
}
