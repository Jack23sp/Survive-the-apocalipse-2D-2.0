using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    public static AnimatorManager singleton;
    public RuntimeAnimatorController noWeaponRuntimeController;

    void Start()
    {
        if (!singleton) singleton = this;    
    }
}
