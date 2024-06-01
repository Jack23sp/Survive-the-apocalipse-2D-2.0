using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnpointManager : MonoBehaviour
{
    public static SpawnpointManager singleton;
    public ScriptableItem Instantresurrect;
    public Sprite prefered;
    public Sprite notPrefered;
    [TextArea(20,20)]
    public string messageInDescription;

    void Awake()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        
    }
}
