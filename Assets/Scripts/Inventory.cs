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
    public UIDocument HUDUI;

    [SerializeField]
    public InventoryItem[] items = new InventoryItem[50];

    [SerializeField]
    public InventoryItem[] quickBarItems = new InventoryItem[8];

    private Color slotDefaultTintColor = new Color(1f, 0f, 0f, 0.5f);
    private Color slotSelectedTintColor = new Color(1f, 0.6f, 0f, 1f);

    void OnValidate()
    {
        UpdateQuickBarUI();

        UpdateInventoryUI();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
        UpdateQuickBarUI();
    }

    public bool AddItem(Item newItem, int amount)
    {
        bool canBeAdded = false;

        for (int i = 0; i < 50; i++)
        {
            InventoryItem itemSlot = items[i];
            if (!itemSlot.item)
            {
                items[i].item = newItem;
                items[i].quantity = amount;
                break;
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

    public void UpdateInventoryUI()
    {
        var root = InventoryUI.rootVisualElement;

        if (root == null)
        {
            Debug.Log("UpdateInventoryUI" + "root line 73 null");
            return;
        }

        var slotsSprites = root.Query<VisualElement>(className: "item-slot-sprite").ToList();
        var slotsQuantity = root.Query<Label>(className: "quantity").ToList();

        for (int i = 0; i < slotsSprites.Count(); i++)
        {
            var slotSprite = slotsSprites[i];
            var slotText = slotsQuantity[i];

            var item = items[i];

            if (item.item)
            {
                // Set item image or label
                slotSprite.style.backgroundImage = new StyleBackground(item.item.icon);
                slotText.style.backgroundColor = Color.white;
                slotText.style.color = Color.black;
                slotText.text = items[i].quantity.ToString();
            }
            else
            {
                slotSprite.style.backgroundImage = null;
                slotText.style.backgroundColor = Color.clear;
                slotText.style.color = Color.clear;
                slotText.text = null;
            }

        }
    }

    public void UpdateQuickBarUI()
    {
        if (HUDUI == null)
        {
            Debug.LogWarning("HUDUI is null in UpdateQuickBarUI");
            return;
        }

        var root = HUDUI.rootVisualElement;

        if (root == null)
        {
            Debug.LogWarning("Root is null in UpdateQuickBarUI");
            return;
        }

        var slotsSprites = root.Query<VisualElement>(className: "item-slot-sprite").ToList();
        var slotsQuantity = root.Query<Label>(className: "quantity").ToList();

        for (int i = 0; i < quickBarItems.Count(); i++)
        {
            VisualElement slotSprite = slotsSprites[i];
            Label slotText = slotsQuantity[i];
            InventoryItem item = quickBarItems[i];

            if (quickBarItems[i].item)
            {
                // Set item image or label
                slotSprite.style.backgroundImage = new StyleBackground(item.item.icon);
                slotText.text = items[i].quantity.ToString();

                if (item.isSelected)
                {
                    var slotButton = root.Query<VisualElement>(className: "slot").AtIndex(i);
                    slotButton.style.unityBackgroundImageTintColor = slotSelectedTintColor;
                }
            }
            else
            {
                slotSprite.style.backgroundImage = null;
                slotText.text = null;
            }
        }
    }
}
