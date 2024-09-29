using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScriptBoy.Fly2D
{
    [CustomEditor(typeof(F2DFollowPath))]
    public class F2DFollowPathEditor : Editor
    {
        private F2DFollowPath followPath;
        private F2DFlyPathEditor pathEditor;
        private F2DFlyPath path;

        private void OnEnable()
        {
            followPath = target as F2DFollowPath;
            if (followPath.name.StartsWith("GameObject")) followPath.name = "FollowPath";


        SceneView.duringSceneGui += SceneView_duringSceneGui;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            if (pathEditor != null)
            {
                DestroyImmediate(pathEditor);
            }
        }

        private void SceneView_duringSceneGui(SceneView obj)
        {
            if (followPath.path != path)
            {
                path = followPath.path;

                if (pathEditor != null)
                {
                    DestroyImmediate(pathEditor);
                }

                if (path != null)
                {
                    pathEditor = CreateEditor(path, typeof(F2DFlyPathEditor)) as F2DFlyPathEditor;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (followPath.path != null)
            {
                EditorGUILayout.Space();
                if (pathEditor != null)
                    pathEditor.DrawEditModeButton();
            }
            else if(GUILayout.Button("New Path"))
            {
                GameObject g = new GameObject("FlyPath");
                g.transform.parent = followPath.transform.parent;
                g.transform.localPosition = Vector3.zero;
                followPath.path = g.AddComponent<F2DFlyPath>();
            }
        }
    }
}