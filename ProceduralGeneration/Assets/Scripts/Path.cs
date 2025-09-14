using UnityEngine;
using System.Collections.Generic;

public class Path
{
    Room roomA;
    Room roomB;
    float cost;
    List<Vector3> vertices;

    public Path(Room roomA, Room roomB) 
    {
        this.roomA = roomA;
        this.roomB = roomB;
    }

    public void SetCost(float cost) {
        this.cost = cost;
    }

    public void AddVertex(Vector3 vertex) {
        vertices.Add(vertex);
    }

    public Vector3 GetPosA() {
        return roomA.GetPos();
    }

    public Vector3 GetPosB() {
        return roomB.GetPos();
    }
}
