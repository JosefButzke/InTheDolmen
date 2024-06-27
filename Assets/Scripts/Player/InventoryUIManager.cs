using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static Inventory;

public class InventorySlot
{
    public InventoryItem inventoryItem;
    public GameObject slot;
}

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    public GameObject slotsContainer;
    public GameObject inventorySlot;

    private int slotsNumber = 40;
    private List<InventorySlot> slots = new List<InventorySlot>();
    private int nextEmptySlot = 0;

    void Start()
    {
        Instance = this;

        foreach (Transform child in slotsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotsNumber; i++)
        {
            GameObject slot = Instantiate(inventorySlot, slotsContainer.transform);
            slots.Add(new InventorySlot
            {
                inventoryItem = null,
                slot = slot
            });
        }
    }

    public void AddItem(InventoryItem inventoryItem)
    {
        InventorySlot inventorySlot = slots.Find(s => s?.inventoryItem?.item == inventoryItem.item);

        if(inventorySlot == null)
        {
            slots[nextEmptySlot].inventoryItem = inventoryItem;

            Transform slotImageGameObject = slots[nextEmptySlot].slot.transform.Find("ItemSprite");
            slotImageGameObject.gameObject.SetActive(true);
            slotImageGameObject.GetComponent<Image>().sprite = inventoryItem.item.sprite;

            Transform slotTextGameObject = slots[nextEmptySlot].slot.transform.Find("ItemQuantity");
            slotTextGameObject.gameObject.SetActive(true);
            slotTextGameObject.GetComponent<TextMeshProUGUI>().text = inventoryItem.quantity.ToString();

            int index = slots.FindIndex(s => s.inventoryItem == null);

            nextEmptySlot = index;
        } else
        {
            Transform slotImageGameObject = inventorySlot.slot.transform.Find("ItemSprite");
            slotImageGameObject.gameObject.SetActive(true);
            slotImageGameObject.GetComponent<Image>().sprite = inventoryItem.item.sprite;

            Transform slotTextGameObject = inventorySlot.slot.transform.Find("ItemQuantity");
            slotTextGameObject.gameObject.SetActive(true);
            slotTextGameObject.GetComponent<TextMeshProUGUI>().text = inventoryItem.quantity.ToString();
        }
    }

    public void RemoveItem()
    {

    }
}
