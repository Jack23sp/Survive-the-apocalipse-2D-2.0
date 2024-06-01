using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerPoisoned playerPoisoned;
}

public partial class Database
{
    class poisoning
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentPoisoning { get; set; }
    }

    public void Connect_Poisoning()
    {
        connection.CreateTable<poisoning>();
    }

    public void SavePoisoning(Player player)
    {
        PlayerPoisoned poisoning = player.GetComponent<PlayerPoisoned>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM poisoning WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new poisoning
        {
            characterName = player.name,
            currentPoisoning = poisoning.current
        });

    }
    public void LoadPoisoning(Player player)
    {
        PlayerPoisoned poisoning = player.GetComponent<PlayerPoisoned>();

        foreach (poisoning row in connection.Query<poisoning>("SELECT * FROM poisoning WHERE characterName=?", player.name))
        {
            poisoning.current = row.currentPoisoning;
        }

    }

}

public class PlayerPoisoned : NetworkBehaviour
{
    private Player player;
    [SyncVar]
    public int current;
    [HideInInspector] public int max = 100;
    [HideInInspector] public float cycleAmount = 60.0f;

    public int healthToRemove = 5;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerPoisoned = this;
        max = 100;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        cycleAmount = CoroutineManager.singleton.poisoningInvoke;
        InvokeRepeating(nameof(DecreasePoisoning), cycleAmount, cycleAmount);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public void DecreasePoisoning()
    {
        if (player.playerPoisoned.current >= 100)
        {
            player.playerPoisoned.current = 100;
            player.health.current -= player.playerPoisoned.healthToRemove;
        }
    }

}
