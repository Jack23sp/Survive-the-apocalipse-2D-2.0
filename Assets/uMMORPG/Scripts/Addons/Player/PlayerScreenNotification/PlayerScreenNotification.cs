using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEngine;
using Mirror;
using System;

public partial class Player
{
    [HideInInspector] public PlayerScreenNotification playerScreenNotification;
}


public class PlayerScreenNotification : NetworkBehaviour
{
    private Player player;
    public readonly SyncList<InviteRequest> invitation = new SyncList<InviteRequest>();
    [SyncVar(hook = (nameof(StartSetupOnInvite)))] public InviteRequest actualInviteRequest = new InviteRequest();

    void Awake()
    {
        player = GetComponent<Player>();
        player.playerScreenNotification = this;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        invitation.Callback += OnInvitationChanged;
    }

    void OnInvitationChanged(SyncList<InviteRequest>.Operation op, int index, InviteRequest oldSlot, InviteRequest newSlot)
    {
        if (invitation.Count == 0)
        {
            ActivityInviteSlot.singleton.ResetPanel();
        }
    }

    public void StartSetupOnInvite(InviteRequest oldRequest, InviteRequest newInviteRequest)
    {
        if(actualInviteRequest.title == string.Empty || actualInviteRequest.title == null)
        {
            ActivityInviteSlot.singleton.panel.SetActive(false);
        }
        else
        {
            if (player.name == actualInviteRequest.target && player.isLocalPlayer)
            {
                if (actualInviteRequest.title != null)
                {
                    ActivityInviteSlot.singleton.panel.SetActive(true);
                    ActivityInviteSlot.singleton.Setup(actualInviteRequest.title, actualInviteRequest.description, actualInviteRequest.hasTimer, actualInviteRequest.type, actualInviteRequest.sender, actualInviteRequest.target, false);
                }
                else
                {
                    ActivityInviteSlot.singleton.panel.SetActive(false);
                }
            }
        }
    }

    public bool FindInvitationByType(int type)
    {
        for (int i = 0; i < invitation.Count; i++)
        {
            if (invitation[i].type == type) return true;
        }
        return false;
    }

    [Command]
    public void CmdAddNotification(NetworkIdentity target, InviteRequest inviteRequest)
    {
        Player plTarget = target.GetComponent<Player>();
        if (plTarget)
        {
            if (plTarget.playerScreenNotification)
            {
                if (inviteRequest.type == 5)
                {
                    if (plTarget.playerFriends.friends.Contains(player.name) || plTarget.playerFriends.request.Contains(player.name))
                    {
                        TargetRpcMessageInvitation(player.netIdentity, "You already sent this type of request to this player!");
                        return;
                    }
                    for (int i = 0; i < plTarget.playerScreenNotification.invitation.Count; i++)
                    {
                        if (plTarget.playerScreenNotification.invitation[i].type == 5 && plTarget.playerScreenNotification.invitation[i].sender == player.name)
                        {
                            TargetRpcMessageInvitation(player.netIdentity, "You already sent this type of request to this player!");
                            return;
                        }
                    }
                    TargetRpcMessageInvitation(player.netIdentity, "Invitation send!");
                    plTarget.playerScreenNotification.invitation.Add(inviteRequest);
                }
                else if (inviteRequest.type == 2)
                {
                    if (plTarget.playerAlliance.guildAlly.Contains(player.guild.guild.name))
                    {
                        TargetRpcMessageInvitation(player.netIdentity, "You are already ally with this group!");
                        return;
                    }
                    for (int i = 0; i < plTarget.playerScreenNotification.invitation.Count; i++)
                    {
                        if (plTarget.playerScreenNotification.invitation[i].type == 2 && plTarget.playerScreenNotification.invitation[i].sender == player.name)
                        {
                            TargetRpcMessageInvitation(player.netIdentity, "You already sent this type of request to this player!");
                            return;
                        }
                    }
                    TargetRpcMessageInvitation(player.netIdentity, "Invitation send!");
                    plTarget.playerScreenNotification.invitation.Add(inviteRequest);
                }
                else
                {
                    if (!plTarget.playerScreenNotification.FindInvitationByType(inviteRequest.type))
                    {
                        plTarget.playerScreenNotification.invitation.Add(inviteRequest);
                        TargetRpcMessageInvitation(player.netIdentity, "Invitation send!");
                    }
                    else
                    {
                        TargetRpcMessageInvitation(player.netIdentity, "Invitation not send! Player has already a request of this type from someone else");
                    }
                }
            }
        }
    }

    [Command]
    public void CmdSpawnNotification(NetworkIdentity connection, string message)
    {
        TargetRpcMessageInvitation(connection, message);
    }

    [TargetRpc]
    public void TargetRpcMessageInvitation(NetworkIdentity identity, string message)
    {
        Player target = identity.GetComponent<Player>();
        if (target.isLocalPlayer)
            target.playerNotification.SpawnNotification(ImageManager.singleton.refuse, message);
    }

    [Command]
    public void CmdAcceptAndRemoveFirstInvite()
    {
        InviteRequest req = actualInviteRequest;
        if(actualInviteRequest.title != null && actualInviteRequest.title != string.Empty)
        {
            SetAndResetOtherRequest(actualInviteRequest);
        }
        req = new InviteRequest();
        if (invitation.Count > 0)
        {
            invitation.Remove(invitation[0]);
            if (invitation.Count > 0) 
                req = invitation[0];
            actualInviteRequest = req;
        }
    }

    [Command]
    public void CmdRemoveCompleteFirstInvite(int type)
    {
        Player target = null;
        Player sender = null;
        Player.onlinePlayers.TryGetValue(player.playerScreenNotification.actualInviteRequest.target, out target);
        Player.onlinePlayers.TryGetValue(player.playerScreenNotification.actualInviteRequest.sender, out sender);

        if (!target) return;

        InviteRequest req = actualInviteRequest;
        if (actualInviteRequest.title != null && actualInviteRequest.title != string.Empty)
        {
            target.party.DeclineInvite();
            target.guild.DeclineInvite();
            target.playerAlliance.DeclineInvite();
            target.playerSpawnpoint.DeclineInvite(target.netIdentity, false);
            target.playerPartner.inviter = string.Empty;
            if (type == 5) target.playerFriends.AddFriend(sender.netIdentity);
            target.playerPartner.inviter = string.Empty;
        }
        req = new InviteRequest();
        if (invitation.Count > 0) invitation.Remove(invitation[0]);
        if (invitation.Count > 0) req = invitation[0];
        actualInviteRequest = req;
    }

    public void SetAndResetOtherRequest(InviteRequest invite)
    {
        Player target = null;
        Player sender = null;
        Player.onlinePlayers.TryGetValue(player.playerScreenNotification.actualInviteRequest.target, out target);
        Player.onlinePlayers.TryGetValue(player.playerScreenNotification.actualInviteRequest.sender, out sender);

        if (invite.type == 0)
        {
            player.party.SetInviteToParty(sender.name); 
            
            player.guild.DeclineInvite();
            player.playerAlliance.DeclineInvite();
            player.playerSpawnpoint.DeclineInvite(sender.netIdentity,false);
            player.trading.DeclineInvite();
            player.playerPartner.DeclineInvite();

        }
        if (invite.type == 1)
        {
            player.party.DeclineInvite();
            player.guild.SetInviteFromSelection(sender.netIdentity);
            player.playerAlliance.DeclineInvite();
            player.playerSpawnpoint.DeclineInvite(sender.netIdentity, false);
            player.trading.DeclineInvite();
            player.playerPartner.DeclineInvite();

        }
        if (invite.type == 2)
        {
            player.party.DeclineInvite();
            player.guild.DeclineInvite();
            player.playerAlliance.SetInviteToAlliance(sender.netIdentity);
            player.playerSpawnpoint.DeclineInvite(sender.netIdentity, false);
            player.trading.DeclineInvite();
            player.playerPartner.DeclineInvite();

        }
        if (invite.type == 3)
        {
            player.party.DeclineInvite();
            player.guild.DeclineInvite();
            player.playerAlliance.DeclineInvite();
            player.playerSpawnpoint.SetResurrectAction(sender.netIdentity);
            player.trading.DeclineInvite();
            player.playerPartner.DeclineInvite();

        }
        if (invite.type == 4)
        {
            player.party.DeclineInvite();
            player.guild.DeclineInvite();
            player.playerAlliance.DeclineInvite();
            player.playerSpawnpoint.DeclineInvite(sender.netIdentity, false);
            player.trading.SetRequest(sender.netIdentity);
            player.playerPartner.DeclineInvite();

        }
        if (invite.type == 5)
        {
            player.party.DeclineInvite();
            player.guild.DeclineInvite();
            player.playerAlliance.DeclineInvite();
            player.playerSpawnpoint.DeclineInvite(sender.netIdentity, false);
            player.trading.DeclineInvite();
            player.playerFriends.AddFriend(sender.netIdentity);
            player.playerPartner.DeclineInvite();
        }
        if (invite.type == 6)
        {
            player.party.DeclineInvite();
            player.guild.DeclineInvite();
            player.playerAlliance.DeclineInvite();
            player.playerSpawnpoint.DeclineInvite(sender.netIdentity, false);
            player.trading.DeclineInvite();
            player.playerPartner.InvitePartner(sender.netIdentity);
        }
    }
}
