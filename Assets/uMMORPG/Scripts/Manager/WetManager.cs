using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WetManager : MonoBehaviour
{
    public static WetManager singleton;
    public int decreaseHealthIfWet;

    void Start()
    {
        if (!singleton) singleton = this;
    }

}
