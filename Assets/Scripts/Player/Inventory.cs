using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public class InventoryItem
    {
        public Item item;
        public int quantity;
    }

    private List<InventoryItem> items = new List<InventoryItem>();

    private void Start()
    {
        Instance = this;
    }

    public void AddItem(Item item, int quantity)
    {
        InventoryItem inventoryItemFounded = items.Find(i => i.item == item);

        if (inventoryItemFounded == null)
        {
            InventoryItem newInventoryItem = new InventoryItem
            {
                item = item,
                quantity = quantity
            };

            items.Add(newInventoryItem);
            InventoryUIManager.Instance.AddItem(newInventoryItem);
        }
        else
        {
            inventoryItemFounded.quantity += quantity;
            InventoryUIManager.Instance.AddItem(inventoryItemFounded);
        }
    }
}
