using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerWet playerWet;
}

public partial struct Item
{
    public float wet;
}

public partial class EquipmentItem
{
    public float maxWet;
}

public class PlayerWet : NetworkBehaviour
{
    private Player player;
    [SyncVar]
    public float wetEquipment;
    [SyncVar]
    public float wetEquipmentNaked;
    [SyncVar]
    public float maxWetEquipment;
    [SyncVar]
    public float maxWetEquipmentNaked;
    [SyncVar]
    public float avgWetEquipment;
    private int index = 0;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerWet = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        Coroutine();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public void Coroutine()
    {
        InvokeRepeating(nameof(GetMaxWetEquipment), CoroutineManager.singleton.wetInterval, CoroutineManager.singleton.wetInterval);
        InvokeRepeating(nameof(GetWetEquipment), CoroutineManager.singleton.wetInterval, CoroutineManager.singleton.wetInterval);
        InvokeRepeating(nameof(CheckWet), CoroutineManager.singleton.wetInterval, CoroutineManager.singleton.wetInterval);
        InvokeRepeating(nameof(WetNotification), 60.0f, 60.0f);
    }

    public void WetNotification()
    {
        if (maxWetEquipmentNaked > 0)
        {
            if (wetEquipmentNaked >= avgWetEquipment)
            {
                TargetSpawnAdvertise(player.netIdentity.connectionToClient);
            }
        }
        else if (maxWetEquipment > 0)
        {
            if (wetEquipment >= avgWetEquipment)
            {
                TargetSpawnAdvertise(player.netIdentity.connectionToClient);
            }
        }
    }


    public void CheckWet()
    {
        if (maxWetEquipmentNaked > 0)
        {
            avgWetEquipment = maxWetEquipmentNaked;
        }
        else
        {
            avgWetEquipment = ((maxWetEquipment / 100.0f) * 80.0f);
        }

        if (TemperatureManager.singleton.isRainy)
        {
            if (maxWetEquipmentNaked == 0)
            {
                wetEquipmentNaked = 0;
                for (int i = 0; i < player.equipment.slots.Count; i++)
                {
                    int index = i;
                    if (player.equipment.slots[index].amount > 0 && player.equipment.slots[index].item.data is EquipmentItem && ((EquipmentItem)player.equipment.slots[index].item.data).maxWet > 0.0f)
                    {
                        if (player.equipment.slots[index].item.wet < ((EquipmentItem)player.equipment.slots[index].item.data).maxWet)
                        {
                            ItemSlot slot = player.equipment.slots[index];
                            slot.item.wet += 0.01f;
                            player.equipment.slots[index] = slot;
                        }
                    }
                }
            }
            else
            {
                if (wetEquipmentNaked < maxWetEquipmentNaked)
                {
                    wetEquipmentNaked += 0.01f;
                }
                else if (wetEquipmentNaked >= maxWetEquipmentNaked)
                {
                    wetEquipmentNaked = maxWetEquipmentNaked;
                }
            }
        }

        if (TemperatureManager.singleton.season == "Winter" || TemperatureManager.singleton.season == "Autumn")
        {
            if (wetEquipment > 0)
            {
                if (wetEquipment >= avgWetEquipment)
                {
                    player.health.current -= WetManager.singleton.decreaseHealthIfWet;
                }
            }
            else if (wetEquipmentNaked > 0)
            {
                if (wetEquipmentNaked >= avgWetEquipment)
                {
                    player.health.current -= WetManager.singleton.decreaseHealthIfWet;
                }
            }
        }
        if (TemperatureManager.singleton.season == "Summer")
        {
            for (int i = 0; i < player.equipment.slots.Count; i++)
            {
                int index = i;
                if (player.equipment.slots[index].amount > 0 && player.equipment.slots[index].item.wet > 0.0f)
                {
                    ItemSlot slot = player.equipment.slots[index];
                    slot.item.wet -= 0.01f;
                    if (slot.item.wet < 0.0f)
                        slot.item.wet = 0.0f;
                    player.equipment.slots[index] = slot;
                }
            }
        }
    }
    [TargetRpc]
    public void TargetSpawnAdvertise(NetworkConnection connection)
    {
        //UINotificationManager.singleton.SpawnWetObject();
    }

    public void GetWetEquipment()
    {
        float equipmentBonus = 0;
        foreach (ItemSlot slot in player.equipment.slots)
            if (slot.amount > 0)
                equipmentBonus += slot.item.wet;

        if (maxWetEquipmentNaked == 0)
            wetEquipment = equipmentBonus;
    }

    public void GetMaxWetEquipment()
    {
        float equipmentBonus = 0;
        foreach (ItemSlot slot in player.equipment.slots)
            if (slot.amount > 0 && slot.item.data is EquipmentItem)
                equipmentBonus += ((EquipmentItem)slot.item.data).maxWet;

        maxWetEquipment = equipmentBonus;

        if (maxWetEquipment == 0)
            maxWetEquipmentNaked = 0.03f;
        else
            maxWetEquipmentNaked = 0.0f;
    }
}
