using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerAccuracy playerAccuracy;
}

public partial class EquipmentItem
{
    public LinearFloat accuracy;
}

public partial struct Item
{
    public int accuracyLevel;
}

public class PlayerAccuracy : NetworkBehaviour
{
    private Player player;
    public LinearFloat linearAccuracy;
    private Level level;
    private float currentAccuracy;
    private float equipmentBonus;

    public float _accuracy
    {
        get
        {
            equipmentBonus = 0;
            foreach (Boost slot in player.playerBoost.boosts)
                if (slot.boostType == "Precision")
                    equipmentBonus += slot.perc;

            foreach (Ability slot in player.playerAbility.networkAbilities)
                if (slot.name == "Precision")
                    equipmentBonus += slot.level;

            currentAccuracy = level != null ? linearAccuracy.Get(level.current) + equipmentBonus : 0 + equipmentBonus;
            return currentAccuracy;
        }
    }


    void Awake()
    {
        player = GetComponent<Player>();
        level = player.level;
        player.playerAccuracy = this;
    }
}
