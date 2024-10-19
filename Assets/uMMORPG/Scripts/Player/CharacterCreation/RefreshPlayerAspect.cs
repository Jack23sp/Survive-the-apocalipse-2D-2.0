using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RefreshPlayerAspect : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Camera avatarCamera;
    public RenderTexture instantiatedTexture;
    public RawImage rawImage;

    void Start()
    {
        instantiatedTexture = new RenderTexture(renderTexture);
        avatarCamera.targetTexture = instantiatedTexture;
        rawImage.texture = instantiatedTexture;
    }
}
