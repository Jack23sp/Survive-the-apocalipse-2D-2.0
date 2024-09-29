#if UNITY_EDITOR
using System.Collections;
using System.Threading;
using UnityEditor;

namespace ScriptBoy.Fly2D
{
    public class EditorCoroutine
    {
        private IEnumerator m_Coroutine;

        public static void StartCoroutine(IEnumerator coroutine)
        {
            new EditorCoroutine(coroutine);
        }

        private EditorCoroutine(IEnumerator coroutine)
        {
            m_Coroutine = coroutine;
            EditorApplication.update += Update;
        }

        private void Update()
        {
            if (!m_Coroutine.MoveNext())
            {
                EditorApplication.update -= Update;
            }
        }
    }
}
#endif