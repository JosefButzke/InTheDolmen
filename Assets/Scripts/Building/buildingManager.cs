using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum JointPosition
{
    Top,
    Bottom,
    Left,
    Right
}

public enum JointType
{
    Floor,
    Wall,
    Roof,
    DoorFrame,
    Door
}

public class BuildingManager : MonoBehaviour
{
    public GameObject floor;
    public GameObject wall;
    public GameObject roof;
    public GameObject doorFrame;
    public GameObject door;

    public GameObject currentObj;

    public LayerMask layerMaskBuilding;

    private GameObject partInstanciated;

    private void Start()
    {
        currentObj = wall;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            partInstanciated = null;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            currentObj = wall;
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            currentObj = floor;
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentObj = roof;
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            currentObj = doorFrame;
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            currentObj = door;
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        CreateOlogramOnHover();  
    }

    private void CreateOlogramOnHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50f, layerMaskBuilding))
        {
            if(!partInstanciated)
            {
                Transform transform2 = hit.collider.transform;

                partInstanciated = Instantiate(currentObj, transform2);
            }
        } else
        {
            if(partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }
    }
}
