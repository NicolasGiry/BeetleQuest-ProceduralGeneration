using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class StickToTerrain : MonoBehaviour
{
    public MeshFilter terrain;
    public float roomRadius;
    public float flatThreshold;
    public List<Vector3> flatPoints = new();
    public List<Vector3> noFlatPoints = new();

    private void Update()
    {
        if (!Application.isPlaying && terrain != null)
        {
            Vector3 snappedPos = FindClosestVertex(transform.position);
            transform.position = snappedPos;
            IsZoneFlat(snappedPos);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach( Vector3 pos in flatPoints )
        {
            Gizmos.DrawSphere(pos, 1f);
        }
        Gizmos.color = Color.red;
        foreach (Vector3 pos in noFlatPoints)
        {
            Gizmos.DrawSphere(pos, 1f);
        }
    }

    public Vector3 FindClosestVertex(Vector3 targetPos)
    {
        Mesh mesh = terrain.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Transform terrainTransform = terrain.transform;

        Vector3 closestVertex = Vector3.zero;
        float minDist2D = Mathf.Infinity;

        foreach (Vector3 v in vertices)
        {
            Vector3 worldV = terrainTransform.TransformPoint(v);
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

    public bool IsZoneFlat(Vector3 pos)
    {
        Mesh terrainMesh = terrain.sharedMesh;
        Vector3[] vertices = terrainMesh.vertices;
        Transform terrainTransform = terrain.transform;
        bool isFlat = true;
        flatPoints.Clear();
        noFlatPoints.Clear();

        foreach (Vector3 v in vertices)
        {
            Vector3 worldV = terrainTransform.TransformPoint(v); 

            if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(worldV.x, worldV.z)) < roomRadius)
            {
                
                if (Mathf.Abs(pos.y - worldV.y) > flatThreshold)
                {
                    isFlat = false;
                    noFlatPoints.Add(worldV);
                }
                else
                {
                    flatPoints.Add(worldV);
                }
            }
        }
        return isFlat;
    }
}
