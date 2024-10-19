using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StateSpeedManager : MonoBehaviour
{
    public static StateSpeedManager singleton;
    public float originalSpeed;
    public float newSpeed;

    public float sneakSpeedAmount;
    public float runSpeedAmount;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }
}
