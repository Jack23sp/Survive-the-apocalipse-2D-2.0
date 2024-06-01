using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public partial struct CustomItem
{
    public ScriptableItem items;
    public int amount;
}

[Serializable]
public partial struct ItemInBuilding
{
    public IngredientBuilding itemToCraft;
    public int buildingLevel;
    public List<IngredientBuilding> craftablengredient;
}

[Serializable]
public partial struct IngredientBuilding
{
    public ScriptableItem item;
    public int amount;
}

[Serializable]
public partial struct BuildingToCreate
{
    public string buildingName;
    public GameObject buildingObject;
}


[CreateAssetMenu(menuName = "uMMORPG Item/Building", order = 999)]
public partial class ScriptableBuilding : UsableItem
{
    public bool isObstacle;

    [Header("Prefab to instantiate")]
    public List<BuildingToCreate> buildingList = new List<BuildingToCreate>();

    public bool isBasement;
    public bool isWall;
    public bool isDoor;
    public bool modularAccessory;
    public bool modularForniture;

    public string necessaryTagObject;


    public bool CanUse(Player player, int inventoryIndex)
    {
        return true;
    }

    public override void Use(Player player, int inventoryIndex)
    {
        GameObject searched = player.playerModularBuilding.FindNearestFloorObject();
        GameObject nearestPointToSpawnBuilding = null;
        if(searched) nearestPointToSpawnBuilding = player.playerModularBuilding.FindNearestFloorPointAvailable(searched.GetComponent<ModularBuilding>());
        ModularBuildingManager.singleton.inventoryIndex = inventoryIndex;

        if (searched != null && nearestPointToSpawnBuilding != null)
        {
            ModularBuildingManager.singleton.prevPlacementBase = nearestPointToSpawnBuilding.GetComponent<PlacementBase>();
            GameObject g = Instantiate(buildingList[0].buildingObject, new Vector3(nearestPointToSpawnBuilding.transform.position.x, nearestPointToSpawnBuilding.transform.position.y, 0.0f), Quaternion.identity);
            ModularBuildingManager.singleton.spawnedBuilding = g;
            ModularBuildingManager.singleton.AbleBasementPositioning();

        }
        else
        {
            GameObject g = Instantiate(buildingList[0].buildingObject, new Vector3(player.transform.position.x, player.transform.position.y, 0.0f), Quaternion.identity);
            g.GetComponent<ModularBuilding>().main = true;
            ModularBuildingManager.singleton.spawnedBuilding = g;
            ModularBuildingManager.singleton.AbleBasementPositioning();
        }
        ModularBuildingManager.singleton.scriptableBuilding = this;
    }


    public void Spawn(Player player, int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, bool main, int modularIndex)
    {
        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableBuilding)
        {
            GameObject buildingObject = Instantiate(buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
            if (player.playerModularBuilding.CheckDistanceFromSpawn(buildingObject.transform))
            {
                Destroy(buildingObject);
                player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                return;
            }
            ModularBuilding modular = buildingObject.GetComponent<ModularBuilding>();
            if (ModularBuildingManager.singleton.spawnedBuilding) Destroy(ModularBuildingManager.singleton.spawnedBuilding);

            if (modular.basementTrigger.Check())
            {
                modular.modularIndex = modularIndex == -1 ? modular.IncreaseIndex() : modularIndex;
                modular.main = main;
                modular.group = group;
                modular.owner = playerName;
                modular.SetPin(main ? UnityEngine.Random.Range(100000, 1000000).ToString() : "");
                NetworkServer.Spawn(buildingObject);
            }
            else
            {
                Destroy(buildingObject);
                return;
            }
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
    static Dictionary<int, ScriptableBuilding> cache;
    public static Dictionary<int, ScriptableBuilding> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableBuilding[] items = Resources.LoadAll<ScriptableBuilding>("");

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