using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace ScriptBoy.Fly2D
{
    [CustomEditor(typeof(F2DFlyZoneManager))]
    public class F2DFlyZoneManagerEditor : Editor
    {
        /*
        private FlyZoneManager flyZoneManager;
        private SerializedProperty flyZones;
        private ReorderableList reorderablelist;

        private void OnEnable()
        {
            flyZoneManager = target as FlyZoneManager;
            flyZoneManager.zones.RemoveAll((x) => x == null);
            flyZones = serializedObject.FindProperty("zones");
            reorderablelist = new ReorderableList(serializedObject, flyZones);

            reorderablelist.drawHeaderCallback += DrawHeader;
            reorderablelist.drawElementCallback += DrawElement;
            reorderablelist.onRemoveCallback += RemoveElement;
            reorderablelist.onAddCallback += AddElement;


        }

        private void AddElement(ReorderableList list)
        {
            GameObject g = new GameObject("FlyZone");
            FlyZone newZone = g.AddComponent<FlyZone>();
            Undo.RegisterCreatedObjectUndo(g,Undo.GetCurrentGroupName());

            list.serializedProperty.arraySize++;
            list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize-1).objectReferenceValue = newZone;
        }

        private void RemoveElement(ReorderableList list)
        {
            FlyZone zone = (FlyZone)list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;
            if (zone) Undo.DestroyObjectImmediate(zone.gameObject);
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderablelist.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element);
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect,"Fly Zone");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            serializedObject.Update();
            reorderablelist.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        */
    }
}
