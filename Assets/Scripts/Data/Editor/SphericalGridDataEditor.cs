using UnityEditor;
using UnityEngine;

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