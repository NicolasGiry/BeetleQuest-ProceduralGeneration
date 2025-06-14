using UnityEngine;

[ExecuteInEditMode]
public class StickToTerrain : MonoBehaviour
{
    public MeshFilter terrain;
    public float roomRadius;
    public float flatThreshold;

    private void Update()
    {
        if (!Application.isPlaying && terrain != null)
        {
            Vector3 snappedPos = FindClosestVertex(transform.position);
            transform.position = snappedPos;
            if (IsZoneFlat(snappedPos))
            {
                print("FLAT");
            } else
            {
                print("not FLAT");
            }
        }
    }

    Vector3 FindClosestVertex(Vector3 targetPos)
    {
        Mesh mesh = terrain.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Transform terrainTransform = terrain.transform;

        Vector3 closestVertex = Vector3.zero;
        float minDist2D = Mathf.Infinity;

        foreach (Vector3 v in vertices)
        {
            Vector3 worldV = terrainTransform.TransformPoint(v);

            // Calcul uniquement sur XZ
            float distXZ = Vector2.Distance(
                new Vector2(worldV.x, worldV.z),
                new Vector2(targetPos.x, targetPos.z)
            );

            if (distXZ < minDist2D)
            {
                minDist2D = distXZ;
                closestVertex = worldV;
            }
        }

        return closestVertex;
    }

    bool IsZoneFlat(Vector3 pos)
    {
        Mesh terrainMesh = terrain.sharedMesh;
        Vector3[] vertices = terrainMesh.vertices;
        Transform terrainTransform = terrain.transform;
        bool isFlat = true;

        foreach (Vector3 v in vertices)
        {
            Vector3 worldV = terrainTransform.TransformPoint(v); // CORRECTION

            // Calcul en 2D (XZ)
            if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(worldV.x, worldV.z)) < roomRadius)
            {
                
                if (Mathf.Abs(pos.y - worldV.y) > flatThreshold)
                {
                    isFlat = false;
                    Debug.DrawLine(pos, worldV, Color.red, 0.1f);
                }
                else
                    Debug.DrawLine(pos, worldV, Color.green, 0.1f);

            }
        }
        return isFlat;
    }
}
