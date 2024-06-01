using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerTeleport playerTeleport;
}

public class PlayerTeleport : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = nameof(ItemInUse))]
    public int itemInUse = -1;
    [SyncVar(hook = nameof(Inviter))]
    public string inviterName;
    [SyncVar]
    public int countdown;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerTeleport = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        InvokeRepeating(nameof(DecreaseCountdown), 1.0f, 1.0f);
    }

    public void Inviter(string oldString, string newString)
    {
        if (player.isLocalPlayer)
        {
            if (newString != string.Empty)
            {
                if (GameObjectSpawnManager.singleton.spawnedTeleport == null)
                {
                    GameObjectSpawnManager.singleton.spawnedTeleport = Instantiate(GameObjectSpawnManager.singleton.teleportInviteSlot, GameObjectSpawnManager.singleton.canvas);

                }
            }
        }
    }

    public void ItemInUse(int oldInt, int newInt)
    {
        if (player.isLocalPlayer)
        {
            if (newInt > -1)
            {
                if (GameObjectSpawnManager.singleton.spawnedteleportInviter == null)
                {
                    GameObjectSpawnManager.singleton.spawnedteleportInviter = Instantiate(GameObjectSpawnManager.singleton.teleportInviter, GameObjectSpawnManager.singleton.canvas);
                }
            }
            if (newInt == -1)
            {
                if (GameObjectSpawnManager.singleton.spawnedteleportInviter != null)
                {
                    Destroy(GameObjectSpawnManager.singleton.spawnedteleportInviter);
                }
            }
        }
    }

    public void DecreaseCountdown()
    {
        if (player.playerTeleport.countdown > 0)
        {
            player.playerTeleport.countdown--;
        }
        if (player.playerTeleport.countdown == 0)
        {
            inviterName = string.Empty;
        }
    }

    [Command]
    public void CmdTeleportToFriends()
    {
        Player.onlinePlayers.TryGetValue(inviterName, out Player inviter);
        if (inviter)
        {
            player.movement.Warp(inviter.transform.position);
            inviterName = string.Empty;
            countdown = 0;
        }
    }

    [Command]
    public void CmdTeleportDecline()
    {
        inviterName = string.Empty;
        countdown = 0;
    }

    [Command]
    public void CmdSendTeleportInvite(string playerName)
    {
        Player.onlinePlayers.TryGetValue(playerName, out Player inviter);
        ItemSlot slot = player.inventory.slots[itemInUse];
        if (inviter && inviter.playerTeleport.itemInUse == -1 && slot.item.data is TeleportItem)
        {
            inviter.playerTeleport.inviterName = name;
            inviter.playerTeleport.countdown = CoroutineManager.singleton.teleportSeconds;
            slot.amount--;
            player.inventory.slots[itemInUse] = slot;
        }
        itemInUse = -1;
    }

    [Command]
    public void CmdRemoveTeleport()
    {
        itemInUse = -1;
    }

}
