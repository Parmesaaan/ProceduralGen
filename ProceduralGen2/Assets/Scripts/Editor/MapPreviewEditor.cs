using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapPreview), true)]
public class MapPreviewEditor : Editor {
    public override void OnInspectorGUI() {
        MapPreview mapPreview = (MapPreview) target;

        if (DrawDefaultInspector()) {
            if (mapPreview.autoUpdate) {
                mapPreview.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate")) {
            mapPreview.DrawMapInEditor();
        }
    }
}

