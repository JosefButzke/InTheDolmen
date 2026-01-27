using UnityEngine;

public class FurnaceManager : Interactable
{
    public GameObject fire;
    private bool stop = false;
    private float speed = 180f;

    void Update()
    {
        if (stop)
        {
            fire.transform.Rotate(0f, 0f, speed * Time.deltaTime);
        }
    }

    public override void OnHoverEnter()
    {
        Debug.Log("Show UI");
        UIManager.Instance.ToggleInteractableUI(true, "Furnace", Color.gray);

        if (fire != null && !stop)
        {
            stop = true;
            fire.transform.position += Vector3.up * 0.15f;
        }

    }

    public override void OnHoverExit()
    {
        Debug.Log("Remove UI");
        UIManager.Instance.ToggleInteractableUI(false);

        if (fire != null && stop)
        {
            stop = false;
            fire.transform.position -= Vector3.up * 0.15f;
        }

    }

    public override void OnInteract()
    {
        Debug.Log("Collected");
        UIManager.Instance.ToggleCraftingTableUI(true);
    }
}
