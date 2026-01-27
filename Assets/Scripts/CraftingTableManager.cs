using UnityEngine;
using UnityEngine.UIElements;

public class CraftingTableManager : Interactable
{
    Light lightGameObject = null;

    public override void OnHoverEnter()
    {
        Debug.Log("Show UI");
        UIManager.Instance.ToggleInteractableUI(true, "Crafting Table", Color.gray);
        if (lightGameObject == null)
        {
            lightGameObject = GetComponentInChildren<Light>();
        }
        else
        {
            lightGameObject.enabled = true;
        }
    }

    public override void OnHoverExit()
    {
        Debug.Log("Remove UI");
        UIManager.Instance.ToggleInteractableUI(false);

        if (lightGameObject != null)
        {
            lightGameObject.enabled = false;
        }
    }

    public override void OnInteract()
    {
        Debug.Log("Collected");
        UIManager.Instance.ToggleCraftingTableUI(true);
    }
}
