using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEditor;
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
    [SerializeField]
        int seed;
    [SerializeField] bool randomSeed;
    [SerializeField] bool visualize;
    [SerializeField] List<Material> materials = new();

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
    [SerializeField] [Range(5, 200)]
        int nbRooms;
    [SerializeField] Vector3 startRoomPos;
    [SerializeField] int minPosX;
    [SerializeField] int maxPosX;
    [SerializeField] int minPosZ;
    [SerializeField] int maxPosZ;
    [SerializeField] int minSizeX;
    [SerializeField] int maxSizeX;
    [SerializeField] int minSizeZ;
    [SerializeField] int maxSizeZ;
    [SerializeField] int margin;
    [SerializeField] float corridorWidthDetection;
    //[SerializeField] List<Vector3> posRoomsPlaced = new();
    [SerializeField] List<Room> roomsPlaced = new();
    //[SerializeField] List<float> sizeXPlaced = new();
    //[SerializeField] List<float> sizeZPlaced = new();
    //[SerializeField] List<int> roomFirstTileIndex = new();
    [SerializeField] List<Vector3> startPosCorridors = new();
    [SerializeField] List<Vector3> endPosCorridors = new();
    [SerializeField] List<Vector3> startPosCorridorsNull = new();
    [SerializeField] List<Vector3> endPosCorridorsNull = new();
    Room startRoom;
    Room bossRoom;

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

        if (randomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }
        Random.seed = seed;

        StartCoroutine(StartGenerationCoroutine());
    }

    void Update()
    {
        if (Input.GetKeyDown("space")) {
            if (randomSeed)
            {
                seed = Random.Range(0, int.MaxValue);
            }
            Random.seed = seed;
            debut = Time.realtimeSinceStartup;
            DestroyPrecedent();
            StartCoroutine(StartGenerationCoroutine());
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i=0; i<startPosCorridors.Count; i++) {
            Gizmos.DrawLine(startPosCorridors[i], endPosCorridors[i]);
        }

        if (visualize) {
            Gizmos.color = Color.red;
            for (int i=0; i<startPosCorridorsNull.Count; i++) {
                Gizmos.DrawLine(startPosCorridorsNull[i], endPosCorridorsNull[i]);
            }
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
                StartCoroutine(PlaceRooms());
                //StartCoroutine(ConnectRooms());
                break;

            default:
                print("Error: invalid algorithm index");
                break;
        }
        print("duration: " + (Time.realtimeSinceStartup - debut));
    }

    void DestroyPrecedent() {
        foreach(GameObject path in pathInstances) {
            Destroy(path);
        }
        foreach (Room room in roomsPlaced)
        {
            room.OnDestroyRoom();
        }
        roomsPlaced.Clear();

        pathInstances.Clear();
        //posRoomsPlaced.Clear();
        //sizeXPlaced.Clear();
        //sizeZPlaced.Clear();
        //roomFirstTileIndex.Clear();
        startPosCorridors.Clear();
        endPosCorridors.Clear();
        startPosCorridorsNull.Clear();
        endPosCorridorsNull.Clear();
        
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

    IEnumerator PlaceRooms() {

        // générer la première salle toujours à la même position
        startRoom = new Room(0, startRoomPos, new Vector2(Random.Range(minSizeX, maxSizeX), Random.Range(minSizeZ, maxSizeZ)));
        PlaceRoomInstance(startRoom);
        Room currentRoom = startRoom;

        for (int i=1; i<nbRooms; i++) {
            Vector3 roomPos = currentRoom.GetPos() + new Vector3(Random.Range(minPosX, maxPosX), 0, Random.Range(minPosZ, maxPosZ));
            Vector2 size = new Vector2(Random.Range(minSizeX, maxSizeX), Random.Range(minSizeZ, maxSizeZ));
            Room room = new Room(roomsPlaced.Count, roomPos, size);

            if  (CanBePlaced(room))
            {
                PlaceRoomInstance(room);
                if (visualize)
                {
                    yield return null;
                }
                ConnectRooms(currentRoom, room);
                currentRoom = room;
            }
        }
        ConnectRooms();
    }

    bool CanBePlaced(Room room)
    {
        foreach (Room other in roomsPlaced)
        {
            if (other != room && room.Overlaps(other, margin))
            {
                return false;
            }
        }
        return true;
    }

    void PlaceRoomInstance(Room room)
    {
        List<GameObject> tiles = new();
        for (int x = (int)room.GetPos().x; x < room.GetPos().x + room.GetSize().x; x++)
        {
            for (int z = (int)room.GetPos().z; z < room.GetPos().z + room.GetSize().y; z++)
            {
                tiles.Add(Instantiate(pathTile, new Vector3(x, 0, z), Quaternion.identity));
            }
        }
        room.SetRoomTiles(tiles);
        roomsPlaced.Add(room);
    }

    Room GetLeftmost()
    {
        Room leftmost = roomsPlaced[0];
        foreach(Room other in roomsPlaced)
        {
            if (other.GetPos().z < leftmost.GetPos().z)
            {
                leftmost = other;
            }
        }
        return leftmost;
    }

    Room GetRightmost()
    {
        Room rightmost = roomsPlaced[0];
        foreach (Room other in roomsPlaced)
        {
            if (other.GetPos().z > rightmost.GetPos().z)
            {
                rightmost = other;
            }
        }
        return rightmost;
    }

    Room FindClosestRoom(Room current, List<Room> rooms, bool free)
    {
        Room best = null;
        float minDistance = float.MaxValue;
        foreach (Room other in rooms)
        {
            float distance = utils.Distance(current.GetPos(), other.GetPos());
            if (other.GetFree() == free && distance < minDistance) {
                minDistance = distance;
                best = other;
            }
        }
        return best;
    }


    private void ConnectRooms(Room a, Room b)
    {
        if (a.GetConnexions().Contains(b)) return;

        a.AddConnexion(b);
        b.AddConnexion(a);

        startPosCorridors.Add(a.GetCenter());
        endPosCorridors.Add(b.GetCenter());
    }


    void ConnectRooms() {
        startRoom = GetLeftmost();
        bossRoom = GetRightmost();

        startRoom.SetRoomType(RoomType.Start);
        bossRoom.SetRoomType(RoomType.Boss);
        startRoom.SetFree(false);






        //for (int i = 0; i < mainPath.Count - 1; i++)
        //{
        //    ConnectRooms(mainPath[i], mainPath[i + 1]);
        //}


        //List<Room> orderedPath = new List<Room>();
        //List<Room> unvisited = new List<Room>(roomsPlaced);

        //Room current = startRoom;
        //orderedPath.Add(current);
        //unvisited.Remove(current);

        //while (unvisited.Count > 0)
        //{
        //    Room next = unvisited
        //        .OrderBy(r => Vector2.Distance(current.GetCenter(), r.GetCenter()))
        //        .First();

        //    orderedPath.Add(next);
        //    unvisited.Remove(next);

        //    current.AddConnexion(next);
        //    next.AddConnexion(current);
        //    startPosCorridors.Add(current.GetCenter());
        //    endPosCorridors.Add(next.GetCenter());

        //    current = next;
        //}









        //List<Room> path = new List<Room>();
        //Room current = startRoom;
        //path.Add(current);

        //while (current != bossRoom)
        //{
        //    current.SetFree(false);
        //    Room next = FindClosestRoom(current, roomsPlaced, true);
        //    if (next == null) break;
        //    current.AddConnexion(next);
        //    next.AddConnexion(current);
        //    path.Add(next);
        //    startPosCorridors.Add(current.GetCenter());
        //    endPosCorridors.Add(next.GetCenter());

        //    current = next;
        //}

        //// choose locked room

        //int indexLockedRoom = Random.Range(0, path.Count-1);
        //Room lockedRoom = path[indexLockedRoom];
        //lockedRoom.SetRoomType(RoomType.Lock);

        //foreach (Room salle in roomsPlaced)
        //{
        //    if (!path.Contains(salle))
        //    {
        //        Room candidate = FindClosestRoom(salle, path, false);
        //        salle.AddConnexion(candidate);
        //        candidate.AddConnexion(salle);
        //        salle.SetFree(false);
        //        path.Add(salle);
        //        startPosCorridors.Add(salle.GetCenter());
        //        endPosCorridors.Add(candidate.GetCenter());
        //    }
        //}

        //yield return null;
        ColorRooms();


        //for (int i=0; i<posRoomsPlaced.Count; i++) {
        //    for (int j=i+1; j<posRoomsPlaced.Count; j++) {
        //        Vector3 dir = (GetCenter(j) - GetCenter(i)).normalized;
        //        float distance = utils.Distance(GetCenter(i), GetCenter(j))-1;
        //        RaycastHit[] hits;
        //        hits = Physics.SphereCastAll(GetCenter(i), corridorWidthDetection, dir, distance);
        //        if (hits.Length > 0 && HitsDetectedNotInRooms(hits, i, j)) 
        //        {
        //            startPosCorridorsNull.Add(GetCenter(i));
        //            endPosCorridorsNull.Add(GetCenter(j));
        //        } else {
        //            startPosCorridors.Add(GetCenter(i));
        //            endPosCorridors.Add(GetCenter(j));
        //        }
        //        if (visualize) {
        //            yield return null;
        //        }
        //    }
        //}
        //yield return null;
        //startPosCorridorsNull.Clear();
        //endPosCorridorsNull.Clear();
    }

    void ColorRooms()
    {
        foreach (Room room in roomsPlaced)
        {
            room.ColorRoom(materials);
        }
    }

    //bool HitsDetectedNotInRooms(RaycastHit[] hits, int indexRoomA, int indexRoomB) {
    //    foreach (RaycastHit hit in hits)
    //    {
    //        GameObject hitObject = hit.collider.gameObject;
    //        if (pathInstances.Contains(hitObject) && !PathInRoom(hitObject, indexRoomA) && !PathInRoom(hitObject, indexRoomB)) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //bool PathInRoom(GameObject path, int roomIndex) {
    //    if (roomIndex >= roomFirstTileIndex.Count-1) {
    //        return pathInstances.IndexOf(path) >= roomFirstTileIndex[roomIndex];
    //    }
    //    return pathInstances.IndexOf(path) >= roomFirstTileIndex[roomIndex] && pathInstances.IndexOf(path) < roomFirstTileIndex[roomIndex+1];
    //}
}