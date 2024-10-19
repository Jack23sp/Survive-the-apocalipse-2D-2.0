using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjuryManager : MonoBehaviour
{
    public static InjuryManager singleton;
    public float limitInjury;

    void Start()
    {
        if (!singleton) singleton = this;    
    }


}
