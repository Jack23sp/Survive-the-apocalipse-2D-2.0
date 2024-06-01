using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Fence : BuildingAccessory
{
    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.fences.Contains(this)) ModularBuildingManager.singleton.fences.Add(this);
        }
    }
    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.fences.Contains(this)) ModularBuildingManager.singleton.fences.Remove(this);
        }
    }
}
