using UnityEngine;

public class HighlightOnHover : MonoBehaviour
{
    public GameObject modelToHighlight;
    private Color defaultColor;

    public void EnableOutline()
    {
        Renderer rend = modelToHighlight.GetComponent<Renderer>();

        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Color newColor = rend.material.color;
        defaultColor = newColor;
        newColor.r = newColor.r * 1.2f;
        rend.material.color = newColor;
    }

    public void DisableOutline()
    {
        Renderer rend = modelToHighlight.GetComponent<Renderer>();
        rend.material.color = defaultColor;
    }
}
