using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap
    };

    [SerializeField] DrawMode drawMode;

    [SerializeField] ProceduralGenerationManager generationManager;
    const int mapChunkSize = 241;
    [SerializeField] [Range(0,6)] int lod;

    [SerializeField][Range(0.011f,100)] float noiseScale;
    [SerializeField] int octaves;
    [SerializeField][Range(0,1)] float persistance;
    [SerializeField][Range(1,100)] float lacunarity;
    [SerializeField] public int seed;
    [SerializeField] Vector2 offset;
    [SerializeField][Range(1f,100f)] float heightMultiplier;
    [SerializeField] AnimationCurve heightCurve;
    public float[,] noiseMap;

    [SerializeField] TerrainType[] regions;
    [SerializeField] StickToTerrain flatDetector;


    [SerializeField] Slider lodSlider;
    [SerializeField] Slider noiseScaleSlider;
    [SerializeField] Slider octavesSlider;
    [SerializeField] Slider persistanceSlider;
    [SerializeField] Slider lacunaritySlider;
    [SerializeField] Slider heightMultiplierSlider;

    [SerializeField] TMP_Text lodtext;
    [SerializeField] TMP_Text noiseScaleText;
    [SerializeField] TMP_Text octavesText;
    [SerializeField] TMP_Text persistanceText;
    [SerializeField] TMP_Text lacunarityText;
    [SerializeField] TMP_Text heightMultiplierText;


    private void Start()
    {
        lodSlider.value = lod;
        noiseScaleSlider.value = noiseScale;
        octavesSlider.value = octaves;
        persistanceSlider.value = persistance;
        lacunaritySlider.value = lacunarity;
        heightMultiplierSlider.value = heightMultiplier;

        lodtext.text = "" + lod;
        noiseScaleText.text = "" + noiseScale;
        octavesText.text = "" + octaves;
        persistanceText.text = "" + persistance;
        lacunarityText.text = "" + lacunarity;
        heightMultiplierText.text = "" + heightMultiplier;

        lodSlider.onValueChanged.AddListener((value) => OnSliderChanged("lod", value));
        noiseScaleSlider.onValueChanged.AddListener((value) => OnSliderChanged("noiseScale", value));
        octavesSlider.onValueChanged.AddListener((value) => OnSliderChanged("octaves", value));
        persistanceSlider.onValueChanged.AddListener((value) => OnSliderChanged("persistance", value));
        lacunaritySlider.onValueChanged.AddListener((value) => OnSliderChanged("lacunarity", value));
        heightMultiplierSlider.onValueChanged.AddListener((value) => OnSliderChanged("heightMultiplier", value));
    }

    void OnSliderChanged(string sliderName, float value)
    {
        switch (sliderName)
        {
            case "lod":
                lod = (int) value;
                lodtext.text = "" + value;
                break;
            case "noiseScale":
                noiseScale = value;
                noiseScaleText.text = "" + value.ToString("0.##");
                break;
            case "octaves":
                octaves = (int) value;
                octavesText.text = "" + value;
                break;
            case "persistance":
                persistance = value;
                persistanceText.text = "" + value.ToString("0.##");
                break;
            case "lacunarity":
                lacunarity = value;
                lacunarityText.text = "" + value.ToString("0.##");
                break;
            case "heightMultiplier":
                heightMultiplier = value;
                heightMultiplierText.text = "" + value.ToString("0.##");
                break;
        }

        GenerateMap();
    }

    public void SetLod(int newLod)
    {
        lod = newLod;
        GenerateMap();
    }

    public void SetNoiseScale(float newNoiseScale)
    {
        noiseScale = newNoiseScale;
        GenerateMap();
    }

    public void SetOctaves(int newOctaves)
    {
        octaves = newOctaves;
        GenerateMap();
    }

    public void SetPersistance(float newPersistance)
    {
        persistance = newPersistance;
        GenerateMap();
    }

    public void SetLacunarity(float newLacunarity)
    {
        lacunarity = newLacunarity;
        GenerateMap();
    }

    public void SetHeightMultiplier(float newHeightMultiplier)
    {
        heightMultiplier = newHeightMultiplier;
        GenerateMap();
    }

    public void SetDrawMode(DrawMode newDrawMode)
    {
        drawMode = newDrawMode;
        GenerateMap();
    }

    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        GenerateMap();
    }

    public int GetLod()
    {
        return lod;
    }

    public float GetNoiseScale()
    {
        return noiseScale;
    }

    public int GetOctaves()
    {
        return octaves;
    }

    public float GetPersistance()
    {
        return persistance;
    }

    public float GetLacunarity()
    {
        return lacunarity;
    }

    public float GetHeightMultiplier()
    {
        return heightMultiplier;
    }

    public DrawMode GetDrawMode()
    {
        return drawMode;
    }

    public void GenerateMap()
    {
        generationManager.DestroyPrecedent();
        noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize*mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height) {
                        colorMap[y*mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }

                // TROP LOOONG
                // if (flatDetector.IsZoneFlat(flatDetector.FindClosestVertex(new Vector3(x, 5f, y)))) {
                //     colorMap[y*mapChunkSize + x] = regions[0].color;
                // } else {
                //     colorMap[y*mapChunkSize + x] = regions[1].color;
                // }
            }
        }

        MapDisplay mapDisplay = FindAnyObjectByType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap) {
            mapDisplay.DrawNoiseMap(noiseMap);
        } else if (drawMode == DrawMode.ColorMap) {
            mapDisplay.DrawColorMap(colorMap, mapChunkSize, mapChunkSize);
        }

        
        mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve, lod));        
    }
}


[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}