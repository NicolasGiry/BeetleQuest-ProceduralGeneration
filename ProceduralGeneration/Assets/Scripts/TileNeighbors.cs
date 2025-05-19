using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileNeighbors : MonoBehaviour
{
    //[SerializeField] List<GameObject> neighbors = new List<GameObject>();
    //[SerializeField] float detectionRadius;
    //[SerializeField] LayerMask tileLayer;
    //[SerializeField] Vector3 offset;

    //public void DetectNeighbors()
    //{
    //    //neighbors.Clear();

    //    Collider[] hits = Physics.OverlapSphere(transform.position + offset, detectionRadius, tileLayer);

    //    foreach (Collider hit in hits)
    //    {
    //        if (hit.CompareTag("Tile") && hit.gameObject != transform.gameObject)
    //        {
    //            neighbors.Add(hit.gameObject);
    //        }
    //    }
    //}

    //public List<GameObject> GetNeighbors() {
    //    return neighbors;
    //}

    //void OnDrawGizmosSelected()
    //{
    //    // Draw a yellow sphere at the transform's position
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(transform.position + offset, detectionRadius);
    //}
}
