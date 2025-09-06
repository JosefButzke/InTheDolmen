using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CloudsGenerator))]
public class CloudGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CloudsGenerator generator = (CloudsGenerator)target;

        if (GUILayout.Button("Regenerate Clouds"))
        {
        }
    }
}
