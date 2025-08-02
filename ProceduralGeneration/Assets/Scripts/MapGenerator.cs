using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh
    };

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

    public void GenerateMap()
    {
        generationManager.DestroyPrecedent();
        noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        MapDisplay mapDisplay = FindAnyObjectByType<MapDisplay>();

        mapDisplay.DrawNoiseMap(noiseMap);
        mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve, lod));        
    }
}
