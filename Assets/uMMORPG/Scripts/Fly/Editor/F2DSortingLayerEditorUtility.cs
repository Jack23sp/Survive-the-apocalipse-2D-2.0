using System.Reflection;
using UnityEditor;

namespace ScriptBoy.Fly2D
{
    public class SortingLayerEditorUtility
    {
        private static System.Type type;

        public new static System.Type GetType()
        {
            if (type == null)
            {
                type = System.Type.GetType("UnityEditor.SortingLayerEditorUtility,UnityEditor");
            }
            return type;
        }

        private static MethodInfo renderSortingLayerFields;

        public static void RenderSortingLayerFields(SerializedProperty sortingOrder, SerializedProperty sortingLayer)
        {
            if (renderSortingLayerFields == null)
            {
                renderSortingLayerFields = GetType().GetMethod("RenderSortingLayerFields",
                    new System.Type[] { typeof(SerializedProperty), typeof(SerializedProperty) });
            }
            renderSortingLayerFields.Invoke(null, new SerializedProperty[] { sortingOrder, sortingLayer });
        }
    }
}