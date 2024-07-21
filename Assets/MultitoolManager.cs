using System.Collections;
using UnityEngine;

public class MultitoolManager : MonoBehaviour
{
    public static MultitoolManager Instance
    {
        get; private set;
    }

    public LayerMask currentLayerMask;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }


    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 15f, Color.white);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, 15f, currentLayerMask))
            {
                GameObject obj = hit.collider.gameObject;
                obj.GetComponent<BoxCollider>().enabled = false;
                obj.GetComponent<ResourceGatherAnimation>().enabled = true;
            }
        }
    }
}
