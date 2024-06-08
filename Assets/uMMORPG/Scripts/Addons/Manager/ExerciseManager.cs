using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseManager : MonoBehaviour
{
    public static ExerciseManager singleton;
    public RuntimeAnimatorController dumbbellAnimator;
    public RuntimeAnimatorController treadmillAnimator;

    void Awake()
    {
        if(!singleton) singleton = this;
    }


}
