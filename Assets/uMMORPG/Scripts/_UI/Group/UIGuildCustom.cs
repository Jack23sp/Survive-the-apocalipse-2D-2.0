using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial struct Guild
{
    public bool CanChangeBadge(string requesterName)
    {
        int index = GetMemberIndex(requesterName);
        return index != -1 &&
               members[index].rank >= GuildRank.Vice;
    }

}

public partial class UIGuildCustom : MonoBehaviour
{
    public static UIGuildCustom singleton;
    public Text nameText;
    public Text masterText;
    public Text currentCapacityText;
    public Text maximumCapacityText;
    public InputField noticeInput;
    public Button noticeEditButton;
    public Button noticeSetButton;
    public UIGuildMemberSlot slotPrefab;
    public Transform memberContent;
    public Color onlineColor = Color.cyan;
    public Color offlineColor = Color.gray;
    public Button leaveButton;
    public Button closeButton;
    public Guild currentGuild;
    public GameObject badgePanel;
    public Button badgeButton;
    public Image foregroundBadge;
    public Image backgroundBadge;

    public void Start()
    {
        if (!singleton) singleton = this;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            closeButton.image.raycastTarget = false;
            closeButton.gameObject.SetActive(false);
            currentGuild = new Guild();
        });

        noticeInput.onValueChanged.AddListener(delegate { CheckSetInteractable(); });
    }

    public void SetBadge(Guild guild)
    {
        backgroundBadge.sprite = BadgeManager.singleton.background[guild.background];
        backgroundBadge.preserveAspect = true;
        foregroundBadge.sprite = BadgeManager.singleton.foreground[guild.foreground];
        backgroundBadge.preserveAspect = true;
        if (guild.background > -1 || guild.foreground > -1)
        {
            badgeButton.image.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
        else
        {
            badgeButton.image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }

    public void CheckSetInteractable()
    {
        Player player = Player.localPlayer;

        if (player)
        {
            noticeSetButton.interactable = currentGuild.CanNotify(player.name) &&
                                           noticeInput.interactable &&
                                           NetworkTime.time >= player.nextRiskyActionTime;
        }
    }

    public void Open(Guild guild)
    {
        currentGuild = guild;
        Player player = Player.localPlayer;
        if (player)
        {
            bool MyGuild = currentGuild.name == player.guild.guild.name;
            currentGuild = MyGuild ? player.guild.guild : currentGuild;

            if (currentGuild.CanChangeBadge(player.name))
            {
                badgeButton.interactable = true;
            }
            else
            {
                badgeButton.interactable = false;
            }

            badgeButton.interactable = MyGuild;
            badgeButton.onClick.RemoveAllListeners();
            badgeButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                badgePanel.SetActive(true);
                badgePanel.GetComponent<BadgeCustom>().Open();
            });

            closeButton.image.raycastTarget = true;
            closeButton.gameObject.SetActive(true);

            SetBadge(currentGuild);

            int memberCount = currentGuild.members != null ? currentGuild.members.Length : 0;

            // guild properties
            nameText.text = currentGuild.name;
            masterText.text = currentGuild.master;
            currentCapacityText.text = memberCount.ToString();
            maximumCapacityText.text = GuildSystem.Capacity.ToString();

            // notice edit button
            noticeEditButton.interactable = MyGuild && currentGuild.CanNotify(player.name) &&
                                            !noticeInput.interactable;
            noticeEditButton.onClick.RemoveAllListeners();
            noticeEditButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                noticeInput.interactable = true;
            });

            // notice set button
            noticeSetButton.interactable = MyGuild && currentGuild.CanNotify(player.name) &&
                                           noticeInput.interactable &&
                                           NetworkTime.time >= player.nextRiskyActionTime;
            noticeSetButton.onClick.RemoveAllListeners();
            noticeSetButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                noticeInput.interactable = false;
                if (noticeInput.text.Length > 0 &&
                    !string.IsNullOrWhiteSpace(noticeInput.text) &&
                    noticeInput.text != currentGuild.notice && MyGuild)
                {
                    player.guild.CmdSetNotice(noticeInput.text);
                    noticeSetButton.interactable = false;
                }
            });

            // notice input: copies notice while not editing it
            if (!noticeInput.interactable) noticeInput.text = currentGuild.notice ?? "";
            noticeInput.characterLimit = GuildSystem.NoticeMaxLength;

            // leave
            leaveButton.gameObject.SetActive(true);
            leaveButton.interactable = (MyGuild && (currentGuild.CanLeave(player.name) || currentGuild.CanTerminate(player.name))) || (currentGuild.name != player.guild.guild.name && player.guild.guild.master == player.name);
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
                if (MyGuild)
                {
                    if (player.guild.guild.members.Length > 1)
                        player.guild.CmdLeave();
                    else
                    {
                        for (int i = 0; i < player.playerAlliance.guildAlly.Count; i++)
                        {
                            player.playerAlliance.CmdRemoveAllyGuild(player.playerAlliance.guildAlly[i], currentGuild.name);
                            player.playerAlliance.CmdElimininateGuildAlltFromDatabase(currentGuild.name, player.playerAlliance.guildAlly[i]);
                        }
                        player.guild.CmdTerminate();
                    }
                }
                else
                {
                    if (player.guild.InGuild() && player.guild.guild.CanTerminate(player.name))
                    {
                        player.playerAlliance.CmdRemoveAllyGuild(currentGuild.name, player.guild.guild.name);
                        player.playerAlliance.CmdElimininateGuildAlltFromDatabase(player.guild.guild.name, currentGuild.name);
                    }
                }
                closeButton.onClick.Invoke();
            });

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, memberCount, memberContent);

            // refresh all members
            for (int i = 0; i < memberCount; ++i)
            {
                UIGuildMemberSlot slot = memberContent.GetChild(i).GetComponent<UIGuildMemberSlot>();
                GuildMember member = currentGuild.members[i];

                slot.onlineStatusImage.color = member.online ? onlineColor : offlineColor;
                slot.nameText.text = member.name;
                slot.levelText.text = member.level.ToString();
                slot.rankText.text = member.rank.ToString();
                slot.promoteButton.interactable = MyGuild && currentGuild.CanPromote(player.name, member.name);
                slot.promoteButton.onClick.RemoveAllListeners();
                slot.promoteButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.guild.CmdPromote(member.name);
                });
                slot.demoteButton.interactable = MyGuild && currentGuild.CanDemote(player.name, member.name);
                slot.demoteButton.onClick.RemoveAllListeners();
                slot.demoteButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.guild.CmdDemote(member.name);
                });
                slot.kickButton.interactable = MyGuild && currentGuild.CanKick(player.name, member.name);
                slot.kickButton.onClick.RemoveAllListeners();
                slot.kickButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.guild.CmdKick(member.name);
                });
            }
        }
    }
}
