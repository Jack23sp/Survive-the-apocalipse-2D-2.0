using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScriptBoy.Fly2D
{
    [CustomEditor(typeof(F2DLandingMask))]
    public class F2DLandingMaskEditor : Editor
    {
        private F2DLandingMask landingMask;

        private SerializedProperty type;
        private SerializedProperty collider;
        private SerializedProperty radius;


        private void OnEnable()
        {
            landingMask = target as F2DLandingMask;
            if (landingMask.name.StartsWith("GameObject")) landingMask.name = "LandingMask";


            type = serializedObject.FindProperty("type");
            collider = serializedObject.FindProperty("collider");
            radius = serializedObject.FindProperty("radius");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(type);
            if (landingMask.type == AreaType.Collider)
            {
                EditorGUILayout.PropertyField(collider);
            }
            else
            {
                EditorGUILayout.PropertyField(radius);
            }

            if (GUI.changed) serializedObject.ApplyModifiedProperties();
        }
    }
}
