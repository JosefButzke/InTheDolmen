using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        foreach (Vector3 vertex in GetComponent<MeshFilter>().sharedMesh.vertices)
        {
            Gizmos.DrawSphere(vertex, 0.1f);
        }
    }
}
