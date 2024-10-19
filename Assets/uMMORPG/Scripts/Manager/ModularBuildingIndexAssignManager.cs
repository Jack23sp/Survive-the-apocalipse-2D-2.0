using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ModularBuildingIndexAssignManager : NetworkBehaviour
{
    public static ModularBuildingIndexAssignManager singleton;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }

    [SyncVar]
    public int incrementalModularBuildingIndex;

    public int AskNewBaseIndex()
    {
        incrementalModularBuildingIndex++;
        return incrementalModularBuildingIndex;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!singleton) singleton = this;
        InvokeRepeating(nameof(SaveBuilding), 30.0f, 100.0f);
        LoadBuilding();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        //SaveBuilding();
    }

    public void SaveBuilding()
    {
        Database.singleton.SaveBuilding();
    }

    public void LoadBuilding()
    {
        Database.singleton.LoadBuildingAccessory();
    }

    public void LoadOtherStuff()
    {

    }
}
