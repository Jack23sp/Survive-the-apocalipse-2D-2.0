using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class BathroomSink : BuildingAccessory
{
    public Aquifer aquifer;
    
    private Player plInteractCheck;
    #region effect
    public ParticleSystem pSystem;
    #endregion

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
        Invoke(nameof(CheckPlayer), 3.0f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerThatInteractWhitThis.Callback += PlayerInteraction;
        Invoke(nameof(FindNearestFloorObject), 0.5f);
    }

    public void CheckPlayer()
    {
        if (netIdentity.observers != null && netIdentity.observers.Count > 0)
        {
            for (int i = playerThatInteractWhitThis.Count - 1; i >= 0; i--)
            {
                if (!Player.onlinePlayers.TryGetValue(playerThatInteractWhitThis[i], out plInteractCheck))
                {
                    playerThatInteractWhitThis.RemoveAt(i);
                }
            }
        }
        Invoke(nameof(CheckPlayer), 3.0f);
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

    void PlayerInteraction(SyncList<string>.Operation op, int index, string oldSlot, string newSlot)
    {
        if (playerThatInteractWhitThis.Count == 0)
        {
            if (pSystem)
            {
                pSystem.gameObject.SetActive(false);
                pSystem.Stop();
            }
        }
        else
        {

            if (pSystem)
            {
                pSystem.gameObject.SetActive(true);
                SetOrder();
                pSystem.Play();
            }
        }
    }

    public void SetOrder()
    {
        if (pSystem)
        {
            ParticleSystemRenderer rend = pSystem.GetComponent<ParticleSystemRenderer>();
            if (rend)
            {
                rend.sortingOrder = renderer.sortingOrder + 1;
            }
        }
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
