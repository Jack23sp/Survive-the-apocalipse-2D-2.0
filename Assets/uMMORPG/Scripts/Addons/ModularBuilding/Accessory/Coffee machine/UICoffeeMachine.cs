using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public partial class Player
{
    [Command]
    public void CmdAddCoffeeToInventory(int coffeeBeansAmount)
    {
        int correctedAmount = coffeeBeansAmount / 20;
        ScriptableItem itm = null;
        if (ScriptableItem.All.TryGetValue("Coffee thermos".GetStableHashCode(), out itm))
        {
            if (inventory.CanAddItem (new Item(itm), correctedAmount))
            {
                ScriptableItem itm2 = null;
                if (ScriptableItem.All.TryGetValue("Coffee".GetStableHashCode(), out itm2))
                {
                    if (inventory.CountItem(new Item(itm2)) < coffeeBeansAmount) return; // cheater
                    inventory.RemoveItem(new Item(itm2),(correctedAmount * 20));
                    inventory.AddItem (new Item(itm), correctedAmount);
                }
                TargetRefreshCoffeePanel(true, correctedAmount);
            }
            else
            {
                TargetRefreshCoffeePanel(false, correctedAmount);
            }
        }      
    }

    [TargetRpc]
    public void TargetRefreshCoffeePanel(bool condition, int amount)
    {
        if (UICoffeeMachine.singleton)
            UICoffeeMachine.singleton.Refresh(condition, amount);
    }
}

public class UICoffeeMachine : MonoBehaviour
{
    public static UICoffeeMachine singleton;
    public GameObject panel;
    public Button closeButton;
    public Button useButton;
    public TextMeshProUGUI description;
    public TextMeshProUGUI actionDescription;
    public Slider slider;
    public TextMeshProUGUI sliderValue;
    public TextMeshProUGUI minValue;
    public TextMeshProUGUI maxValue;

    void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            description.text = string.Empty;
            actionDescription.text = string.Empty;
            slider.value = 0;
            panel.SetActive(false);
            closeButton.image.raycastTarget = false;
            BlurManager.singleton.Show();
            closeButton.interactable = false;
        });

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() =>
        {
            Player.localPlayer.CmdAddCoffeeToInventory(Convert.ToInt32(slider.value));
        });

        slider.onValueChanged.AddListener(delegate { CheckValue(); });
    }

    public void CheckValue()
    {
        if (ScriptableItem.All.TryGetValue("Coffee".GetStableHashCode(), out ScriptableItem itemData))
        {
            int max = Player.localPlayer.inventory.CountItem(new Item(itemData));
            maxValue.text = max.ToString();
            description.text = "Add " + (Convert.ToInt32(slider.value) / 20) + " Thermos of coffee to inventory";
        }
        sliderValue.text = Convert.ToInt32(slider.value).ToString();
    }

    public void Refresh(bool condition, int amount)
    {
        minValue.text = "0";
        if (ScriptableItem.All.TryGetValue("Coffee".GetStableHashCode(), out ScriptableItem itemData))
        {
            int max = Player.localPlayer.inventory.CountItem(new Item(itemData));
            maxValue.text = max.ToString();
            slider.maxValue = max;
            slider.value = 0;
        }
        
        if(condition)
        {
            actionDescription.text = amount + " Coffee thermos added to inventory";
        }
        else
        {
            actionDescription.text = "No coffee added to inventory";
        }
    }


    public void Open()
    {
        closeButton.image.raycastTarget = true;
        closeButton.interactable = true;
        panel.SetActive(true);
        slider.value = 0;
        if (ScriptableItem.All.TryGetValue("Coffee".GetStableHashCode(), out ScriptableItem itemData))
        {
            slider.maxValue = Player.localPlayer.inventory.CountItem(new Item(itemData));
        }
        else
        {
            slider.maxValue = 0;
            maxValue.text = "0";
        }

        actionDescription.text = string.Empty;
        CheckValue();
    }
}
