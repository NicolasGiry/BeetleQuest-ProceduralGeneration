using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Sources;
using UnityEngine;

public class ProceduralGeometry : MonoBehaviour
{
    float TAU = 6.283185307179586f;
    //[SerializeField] List<Vector3> points = new();
    [SerializeField][Range(0.01f, 1f)] float innerRadius;
    [SerializeField][Range(0.01f, 1f)] float thickness;
    [SerializeField] [Range(3,32)]
        int angularSegments;
    float outerRadius => innerRadius + thickness;
    int vertexCount => angularSegments * 2;

    Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "QuadRing";
        GetComponent<MeshFilter>().sharedMesh = mesh;

        GenerateMesh();

        //int[] trianglesIndexes = new int[] {
        //    2, 0, 1,
        //    2, 1, 3
        //    };

        //mesh.SetVertices(points);
        //mesh.triangles = trianglesIndexes;
        //mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, innerRadius);
        Gizmos.DrawWireSphere(transform.position, outerRadius);
    }

    private void Update()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        mesh.Clear();
        int vCount = vertexCount;

        List<Vector3> points = new();

        for (int i=0; i<angularSegments; i++)
        {
            float t = i / (float)angularSegments;
            float angRad = t * TAU;
            Vector2 dir = new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad));

            points.Add(dir * outerRadius);
            points.Add(dir * innerRadius);
        }

        List<int> trianglesIndices = new();
        for (int i = 0; i < angularSegments; i++)
        {
            int indexRoot = i * 2;
            int indexInnerRoot = indexRoot + 1;
            int indexOuterNext = (indexRoot + 2) % vCount;
            int indexInnerNext = (indexRoot + 3) % vCount;


            trianglesIndices.Add(indexRoot);
            trianglesIndices.Add(indexOuterNext);
            trianglesIndices.Add(indexInnerNext);

            trianglesIndices.Add(indexRoot);
            trianglesIndices.Add(indexInnerNext);
            trianglesIndices.Add(indexInnerRoot);

        }

        mesh.SetVertices(points);
        mesh.triangles = trianglesIndices.ToArray();
        mesh.RecalculateNormals();

    }
}
