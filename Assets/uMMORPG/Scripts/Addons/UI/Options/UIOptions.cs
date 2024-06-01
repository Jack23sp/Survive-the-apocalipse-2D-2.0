using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIOptions : MonoBehaviour
{
    public static UIOptions singleton;
    public OptionSlot marriageObject;
    public OptionSlot partyObject;
    public OptionSlot groupObject;
    public OptionSlot allyObject;
    public OptionSlot tradeObject;
    public OptionSlot friendsObject;
    public OptionSlot footstepObject;
    public OptionSlot soundsObject;
    public OptionSlot buttonSoundsObject;
    public OptionSlot buttonPostProcessing;
    public Slider sensibilityBuildingPlacement;

    public Button openBugSlot;

    private Player player;

    private void Awake()
    {
        if (!singleton) singleton = this;

        sensibilityBuildingPlacement.onValueChanged.AddListener(ChangeSensibility);
    }

    public void ChangeSensibility(float sensibility)
    {
        player.playerOptions.CmdChangeSensibility(sensibility);
    }

    public void Open()
    {
        player = Player.localPlayer;
        if (!player) return;

        player.playerOptions.ManageMarriage(false, player.playerOptions.blockMarriage);
        player.playerOptions.ManageParty(false, player.playerOptions.blockParty);
        player.playerOptions.ManageGroup(false, player.playerOptions.blockGroup);
        player.playerOptions.ManageAlly(false, player.playerOptions.blockAlly);
        player.playerOptions.ManageTrade(false, player.playerOptions.blockTrade);
        player.playerOptions.ManageFriend(false, player.playerOptions.blockFriend);
        player.playerOptions.ManageFootstep(false, player.playerOptions.blockFootstep);
        player.playerOptions.ManageButtonSounds(false, player.playerOptions.blockButtonSounds);
        player.playerOptions.ManageSound(false, player.playerOptions.blockSound);
        player.playerOptions.ManagePostProcessing(false, player.playerOptions.postProcessing);
        player.playerOptions.ManageBuildingSensibility(player.playerOptions.buildingSensibility, player.playerOptions.buildingSensibility);

        marriageObject.button.onClick.RemoveAllListeners();
        marriageObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockMarriage();
        });

        partyObject.button.onClick.RemoveAllListeners();
        partyObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockParty();
        });

        groupObject.button.onClick.RemoveAllListeners();
        groupObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockGroup();
        });

        allyObject.button.onClick.RemoveAllListeners();
        allyObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockAlly();
        });

        tradeObject.button.onClick.RemoveAllListeners();
        tradeObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockTrade();
        });

        friendsObject.button.onClick.RemoveAllListeners();
        friendsObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockFriends();
        });

        footstepObject.button.onClick.RemoveAllListeners();
        footstepObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockFootstep();
        });

        soundsObject.button.onClick.RemoveAllListeners();
        soundsObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockSound();
        });

        buttonSoundsObject.button.onClick.RemoveAllListeners();
        buttonSoundsObject.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdBlockButtonSound();
        });

        buttonPostProcessing.button.onClick.RemoveAllListeners();
        buttonPostProcessing.button.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerOptions.CmdManagePostProcessing();
        });

        openBugSlot.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            BugSent.singleton.Open();
        });
    }

}
