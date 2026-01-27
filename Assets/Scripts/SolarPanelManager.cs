using UnityEngine;

public class SolarPanelManager : Interactable
{

    public override void OnHoverEnter()
    {
        Debug.Log("Show UI");
        UIManager.Instance.ToggleInteractableUI(true, "Solar Panel", Color.gray);
    }

    public override void OnHoverExit()
    {
        Debug.Log("Remove UI");
        UIManager.Instance.ToggleInteractableUI(false);
    }

    public override void OnInteract()
    {
        Debug.Log("Collected");
        UIManager.Instance.ToggleCraftingTableUI(true);
    }
}
