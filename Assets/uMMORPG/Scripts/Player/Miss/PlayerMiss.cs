using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerMiss playerMiss;
}

public class PlayerMiss : MonoBehaviour
{
    private Player player;
    public Level level;
    public LinearFloat missPerLevel;

    private float currentMiss;
    private float equipmentBonus;

    public float _current
    {
        get
        {
            equipmentBonus = 0;
            foreach (Boost slot in player.playerBoost.boosts)
                if (slot.boostType == "Dexterity")
                    equipmentBonus += slot.perc;

            foreach (Ability slot in player.playerAbility.networkAbilities)
                if (slot.name == "Dexterity")
                    equipmentBonus += slot.level * AbilityManager.singleton.FindAbility("Dexterity").bonus;

            currentMiss = level != null ? missPerLevel.Get(level.current) + equipmentBonus : 0 + equipmentBonus;

            return currentMiss;
        }
    }


    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerMiss = this;
    }

    public void Awake()
    {
        Assign();
        //CalculateMiss();
    }

}
