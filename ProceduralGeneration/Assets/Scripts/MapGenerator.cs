using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh
    };

    //[SerializeField] DrawMode drawMode;
    [SerializeField][Range(1, 1024)] int mapWidth;
    [SerializeField][Range(1, 1024)] int mapHeight;
    [SerializeField][Range(0.011f,100)] float noiseScale;
    [SerializeField] int octaves;
    [SerializeField][Range(0,1)] float persistance;
    [SerializeField][Range(1,100)] float lacunarity;
    [SerializeField] int seed;
    [SerializeField] Vector2 offset;
    [SerializeField][Range(1f,100f)] float heightMultiplier;
    [SerializeField] AnimationCurve heightCurve;


    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        MapDisplay mapDisplay = FindAnyObjectByType<MapDisplay>();

        mapDisplay.DrawNoiseMap(noiseMap);
        mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve));        
    }
}
