using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldGenerator generator = (WorldGenerator)target;

        if(GUILayout.Button("Regenerate World")) {
            generator.RegenerateWorld();
        }
    }
}
