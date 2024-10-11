using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[System.Serializable]
public partial struct CraftinItemSlot
{
    public int totalSeconds;
    public string item;
    public int amount;
    public string owner;
    public string guild;
    public int index;
    public int sex;
}

public partial class Database
{
    class craft_item_accessory
    {
        public int buildingindex { get; set; }
        public int totalSeconds { get; set; }
        public string item { get; set; }
        public int amount { get; set; }
        public string owner { get; set; }
        public string guild { get; set; }
        public int ind { get; set; }
        public int sex { get; set; }
    }

    public void SaveCraftAccessory(int index)
    {
        for (int e = 0; e < ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem.Count; e++)
        {
            connection.InsertOrReplace(new craft_item_accessory
            {
                buildingindex = index,
                totalSeconds = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].totalSeconds,
                item = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].item,
                amount = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].amount,
                owner = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].owner,
                guild = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].guild,
                ind = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].index,
                sex = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].sex
            });
        }
    }

    public void LoadCraftAccessory(int index, CraftAccessory craftAccessory)
    {
        foreach (craft_item_accessory row in connection.Query<craft_item_accessory>("SELECT * FROM craft_item_accessory WHERE buildingindex=?", index))
        {
            craftAccessory.craftingItem.Add(new CraftinItemSlot()
            {
                totalSeconds = row.totalSeconds,
                item = row.item,
                amount = row.amount,
                owner = row.owner,
                guild = row.guild,
                index = row.ind,
                sex = row.sex
            });
        }
    }
}

public class CraftAccessory : BuildingAccessory
{

    public readonly SyncList<CraftinItemSlot> craftingItem = new SyncList<CraftinItemSlot>();
    public readonly SyncList<string> playerThatInteractWhitThis = new SyncList<string>();

    private Player plInteractCheck;
    #region effect
    public ParticleSystem pSystem;
    #endregion


    public bool checkSex;
    public bool hasAnimation;

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        craftingItem.Callback += OnBeltChanged;
        if (craftingItem.Count > 0)
        {
            if (pSystem)
            {
                pSystem.gameObject.SetActive(true);
                pSystem.Play();
                SetOrder();
            }
        }
        else
        {
            if (pSystem)
            {
                pSystem.gameObject.SetActive(false);
                pSystem.Stop();
            }
        }
    }


    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.craftAccessories.Contains(this)) ModularBuildingManager.singleton.craftAccessories.Add(this);
        }
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


    protected void OnBeltChanged(SyncList<CraftinItemSlot>.Operation op, int index, CraftinItemSlot oldSlot, CraftinItemSlot newSlot)
    {
        if (UICraft.singleton)
        {
            if (ModularBuildingManager.singleton.buildingAccessory && UICraft.singleton.craftAccessory && UICraft.singleton.panel.activeInHierarchy)
            {
                UICraft.singleton.SpawnEndCraftAtBegins();
                UICraft.singleton.SpawnCraftAtBegins(true, true);
                UICraft.singleton.Craft(UICraft.singleton.craftIndex);
            }
        }

        if (craftingItem.Count > 0)
        {
            if (pSystem)
            {
                pSystem.gameObject.SetActive(true);
                pSystem.Play();
                SetOrder();
            }
        }
        else
        {
            if (pSystem)
            {
                pSystem.gameObject.SetActive(false);
                pSystem.Stop();
            }
        }
    }
}
