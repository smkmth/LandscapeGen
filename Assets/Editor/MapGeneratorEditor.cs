using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapGenerator mapgen = (MapGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            mapgen.DrawMapInEditor();
        }

        if (DrawDefaultInspector())
        {
            if (mapgen.autoUpdate)
            {
                mapgen.DrawMapInEditor();
            }

        }

    }
}
