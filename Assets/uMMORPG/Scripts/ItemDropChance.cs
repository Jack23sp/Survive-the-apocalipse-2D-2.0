// Defines the drop chance of an item for monster loot generation.
using System;
using UnityEngine;

[Serializable]
public class ItemDropChance
{
    public ScriptableItem item;
    [Range(0,1)] public float probability;
    [Range(1,20)] public int amount;
}
