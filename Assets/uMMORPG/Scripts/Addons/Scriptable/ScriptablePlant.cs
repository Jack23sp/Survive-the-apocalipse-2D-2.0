using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "uMMORPG Item/Plant", order = 999)]
public partial class ScriptablePlant : UsableItem
{
    [Header("Plant Grow Amount")]
    public float GrowAmount;
    public float GrowInterval;
    public string GrowSeason;
    public Vector2 scaleDimension;
    public int maxSeeds;
    public int plantAmountHarvest;
    public ScriptablePlant harvestPlant;
    public ScriptablePlant harvestSeeds;

    [Header("Plant characteristics")]
    public int foodToAdd;
    public int waterToAdd;

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptablePlant> cache;
    public static Dictionary<int, ScriptablePlant> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptablePlant[] items = Resources.LoadAll<ScriptablePlant>("");

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
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return base.CanUse(player, inventoryIndex);
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
        if (foodToAdd > 0 || waterToAdd > 0 )
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
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        return tip.ToString();
    }

}