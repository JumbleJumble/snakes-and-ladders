using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Inverse(this Vector3 v)
    {
        return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
    }
}
