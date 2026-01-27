using UnityEngine;
using UnityEngine.UIElements;

public class CraftingTableManager : MonoBehaviour
{
    public UIDocument craftingTableUI;

    void Awake()
    {
        craftingTableUI.enabled = false;
    }
}
