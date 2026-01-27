using System;
using System.Diagnostics;

[System.Serializable]
public class InventoryItem
{
    public Item item;
    public int quantity;
    public bool isSelected;

    public static event Action OnCablesManagerRequired;

    public InventoryItem(Item data, int qty)
    {
        item = data;
        quantity = qty;
    }

    public void OnSelect()
    {
        isSelected = true;
        Inventory.Instance.UpdateQuickBarUI();
        OnCablesManagerRequired?.Invoke();
    }
}