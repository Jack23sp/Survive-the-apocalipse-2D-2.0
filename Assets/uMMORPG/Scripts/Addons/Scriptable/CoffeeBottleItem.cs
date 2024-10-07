using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "uMMORPG Item/Coffee Bottle", order = 999)]

public partial class CoffeeBottleItem : UsableItem
{
    public void Drink(Player player, int inventoryIndex, bool isInventory)
    {
        ItemSlot slot;
        slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];

        player.playerTired.tired = player.playerTired.maxTiredness;

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
