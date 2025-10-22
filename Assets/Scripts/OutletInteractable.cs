using UnityEditor;
using UnityEngine;

public class OutletInteractable : Interactable
{
    public OutletConnectionType connectionType;
    private Material allowedConnectionMaterial;
    private Material blockedConnectionMaterial;
    private Material defaultConnectionMaterial;

    void Awake()
    {
        allowedConnectionMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OutletOutputAllow.mat");
        blockedConnectionMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OutletOutputBlocked.mat");
        defaultConnectionMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OutletOutputDefault.mat");
    }

    override public void OnHoverEnter()
    {
        MeshRenderer render = model.GetComponent<MeshRenderer>();

        if (OutletManager.Instance.pendingConnectionType == null || OutletManager.Instance.pendingConnectionType == connectionType)
        {
            render.material = allowedConnectionMaterial;
        }
        else
        {
            render.material = blockedConnectionMaterial;
        }
    }

    override public void OnHoverExit()
    {
        MeshRenderer render = model.GetComponent<MeshRenderer>();
        render.material = defaultConnectionMaterial;
    }

    override public void OnInteract()
    {

    }
}
