using UnityEngine;

public class Collectable : Interactable
{
    public Item reward;
    public int amount;

    override public void OnHoverEnter()
    {
        Debug.Log("Show UI");
    }

    override public void OnHoverExit()
    {
        Debug.Log("Remove UI");
    }

    override public void OnInteract()
    {
        Debug.Log("Collected");
        Inventory.Instance.AddItem(reward, amount);
        Destroy(gameObject);
    }
}
