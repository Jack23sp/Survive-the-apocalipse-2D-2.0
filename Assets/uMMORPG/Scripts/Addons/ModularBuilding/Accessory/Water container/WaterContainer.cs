using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public partial class Database
{
    class watercontainer
    {
        public int ind { get; set; }
        public int water { get; set; }
    }

    public void SaveWaterContainer(int index)
    {
        connection.InsertOrReplace(new watercontainer
        {
            ind = index,
            water = ((WaterContainer)ModularBuildingManager.singleton.buildingAccessories[index]).water
        });
    }

    public void LoadWaterContainer(int index, WaterContainer watercontainer)
    {
        foreach (watercontainer row in connection.Query<watercontainer>("SELECT * FROM watercontainer WHERE ind=?", index))
        {
            watercontainer.water = row.water;
        }
    }
}

public class WaterContainer : BuildingAccessory
{
    [SyncVar]
    public int water;

    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.waterContainers.Contains(this)) ModularBuildingManager.singleton.waterContainers.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.waterContainers.Contains(this)) ModularBuildingManager.singleton.waterContainers.Remove(this);
        }
    }
}
