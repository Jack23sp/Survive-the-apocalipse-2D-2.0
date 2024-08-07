using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPartner : MonoBehaviour
{

    public static UIPartner singleton;
    public Button partnerButton;
    public GameObject partnerPanel;
    public GameObject playerDummy;
    public Camera creationCamera;
    public SeePartnerSlot partnerSlot;
    public Button closeButton;

    void OnEnable()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            partnerSlot.gameObject.SetActive(false);
        });

        partnerButton.onClick.RemoveAllListeners();
        partnerButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.playerPartner.CmdLoadPartner(Player.localPlayer.playerPartner.partnerName);
        });
    }
}
