using UnityEditor;
using UnityEngine;

public class OutletInteractable : Interactable
{
    public CableConnectionType connectionType;
    private Material allowedConnectionMaterial;
    private Material blockedConnectionMaterial;
    private Material defaultConnectionMaterial;
    public float energyIn = 0f;
    public float energyOut = 0f;

    void Awake()
    {
        allowedConnectionMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OutletOutputAllow.mat");
        blockedConnectionMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OutletOutputBlocked.mat");
        defaultConnectionMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OutletOutputDefault.mat");
    }

    override public void OnHoverEnter()
    {
        MeshRenderer render = gameObject.GetComponent<MeshRenderer>();

        if (CablesManager.Instance.pendingConnectionType == null || CablesManager.Instance.pendingConnectionType == connectionType)
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
        MeshRenderer render = gameObject.GetComponent<MeshRenderer>();
        render.material = defaultConnectionMaterial;
    }

    override public void OnInteract()
    {

    }
}
