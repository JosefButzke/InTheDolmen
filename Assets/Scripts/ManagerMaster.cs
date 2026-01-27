using UnityEngine;

public class ManagerMaster : MonoBehaviour
{
    private void OnEnable()
    {
        InventoryItem.OnCablesManagerRequired += OnCablesManagerRequired;
    }

    private void OnCablesManagerRequired()
    {
        GetComponent<CablesManager>().enabled = true;
    }
}
