using System.Text;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "uMMORPG Item/Food", order = 999)]
public partial class FoodItem : UsableItem
{
    [Header("Equipment")]
    public int foodToAdd;
    public int waterToAdd;
    public int maxBlood;
    public int petHealthToAdd;

    // usage
    // -> can we equip this into any slot?
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return true;
    }

    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        if (player.inventory.slots[inventoryIndex].item.waterContainer > 0)
        {
            int currentThirsty = player.playerThirsty.max - player.playerThirsty.current;
            if (currentThirsty <= player.inventory.slots[inventoryIndex].item.waterContainer)
            {
                player.playerThirsty.current += currentThirsty;
                ItemSlot currentSlot = player.inventory.slots[inventoryIndex];
                currentSlot.item.waterContainer -= currentThirsty;
                player.inventory.slots[inventoryIndex] = currentSlot;
            }
            else
            {
                player.playerThirsty.current += player.inventory.slots[inventoryIndex].item.waterContainer;
                ItemSlot currentSlot = player.inventory.slots[inventoryIndex];
                currentSlot.item.waterContainer -= currentThirsty;
                player.inventory.slots[inventoryIndex] = currentSlot;
            }
        }
        if (foodToAdd > 0 || waterToAdd > 0)
        {
            player.playerHungry.current += foodToAdd;
            if (player.playerHungry.current > player.playerHungry.max)
            {
                player.playerHungry.current = player.playerHungry.max;
            }

            player.playerThirsty.current += waterToAdd;
            if (player.playerThirsty.current > player.playerThirsty.max)
            {
                player.playerThirsty.current = player.playerThirsty.max;
            }

            ItemSlot slot = player.inventory.slots[inventoryIndex];
            slot.DecreaseAmount(1);
            player.inventory.slots[inventoryIndex] = slot;
        }
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        //if(foodToAdd > 0)tip.Replace("{FOODTOADD}", "Add : " + foodToAdd.ToString() + " to food \n");
        //if (waterToAdd > 0) tip.Replace("{WATERTOADD}", "Add : " + waterToAdd.ToString() + " to water\n");
        //if(armorToAdd > 0)tip.Replace("{ARMORTOADD}", "Add : " + armorToAdd.ToString() + " to armor\n");
        //if(maxUnsanity.baseValue > 0)tip.Replace("{UNSANITY}", "Has : " + maxUnsanity.baseValue.ToString() + " unsanity\n");
        return tip.ToString();
    }
}
