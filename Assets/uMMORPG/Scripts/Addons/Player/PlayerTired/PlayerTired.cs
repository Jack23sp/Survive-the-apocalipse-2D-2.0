using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Player
{
    [HideInInspector] public PlayerTired playerTired;
}

public class PlayerTired : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = nameof(ManageUITiredness))] public int tired = 200;
    public int tiredLimitForAim = 30;
    public int maxTiredness = 100;


    void Start()
    {
        Assign();
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerTired = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        InvokeRepeating(nameof(ManageTiredness), 7.0f, 7.0f);
        InvokeRepeating(nameof(BurnFat), 5.0f, 5.0f);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        ManageUITiredness(tired, tired);
    }

    public void BurnFat()
    {
        if (player.state == "MOVING")
        {
            switch (player.playerMove.states.Contains("RUN"))
            {
                case true:
                    player.playerCharacterCreation.fat -= 0.025f;
                    break;

                case false:
                    player.playerCharacterCreation.fat -= 0.01f;
                    break;
            }
        }
    }


    public void ManageTiredness()
    {
        if (player.health.current > 0 && tired > 0 && player.playerAdditionalState.additionalState != "SLEEP" && !player.playerAccessoryInteraction.whereActionIsGoing) tired--;

        if (player.playerAccessoryInteraction.whereActionIsGoing)
        {
            BuildingAccessory acc = player.playerAccessoryInteraction.whereActionIsGoing.gameObject.GetComponent<BuildingAccessory>();
            if (acc.craftingAccessoryItem.name.ToUpper() == "BED")
            {
                tired += 7;
                player.health.current += Convert.ToInt32((player.health.baseHealth.Get(player.level.current) / 100) * 10);
                player.mana.current += Convert.ToInt32((player.mana.baseMana.Get(player.level.current) / 100) * 10);
            }
            else if (acc.craftingAccessoryItem.name.ToUpper().Contains("CHAIR") || acc.craftingAccessoryItem.name.ToUpper().Contains("SOFA"))
            {
                tired += 3;
                player.health.current += Convert.ToInt32((player.health.baseHealth.Get(player.level.current) / 100) * 3);
                player.mana.current += Convert.ToInt32((player.mana.baseMana.Get(player.level.current) / 100) * 3);
            }
        }
        else
        {
            if (player.playerAdditionalState.additionalState == "SLEEP")
            {
                tired++;
            }
        }

        if (tired == 0 && player.playerAdditionalState.additionalState != "SLEEP" && player.health.current > 0 )
        {
            if (player.playerAccessoryInteraction.whereActionIsGoing)
            {
                player.playerAccessoryInteraction.RemoveInteraction();
            }
            player.playerAdditionalState.additionalState = "SLEEP";
            return;
        }

    }

    public void ManageUITiredness(int oldValue, int newValue)
    {
        UIPlayerInformation.singleton.Tired();
    }


}
