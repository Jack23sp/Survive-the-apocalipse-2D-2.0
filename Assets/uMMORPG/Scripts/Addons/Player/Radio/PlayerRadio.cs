using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Player
{
    [HideInInspector] public PlayerRadio playerRadio;
}

public partial struct Item
{
    public int radioCurrentBattery;
}

public partial class Database
{
    class radio
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int isActive { get; set; }
    }

    public void Connect_Radio()
    {
        connection.CreateTable<radio>();
    }

    public void SaveRadio(Player player)
    {
        PlayerRadio radio = player.GetComponent<PlayerRadio>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM radio WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new radio
        {
            characterName = player.name,
            isActive = Convert.ToInt32(radio.isOn)
        });

    }
    public void LoadRadio(Player player)
    {
        PlayerRadio radio = player.GetComponent<PlayerRadio>();

        foreach (radio row in connection.Query<radio>("SELECT * FROM radio WHERE characterName=?", player.name))
        {
            radio.isOn = Convert.ToBoolean(row.isActive);
        }

    }

}

public class PlayerRadio : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = nameof(RefreshUI))]
    public bool isOn;
    [HideInInspector] public ItemSlot radioItem;
    float cycleAmount;

    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerRadio = this;
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
        cycleAmount = CoroutineManager.singleton.radioInvoke;
        InvokeRepeating(nameof(DecreaseRadio), cycleAmount, cycleAmount);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        InvokeRepeating(nameof(SpawnMessageRoutine), 300.0f, 300.0f);
    }

    public void RefreshUI(bool oldBool, bool newBool)
    {
        //if (UIRadioTorchManager.singleton) UIRadioTorchManager.singleton.RefreshRadioTorchUI();
        //if (TorchRadioManager.singleton) TorchRadioManager.singleton.RefreshRadioTorch();
    }

    public void SpawnMessageRoutine()
    {
        if (radioItem.amount > 0)
        {
            if (radioItem.item.radioCurrentBattery <= 20)
            {
                //UINotificationManager.singleton.SpawnRadioObject();
            }
        }
    }

    public void CheckRadio()
    {
        if (player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Radio")) != -1)
        {
            radioItem = player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Radio"))];
        }
        else
        {
            radioItem.amount = 0;
        }
    }

    public void DecreaseRadio()
    {
        if (radioItem.amount > 0 && isOn && radioItem.item.radioCurrentBattery > 0)
        {
            radioItem.item.radioCurrentBattery--;
            player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Radio"))] = radioItem;

            isOn = radioItem.item.radioCurrentBattery == 0 ? isOn = false : isOn = true;
        }
    }

    [Command]
    public void CmdSetRadio()
    {
        if (radioItem.item.name != string.Empty && NetworkTime.time >= nextRiskyActionTime)
        {
            isOn = !isOn;
            if (radioItem.item.radioCurrentBattery == 0)
            {
                isOn = false;
            }
            nextRiskyActionTime = NetworkTime.time + 1.5f;
        }
    }
}
