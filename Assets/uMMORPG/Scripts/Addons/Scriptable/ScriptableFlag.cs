using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "uMMORPG Item/Flag", order = 999)]
public partial class ScriptableFlag : ScriptableBuildingAccessory
{
    //[Header("Prefab to instantiate")]
    //public List<BuildingToCreate> buildingList = new List<BuildingToCreate>();
    public bool CanUse(Player player, int inventoryIndex)
    {
        return true;
    }

    public override void Use(Player player, int inventoryIndex)
    {
        ModularBuildingManager.singleton.inventoryIndex = inventoryIndex;
        if (inventoryIndex == -1)
        {
            GameObject g = Instantiate(player.playerModularBuilding.fakeBuildingID.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[player.playerModularBuilding.fakeBuildingID.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, player.playerModularBuilding.fakeBuildingID.transform.position, Quaternion.identity);
            ModularBuildingManager.singleton.spawnedBuilding = g;
        }
        else
        {
            GameObject g = Instantiate(buildingList[0].buildingObject, new Vector3(player.transform.position.x, player.transform.position.y, 0.0f), Quaternion.identity);
            ModularBuildingManager.singleton.spawnedBuilding = g;
        }

        ModularBuildingManager.singleton.scriptableExternal = this;
        ModularBuildingManager.singleton.AbleExternalFlagAccessory();
    }


    public void Spawn(Player player, int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName)
    {
        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableFlag)
        {
            GameObject buildingObject = Instantiate(buildingList[buildingIndex].buildingObject, position, buildingList[buildingIndex].buildingObject.transform.rotation);
            if (player.playerModularBuilding.CheckDistanceFromSpawn(buildingObject.transform))
            {
                Destroy(buildingObject);
                player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                return;
            }
            BuildingAccessory modular = buildingObject.GetComponent<BuildingAccessory>();
            modular.group = group;
            modular.owner = playerName;
            NetworkServer.Spawn(buildingObject);
        }
        else
        {
            return;
        }

        slot.DecreaseAmount(1);
        if (isInventory)
        {
            player.inventory.slots[inventoryIndex] = slot;
        }
        else
        {
            player.playerBelt.belt[inventoryIndex] = slot;
        }

    }


    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableFlag> cache;
    public static Dictionary<int, ScriptableFlag> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableFlag[] items = Resources.LoadAll<ScriptableFlag>("");

                // check for duplicates, then add to cache
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableAbility with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {

    }
}