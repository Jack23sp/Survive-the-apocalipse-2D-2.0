using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScriptBoy.Fly2D
{
    [CustomEditor(typeof(F2DFlyPath))]
    public class F2DFlyPathEditor : Editor
    {
        private F2DFlyPath flyPath;
        private Transform transform;
        private bool editMode;

        private Vector2 prevMousePosition;
        private bool Duplicated;

        private void OnEnable()
        {
            flyPath = target as F2DFlyPath;
            if (flyPath != null)
            {
                if (flyPath.name.StartsWith("GameObject")) flyPath.name = "FlyPath";
                transform = flyPath.transform;
            }

            SceneView.duringSceneGui += SceneView_duringSceneGui;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
        }

        private void SceneView_duringSceneGui(SceneView obj)
        {
            if (flyPath == null) return;

            if (transform == null) transform = flyPath.transform;

            if (flyPath.localPositions == null) flyPath.localPositions = new List<Vector2>();

            while (flyPath.localPositions.Count < 2)
            {
                flyPath.localPositions.Add(Vector3.zero);
            }

            DrawLines();
            if (editMode)
            {
                DrawHandles();
            }
        }

        private void DrawLines()
        {
            var positions = flyPath.localPositions;
            int count = positions.Count;

            Vector3 a = transform.TransformPoint(positions[count - 1]);
            Vector3 b = transform.TransformPoint(positions[0]);

            var color = Color.white;
            color.a = 0.1f;
            Handles.color = color;

            Handles.DrawLine(a, b);

            color = editMode ? Color.yellow : Color.white;
            Handles.color = color;
            for (int i = 1; i < count; i++)
            {
                a = b;
                b = transform.TransformPoint(positions[i]);
                Handles.DrawLine(a, b);
            }
        }

        private void DrawHandles()
        {
            var positions = flyPath.localPositions;
            int count = positions.Count;
            Event e = Event.current;

            if (e.control)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector2 position = transform.TransformPoint(positions[i]);
                    Vector3 GUIPoint = HandleUtility.WorldToGUIPoint(position);
                    Rect rect = new Rect(GUIPoint.x - 10, GUIPoint.y - 10, 20, 20);

                    float handleSize = HandleUtility.GetHandleSize(position) * 0.1f;

                    int controlID = GUIUtility.GetControlID(FocusType.Passive);
                    EventType eType = e.GetTypeForControl(controlID);

                    if (rect.Contains(e.mousePosition))
                    {
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(controlID, position, Quaternion.identity, handleSize, eType);

                        Vector2 prevPosition = transform.TransformPoint(positions[Mod.get(i - 1, count)]);
                        Vector2 nextPosition = transform.TransformPoint(positions[Mod.get(i + 1, count)]);

                        Handles.DrawPolyLine(prevPosition, position, nextPosition);

                        if (e.type == EventType.MouseDown && e.button == 0 && count > 3)
                        {
                            Undo.RecordObject(target, "Delete");
                            positions.RemoveAt(i);
                            GUI.changed = true;
                            return;
                        }
                    }
                    else
                    {
                        Handles.color = Color.white;
                        Handles.SphereHandleCap(controlID, position, Quaternion.identity, handleSize, EventType.Repaint);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    Vector2 position = transform.TransformPoint(positions[i]);

                    int controlID = GUIUtility.GetControlID(FocusType.Passive);
                    EventType eType = e.GetTypeForControl(controlID);
                    if (GUIUtility.hotControl == controlID)
                    {
                        if (eType == EventType.MouseUp)
                        {
                            Duplicated = false;
                            e.Use();
                            GUIUtility.hotControl = 0;
                        }
                        else if (eType == EventType.MouseDrag)
                        {
                            GUI.changed = true;

                            Vector2 mousePosition = e.mousePosition;
                            Ray ray1 = HandleUtility.GUIPointToWorldRay(prevMousePosition);
                            Ray ray2 = HandleUtility.GUIPointToWorldRay(mousePosition);
                            prevMousePosition = mousePosition;

                            Vector2 worldDelta;

                            if (e.shift)
                            {
                                worldDelta = Vector3.ProjectOnPlane(ray2.origin, Vector3.forward) - (Vector3)position;
                                position = position + worldDelta;
                                position = GridSnapping.Snap2D(position);
                            }
                            else
                            {
                                worldDelta = Vector3.ProjectOnPlane(ray2.origin, Vector3.forward) - Vector3.ProjectOnPlane(ray1.origin, Vector3.forward);
                                position += worldDelta;
                            }

                            if (e.alt && !Duplicated)
                            {
                                Vector2 a = position;
                                Vector2 b = ray2.origin;
                                Vector2 moveDir = b - a;

                                float dis = moveDir.magnitude;
                                float maxDis = HandleUtility.GetHandleSize(position) * 0.1f;

                                if (!Duplicated && dis > maxDis)
                                {
                                    Vector2 lineDir = transform.TransformPoint(positions[i]) - transform.TransformPoint(positions[Mod.get(i - 1, count)]);
                                    float dot = Vector3.Dot(moveDir, lineDir);
                                    if (dot > 0)
                                    {
                                        i++;
                                        GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                                    }

                                    Undo.RecordObject(target, "New Point");

                                    position = transform.InverseTransformPoint(b);
                                    positions.Insert(i, position); ;

                                    Duplicated = true;
                                    e.Use();
                                    return;
                                }
                            }
                            else
                            {
                                Undo.RecordObject(target, "Move");
                                position = transform.InverseTransformPoint(position);
                                positions[i] = position;
                            }
                            e.Use();
                        }
                    }

                    Vector3 GUIPoint = HandleUtility.WorldToGUIPoint(position);
                    Rect rect = new Rect(GUIPoint.x - 10, GUIPoint.y - 10, 20, 20);
                    float handleSize = HandleUtility.GetHandleSize(position) * 0.1f;

                    if (rect.Contains(e.mousePosition))
                    {
                        Handles.color = Color.yellow;
                        Handles.SphereHandleCap(controlID, position, Quaternion.identity, handleSize, eType);

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            prevMousePosition = e.mousePosition;
                            GUIUtility.hotControl = controlID;
                            Duplicated = false;
                            e.Use();
                        }
                    }
                    else
                    {
                        Handles.color = Color.white;
                        Handles.SphereHandleCap(controlID, position, Quaternion.identity, handleSize, EventType.Repaint);
                    }
                }
            }
        }

        public void DrawEditModeButton()
        {
            EditMode.Toggle(ref editMode,"Edit Path");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            DrawEditModeButton();
        }
    }
}