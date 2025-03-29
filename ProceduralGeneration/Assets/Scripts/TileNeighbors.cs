using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileNeighbors : MonoBehaviour
{
    [SerializeField] NeighborDetector neighborDetector;
    [SerializeField] Vector3[] neighborPositions;
    public List<GameObject> neighbors = new List<GameObject>();


    public void DetectNeighbors()
    {
        for (int i = 0; i < 26; i++)
        {
            neighborDetector.DetectNeighbors(neighborPositions[i]);
        }
        neighbors = neighborDetector.GetNeighbors();
    }
}
