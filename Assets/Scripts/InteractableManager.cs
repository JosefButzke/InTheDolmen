using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    [Header("Hovering")]
    public float maxDistance = 5f;
    public GameObject lastHoveredObject;

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(Player.Instance.cameraPlayer.transform.position, Player.Instance.cameraPlayer.transform.forward);
        RaycastHit hit;
        // Debug.DrawRay(ray.origin, ray.direction * 5f, Color.green);
        // Cast a ray forward from the camera
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            GameObject hitObj = hit.collider.gameObject;


            // If new object hovered
            if (hitObj != lastHoveredObject)
            {
                Debug.Log("Hovering");
                // if previously hovering something, call OnHoverExit
                if (lastHoveredObject != null)
                {
                    Debug.Log("Hovering New");
                    Interactable prevHover = lastHoveredObject.GetComponent<Interactable>();
                    if (prevHover != null)
                        prevHover.OnHoverExit();
                }

                // call OnHoverEnter on the new one
                Interactable hover = hitObj.GetComponent<Interactable>();
                if (hover != null)
                {
                    hover.OnHoverEnter();
                }


                lastHoveredObject = hitObj;
            }
        }
        else
        {
            // if ray hits nothing, stop hovering
            if (lastHoveredObject != null)
            {
                Interactable prevHover = lastHoveredObject.GetComponent<Interactable>();
                if (prevHover != null)
                    prevHover.OnHoverExit();
                lastHoveredObject = null;
            }
        }
    }
}
