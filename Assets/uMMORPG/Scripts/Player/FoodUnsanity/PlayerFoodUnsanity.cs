using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerFoodUnsanity playerFoodUnsanity;
}

public partial struct Item
{
    public int currentUnsanity;
}

public class PlayerFoodUnsanity : NetworkBehaviour
{
    private Player player;
    public float cycleAmount = 60.0f;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerFoodUnsanity = this;
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
        cycleAmount = CoroutineManager.singleton.unsanityInvoke;
        InvokeRepeating(nameof(DecreaseUnsanity), cycleAmount, cycleAmount);
    }

    public void DecreaseUnsanity()
    {
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            if (player.inventory.slots[i].amount > 0)
            {
                if (player.inventory.slots[i].item.data is FoodItem)
                {
                    if (player.inventory.slots[i].item.currentUnsanity > 0)
                    {
                        ItemSlot slot = player.inventory.slots[i];
                        slot.item.currentUnsanity--;
                        if (slot.item.currentUnsanity == 0)
                            player.inventory.slots[i] = new ItemSlot();
                        else
                            player.inventory.slots[i] = slot;
                    }
                }
            }
        }
    }

}
