using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Player
{
    [HideInInspector] public PlayerFriendResurrect friendResurrect;
}

public class PlayerFriendResurrect : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = (nameof(ManageResurrectRequest)))]
    public NetworkIdentity friendActResurrect;


    void Awake()
    {
        player = GetComponent<Player>();
        player.friendResurrect = this;
    }

    public void ManageResurrectRequest(NetworkIdentity oldNetworkIdentity, NetworkIdentity newNetworkIdentity)
    {
        // Active group invite
    }
}
