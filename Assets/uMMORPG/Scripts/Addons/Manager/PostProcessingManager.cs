using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager singleton;

    public GameObject postEffect;

    void Start()
    {
        if(!singleton) singleton = this;
    }
}
