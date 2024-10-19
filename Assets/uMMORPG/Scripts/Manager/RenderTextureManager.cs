using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureManager : MonoBehaviour
{

    public static RenderTextureManager singleton;
    public RenderTexture playerRenderTexure;
    public GameObject creationPlayer;
    public Camera creationCamera;
    public Camera minimapCamera;

    void Awake()
    {
        if (!singleton) singleton = this;
    }
}
