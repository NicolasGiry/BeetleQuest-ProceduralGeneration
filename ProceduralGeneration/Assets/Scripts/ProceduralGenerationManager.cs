using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT,
    FORWARD,
    BACKWARD
};

enum DirectionnelDirection
{
    Right,
    RightForward,
    Forward,
    LeftForward,
    Left
};

public class ProceduralGenerationManager : MonoBehaviour
{
    [Header("General Parameters:")]
    [SerializeField] [Tooltip("0: Directionnel ; 1: Pseudo Room placement")]
        int algorithm;
    [SerializeField] [Tooltip("0 for random seed")]
        int seed;
    [SerializeField] bool visualize;

    [Header("Generation parameters")]
    [SerializeField] int pathLength;

    [Header("Directionnel Generation Parameters")]
    [SerializeField] int minDirectionLenght;
    [SerializeField] int maxDirectionLenght;
    [SerializeField] [Range (0f, 1f)] 
        float arrondissement;
    [SerializeField] int minBeforeUp;
    [SerializeField] int maxBeforeUp;
    [SerializeField] int maxNumberOfRaises;
    [SerializeField] int minUpSize;
    [SerializeField] int maxUpSize;
    [SerializeField] int iterations;

    [Header("Pseudo Room Placement Parameters")]
    [SerializeField] [Range(10, 200)]
        int nbRooms;
    [SerializeField] int minPosX;
    [SerializeField] int maxPosX;
    [SerializeField] int minPosZ;
    [SerializeField] int maxPosZ;
    [SerializeField] int minSizeX;
    [SerializeField] int maxSizeX;
    [SerializeField] int minSizeZ;
    [SerializeField] int maxSizeZ;
    [SerializeField] List<Vector3> posRoomsPlaced = new();
    [SerializeField] List<float> sizeXPlaced = new();
    [SerializeField] List<float> sizeZPlaced = new();
    [SerializeField] List<int> roomConnectedIndex = new();


    [Header("Debug")]
    [SerializeField] new Camera camera;
    [SerializeField] List<GameObject> pathInstances = new();
        List<GameObject> pathInstancesTemp = new();
    [SerializeField] List<GameObject> pathInstancesToCompute = new();
        List<GameObject> pathInstancesToRemove = new();
    [SerializeField] Vector3 currentTileLocation;
    [SerializeField] List<Direction> directionsChosed = new();

    [Header("Tiles Prefabs")]
    [SerializeField] GameObject pathTile;
    [SerializeField] GameObject grassTile;
    [SerializeField] GameObject slopePathTile;
    [SerializeField] GameObject slopeGrassTile;
    [SerializeField] GameObject cliffTile;
    [SerializeField] GameObject ladderTile;

    [Header("Directions")]
    [SerializeField] Vector3[] directionsTab;
    [SerializeField] Vector3[] neighborsPositions = new Vector3[26];

    int nbGenerated;
    Direction nextDirection;
    Direction slopeDirection;
    int directionCount;
    int stopDirectionLength;
    DirectionnelDirection currentDirectionnelDirection;
    Utils utils = new();
    float debut;

    [System.Obsolete]
    void Start()
    {
        debut = Time.realtimeSinceStartup;

        if (seed == 0)
        {
            seed = Random.Range(0, int.MaxValue);
        }
        Random.seed = seed;

        StartCoroutine(StartGenerationCoroutine());
    }

    void Update()
    {
        if (Input.GetKeyDown("space")) {
            // if (seed == 0)
            // {
            //     seed = Random.Range(0, int.MaxValue);
            // }
            // Random.seed = seed;
            debut = Time.realtimeSinceStartup;
            DestroyPrecedent();
            StartCoroutine(StartGenerationCoroutine());
        }
    }

    IEnumerator StartGenerationCoroutine()
    {
        directionsChosed.Add(Direction.FORWARD);

        switch (algorithm) {
            case 0:
                for (int i = 0; i < pathLength; i++)
                {
                    nextDirection = ChooseNextDirectionDirectionnel();
                    PlaceNextTileDirectionnel();
                    camera.transform.position = pathInstances[pathInstances.Count - 1].transform.position + new Vector3(0,200,0);
                    if (visualize && i%2==0)
                        yield return null;
                    break;
                }
                RaisePath();
                yield return null;
                FillWorld();
                break;

            case 1:
                PlaceRooms();
                //ConnectRooms();
                break;

            default:
                print("Error: invalid algorithm index");
                break;
        }
    }

    void DestroyPrecedent() {
        foreach(GameObject path in pathInstances) {
            Destroy(path);
        }
        pathInstances.Clear();
        posRoomsPlaced.Clear();
        sizeXPlaced.Clear();
        sizeZPlaced.Clear();
    }

    // **************************** Directionnelle Generation **************************************

    Direction ChooseNextDirectionDirectionnel()
    {
        if (directionCount < stopDirectionLength)
        {
            directionCount++;
            return ContinueDirection();
        }

        directionCount = 0;
        stopDirectionLength = Random.Range(minDirectionLenght, maxDirectionLenght);

        float directionChoice = Random.Range(0f, 1f);

        if (directionChoice < arrondissement)
        {
            if (Random.Range(0,2) % 2 == 0)
            {
                currentDirectionnelDirection++;
            } else
            {
                currentDirectionnelDirection--;
            }
        } else
        {
            if (Random.Range(0, 2) % 2 == 0)
            {
                currentDirectionnelDirection += 2;
            }
            else
            {
                currentDirectionnelDirection-= 2;
            }
        }

        if ((int)currentDirectionnelDirection > 4)
        {
            currentDirectionnelDirection = (DirectionnelDirection)4;
        }
        else if ((int)currentDirectionnelDirection < 0)
        {
            currentDirectionnelDirection = 0;
        }

        return ContinueDirection();
    }

    Direction ContinueDirection()
    {
        switch (currentDirectionnelDirection)
        {
            case DirectionnelDirection.RightForward:
                if (nextDirection == Direction.FORWARD)
                {
                    return Direction.RIGHT;
                } else
                {
                    return Direction.FORWARD;
                }
            case DirectionnelDirection.LeftForward:
                if (nextDirection == Direction.FORWARD)
                {
                    return Direction.LEFT;
                }
                else
                {
                    return Direction.FORWARD;
                }
            case DirectionnelDirection.Forward:
                return Direction.FORWARD;
            case DirectionnelDirection.Right:
                return Direction.RIGHT;
            case DirectionnelDirection.Left:
                return Direction.LEFT;
            default:
                return Direction.FORWARD;
        }
    }

    void PlaceNextTileDirectionnel()
    {
        Vector3 nextTileLocation = ComputeNextTileLocationDirectionnel();
        GameObject pathTileinstance;

        pathTileinstance = Instantiate(pathTile, nextTileLocation, utils.TileOrientation(nextDirection, slopeDirection));
        pathTileinstance.name = "Path_"+nbGenerated;

        nbGenerated++;
        pathInstances.Add(pathTileinstance);
        currentTileLocation = nextTileLocation;
    }

    Vector3 ComputeNextTileLocationDirectionnel()
    {
        return currentTileLocation + directionsTab[(int)nextDirection];
    }

    void RaisePath()
    {
        int indexToRaise = 0;
        for (int i=0; i<maxNumberOfRaises; i++)
        {
            indexToRaise += Random.Range(minBeforeUp, maxBeforeUp);
            int upSize = Random.Range(minUpSize, maxUpSize);
            int currentUpSize = 0;
            int k;
            for (k = 0; k < upSize; k++)
            {
                Vector3 position = pathInstances[indexToRaise + k].transform.position + new Vector3(0, currentUpSize, 0);
                Quaternion rotation = pathInstances[indexToRaise + k].transform.rotation;
                Destroy(pathInstances[indexToRaise + k]);
                GameObject slope = Instantiate(slopePathTile, position, rotation);
                slope.name = "Slope_"+(indexToRaise + k);
                pathInstances[indexToRaise + k] = slope;
                currentUpSize++;
            }
            for (int j=indexToRaise+k; j<pathInstances.Count; j++)
            {
                pathInstances[j].transform.position += new Vector3(0, upSize, 0);
            }
        }
        print("RAISE FINISHED");
    }

    void FillWorld()
    {
        foreach (GameObject instance in pathInstances) {
            pathInstancesToCompute.Add(instance);
        }

        for (int i=0; i<iterations; i++)
        {
            pathInstancesTemp.Clear();
            pathInstancesToRemove.Clear();

            foreach (GameObject tile in pathInstancesToCompute)
            {
                TileNeighbors tileNeighbor = tile.GetComponent<TileNeighbors>();
                tileNeighbor.DetectNeighbors();
            }

            // add grass tile 
            foreach (GameObject tile in pathInstancesToCompute) 
            {
                List<GameObject> neighbors = tile.GetComponent<TileNeighbors>().GetNeighbors();
                
                bool[] hasNeighbor = CheckNeighbors(tile, neighbors);

                CheckAndPlaceHorizontal(hasNeighbor, tile.transform.position);
                CheckAndPlaceVertical(hasNeighbor, tile.transform.position);
                CheckAbove(hasNeighbor, tile);

                pathInstancesToRemove.Add(tile);
            }

            foreach (GameObject toRemove in pathInstancesToRemove) {
                pathInstancesToCompute.Remove(toRemove);
            }

            foreach(GameObject temp in pathInstancesTemp)
            {
                pathInstancesToCompute.Add(temp);
            }
            
        }

        print("FILL FINISHED IN " + (Time.realtimeSinceStartup - debut));
    }

    bool[] CheckNeighbors(GameObject tile, List<GameObject> neighbors) {
        bool[] hasNeighbor = new bool[26];
        foreach (GameObject neighbor in neighbors) {
            Vector3 diff = neighbor.transform.position - tile.transform.position;
            for(int i=0; i<26; i++) {
                if (diff == neighborsPositions[i]) {
                    hasNeighbor[i] = true;
                }
            }
        }

        return hasNeighbor;
    }

    void CheckAndPlaceHorizontal(bool[] hasNeighbor, Vector3 position) 
    {
        //288353
        if (!hasNeighbor[14] && !hasNeighbor[6]) {
            pathInstancesTemp.Add(Instantiate(grassTile, position + neighborsPositions[14], Quaternion.identity));
        } else if (!hasNeighbor[13] && !hasNeighbor[4]) {
            pathInstancesTemp.Add(Instantiate(grassTile, position + neighborsPositions[13], Quaternion.identity));
        }
    }

    void CheckAndPlaceVertical(bool[] hasNeighbor, Vector3 position) 
    {
        if (!hasNeighbor[11] && !hasNeighbor[2]) {
            pathInstancesTemp.Add(Instantiate(grassTile, position + neighborsPositions[11], Quaternion.identity));
        } else if (!hasNeighbor[16] && !hasNeighbor[8]) {
            pathInstancesTemp.Add(Instantiate(grassTile, position + neighborsPositions[16], Quaternion.identity));
        }
    }

    void CheckAbove(bool[] hasNeighbor, GameObject tile) {
        if (hasNeighbor[22]) {
            pathInstancesToRemove.Add(tile);
            Destroy(tile);
        }
    }


    // **************************** Pseudo Room Placement Generation **************************************

    void PlaceRooms() {
        for (int i=0; i<nbRooms; i++) {
            Vector3 roomPos = new Vector3(Random.Range(minPosX, maxPosX), 0, Random.Range(minPosZ, maxPosZ));
            float sizeX = Random.Range(minSizeX, maxSizeX);
            float sizeZ = Random.Range(minSizeZ, maxSizeZ);

            if (RoomPlacementIsCorrect(roomPos, sizeX, sizeZ)) {
                posRoomsPlaced.Add(roomPos);
                sizeXPlaced.Add(sizeX);
                sizeZPlaced.Add(sizeZ);

                PlaceRoomInstance(roomPos, sizeX, sizeZ);
            }
        }
        print("duration: " + (Time.realtimeSinceStartup - debut));
    }

    bool RoomPlacementIsCorrect(Vector3 roomPos, float sizeX, float sizeZ) {
        for (int i=0; i<posRoomsPlaced.Count; i++) {
            Vector3 currentPos = posRoomsPlaced[i];
            float currentSizeX = sizeXPlaced[i];
            float currentSizeZ = sizeZPlaced[i];

            if (roomPos.x <= currentPos.x + currentSizeX && roomPos.x >= currentPos.x && 
                roomPos.z <= currentPos.z + currentSizeZ && roomPos.z >= currentPos.z ||
                roomPos.x + sizeX <= currentPos.x + currentSizeX && roomPos.x + sizeX >= currentPos.x &&
                roomPos.z <= currentPos.z + currentSizeZ && roomPos.z >= currentPos.z ||
                roomPos.x <= currentPos.x + currentSizeX && roomPos.x >= currentPos.x && 
                roomPos.z + sizeZ <= currentPos.z + currentSizeZ && roomPos.z + sizeZ >= currentPos.z ||
                roomPos.x + sizeX <= currentPos.x + currentSizeX && roomPos.x + sizeX >= currentPos.x &&
                roomPos.z + sizeZ <= currentPos.z + currentSizeZ && roomPos.z + sizeZ >= currentPos.z ||
                roomPos.x <= currentPos.x && roomPos.x + sizeX >= currentPos.x + currentSizeX &&
                (roomPos.z <= currentPos.z + currentSizeZ && roomPos.z >= currentPos.z || 
                roomPos.z + sizeZ <= currentPos.z + currentSizeZ && roomPos.z + sizeZ >= currentPos.z) ||
                roomPos.x <= currentPos.x && roomPos.x + sizeX >= currentPos.x && 
                roomPos.z <= currentPos.z && roomPos.z + sizeZ >= currentPos.z ||
                roomPos.x >= currentPos.x && roomPos.x <= currentPos.x + currentSizeX && 
                roomPos.z <= currentPos.z && roomPos.z + sizeZ >= currentPos.z) {

                return false;
            }
        }
        return true;
    }

    void PlaceRoomInstance(Vector3 roomPos, float sizeX, float sizeZ) {
        for (int x = (int) roomPos.x; x < roomPos.x + sizeX; x++) {
            for (int z = (int) roomPos.z; z < roomPos.z + sizeZ; z++) {
                pathInstances.Add(Instantiate(pathTile, new Vector3(x, 0, z), Quaternion.identity));
            }
        }
    }

    // void ConnectRooms() {
    //     int index = 0;
    //     foreach(Vector3 roomPosA in posRoomsPlaced) {
    //         float distanceMin = 10000000f;
    //         float distance; 
    //         foreach(Vector3 roomPosB in posRoomsPlaced) {
    //             if (!roomPosA.Equals(roomPosB)) {
    //                 distance = utils.Distance(roomPosA, roomPosB);
    //                 if (distance < distanceMin) {
    //                     distanceMin = distance;
    //                     index = posRoomsPlaced.FindIndex(roomPosB);
    //                 }
    //             }
    //         }
    //         roomConnectedIndex.Add(index);
    //     }
    // }


}