using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostManager : MonoBehaviour
{
    public static BoostManager singleton;
    public List<ScriptableBoost> allBoosts;

    void Start()
    {
        if (!singleton) singleton = this;
    }

}
