using UnityEditor;
using UnityEngine;
using StaticMeshes;

namespace StaticMeshes.Editor
{
    [CustomEditor(typeof(SphericalGridData))]
    public class SphericalGridDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SphericalGridData sphericalGridData = (SphericalGridData)target;
            if (GUILayout.Button("Bake grid data"))
            {
                sphericalGridData.BakeData();
                EditorUtility.SetDirty(target);
            }
        }
    }

}