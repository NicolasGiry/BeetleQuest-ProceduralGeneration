using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ProceduralGenerationManager : MonoBehaviour
{
    [Header("General Parameters:")]
    [SerializeField]
    int seed;
    [SerializeField] bool randomSeed;
    [SerializeField] bool visualize;

    [Header("Pseudo Room Placement Parameters")]
    [SerializeField] Vector3 startRoomPos;
    [SerializeField] int minPosX;
    [SerializeField] int maxPosX;
    [SerializeField] int minPosZ;
    [SerializeField] int maxPosZ;
    [SerializeField] int minSizeX;
    [SerializeField] int maxSizeX;
    [SerializeField] int minSizeZ;
    [SerializeField] int maxSizeZ;
    [SerializeField] float heightRoomOffset;
    [SerializeField] int minSecondaryPathLenght;
    [SerializeField] int maxSecondaryPathLenght;
    [SerializeField] int margin;
    [SerializeField] int nbLock;
    [SerializeField] List<Room> roomsPlaced = new();
    [SerializeField] List<Vector3> startPosCorridors = new();
    [SerializeField] List<Vector3> endPosCorridors = new();
    [SerializeField] List<Vector3> startPosCorridorsNull = new();
    [SerializeField] List<Vector3> endPosCorridorsNull = new();
    [SerializeField] float corridorWidth = 1.5f;
    [SerializeField] List<Vector3> pathVertices = new();

    [SerializeField] float maxDistanceConnexion;
    [SerializeField] List<Path> paths = new();


    [Header("Debug")]
    [SerializeField] new Camera camera;
    [SerializeField] List<GameObject> pathInstances = new();

    [Header("Tiles Prefabs")]
    [SerializeField] GameObject pathTile;
    [SerializeField] GameObject grassTile;
    [SerializeField] GameObject slopePathTile;
    [SerializeField] GameObject slopeGrassTile;
    [SerializeField] GameObject cliffTile;
    [SerializeField] GameObject ladderTile;

    [Header("Rooms Prefabs")]
    [SerializeField] List<GameObject> startRooms;
    [SerializeField] List<GameObject> arenas;
    [SerializeField] List<GameObject> firecamps;
    [SerializeField] List<GameObject> chests;
    [SerializeField] List<GameObject> narrativePlace;
    [SerializeField] List<GameObject> shop;
    [SerializeField] List<GameObject> temple;
    [SerializeField] List<List<GameObject>> roomsPrefab;
    [SerializeField] List<GameObject> rooms;


    [Header("Terrain")]
    [SerializeField] MeshFilter terrain;
    [SerializeField] MapGenerator mapGenerator;
    [SerializeField] float roomRadius;
    [SerializeField] float flatThreshold;
    [SerializeField] float minDistanceBetween2Rooms;
    [SerializeField] List<Vector3> possibleRoomsPlacement = new();
    [SerializeField] Vector3 anchorPos;
    [SerializeField] int terrainSize;
    float[,] noiseMap;

    [Header("UI")]
    [SerializeField] Slider flatThresholdSlider;
    [SerializeField] Slider roomRadiusSlider;
    [SerializeField] Slider distanceSlider;
    [SerializeField] Toggle randomSeedToggle;
    [SerializeField] TMP_InputField seedTextArea;


    [SerializeField] TMP_Text flatThresholdText;
    [SerializeField] TMP_Text roomRadiusText;
    [SerializeField] TMP_Text distanceText;
    [SerializeField] TMP_Text ErrorText;

    [SerializeField] Animator waitingScreenAnimator;

    List<Vector3> cornerPos = new();
    List<Vector3> roomCheckPos = new();

    [SerializeField]
    [Tooltip("[StartRooms, FireCamps, Chests, NarrativePlaces, Shops, Temples]")]
    int[] numbersOfRoomsType = {1, 2, 3, 2, 1, 3};

    float debut;

    [System.Obsolete]
    void Start()
    {
        flatThresholdSlider.value = flatThreshold;
        roomRadiusSlider.value = roomRadius;
        distanceSlider.value = minDistanceBetween2Rooms;
        randomSeedToggle.isOn = randomSeed;
        seedTextArea.text = "" + seed;
        seedTextArea.interactable = !randomSeed;

        flatThresholdText.text = "" + flatThreshold;
        roomRadiusText.text = "" + roomRadius;
        distanceText.text = "" + minDistanceBetween2Rooms;

        flatThresholdSlider.onValueChanged.AddListener((value) => OnSliderChanged("flatThreshold", value));
        roomRadiusSlider.onValueChanged.AddListener((value) => OnSliderChanged("roomRadius", value));
        distanceSlider.onValueChanged.AddListener((value) => OnSliderChanged("minDistanceBetween2Rooms", value));
        randomSeedToggle.onValueChanged.AddListener((value) => OnToggleChanged("randomSeed", value));
        seedTextArea.onValueChanged.AddListener((value) => OnTextChanged("seed", value));


        StartGeneration();
    }

    void OnTextChanged(string textName, string value)
    {
        switch (textName)
        {
            case "seed":
                seed = int.Parse(value);
                break;
        }
    } 

    void OnToggleChanged(string toggleName, bool value)
    {
        switch (toggleName)
        {
            case "randomSeed":
                randomSeed = value;
                seedTextArea.interactable = !value;
                break;
        }
    }

    void OnSliderChanged(string sliderName, float value)
    {
        switch (sliderName)
        {
            case "flatThreshold":
                flatThreshold = value;
                flatThresholdText.text = "" + value.ToString("0.##");
                break;
            case "roomRadius":
                roomRadius = value;
                roomRadiusText.text = "" + value.ToString("0.##");
                distanceSlider.minValue = value;
                terrainSize = 241 - (int) value;
                break;
            case "minDistanceBetween2Rooms":
                minDistanceBetween2Rooms = value;
                distanceText.text = "" + value.ToString("0.##");
                break;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    [System.Obsolete]
    void Update()
    {
        if (Input.GetKeyDown("space")) {
            if (randomSeed)
            {
                seed = Random.Range(0, int.MaxValue);
            }
            Random.seed = seed;
            debut = Time.realtimeSinceStartup;
            //DestroyPrecedent();
            StartGeneration();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i=0; i<startPosCorridors.Count; i++) {
            Gizmos.DrawLine(startPosCorridors[i], endPosCorridors[i]);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < cornerPos.Count; i++)
        {
            Gizmos.DrawSphere(cornerPos[i] + new Vector3(0, heightRoomOffset, 0), 0.25f);
        }

        for (int i = 0; i < roomCheckPos.Count; i++)
        {
            Gizmos.DrawSphere(roomCheckPos[i], roomRadius / 2);
        }

        if (visualize) {
            Gizmos.color = Color.red;
            for (int i=0; i<startPosCorridorsNull.Count; i++) {
                Gizmos.DrawLine(startPosCorridorsNull[i], endPosCorridorsNull[i]);
            }
        }

        Gizmos.color = Color.blue;
        foreach (Vector3 vertex in pathVertices)
        {
            Gizmos.DrawSphere(vertex, 2f);
        }

        foreach(Path path in paths) {
            Gizmos.DrawLine(path.GetPosA(), path.GetPosB());
        }
    }

    public void LaunchGeneration()
    {
        StartCoroutine(LaunchGenerationCoroutine());
    }

    IEnumerator LaunchGenerationCoroutine()
    {
        waitingScreenAnimator.SetBool("Loading", true);
        yield return new WaitForSeconds(0.2f);
        StartGeneration();
        yield return new WaitForSeconds(0.1f);
        waitingScreenAnimator.SetBool("Loading", false);
    }

    void StartGeneration()
    {
        debut = Time.realtimeSinceStartup;
        if (randomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }
        Random.seed = seed;
        seedTextArea.text = ""+seed;
        mapGenerator.seed = seed;
        DestroyPrecedent();
        mapGenerator.GenerateMap();
        FindPossibleRoomsPlacement();
        PlaceRooms();
        if (rooms.Count == 0)
        {
            ErrorText.text = "The parameters provided did not allow any rooms to be placed.";
        } else
        {
            ErrorText.text = "";
        }
        ConnectRooms();
        print("duration: " + (Time.realtimeSinceStartup - debut));
        
    }

    public void DestroyPrecedent() {
        foreach(GameObject path in pathInstances) {
            Destroy(path);
        }
        foreach (Room room in roomsPlaced)
        {
            room.OnDestroyRoom();
        }
        foreach (GameObject room in rooms) 
        {
            Destroy(room);
        }
        roomsPlaced.Clear();
        pathInstances.Clear();
        startPosCorridors.Clear();
        endPosCorridors.Clear();
        startPosCorridorsNull.Clear();
        endPosCorridorsNull.Clear();
        cornerPos.Clear();
        roomCheckPos.Clear();
        possibleRoomsPlacement.Clear();
        if (roomsPrefab != null)
            roomsPrefab.Clear();
        rooms.Clear();
    }

    // **************************** Pseudo Room Placement Generation **************************************

    void FindPossibleRoomsPlacement()
    {
        // pour que pos soit gard�e : 
        //      - pos + rayonMin est plat (+- flatThreshold)
        //      - pos suffisament loin de toute autre salle 

        Vector3 currentPos = FindClosestVertex(anchorPos + new Vector3(roomRadius, 0, roomRadius));

        for (int i = 0; i < terrainSize / roomRadius; i++)
        {
            for (int j = 0; j < terrainSize / roomRadius; j++)
            {
                if (FarEnough(currentPos) && IsZoneFlat(currentPos))
                {
                    possibleRoomsPlacement.Add(currentPos);
                    roomCheckPos.Add(currentPos);
                }
                currentPos = FindClosestVertex(currentPos + new Vector3(0, 0, roomRadius));
            }
            currentPos = FindClosestVertex(currentPos + new Vector3(roomRadius, 0, 0));
            currentPos.z = anchorPos.z + roomRadius;
        }
    }

    void PlaceRooms()
    {
        roomsPrefab = new List<List<GameObject>> { startRooms, arenas, firecamps, chests, narrativePlace, shop, temple };
        //for (int i = 0; i < numbersOfRoomsType.Count(); i++)
        //{
        //    for (int j = 0; j < Mathf.Min(numbersOfRoomsType[i], roomCheckPos.Count); j++)
        //    {
        //        GenerateRoom(i);
        //    }
        //}

        foreach(Vector3 roomPos in roomCheckPos)
        {
            GameObject room = Instantiate(roomsPrefab[0][0], FindClosestVertex(roomPos), Quaternion.identity);
            room.transform.localScale = new Vector3(roomRadius, roomRadius, roomRadius);
            rooms.Add(room);

            Room roomInfo = new Room(rooms.Count-1, room.transform.position, roomRadius);
            roomsPlaced.Add(roomInfo);
        }
    }

    void GenerateRoom(int type)
    {
        GameObject room;
        int version = Random.Range(0, roomsPrefab[type].Count());
        Transform roomTransform = roomsPrefab[type][version].GetComponent<Transform>();
        GameObject roomPref = roomsPrefab[type][version];
        Vector3 roomPos = roomCheckPos[rooms.Count()];
        room = Instantiate(roomPref, roomPos, Quaternion.identity);
        rooms.Add(room);
        room.transform.position = FindClosestVertex(room.transform.position);
        room.transform.localScale = new Vector3(roomRadius * 2, roomRadius * 2, roomRadius * 2);
        foreach (Transform t in roomTransform)
        {
            t.position = new Vector3(t.position.x, 0, t.position.z);
        }

        Room roomInfo = new Room(rooms.Count-1, room.transform.position, roomRadius);
        roomsPlaced.Add(roomInfo);
    }

    int GetNbRooms()
    {
        int nbRooms = 0;
        for (int i = 0; i < numbersOfRoomsType.Length; i++)
        {
            nbRooms += numbersOfRoomsType[i];
        }
        return nbRooms;
    }

    bool IsZoneFlat(Vector3 pos)
    {
        Mesh terrainMesh = terrain.sharedMesh;
        Vector3[] vertices = terrainMesh.vertices;
        foreach (Vector3 v in vertices)
        {
            if (Vector3.Distance(pos, v) < roomRadius)
            {
                if (Mathf.Abs(pos.y - v.y) > flatThreshold)
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool FarEnough(Vector3 pos)
    {
        foreach (Vector3 roomPos in possibleRoomsPlacement)
        {
            if (Vector3.Distance(roomPos, pos) < minDistanceBetween2Rooms)
            {
                return false;
            }
        }
        return true;
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
            float distXZ = Vector2.Distance(new Vector2(worldV.x, worldV.z), new Vector2(targetPos.x, targetPos.z));

            if (distXZ < minDist2D)
            {
                minDist2D = distXZ;
                closestVertex = worldV;
            }
        }

        return closestVertex;
    }

    void ConnectRooms()
    {
        print(roomsPlaced.Count);
       foreach (Room roomA in roomsPlaced)
        {
            foreach (Room roomB in roomsPlaced)
            {
                if (!roomA.Equals(roomB) && Vector3.Distance(roomA.GetPos(), roomB.GetPos()) < maxDistanceConnexion) 
                {
                    Path path = new Path(roomA, roomB);
                    paths.Add(path);
                    print("Path : " + roomA.GetPos() + " - " + roomB.GetPos());
                }
            }
        }
    }

}