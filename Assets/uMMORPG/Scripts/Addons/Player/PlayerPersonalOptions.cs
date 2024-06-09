using Mirror;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPersonalOptions : MonoBehaviour
{
    public static PlayerPersonalOptions singleton;
    public Player sender;

    public GameObject panel;

    public Button playerAbs;
    public Button playerJumpingJack;
    public Button playerPushUp;
    public Button playerClose;

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
        this.gameObject.SetActive(false);
    }

    public void CloseWithOptions(bool option)
    {
        if (UIButtonSounds.singleton && option) UIButtonSounds.singleton.ButtonPress(1);
        circleAnimation.Play("Close");
        Invoke(nameof(Close), 1.1f);

    }

    void Check()
    {
        panel.SetActive(true);
        circleAnimation.Play("Open");

        sender = Player.localPlayer;

        playerClose.onClick.RemoveAllListeners();
        playerClose.onClick.AddListener(() =>
        {
            CloseWithOptions(true);
        });

        playerAbs.onClick.RemoveAllListeners();
        playerAbs.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.playerAdditionalState.CmdSetAnimation("ABS", "Sportive");
            CloseWithOptions(false);
        });

        playerJumpingJack.onClick.RemoveAllListeners();
        playerJumpingJack.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.playerAdditionalState.CmdSetAnimation("JUMPINGJACK", "Sportive");
            CloseWithOptions(false);
        });

        playerPushUp.onClick.RemoveAllListeners();
        playerPushUp.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.playerAdditionalState.CmdSetAnimation("PUSHUPS", "Medician");
            CloseWithOptions(false);
        });
    }
}
