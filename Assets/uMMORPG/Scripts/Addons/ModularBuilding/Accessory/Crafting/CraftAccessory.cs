using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[System.Serializable]
public partial struct CraftinItemSlot
{
    public string timeBegin;
    public string timeEnd;
    public string serverTimeBegin;
    public string serverTimeEnd;
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
        public string timeBegin { get; set; }
        public string timeEnd { get; set; }
        public string serverTimeBegin { get; set; }
        public string serverTimeEnd { get; set; }
        public string item { get; set; }
        public int amount { get; set; }
        public string owner { get; set; }
        public string guild { get; set; }
        public int ind { get; set; }
        public int sex { get; set; }
    }

    public void SaveCraftAccessory(int index)
    {
        for (int i = 0; i < ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem.Count; i++)
        {
            for (int e = 0; e < ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem.Count; e++)
            {
                connection.InsertOrReplace(new craft_item_accessory
                {
                    buildingindex = index,
                    timeBegin = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].timeBegin,
                    timeEnd = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].timeEnd,
                    serverTimeBegin = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].serverTimeBegin,
                    serverTimeEnd = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].serverTimeEnd,
                    item = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].item,
                    amount = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].amount,
                    owner = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].owner,
                    guild = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].guild,
                    ind = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].index,
                    sex = ((CraftAccessory)ModularBuildingManager.singleton.buildingAccessories[index]).craftingItem[e].sex
                });
            }
        }
    }

    public void LoadCraftAccessory(int index, CraftAccessory craftAccessory)
    {
        foreach (craft_item_accessory row in connection.Query<craft_item_accessory>("SELECT * FROM craft_item_accessory WHERE ind=?", index))
        {
            craftAccessory.craftingItem.Add(new CraftinItemSlot()
            {
                timeBegin = row.timeBegin,
                timeEnd = row.timeEnd,
                serverTimeBegin = row.serverTimeBegin,
                serverTimeEnd = row.serverTimeEnd,
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
    public bool checkSex;

    public override void OnStartClient()
    {
        base.OnStartClient();
        craftingItem.Callback += OnBeltChanged;
    }

    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.craftAccessories.Contains(this)) ModularBuildingManager.singleton.craftAccessories.Add(this);
        }
    }



    void OnBeltChanged(SyncList<CraftinItemSlot>.Operation op, int index, CraftinItemSlot oldSlot, CraftinItemSlot newSlot)
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
    }
}
