using UnityEngine;
using UnityEngine.UIElements;

public class CraftingTable : Interactable
{
    public override void OnHoverEnter()
    {
        Debug.Log("Show UI");
    }

    public override void OnHoverExit()
    {
        Debug.Log("Remove UI");
    }

    public override void OnInteract()
    {
        Debug.Log("Collected");
        GetComponent<UIDocument>().enabled = true;
    }
}
