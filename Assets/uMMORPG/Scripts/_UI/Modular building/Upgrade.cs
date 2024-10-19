using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : BuildingAccessory
{
    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.upgrades.Contains(this)) ModularBuildingManager.singleton.upgrades.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.upgrades.Contains(this)) ModularBuildingManager.singleton.upgrades.Remove(this);
        }
    }
}
