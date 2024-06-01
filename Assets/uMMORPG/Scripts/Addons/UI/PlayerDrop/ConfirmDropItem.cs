using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmDropItem : MonoBehaviour
{
    public static ConfirmDropItem singleton;
    public Button drop;
    public Button cancel;
    public Button closeButton;
    public GameObject panel;
    public Slider slider;
    public TextMeshProUGUI sliderValue;
    public int indexSlot;
    public bool inventory;
    private int amount;

    void Start()
    {
        if (!singleton) singleton = this;
        

        drop.onClick.RemoveAllListeners();
        drop.onClick.AddListener(() =>
        {
            Player.localPlayer.playerItemDrop.CmdSpawnDropSpecificItem(indexSlot, inventory, Convert.ToInt32(slider.value));
            if (inventory)
                amount = Player.localPlayer.inventory.slots[indexSlot].amount;
            else
                amount = Player.localPlayer.playerBelt.belt[indexSlot].amount;

            if (amount == 0)
            {
                closeButton.onClick.Invoke();
                UISelectedItem.singleton.closeButton.onClick.Invoke();
            }
            //Manage(false);
        });

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Manage(false);
            BlurManager.singleton.Show();
        });

        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(() =>
        {
            Manage(false);
            BlurManager.singleton.Show();
        });

        slider.onValueChanged.AddListener(delegate { CheckValue(); });
    }

    public void CheckValue()
    {
        sliderValue.text = slider.value.ToString();
    }

    public void Manage(bool condition)
    {
        if(inventory)
            slider.maxValue = Player.localPlayer.inventory.slots[indexSlot].amount;
        else
            slider.maxValue = Player.localPlayer.playerBelt.belt[indexSlot].amount;
        slider.value = 0;
        closeButton.image.raycastTarget = condition;
        closeButton.image.enabled = condition;
        panel.SetActive(condition);
    }
}
