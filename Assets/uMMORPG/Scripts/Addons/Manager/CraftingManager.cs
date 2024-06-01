using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager singleton;

    public float openSize;
    public float closeSize;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

}
