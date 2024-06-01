using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerPartner playerPartner;
}

public partial class Database
{
    class partner
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string partnerName { get; set; }
    }

    public void Connect_Partner()
    {
        connection.CreateTable<partner>();
    }

    public void SavePartner(Player player)
    {
        PlayerPartner partner = player.GetComponent<PlayerPartner>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM partner WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new partner
        {
            characterName = player.name,
            partnerName = partner.partnerName
        });

    }
    public void LoadPartner(Player player)
    {
        PlayerPartner partner = player.GetComponent<PlayerPartner>();

        foreach (partner row in connection.Query<partner>("SELECT * FROM partner WHERE characterName=?", player.name))
        {
            partner.partnerName = row.partnerName;
        }

    }

    public void DeletePartner(string partnerName)
    {
        connection.Execute("DELETE FROM partner WHERE characterName=?", partnerName);

    }
}

public class PlayerPartner : NetworkBehaviour
{
    public Player player;

    [SyncVar]
    public string inviter;
    [SyncVar(hook = (nameof(SetHearth)))]
    public string partnerName;

    [Header("Partner")]
    [SyncVar]
    public Player _partner;

    public GameObject hearthRender;

    public int defaultHealth;
    public float defaultDefense;
    public int defaultMana;

    private void Assign()
    {
        player = GetComponent<Player>();
        player.playerPartner = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        SetHearth(partnerName, partnerName);
    }

    public void SetHearth(string oldPartner, string newPartner)
    {
        if (newPartner == string.Empty)
        {
            hearthRender.SetActive(false);
            if (oldPartner != null)
            {
                if (_partner) _partner.playerPartner.hearthRender.SetActive(false);
            }
        }
        else
        {
            hearthRender.SetActive(true);
            if (_partner) _partner.playerPartner.hearthRender.SetActive(true);
        }
        if (UIStats.singleton) StatsManager.singleton.ManageStatSlot(UIStats.singleton.content.GetChild(StatsManager.singleton.FindIndexOfPartner()).GetComponent<UIStatsSlot>());
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        if (partnerName != string.Empty)
        {
            if (_partner == null)
            {
                if (Player.onlinePlayers.ContainsKey(partnerName))
                {
                    _partner = Player.onlinePlayers[partnerName];
                    Player.onlinePlayers[partnerName].playerPartner._partner = player;
                }
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (partnerName != string.Empty)
        {
            if (_partner != null)
            {
                if (Player.onlinePlayers.ContainsKey(partnerName))
                {
                    Player.onlinePlayers[partnerName].playerPartner._partner = null;
                }
            }
        }
    }

    public void InvitePartner(NetworkIdentity identity)
    {
        Player sender = identity.GetComponent<Player>();
        if (sender is Player && sender.playerPartner.partnerName == string.Empty && !player.playerOptions.blockMarriage)
        {
            player.playerPartner.inviter = sender.name;
        }
    }

    [Command]
    public void CmdAcceptInvitePartner()
    {
        Player onlinePlayer;
        Player _Ppartner;
        if (Player.onlinePlayers.TryGetValue(inviter, out onlinePlayer))
        {
            if (onlinePlayer && onlinePlayer.playerPartner.partnerName == string.Empty && player.playerPartner.partnerName == string.Empty)
            {
                onlinePlayer.playerPartner.partnerName = name;
                partnerName = onlinePlayer.name;
                if (Player.onlinePlayers.ContainsKey(inviter))
                {
                    _Ppartner = Player.onlinePlayers[inviter];
                    _Ppartner.playerPartner._partner = player.gameObject.GetComponent<Player>();
                }
                player.playerPartner._partner = onlinePlayer.gameObject.GetComponent<Player>();
            }
        }

        player.playerPartner.inviter = string.Empty;
    }

    [Command]
    public void DeclineInvite()
    {
        player.playerPartner.inviter = string.Empty;
    }

    [Command]
    public void CmdRemovePartner()
    {
        Player myPartner;
        if (Player.onlinePlayers.TryGetValue(player.playerPartner.partnerName, out myPartner))
        {
            Debug.Log($"Before - partnerName: {myPartner.playerPartner.partnerName}, _partner: {myPartner.playerPartner._partner}");

            myPartner.playerPartner.partnerName = string.Empty;
            myPartner.playerPartner._partner = null;

            Debug.Log($"After - partnerName: {myPartner.playerPartner.partnerName}, _partner: {myPartner.playerPartner._partner}");
        }
        else
        {
            Database.singleton.DeletePartner(player.playerPartner.partnerName);
        }
        partnerName = string.Empty;
        _partner = null;
    }
}
