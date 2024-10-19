using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class WeaponItem
{
    public ScriptableAbility weaponAbility;
    public bool isMetal;
    public bool isWood;
    public float weaponPrecision;

    public float CalculateWeaponDamage(Player player)
    {
        float b = 0.0f;
        ItemSlot slot = player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Weapon"))];
        switch (slot.item.name)
        {
            case "Baseball bat":
                b = AbilityManager.singleton.FindNetworkAbilityLevel("Baseball player", player.name);
                break;
            case "Robin hood":
                b = AbilityManager.singleton.FindNetworkAbilityLevel("Robin hood", player.name);
                break;
            case "Samurai":
                b = AbilityManager.singleton.FindNetworkAbilityLevel("Samurai", player.name);
                break;
            case "Shorthand master":
                b = AbilityManager.singleton.FindNetworkAbilityLevel("Shorthand master", player.name);
                break;
        }

        return b;
    }
}
