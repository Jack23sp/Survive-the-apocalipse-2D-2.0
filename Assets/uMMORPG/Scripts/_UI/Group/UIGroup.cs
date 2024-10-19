using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public partial class Player
{
    [Command]
    public void CmdLoadGroup(string guildName)
    {
        Guild g = Database.singleton.LoadGuild(guildName);
    }

    [TargetRpc]
    public void TargetSetLoadedGroup(Guild guild)
    {
        if (UIGroup.singleton)
        {
            UIGroup.singleton.selectedGuild = guild;
        }
    }
}

public partial class Player
{
    [Command]
    public void CmdCreate(string guildName)
    {
        // validate
        if (health.current > 0 && gold >= GuildSystem.CreationPrice &&
            !guild.InGuild())
        {
            if (GuildSystem.CreateGuild(name, level.current, guildName))
            {
                gold -= GuildSystem.CreationPrice;
            }
            else
                chat.TargetMsgInfo("Guild name invalid!");
        }
    }
}

public class UIGroup : MonoBehaviour
{
    public static UIGroup singleton;
    public Transform groupContent;
    public GameObject allyToSpawn;

    public Transform personalGroupContent;
    public GameObject personalGroupToSpawn;

    public Transform partyContent;
    public GameObject partToSpawn;

    public Guild selectedGuild;
    public Party party;

    public List<Guild> allyList = new List<Guild>();
    private Player player;

    public GameObject guildObject;
    public GameObject partyObject;

    public TMP_InputField groupName;

    public Button groupButton;

    public TextMeshProUGUI costValue;
    public Image goldImage;

    void Start()
    {
        if (!singleton) singleton = this;

        if (!player) player = Player.localPlayer;
        if (!player) return;

        groupButton.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(12);
            if (groupName.text != string.Empty)
                player.CmdCreate(groupName.text);
        });

        goldImage.sprite = ImageManager.singleton.gold;
        costValue.text = GuildSystem.CreationPrice.ToString();
    }

    public void Reset()
    {
        groupName.text = string.Empty;
        groupContent.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        personalGroupContent.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        partyContent.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
    }

    public void RefreshGuild()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        groupName.text = string.Empty;

        if (player.guild.InGuild())
        {
            UIUtils.BalancePrefabs(personalGroupToSpawn, 1, personalGroupContent);
            for (int i = 0; i < personalGroupContent.childCount; i++)
            {
                int index = i;
                GroupSlot slot = personalGroupContent.GetChild(index).GetComponent<GroupSlot>();
                slot.statName.text = player.guild.guild.name;
                slot.statAmount.text = player.guild.guild.members.Length + " / " + GuildSystem.Capacity;
                slot.statButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    selectedGuild = player.guild.guild;
                    guildObject.SetActive(true);
                    guildObject.GetComponent<UIGuildCustom>().Open(player.guild.guild);
                });
            }
        }
        else
        {
            UIUtils.BalancePrefabs(personalGroupToSpawn, 0, personalGroupContent);
        }

        if (player.party.InParty())
        {
            UIUtils.BalancePrefabs(allyToSpawn, 1, partyContent);
            for (int i = 0; i < partyContent.childCount; i++)
            {
                int index = i;
                GroupSlot slot = partyContent.GetChild(index).GetComponent<GroupSlot>();
                slot.statAmount.text = player.party.party.members.Length + " / " + Party.Capacity;
                slot.statName.text = player.party.party.master + "'s party";
                slot.statButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    partyObject.SetActive(true);
                    partyObject.GetComponent<UIPartyCustom>().Open();

                });
            }
        }
        else
        {
            UIUtils.BalancePrefabs(allyToSpawn, 0, partyContent);
        }

        if (player.playerAlliance.guildAlly.Count > 0)
        {
            UIUtils.BalancePrefabs(allyToSpawn, player.playerAlliance.guildAlly.Count, groupContent);
            for (int i = 0; i < player.playerAlliance.guildAlly.Count; i++)
            {
                int index = i;
                GroupSlot slot = groupContent.GetChild(index).GetComponent<GroupSlot>();
                slot.statName.text = player.playerAlliance.guildAlly[index];
                slot.statAmount.text = "";
                slot.statButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    guildObject.SetActive(true);
                    player.playerAlliance.CmdLoadGuild(player.playerAlliance.guildAlly[index]);
                });
            }
        }
        else
        {
            UIUtils.BalancePrefabs(allyToSpawn, 0, groupContent);
        }

    }
}
