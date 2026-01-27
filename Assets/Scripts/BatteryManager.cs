using System.Collections.Generic;
using UnityEngine;

public class BatteryManager : Interactable
{
    private OutletInteractable outletIn;
    private OutletInteractable outletOut;
    private float totalCharge = 10f;
    public float currentCharge = 0f;

    [SerializeField]
    public List<GameObject> cells;

    public Material cellFullMaterial;
    public Material cellEmptyMaterial;

    // Update is called once per frame
    void Update()
    {
        if (outletIn == null)
        {
            return;
        }

        if (currentCharge < totalCharge)
        {
            currentCharge = currentCharge + outletIn.energyOut * Time.deltaTime / 3f;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            Renderer render = cells[i].GetComponent<Renderer>();
            if (currentCharge >= i + 1)
            {
                if (render.material != cellFullMaterial)
                {
                    render.material = cellFullMaterial;
                }
            }
            else
            {
                if (render.material != cellEmptyMaterial)
                {
                    render.material = cellEmptyMaterial;
                }
            }
        }
    }

    public void SetEnergyInput(OutletInteractable o)
    {
        outletIn = o;
    }

    public override void OnHoverEnter()
    {
        Debug.Log("Show UI");
        UIManager.Instance.ToggleInteractableUI(true, "Battery", Color.gray);
    }

    public override void OnHoverExit()
    {
        Debug.Log("Remove UI");
        UIManager.Instance.ToggleInteractableUI(false);
    }

    public override void OnInteract()
    {
        Debug.Log("Collected");
        UIManager.Instance.ToggleCraftingTableUI(true);
    }
}
