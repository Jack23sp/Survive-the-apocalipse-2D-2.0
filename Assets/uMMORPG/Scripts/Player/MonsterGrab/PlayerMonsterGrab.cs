using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerMonsterGrab playerMonsterGrab;
}

public partial class WeaponItem
{
    public float distanceToGrab;
}

public class PlayerMonsterGrab : NetworkBehaviour
{
    private Player player;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerMonsterGrab = this;
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
    }
}
