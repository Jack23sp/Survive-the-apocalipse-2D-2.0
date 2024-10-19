using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public static FlagManager singleton;
    public List<Sprite> flags;

    void Start()
    {
        if (!singleton) singleton = this;
    }

}
