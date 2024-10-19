using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class ConfirmBuyBoost : MonoBehaviour
{
    public UIBoost boostMain;
    public TextMeshProUGUI description;

    public Button closeButton;

    public Button acceptButton;
    public Button cancelButton;

    public TextMeshProUGUI descriptionText;

    public void Refresh()
    {
        descriptionText.text = "Do you really want buy : " + UIBoost.singleton.selectedBoost.name + " for " + UIBoost.singleton.selectedBoost.coin + " coins ?";
        description.text = Player.localPlayer.playerBoost.LookAtBoostTemplateDescription(UIBoost.singleton.selectedBoost.name);
        acceptButton.interactable = Player.localPlayer.itemMall.coins >= UIBoost.singleton.selectedBoost.coin;
        acceptButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.playerBoost.CmdAddBoost(UIBoost.singleton.selectedBoost.name);
            cancelButton.onClick.Invoke();
        });

        cancelButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            UIBoost.singleton.selectedBoost = null;
            Destroy(this.gameObject);
        });

        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            cancelButton.onClick.Invoke();
        });
    }

}
