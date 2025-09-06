[System.Serializable]
public class InventoryItem
{
    public Item item;
    public int quantity;

    public InventoryItem(Item data, int qty)
    {
        item = data;
        quantity = qty;
    }
}