using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player
{
    [HideInInspector] public PlayerAimPrecision playerShootPrecision;
}

public class PlayerAimPrecision : MonoBehaviour
{
    private Player player;
    public Level level;
    public LinearFloat precisionPerLevel;

    public float currentAimPrecision;
    private float equipmentBonus;

    public float Calculate()
    {
            equipmentBonus = 0;
            foreach (Boost slot in player.playerBoost.boosts)
                if (slot.boostType == "Sniper")
                    equipmentBonus += slot.perc;

            foreach (Ability slot in player.playerAbility.networkAbilities)
                if (slot.name == "Sniper")
                    equipmentBonus += AbilityManager.singleton.FindAbility("Sniper").bonus * slot.level;


            currentAimPrecision = level != null ? precisionPerLevel.Get(level.current) + equipmentBonus : 0;

            return currentAimPrecision;
    }

    public float CalculateWeapon(ItemSlot itemSlot)
    {
        equipmentBonus = 0;
        if (itemSlot.item.data is WeaponItem && itemSlot.amount > 0 && ((WeaponItem)itemSlot.item.data).accessoryToAdd.Count > 0)
        {
            for (int i = 0; i < itemSlot.item.accessories.Length; i++)
            {
                equipmentBonus += ((WeaponItem)itemSlot.item.accessories[i].data).weaponPrecision;
            }
        }
        return equipmentBonus;
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerShootPrecision = this;
    }

    public void Awake()
    {
        Assign();
        //CalculateMiss();
    }
}
