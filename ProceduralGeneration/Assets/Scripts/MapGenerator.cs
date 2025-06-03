using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] int mapWidth;
    [SerializeField] int mapHeight;
    [SerializeField][Range(0.011f,10)] float noiseScale;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        MapDisplay mapDisplay = FindAnyObjectByType<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
    }
}
