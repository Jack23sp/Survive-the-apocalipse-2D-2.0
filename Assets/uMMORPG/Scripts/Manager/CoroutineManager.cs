using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager singleton;
    public float unsanityInvoke;
    public float hungryInvoke;
    public float poisoningInvoke;
    public float thirstyInvoke;
    public float temperatureInvoke;
    public float radioInvoke;
    public float torchInvoke;
    public float wetInterval;
    public int teleportSeconds;

    void Start()
    {
        if (!singleton) singleton = this;    
    }

}
