using UnityEngine;

public class Interactable : MonoBehaviour
{
    virtual public void OnHoverEnter()
    {
        Debug.Log("[BASE] Show UI");
    }

    virtual public void OnHoverExit()
    {
        Debug.Log("[BASE] Remove UI");
    }

    virtual public void OnInteract()
    {
        Debug.Log("[BASE] Collected");
    }
}
