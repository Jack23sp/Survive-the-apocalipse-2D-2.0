using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Reflection;

public partial class Player
{
    [HideInInspector] public PlayerPick playerPick;

}


public class PlayerPick : NetworkBehaviour
{
    private Player player;
    public Material notTargetMaterial;
    public Material targetMaterial;
    private Ability ab;

    public void Awake()
    {
        player = GetComponent<Player>();
        player.playerPick = this;
    }

    [Command]
    public void CmdAddFlower(NetworkIdentity identity)
    {
        if (player.health.current > 0 && identity.GetComponent<Flower>())
        {
            Flower flower = identity.GetComponent<Flower>();

            if (flower)
            {
                float abilityLevel = AbilityManager.singleton.FindNetworkAbilityLevel("Survivor", player.name);
                int rand = UnityEngine.Random.Range(1, abilityLevel < 10 ? 1 : Convert.ToInt32(abilityLevel / 10));
                if (player.inventory.CanAddItem(new Item(flower.itemToAdd), rand))
                {
                    player.inventory.AddItem(new Item(flower.itemToAdd), rand);
                    int abIndex = AbilityManager.singleton.FindNetworkAbility("Survivor", name);
                    int max = AbilityManager.singleton.FindNetworkAbilityMaxLevel("Survivor", name);
                    ab = player.playerAbility.networkAbilities[abIndex];

                    float next = Mathf.Min(ab.level + AbilityManager.singleton.increaseAbilityOnAction, max);
                    if (next > max) next = max;
                    float attrNext = (float)Math.Round(next, 2);
                    ab.level = attrNext;

                    player.playerAbility.networkAbilities[abIndex] = ab;
                    TargetRpcShowNotification(flower.itemToAdd.name, -1, "Added " + rand + " " + flower.itemToAdd.name + " to inventory");
                    player.quests.SyncPickOnServer(new DetailOfQuest(flower.itemToAdd.name.Replace("(Clone)", ""), rand));
                    player.playerPoints.flowerPick++;
                    NetworkServer.Destroy(flower.gameObject);
                }
            }
        }
    }

    [TargetRpc]
    public void TargetRpcShowNotification(string itemName, int skinIndex, string description)
    {
        if (ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out ScriptableItem itemData))
        {
            if (player.isLocalPlayer)
            {
                NotificationManager.singleton.ShowNotification(GetComponent<NetworkIdentity>(), itemData.name, skinIndex, description);
            }
        }
    }

}
