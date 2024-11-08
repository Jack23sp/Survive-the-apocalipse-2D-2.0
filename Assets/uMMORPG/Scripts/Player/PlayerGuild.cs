﻿using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayerChat))]
[DisallowMultipleComponent]
public partial class PlayerGuild : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerChat chat;

    [Header("Text Meshes")]
    public TextMesh overlay;
    public string overlayPrefix = "[";
    public string overlaySuffix = "]";

    // .guild is a copy for easier reading/syncing. Use GuildSystem to manage
    // guilds!
    [Header("Guild")]
    [SyncVar (hook =(nameof(CheckInviter)))] public string inviteFrom = "";
    [SyncVar(hook = nameof(ManageGuild)), HideInInspector] public Guild guild; // TODO SyncToOwner later but need to sync guild name to everyone!
    public float inviteWaitSeconds = 3;
    private BoxCollider2D checkCollider;
    private List<Collider2D> basement = new List<Collider2D>();

    public void ManageGuild(Guild oldGuild, Guild newGuild)
    {
        if (!player.playerCallback) 
            player.playerCallback = player.GetComponent<PlayerCallback>();
        player.playerCallback.GroupChanged(oldGuild, newGuild);

        if (Player.localPlayer && Player.localPlayer.name == player.name)
        {
            checkCollider = (BoxCollider2D)player.transform.GetComponentInChildren<PlayerGrassDetector>().thisCollider;
            basement = Physics2D.OverlapBoxAll(checkCollider.bounds.center, checkCollider.bounds.size, 0, ModularBuildingManager.singleton.basementLayerMask).ToList();

            for(int i = 0; i < basement.Count; i++)
            {
                if (basement[i].GetComponent<ModularBuilding>())
                    basement[i].GetComponent<ModularBuilding>().RefreshWallOptions();
            }
        }


        if (overlay != null)
            overlay.text = !string.IsNullOrWhiteSpace(guild.name) ? overlayPrefix + guild.name + overlaySuffix : "";
    }

    public void CheckInviter(string oldInviter, string newInviter)
    {
        if (newInviter != string.Empty)
        {
            // Active group invite
        }
    }

    void Start()
    {
        if (!isServer && !isClient) return;

        if (isServer)
            SetOnline(true);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        ManageGuild(guild, guild);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (overlay != null)
            overlay.text = !string.IsNullOrWhiteSpace(guild.name) ? overlayPrefix + guild.name + overlaySuffix : "";
    }

    void OnDestroy()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        // notify guild members that we are offline
        if (isServer)
            SetOnline(false);
    }

    // guild ///////////////////////////////////////////////////////////////////
    public bool InGuild()
    {
        return !string.IsNullOrWhiteSpace(guild.name);
    }

    // ServerCALLBACk to ignore the warning if it's called while server isn't
    // active, which happens if OnDestroy->SetOnline(false) is called while
    // shutting down.
    [ServerCallback]
    public void SetOnline(bool online)
    {
        // validate
        if (InGuild())
            GuildSystem.SetGuildOnline(guild.name, name, online);
    }

    [Command]
    public void CmdInviteTarget()
    {
        // validate
        if (player.target != null &&
            player.target is Player targetPlayer &&
            InGuild() && !targetPlayer.guild.InGuild() &&
            guild.CanInvite(name, targetPlayer.name) &&
            NetworkTime.time >= player.nextRiskyActionTime &&
            Utils.ClosestDistance(player, targetPlayer) <= player.interactionRange)
        {
            // send an invite
            targetPlayer.guild.inviteFrom = name;
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
    }

    [Command]
    public void CmdInviteAccept()
    {
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!InGuild() && inviteFrom != "" &&
            Player.onlinePlayers.TryGetValue(inviteFrom, out Player sender) &&
            sender.guild.InGuild())
        {
            // try to add. GuildSystem does all the checks.
            GuildSystem.AddToGuild(sender.guild.guild.name, sender.name, name, player.level.current);
        }

        // reset guild invite in any case
        inviteFrom = "";
    }

    [Command]
    public void CmdInviteDecline()
    {
        DeclineInvite();
    }

    public void DeclineInvite()
    {
        inviteFrom = "";
    }

    [Command]
    public void CmdKick(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.KickFromGuild(guild.name, name, memberName);
    }

    [Command]
    public void CmdPromote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.PromoteMember(guild.name, name, memberName);
    }

    [Command]
    public void CmdDemote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.DemoteMember(guild.name, name, memberName);
    }

    [Command]
    public void CmdSetNotice(string notice)
    {
        // validate
        // (only allow changes every few seconds to avoid bandwidth issues)
        if (InGuild() && NetworkTime.time >= player.nextRiskyActionTime)
        {
            // try to set notice
            GuildSystem.SetGuildNotice(guild.name, name, notice);
        }

        // reset risky time no matter what. even if set notice failed, we don't
        // want people to spam attempts all the time.
        player.nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
    }

    // helper function to check if we are near a guild manager npc
    public bool IsGuildManagerNear()
    {
        return player.target != null &&
               //player.target is Npc npc &&
               //npc.guildManagement != null && // only if Npc offers guild management
               Utils.ClosestDistance(player, player.target) <= player.interactionRange;
    }

    [Command]
    public void CmdTerminate()
    {
        // validate
        if (InGuild())
            GuildSystem.TerminateGuild(guild.name, name);
    }

    [Command]
    public void CmdCreate(string guildName)
    {
        // validate
        if (player.health.current > 0 && player.gold >= GuildSystem.CreationPrice &&
            !InGuild() && IsGuildManagerNear())
        {
            // try to create the guild. pay for it if it worked.
            if (GuildSystem.CreateGuild(name, player.level.current, guildName))
                player.gold -= GuildSystem.CreationPrice;
            else
                chat.TargetMsgInfo("Guild name invalid!");
        }
    }

    [Command]
    public void CmdLeave()
    {
        // validate
        if (InGuild())
            GuildSystem.LeaveGuild(guild.name, name);
    }
}
