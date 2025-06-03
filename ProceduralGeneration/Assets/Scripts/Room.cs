using UnityEngine;
using System.Collections.Generic;

public enum RoomType
{
    Start,
    Boss,
    Key,
    Lock,
    Principal,
    Secondary
}

public class Room
{
    int id;
    Vector3 pos;
    Vector2 size;
    RoomType type;
    List<Room> connexions = new();
    List<GameObject> tiles = new();
    bool free = true;
    public Vector3 startOffset = new Vector3(-11, 0, -10);
    public Vector3 endOffset = new Vector3(-11, 0, 3);

    public Room(int id, Vector3 pos, Vector2 size, RoomType type = RoomType.Principal)
    {
        this.id = id;
        this.pos = pos;
        this.size = size;
        this.type = type;
    }

    public int GetId()
    {
        return id;
    }
    public Vector3 GetPos()
    {
        return pos;
    }
    public Vector3 GetStartPos()
    {
        return GetCenter() + startOffset;
    }
    public Vector3 GetEndPos()
    {
        return GetCenter() + endOffset;
    }
    public Vector2 GetSize()
    {
        return size;
    }
    public RoomType GetRoomType()
    {
        return type;
    }
    public List<Room> GetConnexions()
    {
        return connexions;
    }
    public Vector3 GetCenter()
    {
        return new Vector3(pos.x + size.x/2, 0, pos.z + size.y/2);
    }
    public bool GetFree()
    {
        return free;
    }
    public void SetFree(bool free)
    {
        this.free = free;
    }
    public void SetRoomType(RoomType roomType)
    {
        type = roomType;
    }
    public void SetRoomTiles(List<GameObject> tiles)
    {
        this.tiles = tiles;
    }
    public void OnDestroyRoom()
    {
        foreach (GameObject tile in tiles)
        {
            GameObject.Destroy(tile);
        }
    }

    public bool Overlaps(Room other, float margin)
    {
        return !(pos.x + size.x + margin <= other.pos.x ||
                 pos.x - margin >= other.pos.x + other.size.x ||
                 pos.z + size.y + margin <= other.pos.z ||
                 pos.z - margin >= other.pos.z + other.size.y);
    }

    public void ColorRoom(List<Material> materials)
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<MeshRenderer>().material = materials[(int)type];
        }
    }

    public void AddConnexion(Room other)
    {
        connexions.Add(other);
    }
}
