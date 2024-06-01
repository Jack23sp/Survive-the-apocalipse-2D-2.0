using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerWeight playerWeight;
}

public partial struct Item
{
    public float weight;
}

public partial class ScriptableItem
{
    public float weight;
}

public class PlayerWeight : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = (nameof(ManageCurrentWeight)))]
    public float current;
    [SyncVar (hook = (nameof(ManageMaxCurrentWeight)))]
    public float max;
    public float defaultCarryWeight;
    [SyncVar]
    public int protectedSlot = 0;


    public void ManageCurrentWeight(float oldValue, float maxValue)
    {
        if (!UIKitchenSink.singleton) UIKitchenSink.singleton.SetWeightValue();
        if (!UIBathroomSink.singleton) UIBathroomSink.singleton.SetWeightValue();
        if (!UIWaterContainer.singleton) UIWaterContainer.singleton.SetWeightValue();
    }

    public void ManageMaxCurrentWeight(float oldValue, float maxValue)
    {
        if (!UIKitchenSink.singleton) UIKitchenSink.singleton.SetWeightValue();
        if (!UIBathroomSink.singleton) UIBathroomSink.singleton.SetWeightValue();
        if (!UIWaterContainer.singleton) UIWaterContainer.singleton.SetWeightValue();
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerWeight = this;
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
    public float GetCurrentWeight()
    {
        float weight = 0.0f;
        for(int i = 0; i < player.playerEquipment.slots.Count; i++)
        {
            int index = i;
            if (player.playerEquipment.slots[index].item.weight > 0)
                weight += player.playerEquipment.slots[index].item.weight;
        }
        return weight;
    }

}
