using System.Collections.Generic;
using UnityEngine;

public class NeighborDetector : MonoBehaviour
{
    public List<GameObject> neighbors = new List<GameObject>();
    [SerializeField] float detectionRadius = 3f;
    [SerializeField] LayerMask tileLayer;

    // public void DetectNeighbors(Vector3 position)
    // {
    //     transform.localPosition = position;
    // }

    public void DetectNeighbors(Vector3 position)
    {
        neighbors.Clear(); // Réinitialise la liste

        Collider[] hits = Physics.OverlapSphere(position, detectionRadius, tileLayer);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Tile"))// && hit.gameObject != transform.parent.gameObject)
            {
                neighbors.Add(hit.gameObject);
            }
        }
    }

    public List<GameObject> GetNeighbors()
    {
        return neighbors;
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Tile"))// && !other.gameObject.Equals(transform.parent.gameObject))
    //     {
    //         neighbors.Add(other.gameObject);
    //         print(other.gameObject.name + " detected by " + transform.parent.gameObject.name);
    //     }
    // }
}
