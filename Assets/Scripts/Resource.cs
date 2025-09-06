using UnityEngine;

public class Resource : MonoBehaviour
{
    public Item item;
    public int amount = 5;

    public void OnInteract()
    {
        Inventory.Instance.AddItem(item, amount);
        Destroy(gameObject);
    }
}
