using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Concrete : BuildingAccessory
{
    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.concretes.Contains(this)) ModularBuildingManager.singleton.concretes.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.concretes.Contains(this)) ModularBuildingManager.singleton.concretes.Remove(this);
        }
    }
}
