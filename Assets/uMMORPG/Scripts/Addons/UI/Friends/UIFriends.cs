using AdvancedPeopleSystem;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFriends : MonoBehaviour
{
    public Player player;
    public static UIFriends singleton;
    public Transform requestTransform;
    public Transform friendTransform;

    public GameObject requestFriendSlot;
    public GameObject friendSlot;

    public SeeFriendSlot friendPanel;
    public GameObject playerDummy;

    public Camera creationCamera;

    void Start()
    {
        if (!singleton) singleton = this;
        friendPanel.closeButton.onClick.RemoveAllListeners();
        friendPanel.closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            friendPanel.gameObject.SetActive(false);
            playerDummy.gameObject.SetActive(false);
            creationCamera.enabled = false;
            friendPanel.closeButton.image.raycastTarget = false;
        });
    }

    public void Open()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;


        UIUtils.BalancePrefabs(requestFriendSlot, player.playerFriends.request.Count, requestTransform);
        for (int i = 0; i < player.playerFriends.request.Count; i++)
        {
            int indexi = i;
            RequestSlot slot = requestTransform.GetChild(indexi).GetComponent<RequestSlot>();
            Player requestOnlinePlayer;
            if (Player.onlinePlayers.TryGetValue(player.playerFriends.request[indexi], out requestOnlinePlayer))
            {
                slot.online.color = FriendsManager.singleton.onlineColor;
                slot.friendRequestName.text = player.playerFriends.request[indexi];
                slot.friendRequestAccept.interactable = player.playerFriends.friends.Count < FriendsManager.singleton.maxFriends;
                slot.friendRequestAccept.onClick.RemoveAllListeners();
                slot.friendRequestAccept.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.playerFriends.CmdAcceptFriends(player.playerFriends.request[indexi]);
                });
                slot.friendRequestRemove.onClick.RemoveAllListeners();
                slot.friendRequestRemove.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.playerFriends.CmdRemoveRequestFriends(player.playerFriends.request[indexi]);
                });
                slot.seeFriend.onClick.RemoveAllListeners();
                slot.seeFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    SetupPlayerPanel(requestOnlinePlayer, player.playerFriends.request[indexi]);
                });
            }
            else
            {
                slot.online.color = FriendsManager.singleton.offlineColor;
                slot.friendRequestName.text = player.playerFriends.request[indexi];
                slot.friendRequestAccept.interactable = false;
                slot.friendRequestRemove.onClick.RemoveAllListeners();
                slot.friendRequestRemove.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.playerFriends.CmdRemoveRequestFriends(player.playerFriends.request[indexi]);
                });
                slot.seeFriend.onClick.RemoveAllListeners();
                slot.seeFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    //SetupPlayerPanel(requestOnlinePlayer, player.playerFriends.playerRequest[indexi]);
                });

            }
        }



        UIUtils.BalancePrefabs(friendSlot, player.playerFriends.friends.Count, friendTransform);
        for (int i = 0; i < player.playerFriends.friends.Count; i++)
        {
            int index = i;
            FriendSlot slot = friendTransform.GetChild(index).GetComponent<FriendSlot>();
            Player friendOnlinePlayer;
            if (Player.onlinePlayers.TryGetValue(player.playerFriends.friends[index], out friendOnlinePlayer))
            {
                slot.online.color = FriendsManager.singleton.onlineColor;
                slot.playerFriendsName.text = player.playerFriends.friends[index];

                slot.removeFriend.onClick.RemoveAllListeners();
                slot.removeFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.playerFriends.CmdRemoveFriends(player.playerFriends.friends[index]);
                });

                slot.messageFriend.onClick.RemoveAllListeners();
                slot.messageFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    UIFriendMessage.singleton.Open(player.playerFriends.friends[index]);
                });

                slot.partyFriend.interactable = false;
                if (friendOnlinePlayer.name != name &&
                    Player.onlinePlayers.TryGetValue(friendOnlinePlayer.name, out Player other) &&
                    other.party.inviteFrom == string.Empty &&
                    NetworkTime.time >= friendOnlinePlayer.nextRiskyActionTime)
                {
                    // can only send invite if no party yet or party isn't full and
                    // have invite rights and other guy isn't in party yet
                    if ((!friendOnlinePlayer.party.InParty() || !friendOnlinePlayer.party.party.IsFull()) && !other.party.InParty())
                    {
                        slot.partyFriend.interactable = true;
                    }
                }

                slot.partyFriend.onClick.RemoveAllListeners();
                slot.partyFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.playerScreenNotification.CmdAddNotification(friendOnlinePlayer.netIdentity, new InviteRequest("Party", "<b>" + player.name + "</b>" + " invite you to a party!", true, 0, player.name, friendOnlinePlayer.name));
                });

                slot.guildFriend.interactable = false;
                if (friendOnlinePlayer && friendOnlinePlayer.health.current > 0 && player.health.current > 0)
                {
                    if (friendOnlinePlayer && player.guild.InGuild() && !friendOnlinePlayer.guild.InGuild() &&
                        player.guild.guild.CanInvite(player.name, friendOnlinePlayer.name))
                    {
                        slot.guildFriend.interactable = true;
                    }
                }
                slot.guildFriend.onClick.RemoveAllListeners();
                slot.guildFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.playerScreenNotification.CmdAddNotification(friendOnlinePlayer.netIdentity, new InviteRequest("Group", "<b>" + player.name + "</b>" + " invite you to the group " + "<b>" + player.guild.guild.name + "</b>", true, 1, player.name, friendOnlinePlayer.name));
                });

                slot.seeFriend.onClick.RemoveAllListeners();
                slot.seeFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    SetupPlayerPanel(friendOnlinePlayer, player.playerFriends.friends[index]);
                });
            }
            else
            {
                slot.online.color = FriendsManager.singleton.offlineColor;
                slot.playerFriendsName.text = player.playerFriends.friends[index];

                ////slot.removeFriend.onClick.RemoveAllListeners();
                ////slot.removeFriend.onClick.AddListener(() =>
                ////{
                ////    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                ////    player.playerFriends.CmdRemoveFriends(player.playerFriends.friends[index]);
                ////});
                slot.seeFriend.onClick.RemoveAllListeners();
                slot.seeFriend.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    //SetupPlayerPanel(friendOnlinePlayer, player.playerFriends.playerFriends[index]);
                });
            }
        }
    }

    public void SetupPlayerPanel(Player pl, string name)
    {
        friendPanel.gameObject.SetActive(true);
        playerDummy.gameObject.SetActive(true);
        creationCamera.enabled = true;
        friendPanel.closeButton.image.raycastTarget = true;

        BuildFriendPreview(pl, playerDummy);

        friendPanel.friendName.text = name;
        friendPanel.guildName.text = "[" + pl.guild.guild.name + "] ";
        friendPanel.level.text = pl.level.current + " / " + pl.level.max;
        friendPanel.health.text = pl.health.health.current + " / " + pl.health.max;
        friendPanel.stamina.text = pl.mana.baseMana.baseValue + " / " + pl.mana.max;
        friendPanel.accuracy.text = pl.playerAccuracy._accuracy + " / 100";
        friendPanel.partner.text = pl.playerPartner.partnerName != string.Empty ? pl.playerPartner.partnerName : string.Empty;
        friendPanel.armor.text = pl.playerArmor.current + " / " + pl.playerArmor.max;

        UIUtils.BalancePrefabs(friendPanel.abilitySlot.gameObject, pl.playerAbility.networkAbilities.Count, friendPanel.abilitiesContent);
        for (int i = 0; i < pl.playerAbility.networkAbilities.Count; i++)
        {
            int index = i;
            AbilitySlot slot = friendPanel.abilitiesContent.GetChild(index).GetComponent<AbilitySlot>();
            slot.statName.text = pl.playerAbility.networkAbilities[index].name;
            slot.image.sprite = pl.playerAbility.abilities[index].image;
            slot.statAmount.text = pl.playerAbility.networkAbilities[index].level + " / " + pl.playerAbility.networkAbilities[index].maxLevel;
            slot.button.gameObject.SetActive(false);
            slot.button.onClick.RemoveAllListeners();
        }

        friendPanel.guildSlot.SetActive(pl.guild.InGuild());
        if(friendPanel.guildSlot.activeInHierarchy)
        {
            friendPanel.personalGroupSlot.statName.text = pl.guild.guild.name;
            friendPanel.personalGroupSlot.statAmount.text = pl.guild.guild.members.Length + " / " + GuildSystem.Capacity;
        }

        UIUtils.BalancePrefabs(friendPanel.guildSlot, pl.playerAlliance.guildAlly.Count, friendPanel.groupContent);
        for (int g = 0; g < pl.playerAlliance.guildAlly.Count; g++)
        {
            GroupSlot groupSlot = friendPanel.groupContent.GetChild(g).GetComponent<GroupSlot>();
            groupSlot.statName.text = pl.playerAlliance.guildAlly[g];
        }

    }

    public void BuildFriendPreview(Player player, GameObject prefab)
    {
        CharacterCustomization characterCustomization = prefab.GetComponent<CharacterCustomization>();

        characterCustomization.SwitchCharacterSettings(player.playerCharacterCreation.sex);
        characterCustomization.SetElementByIndex(CharacterElementType.Hair, player.playerCharacterCreation.hairType);
        characterCustomization.SetElementByIndex(CharacterElementType.Beard, player.playerCharacterCreation.beard);
        Color newCol;
        if (ColorUtility.TryParseHtmlString("#" + player.playerCharacterCreation.hairColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
        if (ColorUtility.TryParseHtmlString("#" + player.playerCharacterCreation.underwearColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
        if (ColorUtility.TryParseHtmlString("#" + player.playerCharacterCreation.eyesColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
        if (ColorUtility.TryParseHtmlString("#" + player.playerCharacterCreation.skinColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
        characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, player.playerCharacterCreation.fat);
        characterCustomization.SetHeight(player.playerCharacterCreation.height);
        if (player.playerCharacterCreation.sex == 0)
        {
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, player.playerCharacterCreation.thin);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, player.playerCharacterCreation.muscle);
        }
        else
        {
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, player.playerCharacterCreation.breast);
        }

        player.playerCharacterCreation.DressSelectablePlayer(player, characterCustomization);
    }
}
