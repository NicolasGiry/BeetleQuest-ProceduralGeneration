using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralGenerationManager : MonoBehaviour
{
    [Header("General Parameters:")]
    [SerializeField]
        int seed;
    [SerializeField] bool randomSeed;
    [SerializeField] bool visualize;
    [SerializeField] List<Material> materials = new();

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
    [SerializeField] int minSecondaryPathLenght;
    [SerializeField] int maxSecondaryPathLenght;
    [SerializeField] int margin;
    [SerializeField] int nbLock;
    [SerializeField] List<Room> roomsPlaced = new();
    [SerializeField] List<Vector3> startPosCorridors = new();
    [SerializeField] List<Vector3> endPosCorridors = new();
    [SerializeField] List<Vector3> startPosCorridorsNull = new();
    [SerializeField] List<Vector3> endPosCorridorsNull = new();
    Room startRoom;
    Room bossRoom;

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
    [SerializeField] List<GameObject> bossRooms;
    [SerializeField] List<GameObject> keyRooms;
    [SerializeField] List<GameObject> lockedRooms;
    [SerializeField] List<GameObject> basicRooms;

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
        StartGeneration();
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
            DestroyPrecedent();
            StartGeneration();
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

    void StartGeneration()
    {
        StartCoroutine(PlaceRooms());
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
        startPosCorridors.Clear();
        endPosCorridors.Clear();
        startPosCorridorsNull.Clear();
        endPosCorridorsNull.Clear();
    }

    // **************************** Pseudo Room Placement Generation **************************************

    IEnumerator PlaceRooms() {
        // générer la première salle tjrs à la même position
        startRoom = new Room(0, startRoomPos, new Vector2(Random.Range(minSizeX, maxSizeX), Random.Range(minSizeZ, maxSizeZ)));
        startRoom.SetRoomType(RoomType.Start);
        roomsPlaced.Add(startRoom);
        Room currentRoom = startRoom;

        for (int i=1; i<nbRooms; i++) {
            Vector3 roomPos = currentRoom.GetPos() + new Vector3(Random.Range(minPosX, maxPosX), 0, Random.Range(minPosZ, maxPosZ));
            Vector2 size = new Vector2(Random.Range(minSizeX, maxSizeX), Random.Range(minSizeZ, maxSizeZ));
            Room room = new Room(roomsPlaced.Count, roomPos, size);
            if  (CanBePlaced(room))
            {
                //PlaceRoomInstance(room);
                roomsPlaced.Add(room);
                if (visualize)
                {
                    yield return null;
                }
                ConnectRooms(currentRoom, room);
                currentRoom = room;
            }
        }
        bossRoom = currentRoom;
        bossRoom.SetRoomType(RoomType.Boss);
        for (int i=0; i<nbLock; i++)
        {
            StartCoroutine(AddLockedRoomAndKey());
        }
        foreach (Room room in roomsPlaced)
        {
            PlaceRoomInstance(room);
        }
    }

    IEnumerator AddLockedRoomAndKey()
    {
        Room lockedRoom;
        List<Room> candidates = roomsPlaced.Where(r => r.GetRoomType() == RoomType.Principal).ToList();

        if (candidates.Count > 0)
        {
            lockedRoom = candidates[Random.Range(1, candidates.Count)];
            lockedRoom.SetRoomType(RoomType.Lock);
            Room currentRoom = roomsPlaced[Random.Range(1, lockedRoom.GetId() - 2)];

            int pathLenght = Random.Range(minSecondaryPathLenght, maxSecondaryPathLenght);
            int iteration = 0;
            for (int i = 0; i < pathLenght && iteration < pathLenght * 100; i++, iteration++)
            {
                Vector3 roomPos = currentRoom.GetPos() + new Vector3(Random.Range(minPosX, maxPosX), 0, Random.Range(minPosZ, maxPosZ));
                Vector2 size = new Vector2(Random.Range(minSizeX, maxSizeX), Random.Range(minSizeZ, maxSizeZ));
                Room room = new Room(roomsPlaced.Count, roomPos, size);
                room.SetRoomType(RoomType.Secondary);
                if (CanBePlaced(room))
                {
                    //PlaceRoomInstance(room);
                    roomsPlaced.Add(room);
                    if (visualize)
                    {
                        yield return null;
                    }
                    ConnectRooms(currentRoom, room);
                    currentRoom = room;
                }
                else
                {
                    i--;
                }
            }
            currentRoom.SetRoomType(RoomType.Key);
        }
        else
        { 
            Debug.LogWarning("Aucune salle valide trouvée pour lockedRoom !");
        }
        ColorRooms();
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
        switch (room.GetRoomType())
        {
            case RoomType.Start:
                tiles.Add(Instantiate(startRooms[Random.Range(0, startRooms.Count-1)], room.GetPos(), Quaternion.identity)); 
                break;
            case RoomType.Boss:
                tiles.Add(Instantiate(bossRooms[Random.Range(0, bossRooms.Count - 1)], room.GetPos(), Quaternion.identity));
                break;
            case RoomType.Key:
                tiles.Add(Instantiate(keyRooms[Random.Range(0, keyRooms.Count - 1)], room.GetPos(), Quaternion.identity));
                break;
            case RoomType.Lock:
                tiles.Add(Instantiate(lockedRooms[Random.Range(0, lockedRooms.Count - 1)], room.GetPos(), Quaternion.identity));
                break;
            case RoomType.Principal: case RoomType.Secondary:
                tiles.Add(Instantiate(basicRooms[Random.Range(0, basicRooms.Count - 1)], room.GetPos(), Quaternion.identity));
                break;
        }

        room.SetRoomTiles(tiles);
        //roomsPlaced.Add(room);

        //List<GameObject> tiles = new();
        //for (int x = (int)room.GetPos().x; x < room.GetPos().x + room.GetSize().x; x++)
        //{
        //    for (int z = (int)room.GetPos().z; z < room.GetPos().z + room.GetSize().y; z++)
        //    {
        //        tiles.Add(Instantiate(pathTile, new Vector3(x, 0, z), Quaternion.identity));
        //    }
        //}
        //room.SetRoomTiles(tiles);
        //roomsPlaced.Add(room);
    }

    private void ConnectRooms(Room a, Room b)
    {
        if (a.GetConnexions().Contains(b)) return;
        a.AddConnexion(b);
        b.AddConnexion(a);
        startPosCorridors.Add(a.GetCenter());
        endPosCorridors.Add(b.GetCenter());
    }

    void ColorRooms()
    {
        foreach (Room room in roomsPlaced)
        {
            room.ColorRoom(materials);
        }
    }
}