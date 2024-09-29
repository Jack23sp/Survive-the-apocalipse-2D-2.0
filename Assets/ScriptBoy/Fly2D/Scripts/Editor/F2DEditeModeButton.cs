using UnityEngine;
using UnityEditor;

namespace ScriptBoy.Fly2D
{
    public class EditMode
    {
        public static void Toggle(ref bool value)
        {
            value = GUILayout.Toggle(value, EditorGUIUtility.IconContent("d_EditCollider"), GUI.skin.button, GUILayout.Width(35), GUILayout.Height(25));
        }

        public static void Toggle(ref bool value, string lable)
        {
            GUILayout.BeginHorizontal();
            value = GUILayout.Toggle(value, EditorGUIUtility.IconContent("d_EditCollider"), GUI.skin.button, GUILayout.Width(35), GUILayout.Height(25));
            GUILayout.Label(lable, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft }, GUILayout.Height(28));
            GUILayout.EndHorizontal();
        }
    }
}