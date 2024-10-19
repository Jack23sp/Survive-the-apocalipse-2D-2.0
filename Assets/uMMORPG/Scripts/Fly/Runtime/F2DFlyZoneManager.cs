using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ScriptBoy.Fly2D
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-100)]
    public sealed class F2DFlyZoneManager : MonoBehaviour
    {
        public enum PreviewMode
        {
            Selected, All
        }


        [FormerlySerializedAs("preview")]
        [SerializeField] private PreviewMode m_Preview;

        private static F2DFlyZoneManager s_Instance;
        public static F2DFlyZoneManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<F2DFlyZoneManager>();

                    if (s_Instance == null)
                    {
                        s_Instance = new GameObject("FlyZone Manager",typeof(F2DFlyZoneManager)).GetComponent<F2DFlyZoneManager>();
                    }
                }

                return s_Instance;
            }
        }

        private List<F2DFlyZone> m_FlyZoneList = new List<F2DFlyZone>();

        public static F2DFlyZone[] FlyZoneArray
        {
            get
            {
#if UNITY_EDITOR
                var list = Instance.m_FlyZoneList;
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    list.RemoveAll((x) => x == null);
                }
                return list.ToArray();
#else
                return Instance.m_FlyZoneList.ToArray();
#endif
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            s_Instance = this;
        }
#endif
        private void Awake()
        {
            s_Instance = this;
        }

#if UNITY_EDITOR
        public static void AddFlyZone_WithoutCreatingInstance(F2DFlyZone flyZone)
        {
            if (s_Instance != null)
            {
                if (s_Instance.m_FlyZoneList.Contains(flyZone) == false)
                {
                    s_Instance.m_FlyZoneList.Add(flyZone);
                }
            }
        }
#endif

        public static void AddFlyZone(F2DFlyZone flyZone)
        {
            if (Instance != null)
            {
                if (s_Instance.m_FlyZoneList.Contains(flyZone) == false)
                {
                    s_Instance.m_FlyZoneList.Add(flyZone);
                }
            }
        }

        public static void RemoveFlyZone(F2DFlyZone flyZone)
        {
            if (s_Instance != null)
            {
                s_Instance.m_FlyZoneList.Remove(flyZone);
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitEditorUpdate()
        {
            var flyZones = FindObjectsOfType<F2DFlyZone>();
            if (flyZones.Length > 0)
            {
                Instance.m_FlyZoneList.Clear();
                Instance.m_FlyZoneList.AddRange(flyZones);
            }

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            if (!EditorApplication.isPlaying && s_Instance != null)
            {
                if (s_Instance.m_Preview == PreviewMode.Selected)
                {
                    Transform activeTransform;
                    if (activeTransform = Selection.activeTransform)
                    {
                        foreach (var zone in FlyZoneArray)
                        {
                            var transform = zone.transform;
                            if (activeTransform == transform || activeTransform.IsChildOf(transform)) zone.Update();
                        }
                    }
                }
                else
                {
                    foreach (var zone in FlyZoneArray) if(zone) zone.Update();
                }
            }
        }
#endif
    }
}