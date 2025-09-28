using UnityEngine;
using ImGuiNET;

public class ImGuiDebug : MonoBehaviour
{
    [SerializeField] bool showWindow = true;

    [Header("Parameters")]
    public MapGenerator mapGenerator;
    public ProceduralGenerationManager generationManager;

    int spacing = 30;

    private void OnEnable()
    {
        ImGuiUn.Layout += OnLayout;
    }

    private void OnDisable()
    {
        ImGuiUn.Layout -= OnLayout;
    }

    void OnLayout()
    {
        if (showWindow)
        {
            //string[] tabs = { "Procedural Generation", "POI Placement"};

            ImGui.Begin("Debug Menu");

            MapGenerator.DrawMode drawMode = mapGenerator.GetDrawMode();
            string preview = drawMode.ToString();

            if (ImGui.BeginCombo("DrawMode", preview))
            {
                foreach (MapGenerator.DrawMode value in System.Enum.GetValues(typeof(MapGenerator.DrawMode)))
                {
                    bool isSelected = (drawMode == value);
                    if (ImGui.Selectable(value.ToString(), isSelected))
                    {
                        mapGenerator.SetDrawMode(value);
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            bool randomSeed = generationManager.GetRandomSeed();
            if (ImGui.Checkbox("Random Seed", ref randomSeed))
            {
                generationManager.SetRandomSeed(randomSeed);
            }
            int seed = generationManager.GetSeed();
            if (randomSeed)
            {
                // rendu "grisé" visuel (alpha réduit)
                var style = ImGui.GetStyle();
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, style.Alpha * 0.5f);
                int tmp = generationManager.GetSeed();
                ImGui.InputInt("Seed", ref tmp);
                ImGui.PopStyleVar();
            }
            else
            {
                if (ImGui.InputInt("Seed", ref seed))
                {
                    generationManager.SetSeed(seed);
                    mapGenerator.SetSeed(seed);
                }
            }

            ImGui.Dummy(new Vector2(0, spacing));

            ImGui.BeginTabBar("MainTabs");

            if (ImGui.BeginTabItem("Procedural Generation"))
            {
                ImGui.Text("Procedural Generation Debug Info");


                
                int lod = mapGenerator.GetLod();
                if (ImGui.SliderInt("Level Of Detail", ref lod, 0, 6))
                {
                    mapGenerator.SetLod(lod);
                }
                float noiseScale = mapGenerator.GetNoiseScale();
                if (ImGui.SliderFloat("Noise Scale", ref noiseScale, 0.01f, 100f))
                {
                    mapGenerator.SetNoiseScale(noiseScale);
                }
                int octaves = mapGenerator.GetOctaves();
                if (ImGui.SliderInt("Octave", ref octaves, 1, 10))
                {
                    mapGenerator.SetOctaves(octaves);
                }

                float persistance = mapGenerator.GetPersistance();
                if (ImGui.SliderFloat("Persistance", ref persistance, 0f, 1f))
                {
                    mapGenerator.SetPersistance(persistance);
                }

                float lacunarity = mapGenerator.GetLacunarity();

                if (ImGui.SliderFloat("Lacunarity", ref lacunarity, 1, 20))
                {
                    mapGenerator.SetLacunarity(lacunarity);
                }
                float heightMultiplier = mapGenerator.GetHeightMultiplier();
                if (ImGui.SliderFloat("Height Multiplier", ref heightMultiplier, 0.01f, 100f))
                {
                    mapGenerator.SetHeightMultiplier(heightMultiplier);
                }
                if (ImGui.Button("Generate"))
                {
                    mapGenerator.GenerateMap();
                }
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("POI Placement"))
            {
                float flatThreshold = generationManager.GetFlatThreshold();
                if (ImGui.SliderFloat("Flat Threshold", ref flatThreshold, 0, 1))
                {
                    generationManager.SetFlatThreshold(flatThreshold);
                }
                float roomRadius = generationManager.GetRoomRadius();
                if (ImGui.SliderFloat("Room Radius", ref roomRadius, 5, 30))
                {
                    generationManager.SetRoomRadius(roomRadius);
                }
                float distance = generationManager.GetDistance();
                if (ImGui.SliderFloat("Distance", ref distance, 1, 100f))
                {
                    generationManager.SetDistance(distance);
                }
                if (ImGui.Button("Generate"))
                {
                    generationManager.StartGeneration();
                }
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.End();
        }
    }
}
