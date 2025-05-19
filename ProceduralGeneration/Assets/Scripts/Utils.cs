using UnityEngine;

public class Utils
{
    public float Distance (Vector3 A, Vector3 B) {
        return Mathf.Sqrt((B.x-A.x)*(B.x-A.x) + (B.z-A.z)*(B.z-A.z));
    }
}
