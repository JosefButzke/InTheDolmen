using UnityEngine;

[System.Serializable] // Makes the struct visible in Inspector
public struct RequiredItem
{
    public Item item;
    public int quantity;
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Crafting/Item")]
public class CraftingItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public CraftingCategory category;
    public CraftingRequirement requirementToCraft;
    public RequiredItem[] requiredItems;
}
