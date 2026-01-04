using UnityEngine;

public class Collectable : Interactable
{
    override public void OnHoverEnter()
    {
        Debug.Log("Show UI");
    }

    override public void OnHoverExit()
    {
        Debug.Log("Remove UI");
    }

    override public void OnInteract()
    {
        Debug.Log("Collected");
        Destroy(gameObject);
    }
}
