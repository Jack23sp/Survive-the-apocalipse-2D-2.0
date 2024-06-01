using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerAlliance playerAlliance;
}

public partial class Database
{
    class guildAlly
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string guildName { get; set; }
        public string ally { get; set; }
    }

    public void Connect_Alliance()
    {
        connection.CreateTable<guildAlly>();
    }

    public void SaveGuildAlly(Player player)
    {
        PlayerAlliance alliance = player.GetComponent<PlayerAlliance>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        if (player.guild.name != string.Empty) connection.Execute("DELETE FROM guildAlly WHERE guildName=?", player.guild.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        if (player.guild.InGuild() && player.guild.guild.master == player.name)
        {
            for (int i = 0; i < alliance.guildAlly.Count; i++)
            {
                connection.InsertOrReplace(new guildAlly
                {
                    guildName = player.guild.guild.name,
                    ally = alliance.guildAlly[i]
                });
            }
        }
    }

    public void DeleteGuildAlly(string guildName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM guildAlly WHERE guildName=?", guildName);
    }

    public void DeleteGuildContaindAlly(string guildName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM guildAlly WHERE ally=? ", guildName);
    }
    public void LoadGuilAlly(Player player)
    {
        PlayerAlliance alliance = player.GetComponent<PlayerAlliance>();

        foreach (guildAlly row in connection.Query<guildAlly>("SELECT * FROM guildAlly WHERE guildName=?", player.name))
        {
            alliance.guildAlly.Add(row.ally);
        }
    }

    public void LoadGuildOnDemandMenu(string guildName)
    {
        if (guildName != null)
        {
            // load guild on demand when the first player of that guild logs in
            // (= if it's not in GuildSystem.guilds yet)
            if (!GuildSystem.guilds.ContainsKey(guildName))
            {
                Guild guild = LoadGuild(guildName);
                GuildSystem.guilds[guild.name] = guild;
            }
        }
    }
}

public class PlayerAlliance : NetworkBehaviour
{
    private Player player;
    [HideInInspector] public ScriptableAbility abilityToLead;
    [SyncVar(hook = (nameof(CheckInviter)))]
    public string guildAllyInviteName;
    [SyncVar]
    public string guildAllyInviteGuildName;
    public double guildInviteWaitSeconds;

    public SyncList<string> guildAlly = new SyncList<string>();
    private BoxCollider2D checkCollider;
    private List<Collider2D> basement = new List<Collider2D>();

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

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        guildAlly.Callback += OnAllyChangedOnLocalPlayer;
    }

    void OnAllyChangedOnLocalPlayer(SyncList<string>.Operation op, int index, string oldGuildAlly, string newGuildAlly)
    {
        if (UIAbilities.singleton) UIAbilities.singleton.RefreshAbilities(true);

        checkCollider = (BoxCollider2D)player.transform.GetComponentInChildren<PlayerGrassDetector>().thisCollider;
        basement = Physics2D.OverlapBoxAll(checkCollider.bounds.center, checkCollider.bounds.size, 0, ModularBuildingManager.singleton.basementLayerMask).ToList();

        for (int i = 0; i < basement.Count; i++)
        {
            if (basement[i].GetComponent<ModularBuilding>())
                basement[i].GetComponent<ModularBuilding>().RefreshWallOptions();
        }
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerAlliance = this;
        abilityToLead = AbilityManager.singleton.allianceAbility;
    }

    public void CheckInviter(string oldInviter, string newInviter)
    {

    }

    [Command]
    public void CmdLoadGuild(string guildName)
    {
        Guild selectedGuild;
        if (!GuildSystem.guilds.ContainsKey(guildName))
        {
            Guild guild = Database.singleton.LoadGuild(guildName);
            GuildSystem.guilds[guild.name] = guild;
            selectedGuild = guild;
        }
        else
            selectedGuild = GuildSystem.guilds[guildName];

        TargetGuild(selectedGuild);

    }

    [TargetRpc] // only send to one client
    public void TargetGuild(Guild guild)
    {
        if (guild.name != string.Empty || guild.name != null)
        {
            UIGuildCustom.singleton.Open(guild);
        }
    }


    public bool CanInviteToAlliance(Player target)
    {
        return target.guild.InGuild() &&
               player.guild.InGuild() &&
               MaxAllianceAmount() > guildAlly.Count &&
               MaxTargetAllianceAmount(target) > target.playerAlliance.guildAlly.Count &&
               !guildAlly.Contains(target.guild.name) &&
               !target.playerAlliance.guildAlly.Contains(player.guild.name) &&
               player.guild.guild.CanTerminate(name) &&
               target.guild.guild.CanTerminate(target.name);
    }

    public int MaxAllianceAmount()
    {
        return Convert.ToInt32(AbilityManager.singleton.FindNetworkAbilityLevel(abilityToLead.name, player.name) / 10);
    }

    public int MaxTargetAllianceAmount(Player target)
    {
        return Convert.ToInt32(AbilityManager.singleton.FindNetworkAbilityLevel(target.playerAlliance.abilityToLead.name, target.name) / 10);
    }


    public void SetInviteToAlliance(NetworkIdentity identity)
    {
        Player sender = identity.GetComponent<Player>();

        if (sender && sender.playerAlliance.CanInviteToAlliance(player) && !player.playerOptions.blockAlly)
        {
            guildAllyInviteName = sender.name;
            guildAllyInviteGuildName = sender.guild.name;
        }
    }

    [Command]
    public void CmdAcceptInviteToAlliance()
    {
        if (!string.IsNullOrEmpty(guildAllyInviteName))
        {
            if (Player.onlinePlayers.TryGetValue(guildAllyInviteName, out Player sender))
            {
                if (sender.playerAlliance.CanInviteToAlliance(player))
                {
                    for (int i = 0; i < player.guild.guild.members.Length; i++)
                    {
                        if (Player.onlinePlayers.TryGetValue(player.guild.guild.members[i].name, out Player member))
                        {
                            if (!member.playerAlliance.guildAlly.Contains(sender.guild.guild.name))
                            {
                                member.playerAlliance.guildAlly.Add(sender.guild.guild.name);
                            }
                        }
                    }
                    for (int i = 0; i < sender.guild.guild.members.Length; i++)
                    {
                        if (Player.onlinePlayers.TryGetValue(sender.guild.guild.members[i].name, out Player member))
                        {
                            if (!member.playerAlliance.guildAlly.Contains(player.guild.guild.name))
                            {
                                member.playerAlliance.guildAlly.Add(player.guild.guild.name);
                            }
                        }
                    }
                }
            }
        }
        guildAllyInviteGuildName = string.Empty;
        guildAllyInviteName = string.Empty;
    }

    [Command]
    public void CmdDeclineInviteToAlliance()
    {
        DeclineInvite();
    }

    public void DeclineInvite()
    {
        guildAllyInviteGuildName = string.Empty;
        guildAllyInviteName = string.Empty;
    }


    [Command]
    public void CmdRemoveAllyGuild(string guildToSearch, string guildToRemove)
    {
        GuildSystem.TerminateGuildAlly(guildToSearch, guildToRemove);
        GuildSystem.TerminateGuildAlly(guildToRemove, guildToSearch);

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + player.playerAlliance.guildInviteWaitSeconds;
    }

    [Command]
    public void CmdElimininateGuildAlltFromDatabase(string myguild, string guildAlly)
    {
        Database.singleton.RemoveGuildAlly(myguild, guildAlly);

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + player.playerAlliance.guildInviteWaitSeconds;
    }
}
