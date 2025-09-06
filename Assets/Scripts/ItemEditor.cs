using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Item item = (Item)target;

        // Draw icon preview
        if (item.icon != null)
        {
            GUILayout.Label(item.icon.texture, GUILayout.Width(128), GUILayout.Height(128));
        }
        // Show default inspector
        base.OnInspectorGUI();
    }
}