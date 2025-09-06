using UnityEngine;

public class CheckHoverObject : MonoBehaviour
{
    private GameObject currentOutlineObject;
    private Camera playerCamera;
    private InputActions inputActions;

    public void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        playerCamera = Player.Instance.cameraPlayer.GetComponent<Camera>();
    }

    void Update()
    {
        // Get the center of the screen
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // Create a ray from the camera through the screen center
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        // Cast the ray
        if (Physics.Raycast(ray, out hit, Parameters.interactableMaxDistance))
        {
            if (currentOutlineObject != hit.collider.gameObject)
            {
                // REMOVE OUTLINE
                if (currentOutlineObject)
                {
                    currentOutlineObject.GetComponent<HighlightOnHover>().DisableOutline();
                    currentOutlineObject = null;
                }


                // ADD OUTLINE
                if (hit.collider.CompareTag(Parameters.interactableTag))
                {
                    currentOutlineObject = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<HighlightOnHover>().EnableOutline();
                }
            }

        }
        else
        {
            if (currentOutlineObject)
            {
                currentOutlineObject.GetComponent<HighlightOnHover>().DisableOutline();
                currentOutlineObject = null;
            }
        }

        if (inputActions.Player.Interact.triggered)
        {
            Debug.Log("Try Interact");
            // ANY FOCUSED OBJECT
            if (!currentOutlineObject)
            {
                return;
            }
            Debug.Log("Can Interact");
            Resource resource = currentOutlineObject.gameObject.GetComponent<Resource>();
            if (resource)
            {
                Debug.Log("OnInteract Called");
                resource.OnInteract();
            }
        }
    }
}
