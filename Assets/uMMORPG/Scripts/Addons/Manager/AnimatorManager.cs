using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    public static AnimatorManager singleton;
    public RuntimeAnimatorController noWeaponRuntimeController;
    public RuntimeAnimatorController sleepRuntimeController;
    public RuntimeAnimatorController harvestingRuntimeController;

    void Start()
    {
        if (!singleton) singleton = this;    
    }
}
