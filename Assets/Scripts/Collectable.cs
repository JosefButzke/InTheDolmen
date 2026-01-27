using UnityEngine;

public class Collectable : Interactable
{
    public Item reward;
    public int amount;

    override public void OnHoverEnter()
    {
        Debug.Log("Show UI");
        UIManager.Instance.ToggleInteractableUI(true, "COLLECT");
    }

    override public void OnHoverExit()
    {
        Debug.Log("Remove UI");
        UIManager.Instance.ToggleInteractableUI(false);
    }

    override public void OnInteract()
    {
        Debug.Log("Collected");
        Inventory.Instance.AddItem(reward, amount);
        Destroy(gameObject);
        UIManager.Instance.ToggleInteractableUI(false);
    }
}
