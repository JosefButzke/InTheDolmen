using UnityEngine;
using UnityEngine.UI;

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
    public GameObject wall;
    public GameObject floor;
    public GameObject roof;
    public GameObject doorFrame;
    public GameObject door;

    public GameObject floorButton;
    public GameObject wallButton;
    public GameObject roofButton;
    public GameObject doorFrameButton;
    public GameObject doorButton;

    public GameObject currentObj;
    public GameObject buttonSelected;

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnChangeSelection(floor, floorButton);
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnChangeSelection(wall, wallButton);
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnChangeSelection(roof, roofButton);
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnChangeSelection(doorFrame, doorFrameButton);
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            OnChangeSelection(door, doorButton);
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }

        CreateOlogramOnHover();
    }

    private void OnChangeSelection(GameObject prefab, GameObject button)
    {
        // clean styles button selected
        if (buttonSelected != null)
        {

            buttonSelected.GetComponent<Image>().color = new Color(255, 255, 255);
        }
        button.GetComponent<Image>().color = new Color(0, 255, 217);
        currentObj = prefab;
        buttonSelected = button;
    }

    private void CreateOlogramOnHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50f, layerMaskBuilding))
        {
            if (!partInstanciated)
            {
                Transform transform2 = hit.collider.transform;

                partInstanciated = Instantiate(currentObj, transform2);
            }
        }
        else
        {
            if (partInstanciated != null)
            {
                Destroy(partInstanciated);
            }
        }
    }
}
