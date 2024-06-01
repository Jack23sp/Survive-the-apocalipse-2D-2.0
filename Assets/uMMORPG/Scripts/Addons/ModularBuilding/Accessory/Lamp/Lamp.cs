using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Lamp : BuildingAccessory
{
    public List<GameObject> lights = new List<GameObject>();
    [SyncVar(hook = (nameof(CheckLight)))]
    public bool isActive;

    public void CheckLight (bool oldValue,bool newValue)
    {
        for(int i = 0; i < lights.Count; i++)
        {
            lights[i].SetActive(newValue);
        }
    }

    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.lamps.Contains(this)) ModularBuildingManager.singleton.lamps.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.lamps.Contains(this)) ModularBuildingManager.singleton.lamps.Remove(this);
        }
    }
}
