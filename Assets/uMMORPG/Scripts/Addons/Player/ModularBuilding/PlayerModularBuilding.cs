using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Player
{
    [HideInInspector] public PlayerModularBuilding playerModularBuilding;
}

public class PlayerModularBuilding : NetworkBehaviour
{
    private Player player;
    private List<ModularBuilding> wallOrdered = new List<ModularBuilding>();
    private List<ModularDoor> doorOrdered = new List<ModularDoor>();
    private List<BuildingAccessory> accessoryOrdered = new List<BuildingAccessory>();
    private List<GameObject> fenceOrdered = new List<GameObject>();
    private List<GameObject> gateOrdered = new List<GameObject>();
    List<ItemCrafting> craft = new List<ItemCrafting>();

    [HideInInspector] public Type fakeBuilding;
    [HideInInspector] public BuildingAccessory oldBuilding;
    [SyncVar]
    public NetworkIdentity fakeBuildingID;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerModularBuilding = this;
        GetComponent<DamagableObject>().player = player;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }

    [Command]
    public void CmdSetPin(NetworkIdentity identity, NetworkIdentity playerIdentity, string firstPIN, string secondPIN)
    {
        ModularBuilding modularBuilding = identity.GetComponent<ModularBuilding>();
        Player player = playerIdentity.GetComponent<Player>();

        if (player)
        {
            if (player.guild.InGuild())
            {
                if (player.guild.guild.name != modularBuilding.group)
                {
                    if (modularBuilding.group == string.Empty ||
                       (modularBuilding.group != string.Empty &&
                       AbilityManager.singleton.FindNetworkAbilityLevel("Thief", player.name) >= modularBuilding.level))
                    {
                        for (int i = 0; i < ModularBuildingManager.singleton.combinedModulars.Count; i++)
                        {
                            if (ModularBuildingManager.singleton.combinedModulars[i].modularIndex == modularBuilding.modularIndex)
                            {
                                ModularBuildingManager.singleton.combinedModulars[i].SetPin(secondPIN);
                                ModularBuildingManager.singleton.combinedModulars[i].owner = player.name;
                                ModularBuildingManager.singleton.combinedModulars[i].group = player.guild.guild.name;
                            }
                        }
                    }
                }
                else
                {
                    if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, player.name) >= 1)
                    {
                        //if (modularBuilding.GetPin() == "0000")
                        //{
                        //    if (firstPIN != secondPIN)
                        //    {
                        //        for (int i = 0; i < ModularBuildingManager.singleton.combinedModulars.Count; i++)
                        //        {
                        //            if (ModularBuildingManager.singleton.combinedModulars[i].modularIndex == modularBuilding.modularIndex)
                        //            {
                        //                ModularBuildingManager.singleton.combinedModulars[i].SetPin(secondPIN);
                        //            }
                        //        }
                        //    }
                        //}
                        //else
                        //{
                            if (firstPIN == modularBuilding.GetPin()
                                && secondPIN != string.Empty
                                && secondPIN != firstPIN)
                            {
                                for (int i = 0; i < ModularBuildingManager.singleton.combinedModulars.Count; i++)
                                {
                                    if (ModularBuildingManager.singleton.combinedModulars[i].modularIndex == modularBuilding.modularIndex)
                                    {
                                        ModularBuildingManager.singleton.combinedModulars[i].SetPin(secondPIN);
                                    }
                                }
                            }
                        //}
                    }
                }
            }
            else
            {
                if (modularBuilding.owner == player.name
                   && secondPIN != firstPIN
                   && firstPIN != string.Empty)
                {
                    for (int i = 0; i < ModularBuildingManager.singleton.combinedModulars.Count; i++)
                    {
                        if (ModularBuildingManager.singleton.combinedModulars[i].modularIndex == modularBuilding.modularIndex)
                        {
                            ModularBuildingManager.singleton.combinedModulars[i].SetPin(secondPIN);
                            ModularBuildingManager.singleton.combinedModulars[i].owner = player.name;
                            ModularBuildingManager.singleton.combinedModulars[i].group = player.guild.guild.name;
                        }
                    }
                }
                else if (modularBuilding.owner != player.name
                         && secondPIN != firstPIN
                         && firstPIN != string.Empty)
                {
                    if (AbilityManager.singleton.FindNetworkAbilityLevel("Thief", player.name) >= modularBuilding.level)
                    {
                        for (int i = 0; i < ModularBuildingManager.singleton.combinedModulars.Count; i++)
                        {
                            if (ModularBuildingManager.singleton.combinedModulars[i].modularIndex == modularBuilding.modularIndex)
                            {
                                ModularBuildingManager.singleton.combinedModulars[i].SetPin(secondPIN);
                                ModularBuildingManager.singleton.combinedModulars[i].owner = player.name;
                                ModularBuildingManager.singleton.combinedModulars[i].group = player.guild.guild.name;
                            }
                        }
                    }
                }
            }
        }
    }

    [TargetRpc]
    public void TargetContinuosPlacing(NetworkIdentity playerIdentity, bool isInventory, string ItemName)
    {
        Player player = playerIdentity.gameObject.GetComponent<Player>();

        if (player)
        {
            if (isInventory)
            {
                for (int i = 0; i < player.inventory.slots.Count; i++)
                {
                    int index = i;
                    if (player.inventory.slots[index].amount == 0) continue;
                    if (player.inventory.slots[index].item.data.name == ItemName)
                    {
                        switch (player.inventory.slots[index].item.data.GetType().ToString())
                        {
                            case "ScriptableBuilding":
                                ((ScriptableBuilding)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableDoor":
                                ((ScriptableDoor)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableWall":
                                ((ScriptableWall)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableBuildingAccessory":
                                ((ScriptableBuildingAccessory)player.inventory.slots[index].item.data).Spawn(player, true, index, 0, player.transform.position);
                                break;
                            case "ScriptableFence":
                                ((ScriptableFence)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableGate":
                                ((ScriptableGate)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableLight":
                                ((ScriptableLight)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableConcrete":
                                ((ScriptableConcrete)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableBillboard":
                                ((ScriptableBillboard)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableFlag":
                                ((ScriptableFlag)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableFurnace":
                                ((ScriptableFurnace)player.inventory.slots[index].item.data).Use(player, index);
                                break;
                            case "ScriptableWaterContainer":
                                ((ScriptableWaterContainer)player.inventory.slots[index].item.data).Use(player, index);
                                break;

                        }
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < player.playerBelt.belt.Count; i++)
                {
                    int index = i;
                    if (player.playerBelt.belt[index].amount == 0) continue;
                    if (player.playerBelt.belt[index].item.data.name == ItemName)
                    {
                        switch (player.playerBelt.belt[index].item.data.GetType().ToString())
                        {
                            case "ScriptableBuilding":
                                ((ScriptableBuilding)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableDoor":
                                ((ScriptableDoor)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableWall":
                                ((ScriptableWall)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableBuildingAccessory":
                                ((ScriptableBuildingAccessory)player.playerBelt.belt[index].item.data).Spawn(player, true, index, 0, player.transform.position);
                                break;
                            case "ScriptableFence":
                                ((ScriptableFence)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableGate":
                                ((ScriptableGate)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableLight":
                                ((ScriptableLight)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableConcrete":
                                ((ScriptableConcrete)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableBillboard":
                                ((ScriptableBillboard)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableFlag":
                                ((ScriptableFlag)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableFurnace":
                                ((ScriptableFurnace)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                            case "ScriptableWaterContainer":
                                ((ScriptableWaterContainer)player.playerBelt.belt[index].item.data).Use(player, index);
                                break;
                        }
                        return;
                    }
                }
            }
        }
    }

    public bool CheckDistanceFromSpawn(Transform t)
    {
        return Vector2.Distance(t.position, NetworkManagerMMO.GetNearestStartPosition(t.position).position) < 20;
    }

    [Command]
    public void CmdAddWall(NetworkIdentity identity, int localPositioning, int typeToSet, int inventoryIndex, bool isInventory, string itemName)
    {
        ModularBuilding modularBuilding = identity.GetComponent<ModularBuilding>();
        bool spawned = false;
        ItemSlot slot = isInventory == true ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data.name != itemName) return;

        if (localPositioning == 1 && modularBuilding.serverUpBasementDecoration == -1)
        {
            modularBuilding.serverUpBasementDecoration = typeToSet; spawned = true;
            modularBuilding.doorHealthUp = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
            modularBuilding.wallHealthUp = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
        }
        if (localPositioning == 2 && modularBuilding.serverLeftBasementDecoration == -1)
        {
            modularBuilding.serverLeftBasementDecoration = typeToSet; spawned = true;
            modularBuilding.doorHealthSx = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
            modularBuilding.wallHealthSx = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
        }
        if (localPositioning == 3 && modularBuilding.serverDownBasementDecoration == -1)
        {
            modularBuilding.serverDownBasementDecoration = typeToSet; spawned = true;
            modularBuilding.doorHealthDown = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
            modularBuilding.wallHealthDown = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
        }
        if (localPositioning == 4 && modularBuilding.serverRightBasementDecoration == -1)
        {
            modularBuilding.serverRightBasementDecoration = typeToSet; spawned = true;
            modularBuilding.doorHealthDx = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
            modularBuilding.wallHealthDx = modularBuilding.maxWallHealth + modularBuilding.level <= 1 ? 0 : modularBuilding.defaultValue * modularBuilding.level;
        }

        if (spawned)
        {
            player.quests.walls.Add(new QuestObject(slot.item.name.Replace("(Clone)", ""), 1));
            player.playerPoints.wallsPlacement++;
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
    }


    [Command]
    public void CmdRepairAccessory(NetworkIdentity identity)
    {
        BuildingAccessory accessory = identity.gameObject.GetComponent<BuildingAccessory>();

        for (int i = 0; i < accessory.craftingAccessoryItem.repairItems.Count; i++)
        {
            if (player.inventory.CountItem(new Item(accessory.craftingAccessoryItem.repairItems[i].items)) < accessory.craftingAccessoryItem.repairItems[i].amount)
            {
                return;
            }
        }

        for (int i = 0; i < accessory.craftingAccessoryItem.repairItems.Count; i++)
        {
            player.inventory.RemoveItem(new Item(accessory.craftingAccessoryItem.repairItems[i].items), accessory.craftingAccessoryItem.repairItems[i].amount);
        }

        accessory.health = accessory.maxHealth;
    }

    [Command]
    public void CmdRepairModular(NetworkIdentity identity)
    {
        ModularBuilding modular = identity.gameObject.GetComponent<ModularBuilding>();

        for (int i = 0; i < modular.building.repairItems.Count; i++)
        {
            if (player.inventory.CountItem(new Item(modular.building.repairItems[i].items)) < modular.building.repairItems[i].amount)
            {
                return;
            }
        }

        for (int i = 0; i < modular.building.repairItems.Count; i++)
        {
            player.inventory.RemoveItem(new Item(modular.building.repairItems[i].items), modular.building.repairItems[i].amount);
        }

        modular.AddHealth(10000);
    }

    [Command]
    public void CmdAddLevel(NetworkIdentity identity)
    {
        ModularBuilding modularBuilding = identity.GetComponent<ModularBuilding>();
        bool canUpgrade = true;
        for (int i = 0; i < modularBuilding.building.upgradeItems.Count; i++)
        {
            int index = i;
            if (player.inventory.CountItem(new Item(modularBuilding.building.upgradeItems[index].items)) < modularBuilding.building.upgradeItems[index].amount)
            {
                canUpgrade = false;
            }
        }

        if (canUpgrade)
        {
            for (int i = 0; i < modularBuilding.building.upgradeItems.Count; i++)
            {
                int index = i;
                player.inventory.RemoveItem(new Item(modularBuilding.building.upgradeItems[index].items), modularBuilding.building.upgradeItems[index].amount);
            }

            for (int i = 0; i < ModularBuildingManager.singleton.combinedModulars.Count; i++)
            {
                if (ModularBuildingManager.singleton.combinedModulars[i].modularIndex == modularBuilding.modularIndex)
                {
                    ModularBuildingManager.singleton.combinedModulars[i].level++;
                }
            }
            modularBuilding.AddHealth(ModularBuildingManager.singleton.healthToAddToWalls);
            TargetRefreshItemToUpgrade();
        }
    }

    [TargetRpc]
    public void TargetRefreshItemToUpgrade()
    {
        if (UICentralManager.singleton)
        {
            UICentralManager.singleton.RefreshModularBuildingLevel();
            UICentralManager.singleton.RefreshItemsToUpgrade();
        }
    }

    [Command]
    public void CmdSetFloor(NetworkIdentity identity, int index)
    {
        ModularBuilding modularBuilding = identity.GetComponent<ModularBuilding>();
        modularBuilding.floorTexture = index;
        TargetCloseUICentralManager();
    }

    [TargetRpc]
    public void TargetCloseUICentralManager()
    {
        if (UICentralManager.singleton)
        {
            UICentralManager.singleton.floorButtonText.text = "Done!";
            UICentralManager.singleton.closeButton.onClick.Invoke();
        }
    }

    [Command]
    public void CmdSetFakeBuildingID(NetworkIdentity identity)
    {
        fakeBuildingID = identity;
    }

    [Command]
    public void CmdRemoveFakeBuildingID(bool condition)
    {
        fakeBuildingID = null;
        RpcManageVisibilityOfObject(fakeBuildingID, condition);
    }

    [Command]
    public void CmdManageVisibilityOfObject( bool condition)
    {
        if (fakeBuildingID)
        {
            fakeBuildingID.gameObject.SetActive(condition);
            RpcManageVisibilityOfObject(fakeBuildingID, condition);
        }
    }

    public void ManageVisibilityOfObject(NetworkIdentity identity, bool condition)
    {
        identity.gameObject.SetActive(condition);
        RpcManageVisibilityOfObject(identity, condition);
    }

    [ClientRpc]
    public void RpcManageVisibilityOfObject(NetworkIdentity identity, bool condition)
    {
        if (identity != null)
            identity.gameObject.SetActive(condition);
    }

    [Command]
    public void CmdCreateAccessory(int inventoryIndex, bool isInventory, string itemName, int direction, Vector3 position, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.gameObject.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[direction].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }

                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }
        ItemSlot slot = isInventory == true ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data.name != itemName) return;

        GameObject inst = Instantiate(((ScriptableBuildingAccessory)slot.item.data).buildingList[direction].buildingObject, position, Quaternion.identity);
        if (player.playerModularBuilding.CheckDistanceFromSpawn(inst.transform))
        {
            Destroy(inst);
            player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
            return;
        }
        NetworkServer.Spawn(inst);

        if (((ScriptableBuildingAccessory)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
        }

        slot.DecreaseAmount(1);
        player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
        player.playerPoints.accessoriesPlacement++;
        if (isInventory)
        {
            player.inventory.slots[inventoryIndex] = slot;
        }
        else
        {
            player.playerBelt.belt[inventoryIndex] = slot;
        }
    }


    [Command]
    public void CmdDeleteWall(NetworkIdentity identity, int localPositioning)
    {
        ModularBuilding modularBuilding = identity.GetComponent<ModularBuilding>();

        if (localPositioning == 1)
        {
            modularBuilding.serverUpBasementDecoration = -1;
            modularBuilding.upDoorOpen = false;
        }
        if (localPositioning == 2)
        {
            modularBuilding.serverLeftBasementDecoration = -1;
            modularBuilding.leftDoorOpen = false;
        }

        if (localPositioning == 3)
        {
            modularBuilding.serverDownBasementDecoration = -1;
            modularBuilding.downDoorOpen = false;
        }

        if (localPositioning == 4)
        {
            modularBuilding.serverRightBasementDecoration = -1;
            modularBuilding.rightDoorOpen = false;
        }

    }

    [Command]
    public void CmdSpawnBuilding(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, bool main, int modularIndex)
    {
        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableBuilding)
        {
            ((ScriptableBuilding)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName, main, modularIndex);
            player.quests.basements.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.basementPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableBuilding)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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

    [Command]
    public void CmdSpawnFence(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.GetComponent<Fence>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }

        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableFence)
        {
            ((ScriptableFence)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableFence)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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

    [Command]
    public void CmdSpawnExternal(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = null;
                if (id.GetComponent<Fence>())
                {
                    ins = Instantiate(id.GetComponent<Fence>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                    if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                    {
                        Destroy(ins);
                        player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                        return;
                    }
                }
                else
                {
                    if (id.GetComponent<Concrete>())
                    {
                        ins = Instantiate(id.GetComponent<Concrete>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                        if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                        {
                            Destroy(ins);
                            player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                            return;
                        }
                    }
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }

        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (((ScriptableConcrete)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
        }

        if (slot.item.data is ScriptableConcrete)
        {
            ((ScriptableConcrete)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
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

    [Command]
    public void CmdSpawnBillboard(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }
        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableBillboard)
        {
            ((ScriptableBillboard)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableBillboard)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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

    [Command]
    public void CmdSpawnFlag(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);

                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }

        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableFlag)
        {
            ((ScriptableFlag)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableFlag)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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

    [Command]
    public void CmdSpawnFurnace(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }

        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableFurnace)
        {
            ((ScriptableFurnace)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableFurnace)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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

    [Command]
    public void CmdSpawnWaterContainer(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }

        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableWaterContainer)
        {
            ((ScriptableWaterContainer)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableWaterContainer)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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


    [Command]
    public void CmdSpawnGate(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }

        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableGate)
        {
            ((ScriptableGate)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableGate)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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

    [Command]
    public void CmdSpawnLamp(int inventoryIndex, bool isInventory, Vector3 position, int buildingIndex, string group, string playerName, NetworkIdentity id)
    {
        if (inventoryIndex == -1)
        {
            if (id != null)
            {
                GameObject ins = Instantiate(id.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[buildingIndex].buildingObject, position, Quaternion.identity);
                if (player.playerModularBuilding.CheckDistanceFromSpawn(ins.transform))
                {
                    Destroy(ins);
                    player.playerNotification.TargetSpawnNotification(ModularBuildingManager.singleton.buildingAreaRestrainSpawnArea);
                    return;
                }
                NetworkServer.Spawn(ins);
                Utilities.CopyInformation(id.gameObject, ins.gameObject);
                NetworkServer.Destroy(id.gameObject);
                return;
            }
        }

        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        if (slot.item.data is ScriptableLight)
        {
            ((ScriptableLight)slot.item.data).Spawn(player, inventoryIndex, isInventory, position, buildingIndex, group, playerName);
            player.quests.accessories.Add(new QuestObject(slot.item.data.name.Replace("(Clone)", ""), 1));
            player.playerPoints.accessoriesPlacement++;
        }
        else
        {
            return;
        }

        if (((ScriptableLight)slot.item.data).continuePlacing)
        {
            TargetContinuosPlacing(player.netIdentity, isInventory, slot.item.data.name);
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


    [Command]
    public void CmdManageGate(NetworkIdentity identity, bool status)
    {
        Gate gate = identity.GetComponent<Gate>();
        TargetManageAnimation(identity, !status);
        gate.navMeshObstacle2D.enabled = status;
    }

    [TargetRpc]
    public void TargetManageAnimation(NetworkIdentity identity, bool condition)
    {
        identity.GetComponent<Gate>().animator.SetBool("OPEN", condition);
        identity.GetComponent<Gate>().navMeshObstacle2D.enabled = condition;
    }

    [Command]
    public void CmdClaimBuilding(NetworkIdentity buildingIdentity, string pin)
    {
        if(pin != string.Empty)
        {
            ModularBuilding modular = buildingIdentity.gameObject.GetComponent<ModularBuilding>();
            if(AbilityManager.singleton.FindNetworkAbilityLevel("Thief", player.name) >= modular.level)
            {
                modular.owner = player.name;
                modular.group = player.guild.guild.name;
            }
        }
    }

    [Command]
    public void CmdManageDoor(GameObject identity, NetworkIdentity playerIdentity, int doorSide, string pin)
    {
        ModularBuilding modularBuilding = identity.GetComponent<ModularBuilding>();
        Player pl = playerIdentity.GetComponent<Player>();

        if (pin != string.Empty)
        {
            if (modularBuilding.GetPin() != pin) return;
            else
            {
                switch (doorSide)
                {
                    case 1:
                        modularBuilding.upDoorOpen = !modularBuilding.upDoorOpen;
                        break;
                    case 2:
                        modularBuilding.leftDoorOpen = !modularBuilding.leftDoorOpen;
                        break;
                    case 3:
                        modularBuilding.downDoorOpen = !modularBuilding.downDoorOpen;
                        break;
                    case 4:
                        modularBuilding.rightDoorOpen = !modularBuilding.rightDoorOpen;
                        break;
                }
            }
        }
        else
        {
            if (pl && 
                (AbilityManager.singleton.FindNetworkAbilityLevel("Thief", pl.name) > modularBuilding.level ||
                (!ModularBuildingManager.singleton.CanDoOtherActionFloor(modularBuilding, pl) 
                &&
                (doorSide == 1 && modularBuilding.upDoorOpen
                || doorSide == 2 && modularBuilding.leftDoorOpen
                || doorSide == 3 && modularBuilding.downDoorOpen
                || doorSide == 4 && modularBuilding.rightDoorOpen)))
                || ModularBuildingManager.singleton.CanDoOtherActionFloor(modularBuilding, pl))
            {
                switch (doorSide)
                {
                    case 1:
                        modularBuilding.upDoorOpen = !modularBuilding.upDoorOpen;
                        break;
                    case 2:
                        modularBuilding.leftDoorOpen = !modularBuilding.leftDoorOpen;
                        break;
                    case 3:
                        modularBuilding.downDoorOpen = !modularBuilding.downDoorOpen;
                        break;
                    case 4:
                        modularBuilding.rightDoorOpen = !modularBuilding.rightDoorOpen;
                        break;
                }
            }
        }
    }

    [Command]
    public void CmdAddToCraft(CraftinItemSlot craftSlot, int currencyType, NetworkIdentity identity, int craftIndex, int sliderValue)
    {
        CraftAccessory buildingAccessory = identity.GetComponent<CraftAccessory>();
        if (!buildingAccessory) return;
        ScriptableBuildingAccessory scriptableBuildingAccessory = buildingAccessory.craftingAccessoryItem;

        DateTime time = DateTime.UtcNow;
        craftSlot.serverTimeEnd = time.AddSeconds(currencyType == 0 ? scriptableBuildingAccessory.itemtoCraft[craftIndex].timeToCraft : 0).ToString();
        craftSlot.serverTimeBegin = time.ToString();

        craft.Clear();

        if (craftSlot.sex < 1)
        {
            for (int i = 0; i < scriptableBuildingAccessory.itemtoCraft.Count; i++)
            {
                craft.Add(scriptableBuildingAccessory.itemtoCraft[i]);
            }
        }
        else
        {
            for (int i = 0; i < scriptableBuildingAccessory.itemtoCraftFemale.Count; i++)
            {
                craft.Add(scriptableBuildingAccessory.itemtoCraftFemale[i]);
            }
        }

        for (int e = 0; e < craft[craftSlot.index].ingredients.Count; e++)
        {
            int index_e = e;
            if (player.inventory.CountItem(new Item(craft[craftSlot.index].ingredients[index_e].item)) < (craft[craftSlot.index].ingredients[index_e].amount * sliderValue))
            {
                return;
            }
        }


        if (currencyType == 0)
        {
            if (player.gold >= (craft[craftSlot.index].gold * sliderValue))
            {
                for (int e = 0; e < craft[craftSlot.index].ingredients.Count; e++)
                {
                    int index_e = e;
                    player.inventory.RemoveItem(new Item(craft[craftSlot.index].ingredients[index_e].item), (craft[craftSlot.index].ingredients[index_e].amount * sliderValue));
                }

                buildingAccessory.craftingItem.Add(craftSlot);
                player.gold -= (craft[craftSlot.index].gold * sliderValue);

                player.quests.crafts.Add(new QuestObject(craftSlot.item.Replace("(Clone)", ""), craftSlot.amount));
                player.playerPoints.craftPoint++;
            }
        }
        else
        {
            if (player.itemMall.coins >= (craft[craftSlot.index].coin * sliderValue))
            {
                for (int e = 0; e < craft[craftSlot.index].ingredients.Count; e++)
                {
                    int index_e = e;
                    player.inventory.RemoveItem(new Item(craft[craftSlot.index].ingredients[index_e].item), (craft[craftSlot.index].ingredients[index_e].amount * sliderValue));
                }

                buildingAccessory.craftingItem.Add(craftSlot);
                player.itemMall.coins -= (craft[craftSlot.index].coin * sliderValue);

                player.quests.crafts.Add(new QuestObject(craftSlot.item.Replace("(Clone)", ""), craftSlot.amount));
                player.playerPoints.craftPoint++;
            }
        }
    }

    [Command]
    public void CmdClaimCraftedItem(int index, NetworkIdentity identity)
    {
        CraftAccessory buildingAccessory = identity.GetComponent<CraftAccessory>();
        TimeSpan difference;
        if (!buildingAccessory) return;

        ScriptableBuildingAccessory scriptableBuildingAccessory = buildingAccessory.craftingAccessoryItem;

        difference = DateTime.Parse(buildingAccessory.craftingItem[index].serverTimeEnd) - DateTime.UtcNow;

        craft.Clear();

        if (buildingAccessory.craftingItem[index].sex < 1)
        {
            for (int i = 0; i < scriptableBuildingAccessory.itemtoCraft.Count; i++)
            {
                craft.Add(scriptableBuildingAccessory.itemtoCraft[i]);
            }
        }
        else
        {
            for (int i = 0; i < scriptableBuildingAccessory.itemtoCraftFemale.Count; i++)
            {
                craft.Add(scriptableBuildingAccessory.itemtoCraftFemale[i]);
            }
        }

        if (difference.TotalSeconds > 0)
        {
            return;
        }

        if (player.inventory.CanAddItem(new Item(craft[buildingAccessory.craftingItem[index].index].itemAndAmount.item), craft[buildingAccessory.craftingItem[index].index].itemAndAmount.amount))
        {
            player.inventory.AddItem(new Item(craft[buildingAccessory.craftingItem[index].index].itemAndAmount.item), craft[buildingAccessory.craftingItem[index].index].itemAndAmount.amount);
            buildingAccessory.craftingItem.Remove(buildingAccessory.craftingItem[index]);
            //TargetManagerUIUpgrade();
        }
    }

    //[TargetRpc]
    //public void TargetManagerUIUpgrade()
    //{
    //    if (UIUpgrade.singleton.panel.activeInHierarchy)
    //    {
    //        if (UIUpgrade.singleton.typeOpen)
    //        {
    //            UIInventoryAccessories.singleton.OpenStandard();
    //        }
    //        else
    //        {
    //            UIInventoryAccessories.singleton.Open();
    //        }
    //        UIUpgrade.singleton.contentItemInInventory.GetChild(UIInventoryAccessories.singleton.contentWeaponSelected).GetComponent<UIUpgradeItemSlot>().button.onClick.Invoke();           
    //    }
    //}

    public GameObject FindNearestFloorObject()
    {
        List<ModularBuilding> floor = FindObjectsOfType<ModularBuilding>().ToList();
        wallOrdered = floor.Select(x => x).Where(x => (x.GetComponent<NetworkIdentity>().isClient || x.GetComponent<NetworkIdentity>().isServer)).ToList();
        wallOrdered = wallOrdered.Select(go => go).Where(go => ModularBuildingManager.singleton.CanDoOtherActionFloor(go, player) && go.CheckCompleteAroundBase() == false && Vector3.Distance(player.transform.position, go.transform.position) < 20).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (wallOrdered.Count > 0)
            return wallOrdered[0].gameObject;
        else return null;
    }

    public GameObject FindNearestFence()
    {
        List<GameObject> accessory = GameObject.FindGameObjectsWithTag("WoodPlacer").ToList();
        fenceOrdered = accessory.Select(go => go).Where(go => ModularBuildingManager.singleton.CanDoOtherActionForniture(go.GetComponentInParent<BuildingAccessory>(), player) && Vector3.Distance(player.transform.position, go.transform.position) < 4).ToList();

        fenceOrdered = fenceOrdered.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (fenceOrdered.Count > 0)
            return fenceOrdered[0].gameObject;
        else return null;
    }

    public GameObject FindNearestGate()
    {
        List<GameObject> accessory = GameObject.FindGameObjectsWithTag("GatePlacer").ToList();
        gateOrdered = accessory.Select(go => go).Where(go => ModularBuildingManager.singleton.CanDoOtherActionForniture(go.GetComponentInParent<BuildingAccessory>(), player) && Vector3.Distance(player.transform.position, go.transform.position) < 4).ToList();

        gateOrdered = gateOrdered.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (gateOrdered.Count > 0)
            return gateOrdered[0].gameObject;
        else return null;
    }


    public GameObject FindNearestDoor()
    {
        List<ModularDoor> floor = FindObjectsOfType<ModularDoor>().ToList();
        doorOrdered = floor.Select(x => x).Where(x => (x.modularBuilding.identity.isClient || x.modularBuilding.identity.isServer) && x.wallManager.isActiveAndEnabled).ToList();
        doorOrdered = doorOrdered.Select(go => go).Where(go => ModularBuildingManager.singleton.CanDoOtherActionFloor(go.modularBuilding, player) && Vector3.Distance(player.transform.position, go.transform.position) < 2).ToList();

        doorOrdered = doorOrdered.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (doorOrdered.Count > 0)
            return doorOrdered[0].gameObject;
        else return null;
    }

    public List<ModularBuilding> FindNearestFloorObjects()
    {

        List<ModularBuilding> floor = FindObjectsOfType<ModularBuilding>().ToList();
        wallOrdered = floor.Select(x => x).Where(x => (x.GetComponent<NetworkIdentity>().isClient || x.GetComponent<NetworkIdentity>().isServer)).ToList();
        wallOrdered = wallOrdered.Select(go => go).Where(go => ModularBuildingManager.singleton.CanDoOtherActionFloor(go, player) && Vector3.Distance(player.transform.position, go.transform.position) < 20).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        return wallOrdered;
    }

    public GameObject FindNearestForniture()
    {
        List<BuildingAccessory> accessory = FindObjectsOfType<BuildingAccessory>().ToList();
        accessoryOrdered = accessory.Select(x => x).Where(x => (x.GetComponent<NetworkIdentity>().isClient || x.GetComponent<NetworkIdentity>().isServer)).ToList();
        accessoryOrdered = accessoryOrdered.Select(go => go).Where(go => ModularBuildingManager.singleton.CanDoOtherActionForniture(go, player) && Vector3.Distance(player.transform.position, go.transform.position) < 4).ToList();

        accessoryOrdered = accessoryOrdered.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (accessoryOrdered.Count > 0)
            return accessoryOrdered[0].gameObject;
        else return null;
    }

    public void AbleFloorPositioningPoints(List<ModularBuilding> buildings)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            int index = i;
            buildings[index].upBasementPositiioning.Manage(!buildings[index].up);
            buildings[index].downBasementPositiioning.Manage(!buildings[index].down);
            buildings[index].leftBasementPositiioning.Manage(!buildings[index].left);
            buildings[index].rightBasementPositiioning.Manage(!buildings[index].right);
        }
    }

    public void DisbleFloorPositioningPoints(List<ModularBuilding> buildings)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            int index = i;
            buildings[index].upBasementPositiioning.Manage(false);
            buildings[index].downBasementPositiioning.Manage(false);
            buildings[index].leftBasementPositiioning.Manage(false);
            buildings[index].rightBasementPositiioning.Manage(false);
        }
    }

    public void AbleFloorPositioningWalls(List<ModularBuilding> buildings)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            int index = i;
            buildings[index].upRenderer.Manage(buildings[index].serverUpBasementDecoration == -1 && buildings[index].upBasementDecoration == -1);
            buildings[index].downRenderer.Manage(buildings[index].serverDownBasementDecoration == -1 && buildings[index].downBasementDecoration == -1);
            buildings[index].leftRenderer.Manage(buildings[index].serverLeftBasementDecoration == -1 && buildings[index].leftBasementDecoration == -1);
            buildings[index].rightRenderer.Manage(buildings[index].serverRightBasementDecoration == -1 && buildings[index].rightBasementDecoration == -1);
        }
    }

    public void DisableFloorPositioningWallsOnCancel(List<ModularBuilding> buildings)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            int index = i;
            buildings[index].upRenderer.Manage(false);
            buildings[index].downRenderer.Manage(false);
            buildings[index].leftRenderer.Manage(false);
            buildings[index].rightRenderer.Manage(false);
        }
    }

    public GameObject FindNearestFloorPointAvailable(ModularBuilding floorObject)
    {
        float distance = 100000.0f;
        GameObject nearestAvaiablePointToSpawnBase = null;
        for (int i = 0; i < floorObject.wallActive.Count; i++)
        {
            int index = i;
            if (floorObject.wallActive[index].activeInHierarchy)
            {
                if (Vector2.Distance(transform.position, floorObject.wallActive[index].transform.position) < distance)
                {
                    distance = Vector2.Distance(transform.position, floorObject.wallActive[index].transform.position);
                    nearestAvaiablePointToSpawnBase = floorObject.wallActive[index];
                }
            }
        }
        return nearestAvaiablePointToSpawnBase;
    }

    [Command]
    public void CmdDeleteAccessory(NetworkIdentity identity)
    {
        if (identity)
        {
            if(!identity.gameObject.GetComponent<ModularBuilding>())
                NetworkServer.Destroy(identity.gameObject);
            else
            {
                ModularBuilding modular = identity.gameObject.GetComponent<ModularBuilding>();
                if(modular)
                {
                    GameObject newMainModular = FindNearestFloor(modular);

                    Collider2D[] coll = Physics2D.OverlapBoxAll(modular.transform.position, modular.thisCollider.bounds.size, 0, ModularBuildingManager.singleton.accessoryLayerToDestroyWithFloor);
                    for (int i = 0; i < coll.Length; i++)
                    {
                        NetworkServer.Destroy(coll[i].gameObject);
                    }
                    NetworkServer.Destroy(identity.gameObject);

                    if (newMainModular)
                    {
                        newMainModular.GetComponent<ModularBuilding>().main = true;
                    }
                }
            }
        }
    }

    public GameObject FindNearestFloor(ModularBuilding modular)
    {
        List<ModularBuilding> floor = ModularBuildingManager.singleton.combinedModulars;
        wallOrdered = floor.Select(x => x).Where(x => x.main == false
                                                && (x.GetComponent<NetworkIdentity>().isClient 
                                                 || x.GetComponent<NetworkIdentity>().isServer) 
                                                 && ModularBuildingManager.singleton.CanDoOtherActionFloor(x, player)).ToList();
        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(modular.transform.position, m.transform.position)).ToList();
        if (wallOrdered.Count > 0)
            return wallOrdered[0].gameObject;
        else return null;
    }

    public GameObject FindNearestFenceObject()
    {
        //List<Fence> floor = FindObjectsOfType<Fence>().ToList();
        ////wallOrdered = floor.Select(x => x).Where(x => (x.GetComponent<NetworkIdentity>().isClient || x.GetComponent<NetworkIdentity>().isServer)).ToList();
        ////wallOrdered = wallOrdered.Select(go => go).Where(go => ModularBuildingManager.singleton.CanDoOtherActionFloor(go, player) && go.CheckCompleteAroundBase() == false && Vector3.Distance(player.transform.position, go.transform.position) < 20).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (wallOrdered.Count > 0)
            return wallOrdered[0].gameObject;
        else return null;
    }
}
