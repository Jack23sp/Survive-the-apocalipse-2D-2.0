using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Player
{
    [HideInInspector] public PlayerConservative playerConservative;
}

public class PlayerConservative : NetworkBehaviour
{
    private Player player;
    [HideInInspector] public ScriptableAbility ability;

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

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerConservative = this;
        ability = AbilityManager.singleton.conservativeAbility;
    }

    public int AmountOfItemLostable()
    {
        return AbilityManager.singleton.FindNetworkAbilityLevel(ability.name, player.name) / 10 > 1 ? 5 - Convert.ToInt32(AbilityManager.singleton.FindNetworkAbilityLevel(ability.name, player.name)) : 5;
    }

    public int AmountOfItemProtected()
    {
        if (player.equipment.slots[7].amount > 0)
        {
            return player.equipment.slots[7].item.data.additionalSlot.Get(player.equipment.slots[7].item.bagLevel);
        }
        return 1;
    }
}
