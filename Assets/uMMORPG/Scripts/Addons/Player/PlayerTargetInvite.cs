using Mirror;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerGuild
{
    public void SetInviteFromSelection(NetworkIdentity identity)
    {
        Player targetPlayer = identity.GetComponent<Player>();
        // validate
        if (targetPlayer && !InGuild() && targetPlayer.guild.InGuild() &&
            targetPlayer.guild.guild.CanInvite(targetPlayer.name, name) &&
            NetworkTime.time >= player.nextRiskyActionTime)
        {
            // send an invite
            player.guild.inviteFrom = targetPlayer.name;
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
    }


}

public partial class PlayerParty
{

    public void SetInviteToParty(string senderName)
    {
        if (senderName != name &&
            Player.onlinePlayers.TryGetValue(senderName, out Player sender) &&
            NetworkTime.time >= player.nextRiskyActionTime)
        {
            // can only send invite if no party yet or party isn't full and
            // have invite rights and other guy isn't in party yet
            if (!InParty() && !sender.party.party.IsFull())
            {
                // send an invite
                player.party.inviteFrom = sender.name;
                Debug.Log(name + " invited " + sender.name + " to party");
            }
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
    }
}

public class PlayerTargetInvite : MonoBehaviour
{
    public static PlayerTargetInvite singleton;
    public Player target;
    public Player sender;

    public GameObject panel;

    public Button playerClose;
    public Button playerParty;
    public Button playerGroup;
    public Button playerAlly;
    public Button playerRevive;
    public Button playerMarriage;
    public Button playerFriend;

    public Animation circleAnimation;

    private void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void Open()
    {
        Check();
    }


    public void Close()
    {
        target = null;
        this.gameObject.SetActive(false);
    }

    void Check()
    {
        panel.SetActive(true);
        circleAnimation.Play("Open");

        sender = Player.localPlayer;

        playerClose.onClick.RemoveAllListeners();
        playerClose.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            circleAnimation.Play("Close");
            Invoke(nameof(Close),1.1f);
        });

        playerParty.onClick.RemoveAllListeners();
        playerParty.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (target && target.health.current > 0 && sender.health.current > 0)
            {
                if (target.name != name &&
                    Player.onlinePlayers.TryGetValue(target.name, out Player other) &&
                    other.party.inviteFrom == string.Empty &&
                    NetworkTime.time >= sender.nextRiskyActionTime)
                {
                    // can only send invite if no party yet or party isn't full and
                    // have invite rights and other guy isn't in party yet
                    if ((!sender.party.InParty() || !sender.party.party.IsFull()) && !other.party.InParty())
                    {
                        sender.playerScreenNotification.CmdAddNotification(target.netIdentity, new InviteRequest("Party", "<b>" + sender.name + "</b>" + " invite you to a party!", true, 0, sender.name, target.name));
                    }
                }
                else
                {
                    sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot invite player in party");
                }
            }
            else
            {
                sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot invite player in party");
            }
        });

        playerGroup.onClick.RemoveAllListeners();
        playerGroup.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (target && target.health.current > 0 && sender.health.current > 0)
            {
                if (target && sender.guild.InGuild() && !target.guild.InGuild() &&
                    sender.guild.guild.CanInvite(sender.name, target.name))
                {
                    sender.playerScreenNotification.CmdAddNotification(target.netIdentity, new InviteRequest("Group", "<b>" + sender.name + "</b>" + " invite you to the group " + "<b>" + sender.guild.guild.name + "</b>", true, 1, sender.name, target.name));
                }
                else
                {
                    sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot invite this player to your group");
                }
            }
            else
            {
                sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot invite this player to your group");
            }
        });

        playerAlly.onClick.RemoveAllListeners();
        playerAlly.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (target && target.health.current > 0 && sender.health.current > 0)
            {
                if (sender.guild.InGuild() &&
                                  target.guild.InGuild() &&
                                  sender.guild.guild.GetMemberIndex(sender.name) != -1 &&
                                            sender.guild.guild.members[sender.guild.guild.GetMemberIndex(sender.name)].rank >= GuildSystem.PromoteMinRank &&
                                  target.guild.guild.GetMemberIndex(target.name) != -1 &&
                                            target.guild.guild.members[target.guild.guild.GetMemberIndex(target.name)].rank >= GuildSystem.PromoteMinRank &&
                                  sender.playerAlliance.guildAlly.Count < sender.playerAlliance.MaxAllianceAmount() &&
                                  target.playerAlliance.guildAlly.Count < sender.playerAlliance.MaxTargetAllianceAmount(target))
                {
                    sender.playerScreenNotification.CmdAddNotification(target.netIdentity, new InviteRequest("Group alliance", "<b>" + sender.name + "</b>" + " invite you to an alliance with the group " + "<b>" + sender.guild.guild.name + "</b>", true, 2, sender.name, target.name));
                }
                else
                {
                    sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot invite this group to your group ally, you and player need to check your LEADER ability level");
                }
            }
            else
            {
                sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot invite this group to your group ally, you and player need to check your LEADER ability level");
            }
        });

        playerRevive.onClick.RemoveAllListeners();
        playerRevive.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (target && target.health.current == 0 && sender.health.current > 0)
            {
                if (sender.health.current > 0 &&
                   target.health.current == 0 &&
                   sender.inventory.CountItem(new Item(PremiumItemManager.singleton.instantResurrectOtherPlayer)) > 0)
                {
                    sender.playerScreenNotification.CmdAddNotification(target.netIdentity, new InviteRequest("Revive", "<b>" + sender.name + "</b>" + " want revive you!", true, 3, sender.name, target.name));
                }
                else
                {
                    sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot revive this player");
                }
            }
            else
            {
                sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot revive this player");
            }
        });

        playerMarriage.onClick.RemoveAllListeners();
        playerMarriage.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (target && target.health.current > 0 && sender.health.current > 0)
            {
                if (sender.playerPartner.partnerName == string.Empty && target.playerPartner.partnerName == string.Empty)
                {
                    sender.playerScreenNotification.CmdAddNotification(target.netIdentity, new InviteRequest("Partner", "<b>" + sender.name + "</b>" + " want be your partner!", true, 6, sender.name, target.name));
                }
                else
                {
                    sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot be the partner of this player");
                }
            }
            else
            {
                sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot be the partner of this player");
            }
        });

        playerFriend.onClick.RemoveAllListeners();
        playerFriend.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (target && target.health.current > 0 && sender.health.current > 0)
            {
                if (target.playerFriends.request.Count < FriendsManager.singleton.maxFriendRequest &&
                    sender.playerFriends.friends.Count < FriendsManager.singleton.maxFriends &&
                    target.playerFriends.friends.Count < FriendsManager.singleton.maxFriends)
                {
                    sender.playerScreenNotification.CmdAddNotification(target.netIdentity, new InviteRequest("Friend", "<b>" + sender.name + "</b>" + " want be your friends!", true, 5, sender.name, target.name));
                }
                else
                {
                    sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot be friend with this player");
                }
            }
            else
            {
                sender.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You cannot be friend with this player");
            }
        });
    }
}
