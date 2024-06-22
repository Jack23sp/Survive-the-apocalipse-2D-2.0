using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class PlayerGuild
{
    [Command]
    public void CmdSetBadge(string playername, string guildname, int background, int foreground)
    {
        if (GuildSystem.guilds.TryGetValue(guildname, out Guild guild) &&
            guild.CanTerminate(playername))
        {
            guild.background = background;
            guild.foreground = foreground;
            TargetRefreshBadge();
            GuildSystem.BroadcastChanges(guild);
        }
    }

    [TargetRpc]
    public void TargetRefreshBadge()
    {
        if (BadgeCustom.singleton)
        {
            BadgeCustom.singleton.Reset();
        }
        //if (UIGuildCustom.singleton)
        //{
        //    UIGuildCustom.singleton.SetBadge(guild);
        //}
    }

}

public class BadgeCustom : MonoBehaviour
{
    public static BadgeCustom singleton;
    public GameObject badgeSlot;
    public Transform backgroundContent;
    public Transform foregroundContent;
    public Image backgroundImage;
    public Image foregroundImage;
    public Button okButton;
    public int background;
    public int foreground;
    public Button closeButton;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Open()
    {
        closeButton.image.raycastTarget = true;
        okButton.interactable = false;
        background = -1;
        foreground = -1;

        Spawn();

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.guild.CmdSetBadge(Player.localPlayer.name, Player.localPlayer.guild.guild.name, background, foreground);
        });

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            closeButton.image.raycastTarget = false;
            closeButton.gameObject.SetActive(false);
            Reset();
        });
    }

    public void Spawn()
    {
        UIUtils.BalancePrefabs(badgeSlot, BadgeManager.singleton.background.Count, backgroundContent);
        for (int i = 0; i < BadgeManager.singleton.background.Count; i++)
        {
            int index = i;
            BadgeSlot slot = backgroundContent.GetChild(index).GetComponent<BadgeSlot>();
            slot.button.image.sprite = BadgeManager.singleton.background[index];
            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(13);
                background = index;
                backgroundImage.gameObject.SetActive(true);
                backgroundImage.sprite = BadgeManager.singleton.background[index];
                backgroundImage.preserveAspect = true;
                if (background > -1 && foreground > -1) okButton.interactable = true;
            });
        }
        UIUtils.BalancePrefabs(badgeSlot, BadgeManager.singleton.foreground.Count, foregroundContent);
        for (int i = 0; i < BadgeManager.singleton.foreground.Count; i++)
        {
            int index = i;
            BadgeSlot slot = foregroundContent.GetChild(index).GetComponent<BadgeSlot>();
            slot.button.image.sprite = BadgeManager.singleton.foreground[index];
            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(14);
                foreground = index;
                foregroundImage.gameObject.SetActive(true);
                foregroundImage.sprite = BadgeManager.singleton.foreground[index];
                foregroundImage.preserveAspect = true;
                if (background > -1 && foreground > -1) okButton.interactable = true;
            });
        }
    }

    public void Reset()
    {
        background = -1;
        foreground = -1;
        okButton.interactable = false;
        backgroundContent.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        foregroundContent.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        foregroundImage.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);

    }
}
