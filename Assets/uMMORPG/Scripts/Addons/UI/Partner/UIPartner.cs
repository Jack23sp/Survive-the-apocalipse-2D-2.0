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
    public bool reset;

    void OnEnable()
    {
        if (!singleton) singleton = this;
        reset = true;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            partnerSlot.gameObject.SetActive(false);
            reset = false;
            partnerSlot.leftArrow.onClick.Invoke();
            reset = true;
        });

        partnerSlot.leftArrow.onClick.RemoveAllListeners();
        partnerSlot.leftArrow.onClick.AddListener(() =>
        {
            ClickArrow(0, reset);
        });

        partnerSlot.rightArrow.onClick.RemoveAllListeners();
        partnerSlot.rightArrow.onClick.AddListener(() =>
        {
            ClickArrow(1, reset);
        });


        partnerButton.onClick.RemoveAllListeners();
        partnerButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.playerPartner.CmdLoadPartner(Player.localPlayer.playerPartner.partnerName);
        });
    }

    public void ClickArrow(int direction,bool condition)
    {
        if (UIButtonSounds.singleton && condition) UIButtonSounds.singleton.ButtonPress(0);
        if (direction == 1) partnerSlot.headerText.text = "Alliance";
        else if (direction == 0) partnerSlot.headerText.text = "Abilities";
    }
}
