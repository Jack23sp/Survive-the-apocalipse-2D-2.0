using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerTemperature playerTemperature;
}

public partial struct Item
{
    public float coverTemperature;
}

public class PlayerTemperature : NetworkBehaviour
{
    private Player player;
    private float cycleAmount;


    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerTemperature = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        cycleAmount = CoroutineManager.singleton.temperatureInvoke;
        //InvokeRepeating(nameof(CheckTemperatureCover), cycleAmount, cycleAmount);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        InvokeRepeating(nameof(SpawnMessageRoutine), 90.0f, 90.0f);
    }

    public void SpawnMessageRoutine()
    {
        //if (actualCover < TemperatureManager.singleton.actualSafeCover)
        //{
        //    UINotificationManager.singleton.SpawnCoverObject();
        //}
    }


    //public float actualCover
    //{
    //    get
    //    {
    //        float equipmentBonus = 0;
    //        foreach (ItemSlot slot in player.equipment.slots)
    //            if (slot.amount > 0)
    //                equipmentBonus += slot.item.data.coverTemperature;

    //        return equipmentBonus;
    //    }
    //}

    //public void CheckTemperatureCover()
    //{
    //    if (player.playerTemperature.actualCover < TemperatureManager.singleton.actualSafeCover)
    //    {
    //        player.health.current -= TemperatureManager.singleton.healthToRemove;
    //    }
    //}
}
