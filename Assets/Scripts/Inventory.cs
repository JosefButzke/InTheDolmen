using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance
    {
        get; private set;
    }

    public UIDocument InventoryUI;

    [SerializeField]
    public InventoryItem[] items = new InventoryItem[40];

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }

    public bool AddItem(Item newItem, int amount)
    {
        bool canBeAdded = false;
        bool canBeStacked = false;
        int amountTmp = amount;
        int amountLeft = amount;

        for (int i = 0; i < 40; i++)
        {
            InventoryItem itemSlot = items[i];
            if (!itemSlot.item)
            {
                canBeAdded = true;
                break;
            }
            if (itemSlot.item == newItem)
            {
                amountTmp = amountTmp - itemSlot.item.maxStack - itemSlot.quantity;

                if (amountTmp <= 0)
                {
                    canBeStacked = true;
                    break;
                }
            }
        }

        if (canBeStacked)
        {
            for (int i = 0; i < 40; i++)
            {
                InventoryItem itemSlot = items[i];
                if (itemSlot.item == newItem)
                {
                    int amountLeftSlot = itemSlot.item.maxStack - itemSlot.quantity;
                    if (amountLeftSlot >= amountLeft)
                    {
                        amountLeft = 0;
                        itemSlot.quantity = itemSlot.quantity + amountLeft;
                    }
                    else
                    {
                        amountLeft = amountLeft - amountLeftSlot;
                        itemSlot.quantity = itemSlot.quantity + amountLeftSlot;
                    }
                    if (amountLeft <= 0)
                    {
                        break;
                    }
                }
            }
        }

        if (canBeAdded)
        {
            for (int i = 0; i < 40; i++)
            {
                InventoryItem itemSlot = items[i];
                if (!itemSlot.item)
                {
                    items[i].item = newItem;
                    items[i].quantity = amount;
                    break;
                }
            }
        }

        return canBeAdded;
    }

    public bool RemoveItem(int index)
    {
        items[index].item = null;
        items[index].quantity = 0;
        return true;
    }

    public void UpdateUI()
    {
        var root = InventoryUI.rootVisualElement;

        var slots = root.Query<VisualElement>(className: "item-slot-sprite").ToList();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            if (i < items.Count())
            {
                var item = items[i];
                if (item.item)
                {
                    Debug.Log(i + item.item.itemName);
                }
                if (item.item)
                {
                    // Set item image or label
                    slot.style.backgroundImage = new StyleBackground(item.item.icon);
                }
                else
                {
                    slot.style.backgroundImage = null;
                }
            }

        }
    }
}
