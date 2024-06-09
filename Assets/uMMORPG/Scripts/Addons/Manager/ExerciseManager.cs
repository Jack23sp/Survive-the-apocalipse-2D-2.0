using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseManager : MonoBehaviour
{
    public static ExerciseManager singleton;
    public RuntimeAnimatorController dumbbellAnimator;
    public RuntimeAnimatorController treadmillAnimator;
    public RuntimeAnimatorController pushUpAnimator;
    public RuntimeAnimatorController jumpinJackAnimator;
    public RuntimeAnimatorController sitUpAnimator;

    void Awake()
    {
        if(!singleton) singleton = this;
    }


}
