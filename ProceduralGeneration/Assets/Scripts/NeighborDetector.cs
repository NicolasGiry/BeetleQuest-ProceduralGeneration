using System.Collections.Generic;
using UnityEngine;

public class NeighborDetector : MonoBehaviour
{
    public List<GameObject> neighbors = new List<GameObject>();

    public void DetectNeighbors(Vector3 position)
    {
        transform.localPosition = position;
    }

    public List<GameObject> GetNeighbors()
    {
        return neighbors;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile") && !other.gameObject.Equals(transform.parent.gameObject))
        {
            neighbors.Add(other.gameObject);
        }
    }
}
