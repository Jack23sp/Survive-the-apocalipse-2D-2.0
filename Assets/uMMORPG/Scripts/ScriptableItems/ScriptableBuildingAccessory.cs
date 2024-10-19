using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

[System.Serializable]
public partial struct ItemCrafting
{
    public ScriptableItemAndAmount itemAndAmount;
    public List<ScriptableItemAndAmount> ingredients;
    public int timeToCraft;
    public long gold;
    public long coin;
}

[CreateAssetMenu(menuName = "uMMORPG Item/Building accessory", order = 999)]
public class ScriptableBuildingAccessory : UsableItem
{
    public string necessaryTagObject;
    public List<BuildingToCreate> buildingList;
    public bool isMetal;
    public bool isWood;
    public bool isStone;
    public bool isConcrete;

    public override bool CanUse(Player player, int inventoryIndex)
    {
        return true;
    }

    public override void Use(Player player, int inventoryIndex)
    {

    }

    public void Spawn(Player player, bool isInventory, int inventoryIndex, int direction, Vector3 position)
    {
        if(inventoryIndex == -1)
        {
            if (Player.localPlayer.playerModularBuilding.fakeBuildingID != null)
            {
                ModularBuildingManager.singleton.spawnedAccesssory = Instantiate(player.playerModularBuilding.fakeBuildingID.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[player.playerModularBuilding.fakeBuildingID.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, position, Quaternion.identity);
                ModularBuildingManager.singleton.scriptableBuildingAccessory = this;
                ModularBuildingManager.singleton.inventoryIndex = -1;
                ModularBuildingManager.singleton.spawnedAccesssory.GetComponent<BuildingAccessory>().CheckPossibleSpawn();
                ModularBuildingManager.singleton.AbleBasementAccessory();
                return;
            }
        }

        ItemSlot slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];
        if(slot.amount > 0)
        {
            if(slot.item.data.name == name)
            {
                ModularBuildingManager.singleton.spawnedAccesssory = Instantiate(((ScriptableBuildingAccessory)slot.item.data).buildingList[direction].buildingObject, position, Quaternion.identity);
                ModularBuildingManager.singleton.scriptableBuildingAccessory = this;
                ModularBuildingManager.singleton.inventoryIndex = inventoryIndex;
                ModularBuildingManager.singleton.spawnedAccesssory.GetComponent<BuildingAccessory>().CheckPossibleSpawn();
                ModularBuildingManager.singleton.AbleBasementAccessory();
            }
        }
    }

    public void SpawnOnServer(Player player, bool isInventory, int inventoryIndex, int direction, Vector3 position)
    {
        ItemSlot slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];
        if (slot.amount > 0)
        {
            if (slot.item.data.name == name)
            {
                GameObject g = Instantiate(((ScriptableBuildingAccessory)slot.item.data).buildingList[direction].buildingObject, position, Quaternion.identity);
                BuildingAccessory accessory = g.GetComponent<BuildingAccessory>();
                if(accessory.CheckPossibleSpawn())
                {
                    NetworkServer.Spawn(g.gameObject);
                }
            }
        }
    }

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableBuildingAccessory> cache;
    public static Dictionary<int, ScriptableBuildingAccessory> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableBuildingAccessory[] items = Resources.LoadAll<ScriptableBuildingAccessory>("");

                // check for duplicates, then add to cache
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableBuildingAccessory with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }


}
