using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;


public partial class EquipmentItem
{
    public LinearInt battery;
}

public partial class Player
{
    [HideInInspector] public PlayerTorch playerTorch;
}

public partial struct Item
{
    public int torchCurrentBattery;
    public int batteryLevel;
}

public partial class Database
{
    class torch
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int isActive { get; set; }
    }

    public void Connect_Torch()
    {
        connection.CreateTable<torch>();
    }

    public void SaveTorch(Player player)
    {
        PlayerTorch torch = player.GetComponent<PlayerTorch>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM torch WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new torch
        {
            characterName = player.name,
            isActive = Convert.ToInt32(torch.isOn)
        });

    }
    public void LoadTorch(Player player)
    {
        PlayerTorch torch = player.GetComponent<PlayerTorch>();

        foreach (torch row in connection.Query<torch>("SELECT * FROM torch WHERE characterName=?", player.name))
        {
            torch.isOn = Convert.ToBoolean(row.isActive);
        }
    }
}

public class PlayerTorch : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = nameof(RefreshUI))]
    public bool isOn;
    public ItemSlot torchItem;
    public GameObject torch;
    public GameObject personalTorch;

    public float cycleAmount;

    private bool instantiate;

    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerTorch = this;
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
        cycleAmount = CoroutineManager.singleton.torchInvoke;
        InvokeRepeating(nameof(DecreaseTorch), cycleAmount, cycleAmount);
        ManageTorch(true, isOn);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        personalTorch.SetActive(true);
        InvokeRepeating(nameof(SpawnMessageRoutine), 300.0f, 300.0f);
        PostProcessingManager.singleton.postEffect.SetActive(player.playerOptions.postProcessing);
    }

    public void SpawnMessageRoutine()
    {
        if (torchItem.amount > 0)
        {
            if (torchItem.item.torchCurrentBattery <= 20)
            {
                //UINotificationManager.singleton.SpawnTorchObject();
            }
        }
    }

    public void CheckTorch()
    {
        if (player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Torch")) != -1)
        {
            torchItem = player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Torch"))];
        }
        else
        {
            torchItem.amount = 0;
        }
        ManageTorch(isOn, isOn);
    }

    public void RefreshUI(bool oldBool, bool newBool)
    {
        torch.SetActive(newBool);
    }

    void ManageTorch(bool oldBool, bool newBool)
    {
        torch.SetActive(newBool);
    }

    public void DecreaseTorch()
    {
        if (torchItem.amount > 0 && isOn && torchItem.item.torchCurrentBattery > 0)
        {
            torchItem.item.torchCurrentBattery--;
            player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Torch"))] = torchItem;

            isOn = torchItem.item.torchCurrentBattery == 0 ? isOn = false : isOn = true;
        }
    }

    [Command]
    public void CmdSetTorch()
    {
        CheckTorch();

        if (torchItem.amount > 0 && torchItem.item.name != string.Empty && NetworkTime.time >= nextRiskyActionTime)
        {
            isOn = !isOn;
            if (torchItem.item.torchCurrentBattery == 0)
            {
                isOn = false;
            }
            nextRiskyActionTime = NetworkTime.time + 1.5f;
        }
    }

}
