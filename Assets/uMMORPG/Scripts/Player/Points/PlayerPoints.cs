using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerPoints playerPoints;
}

public partial class Database
{
    class leaderPoint
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int animalKill { get; set; }
        public int playerKill { get; set; }
        public int monsterKill { get; set; }
        public int craftPoint { get; set; }
        public int flowerPick { get; set; }
        public int basementPlacement { get; set; }
        public int wallsPlacement { get; set; }
        public int woodPick { get; set; }
        public int stonePick { get; set; }
        public int accessoriesPlacement { get; set; }
        public int barrellsPick { get; set; }
        public int boxesPick { get; set; }

    }

    public void Connected_Points()
    {
        connection.CreateTable<leaderPoint>();
    }

    public void SaveLeaderpoint(Player player)
    {
        PlayerPoints leaderPoints = player.GetComponent<PlayerPoints>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM leaderPoint WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new leaderPoint
        {
            characterName = player.name,
            animalKill = leaderPoints.animalKill,
            playerKill = leaderPoints.playerKill,
            monsterKill = leaderPoints.monsterKill,
            craftPoint = leaderPoints.craftPoint,
            flowerPick = leaderPoints.flowerPick,
            basementPlacement = leaderPoints.basementPlacement,
            wallsPlacement = leaderPoints.wallsPlacement,
            woodPick = leaderPoints.woodPick,
            stonePick = leaderPoints.stonePick,
            accessoriesPlacement = leaderPoints.accessoriesPlacement,
            barrellsPick = leaderPoints.barrellsPick,
            boxesPick = leaderPoints.boxesPick
        });

    }
    public void LoadLeaderpoint(Player player)
    {
        PlayerPoints leaderPoints = player.GetComponent<PlayerPoints>();

        foreach (leaderPoint row in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", player.name))
        {
            leaderPoints.animalKill = row.animalKill;
            leaderPoints.playerKill = row.playerKill;
            leaderPoints.monsterKill = row.monsterKill;
            leaderPoints.craftPoint = row.craftPoint;
            leaderPoints.flowerPick = row.flowerPick;
            leaderPoints.basementPlacement = row.basementPlacement;
            leaderPoints.wallsPlacement = row.wallsPlacement;
            leaderPoints.woodPick = row.woodPick;
            leaderPoints.stonePick = row.stonePick;
            leaderPoints.accessoriesPlacement = row.accessoriesPlacement;
            leaderPoints.barrellsPick = row.barrellsPick;
            leaderPoints.boxesPick = row.boxesPick;
        }

    }

}

public class PlayerPoints : NetworkBehaviour
{
    private Player player;

    [SyncVar]
    public int animalKill;
    [SyncVar]
    public int playerKill;
    [SyncVar]
    public int monsterKill;
    [SyncVar]
    public int craftPoint;
    [SyncVar]
    public int flowerPick;
    [SyncVar]
    public int basementPlacement;
    [SyncVar]
    public int wallsPlacement;
    [SyncVar]
    public int woodPick;
    [SyncVar]
    public int stonePick;
    [SyncVar]
    public int accessoriesPlacement;
    [SyncVar]
    public int barrellsPick;
    [SyncVar]
    public int boxesPick;


    void Assign()
    {
        player = GetComponent<Player>();
        player.playerPoints = this;
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
