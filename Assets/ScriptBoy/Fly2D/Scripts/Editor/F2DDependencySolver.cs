using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ScriptBoy.Fly2D
{
    public class DependencySolver
    {
        public static void Check(string[] dependencies)
        {
            EditorCoroutine.StartCoroutine(CheckCoroutine(dependencies));
        }

        private static IEnumerator CheckCoroutine(string[] dependencies)
        {
            float progress = 0;
            EditorUtility.DisplayProgressBar("Checking Dependencies", "", progress);

            WaitForSeconds wait = new WaitForSeconds(0.1f);
            bool reloadScene = false;


            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
                yield return wait;
                progress = Mathf.Lerp(progress, 0.5f, 0.1f);
                EditorUtility.DisplayProgressBar("Checking Dependencies", "", progress);
            }
            EditorUtility.ClearProgressBar();


            for (int i = 0; i < dependencies.Length; i++)
            {
                string package = dependencies[i];

                if (listRequest.Result.FirstOrDefault((x) => x.name == package) == null)
                {
                    Debug.Log("Package '" + package + "' is missing.");

                    var addRequest = Client.Add(package);
                    while (!addRequest.IsCompleted)
                    {
                        yield return wait;
                    }

                    if (addRequest.Status == StatusCode.Success)
                    {
                        Debug.Log("Package '" + package + "' is installed.");
                        reloadScene = true;
                    }
                    else
                    {
                        Debug.Log("Package '" + package + "' installation has failed.");
                    }
                }
                else
                {
                    //Debug.Log("Package '" + package + "' is already installed.");
                }
            }

            if (reloadScene)
            {
                EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
            }
        }
    }
}