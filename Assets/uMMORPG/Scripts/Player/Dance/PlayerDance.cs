using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerDance playerDance;
}

public partial class EquipmentItem
{
    public RuntimeAnimatorController animatorToSet;
}

public partial class Database
{
    class dance
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string danceName { get; set; }
    }

    public void Connect_Dance()
    {
        connection.CreateTable<dance>();
    }

    public void SaveDance(Player player)
    {
        PlayerDance playerDance = player.GetComponent<PlayerDance>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM dance WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < playerDance.networkDance.Count; i++)
        {
            int index = i;
            connection.InsertOrReplace(new dance
            {
                characterName = player.name,
                danceName = player.playerDance.networkDance[index]
            }); ;
        }
    }
    public void LoadDance(Player player)
    {
        PlayerDance playerDance = player.GetComponent<PlayerDance>();

        foreach (dance row in connection.Query<dance>("SELECT * FROM dance WHERE characterName=?", player.name))
        {
            playerDance.networkDance.Add(row.danceName);
        }
    }

}

public class PlayerDance : NetworkBehaviour
{
    private Player player;
    public SyncList<string> networkDance = new SyncList<string>();
    [SyncVar(hook = nameof(ManageDance))]
    public int danceIndex;
    public int prevDanceIndex;
    public Animator animator;
    public RuntimeAnimatorController prevAnimator;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerDance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        networkDance.Callback += OnDanceListUpdated;
    }

    void OnDanceListUpdated(SyncList<string>.Operation op, int index, string oldItem, string newItem)
    {
        //if (player.isLocalPlayer)
        //{
        //    UIEmoji.singleton.ManageOpenPanelDance();
        //}
    }


    public void ManageDance(int oldInt, int newInt)
    {
        if (newInt > -1 && animator.runtimeAnimatorController != DanceManager.singleton.listCompleteOfDance[newInt])
        {
            //if (!playerPlaceholderWeapon) playerPlaceholderWeapon = Player.localPlayer.playerMove.bodyPlayer.GetComponent<PlayerPlaceholderWeapon>();

            //if (player.playerItemEquipment.weapon) player.playerItemEquipment.weapon.SetActive(false);

            if (prevAnimator == null) prevAnimator = animator.runtimeAnimatorController;
                if (player.animator.runtimeAnimatorController != DanceManager.singleton.listCompleteOfDance[newInt].animator)
                    player.animator.runtimeAnimatorController = DanceManager.singleton.listCompleteOfDance[newInt].animator;
        }
        else if (newInt == -1)
        {
            //if (player.playerItemEquipment.weapon) player.playerItemEquipment.weapon.SetActive(true);

            if (player.equipment.slots[0].amount > 0)
            {
                player.animator.runtimeAnimatorController = ((EquipmentItem)player.equipment.slots[0].item.data).animatorToSet;
            }
            else
            {
                player.animator.runtimeAnimatorController = DanceManager.singleton.defaultAnimatorController;
            }
        }
    }

    public void ResetAnimation()
    {
        danceIndex = -1;
    }

    [Command]
    public void CmdResetAnimation()
    {
        danceIndex = -1;
    }

    [Command]
    public void CmdAddDance(string danceName, int currencyType)
    {
        ScriptableDance dance = null;
        for (int i = 0; i < DanceManager.singleton.listCompleteOfDance.Count; i++)
        {
            if (DanceManager.singleton.listCompleteOfDance[i].name == danceName)
            {
                dance = DanceManager.singleton.listCompleteOfDance[i];
            }
        }
        if (currencyType == 0)
        {
            if (player.itemMall.coins >= dance.coinToBuy)
            {
                player.playerDance.networkDance.Add(danceName);
                player.itemMall.coins -= dance.coinToBuy;
            }
        }
        if (currencyType == 1)
        {
            if (player.gold >= dance.goldToBuy)
            {
                player.playerDance.networkDance.Add(danceName);
                player.gold -= dance.goldToBuy;
            }
        }
    }

    [Command]
    public void CmdSpawnDance(string danceName, string playerName, int index)
    {
        if (DanceManager.singleton.FindNetworkDance(danceName, playerName) >= 0)
        {
            player.playerDance.danceIndex = index;
        }
    }

}
