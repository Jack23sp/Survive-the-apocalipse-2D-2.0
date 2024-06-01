using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceManager : MonoBehaviour
{
    public static FurnaceManager singleton;

    public List<ScriptableItem> allowedToFurnace;
    public float furnaceCook;
    public ScriptableItem coal;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public bool FindItemInAllowedForFurnace(string itemName)
    {
        for(int i = 0; i < allowedToFurnace.Count; i++)
        {
            if (allowedToFurnace[i].name == itemName) return true;
        }
        return false;
    }
}
