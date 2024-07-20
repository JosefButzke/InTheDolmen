using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Item")]
public class Item : ScriptableObject
{
    public string itemName = "Item Name";
    public Sprite sprite;
    public int maxStack = 10;
    public bool wearable = false;
    public bool consumable = false;
    public bool canUseInCrafting = false;
    public bool canStack = false;
}
