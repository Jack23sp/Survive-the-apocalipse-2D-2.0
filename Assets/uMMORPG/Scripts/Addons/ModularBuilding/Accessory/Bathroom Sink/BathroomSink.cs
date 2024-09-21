using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class BathroomSink : BuildingAccessory
{
    public Aquifer aquifer;

    public readonly SyncList<string> playerThatInteractWhitThis = new SyncList<string>();

    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.bathroomSinks.Contains(this)) ModularBuildingManager.singleton.bathroomSinks.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.bathroomSinks.Contains(this)) ModularBuildingManager.singleton.bathroomSinks.Remove(this);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Invoke(nameof(FindNearestFloorObject), 0.5f);
    }

    public override void AddPlayerThatAreInteract(string playerName)
    {
        base.AddPlayerThatAreInteract(playerName);
        if (!playerThatInteractWhitThis.Contains(playerName)) playerThatInteractWhitThis.Add(playerName);
    }

    public override void RemovePlayerThatAreInteract(string playerName)
    {
        base.RemovePlayerThatAreInteract(playerName);
        if (playerThatInteractWhitThis.Contains(playerName)) playerThatInteractWhitThis.Remove(playerName);
    }


    public void FindNearestFloorObject()
    {
        List<ModularBuilding> floor = ModularBuildingManager.singleton.combinedModulars;
        List<ModularBuilding> floorOrdered = new List<ModularBuilding>();
        floorOrdered = floor.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (floorOrdered.Count > 0)
        {
            aquifer = floorOrdered[0].aquifer;
            CancelInvoke(nameof(FindNearestFloorObject));
        }
        else
        {
            Invoke(nameof(FindNearestFloorObject), 0.5f);
        }
    }

}
