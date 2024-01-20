using UnityEditor;
using UnityEngine;

namespace StaticMeshes.Editor
{
    [CustomEditor(typeof(SpawnerSettings))]
    [CanEditMultipleObjects]
    public class SpawnerSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty _animateObjectsProp;
        SerializedProperty _meshProp;
        SerializedProperty _matProp;
        SerializedProperty _sphereRadiusProp;
        SerializedProperty _objectsCountProp;
        SerializedProperty _sphericalGridDataProp;

        void OnEnable()
        {
            // Fetch the objects from the ScriptableObject script to display in the inspector
            _animateObjectsProp = serializedObject.FindProperty("AnimateObjects");
            _meshProp = serializedObject.FindProperty("Mesh");
            _matProp = serializedObject.FindProperty("Material");
            _sphereRadiusProp = serializedObject.FindProperty("SphereRadius");
            _objectsCountProp = serializedObject.FindProperty("ObjectsCount");
            _sphericalGridDataProp = serializedObject.FindProperty("SphericalGridData");
        }

        public override void OnInspectorGUI()
        {
            SpawnerSettings spawnerSettings = (SpawnerSettings)target;

            EditorGUILayout.PropertyField(_meshProp, new GUIContent("Meshd"));
            EditorGUILayout.PropertyField(_matProp, new GUIContent("Material"));
            EditorGUILayout.PropertyField(_sphereRadiusProp, new GUIContent("Sphere Radius"));
            if (spawnerSettings.SphericalGridData)
            {
                int maxObjectCount = spawnerSettings.SphericalGridData.Length;
                EditorGUILayout.LabelField($"Maximum number of objects: {maxObjectCount}");
                _objectsCountProp.intValue = Mathf.Clamp(_objectsCountProp.intValue, 0, maxObjectCount);
                _objectsCountProp.intValue = EditorGUILayout.IntSlider(_objectsCountProp.intValue, 0, maxObjectCount);
            }
            EditorGUILayout.PropertyField(_sphericalGridDataProp, new GUIContent("Spherical Grid Data"));

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //The variables and GameObject from the MyGameObject script are displayed in the Inspector with appropriate labels
            EditorGUILayout.LabelField("Optional Settings:");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Adds a little rotation in runtime to every entity spawned");
            EditorGUILayout.PropertyField(_animateObjectsProp, new GUIContent("Animate Objects"));

            // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
