using UnityEngine;
using UnityEditor;

namespace ScriptBoy.Fly2D
{
    [CustomEditor(typeof(F2DFlyZone))]
    public class F2DFlyZoneEditor : Editor
    {
        private F2DFlyZone flyZone;

        private SerializedProperty zoneCenter;
        private SerializedProperty zoneRadius;

        private SerializedProperty flyCount;
        private SerializedProperty flySize;
        private SerializedProperty timeScale;

        private SerializedProperty tiles;
        private SerializedProperty idleFrameCount;
        private SerializedProperty flyFrameCount;
        private SerializedProperty framePerSecond;

        private SerializedProperty landing;
        private SerializedProperty landingCheckPerFrame;
        private SerializedProperty landingMask;
        private SerializedProperty landingOn;
        private SerializedProperty flyingDuration;
        private SerializedProperty landingDuration;

        private SerializedProperty collision;
        private SerializedProperty visualizeCollision;
        private SerializedProperty collidesWith;
        private SerializedProperty flyRadiusScale;
        private SerializedProperty collisionForceScale;
        private SerializedProperty maxCollisionShapes;
        private SerializedProperty castingMethod;

        private SerializedProperty material;
        private SerializedProperty sortingLayerID;
        private SerializedProperty sortingOrder;

        [SerializeField] private bool foldout_TextureSheetAnimation;
        [SerializeField] private bool foldout_Landing;
        [SerializeField] private bool foldout_Collision;


        private GUIStyle _cellStyle;
        private GUIStyle cellStyle
        {
            get
            {
                if (_cellStyle == null)
                {
                    _cellStyle = new GUIStyle(GUI.skin.label);
                    _cellStyle.normal.textColor = Color.white;
                    _cellStyle.alignment = TextAnchor.UpperLeft;
                    _cellStyle.border = new RectOffset(5, 5, 5, 5);
                }

                return _cellStyle;
            }
        }

        private void OnEnable()
        {
            flyZone = target as F2DFlyZone;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                F2DFlyZoneManager.AddFlyZone(flyZone);
            }

            if (flyZone.name.StartsWith("GameObject")) flyZone.name = "FlyZone";

            if (flyZone.zoneCenter == null)
            {
                CreateZoneCenterTransform();
            }
 
            FindProperties();

            Tools.hidden = true;
        }

        private void CreateZoneCenterTransform()
        {
            GameObject g = new GameObject("Center");
            flyZone.zoneCenter = g.transform;

            Vector3 position = flyZone.transform.position;
            flyZone.zoneCenter.SetParent(flyZone.transform);
            flyZone.transform.position = Vector3.zero;
            flyZone.zoneCenter.position = position;
        }

        private void FindProperties()
        {
            zoneCenter = serializedObject.FindProperty("m_ZoneCenter");
            zoneRadius = serializedObject.FindProperty("m_ZoneRadius");

            flyCount = serializedObject.FindProperty("m_FlyCount");
            flySize = serializedObject.FindProperty("m_FlySize");
            timeScale = serializedObject.FindProperty("m_TimeScale");


            tiles = serializedObject.FindProperty("m_Tiles");
            idleFrameCount = serializedObject.FindProperty("m_IdleFrameCount");
            flyFrameCount = serializedObject.FindProperty("m_FlyFrameCount");
            framePerSecond = serializedObject.FindProperty("m_FramePerSecond");


            landing = serializedObject.FindProperty("m_Landing");
            landingMask = serializedObject.FindProperty("m_LandingMask");
            landingOn = serializedObject.FindProperty("m_LandingOn");
            landingCheckPerFrame = serializedObject.FindProperty("m_LandingCheckPerFrame");
            flyingDuration = serializedObject.FindProperty("m_FlyingDuration");
            landingDuration = serializedObject.FindProperty("m_LandingDuration");


            collision = serializedObject.FindProperty("m_Collision");
            visualizeCollision = serializedObject.FindProperty("m_VisualizeCollision");
            collidesWith = serializedObject.FindProperty("m_CollidesWith");
            flyRadiusScale = serializedObject.FindProperty("m_FlyRadiusScale");
            collisionForceScale = serializedObject.FindProperty("m_CollisionForceScale");
            maxCollisionShapes = serializedObject.FindProperty("m_MaxCollisionShapes");
            castingMethod = serializedObject.FindProperty("m_CastingMethod");


            material = serializedObject.FindProperty("m_Material");
            sortingLayerID = serializedObject.FindProperty("m_SortingLayerID");
            sortingOrder = serializedObject.FindProperty("m_SortingOrder");
        }

        private void OnDestroy()
        {
            Tools.hidden = false;
        }

        private void OnSceneGUI()
        {
            Tool tool = Tools.current;

            if (tool == Tool.Move)
            {
                Vector3 position = flyZone.zoneCenter.position;
                Quaternion q = Quaternion.identity;

                EditorGUI.BeginChangeCheck();
                position = Handles.PositionHandle(position, q);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(flyZone.zoneCenter, "Move");
                    flyZone.zoneCenter.position = position;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(zoneCenter);
            EditorGUILayout.PropertyField(zoneRadius);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(timeScale, new GUIContent("Simulation Speed"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(flyCount);
            EditorGUILayout.PropertyField(flySize);


            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(landing);
            EditorGUILayout.PropertyField(collision);
            EditorGUILayout.Space();

            if (landing.boolValue)
            {
                DrawFoldout_Landing();
            }

            if (collision.boolValue)
            {
                DrawFoldout_Collision();
            }

            DrawFoldout_TextureSheetAnimation();

            EditorGUILayout.Space();


            EditorGUILayout.PropertyField(material);
            SortingLayerEditorUtility.RenderSortingLayerFields(sortingOrder, sortingLayerID);

            if (GUI.changed)
            {
                GUI.changed = false;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawFoldout_Landing()
        {
            if (foldout_Landing = EditorGUILayout.Foldout(foldout_Landing, "Landing"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(flyingDuration);
                EditorGUILayout.PropertyField(landingDuration);
                EditorGUILayout.PropertyField(landingCheckPerFrame);
                EditorGUILayout.PropertyField(landingOn);
                EditorGUILayout.PropertyField(landingMask);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private void DrawFoldout_Collision()
        {
            if (foldout_Collision = EditorGUILayout.Foldout(foldout_Collision, "Collision"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(collidesWith);
                EditorGUILayout.PropertyField(castingMethod);
                EditorGUILayout.PropertyField(maxCollisionShapes);
                EditorGUILayout.PropertyField(collisionForceScale);
                EditorGUILayout.PropertyField(flyRadiusScale, new GUIContent("Radius Scale"));
                EditorGUILayout.PropertyField(visualizeCollision, new GUIContent("Visualize"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private void DrawFoldout_TextureSheetAnimation()
        {
            if (foldout_TextureSheetAnimation = EditorGUILayout.Foldout(foldout_TextureSheetAnimation, "Texture Sheet Animation"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(tiles);

                Material material;
                Texture texture;
                if ((material = flyZone.material) && (texture = material.mainTexture))
                {
                    EditorGUILayout.Space();
                    DrawTextureSheet(texture, tiles.vector2IntValue, idleFrameCount.intValue, flyFrameCount.intValue);
                    EditorGUILayout.Space();
                }
                EditorGUILayout.PropertyField(idleFrameCount);
                EditorGUILayout.PropertyField(flyFrameCount);
                EditorGUILayout.PropertyField(framePerSecond);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawTextureSheet(Texture texture , Vector2Int tiles,int idleFrameCount,int flyFrameCount)
        {
            float viewWidth = EditorGUIUtility.currentViewWidth * 0.6f;
            Rect mainRect = GUILayoutUtility.GetRect(viewWidth, viewWidth);
            mainRect = new Rect(mainRect.x + (mainRect.width - mainRect.height) / 2, mainRect.y, mainRect.height, mainRect.height);

            float cellWidth = mainRect.width / tiles.x;
            float cellHeight = mainRect.height / tiles.y;

            Rect cellRect = new Rect(0, 0, cellWidth, cellHeight);

            GUI.DrawTexture(mainRect, texture);

            for (int v = 0; v < tiles.y; v++)
            {
                for (int u = 0; u < tiles.x; u++)
                {
                    int tileIndex = v * tiles.x + u;

                    Vector3 v1 = new Vector2(u * cellWidth + mainRect.x, v * cellHeight + mainRect.y);
                    Vector3 v2 = new Vector2(u * cellWidth + mainRect.x, v * cellHeight + cellHeight + mainRect.y);
                    Vector3 v3 = new Vector2(u * cellWidth + cellWidth + mainRect.x, v * cellHeight + cellHeight + mainRect.y);
                    Vector3 v4 = new Vector2(u * cellWidth + cellWidth + mainRect.x, v * cellHeight + mainRect.y);

                    Handles.DrawPolyLine(v1, v2, v3, v4, v1);
                    cellRect.position = v1;
                    if (tileIndex < idleFrameCount)
                    {
                        GUI.Label(cellRect, new GUIContent("Idle"), cellStyle);
                    }
                    else if(tileIndex - idleFrameCount < flyFrameCount)
                    {
                        GUI.Label(cellRect, new GUIContent("Fly"), cellStyle);
                    }
                }
            }
        }
    }
}