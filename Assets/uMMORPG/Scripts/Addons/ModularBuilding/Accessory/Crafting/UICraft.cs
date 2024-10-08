using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public partial class ScriptableItem
{
    [Header("Crafting part")]
    public long coinToCraft;
    public long goldToCraft;
}

public class UICraft : MonoBehaviour, IUIScript
{
    private Player player;
    public static UICraft singleton;
    public GameObject panel;
    public Button closeButton;
    public Button manageButton;

    public GameObject craftPanel;
    public Button craftButtonGold;
    public Button craftButtonCoin;
    public Slider slider;
    public ScriptableItem selectedItem;
    public Transform ingredientCraftContent;
    public GameObject newSlot;
    public GameObject newSlotIngredient;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI sliderCraftText;

    public GameObject craftSlot;
    public GameObject craftSlotChild;
    public Transform content;
    public Transform contentFinished;
    public CraftAccessory craftAccessory;
    public TextMeshProUGUI titleText;
    private TimeSpan difference;
    public CraftinItemSlot runtimeBuildCraftItem = new CraftinItemSlot();

    private int itemCount = 0;
    public int firstSelected = -1;
    public int craftIndex = -1;

    List<ItemCrafting> crafts = new List<ItemCrafting>();
    private int secondsRemaining = 0;


    public void Start()
    {
        if (!singleton) singleton = this;
        InvokeRepeating(nameof(UpdateEndingItem), 1.0f, 1.0f);
    }

    public void Craft(int index)
    {
        if (index == -1) return;
        craftIndex = index;
        craftPanel.SetActive(true);
        runtimeBuildCraftItem.amount = crafts[index].itemAndAmount.amount;
        runtimeBuildCraftItem.item = crafts[index].itemAndAmount.item.name;
        runtimeBuildCraftItem.owner = player.name;
        runtimeBuildCraftItem.owner = player.guild.guild.name;
        //runtimeBuildCraftItem.timeBegin = DateTime.UtcNow.ToString();
        //runtimeBuildCraftItem.timeEnd = DateTime.UtcNow.AddSeconds(crafts[index].timeToCraft).ToString();
        //runtimeBuildCraftItem.serverTimeBegin = runtimeBuildCraftItem.serverTimeEnd = String.Empty;
        runtimeBuildCraftItem.index = index;
        runtimeBuildCraftItem.amount = Convert.ToInt32(slider.value);
        runtimeBuildCraftItem.sex = player.playerCharacterCreation.sex;
        sliderCraftText.text = Convert.ToInt32(slider.value).ToString();

        UIUtils.BalancePrefabs(newSlotIngredient, crafts[index].ingredients.Count, ingredientCraftContent);
        for (int e = 0; e < crafts[index].ingredients.Count; e++)
        {
            int indexe = e;
            BuyBoostSlot childSlot = ingredientCraftContent.GetChild(indexe).GetComponent<BuyBoostSlot>();
            childSlot.boostImage.sprite = crafts[index].ingredients[indexe].item.image;
            childSlot.boostImage.preserveAspect = true;
            itemCount = player.inventory.CountItem(new Item(crafts[index].ingredients[indexe].item));
            childSlot.timer.text = itemCount + " / " + crafts[index].ingredients[indexe].amount * Convert.ToInt32(slider.value);
            childSlot.timer.color = itemCount < crafts[index].ingredients[indexe].amount * Convert.ToInt32(slider.value) ? Color.red : Color.white;
            childSlot.title.text = crafts[index].ingredients[indexe].item.name;
            childSlot.coinImage.gameObject.SetActive(false);
            childSlot.coins.gameObject.SetActive(false);
        }

        coinText.text = (crafts[index].coin * Convert.ToInt32(slider.value)).ToString();
        goldText.text = (crafts[index].gold * Convert.ToInt32(slider.value)).ToString();
        craftButtonGold.interactable = player.itemMall.coins >= (crafts[index].coin * Convert.ToInt32(slider.value));
        craftButtonGold.interactable = player.gold >= crafts[index].gold * Convert.ToInt32(slider.value);
    }

    //public void SetItemCraftingTime(int index)
    //{
    //    runtimeBuildCraftItem.timeBegin = DateTime.UtcNow.ToString();
    //    runtimeBuildCraftItem.timeEnd = DateTime.UtcNow.AddSeconds(crafts[index].timeToCraft).ToString();
    //    runtimeBuildCraftItem.serverTimeBegin = runtimeBuildCraftItem.serverTimeEnd = String.Empty;
    //}

    public void SetupForButton(int index)
    {
        Craft(index);
    }

    public void Open(CraftAccessory craftAcc)
    {
        slider.minValue = 1;
        slider.maxValue = 50;
        craftAccessory = craftAcc;
        Assign();
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(craftAccessory, Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(craftAccessory.netIdentity, craftAccessory.craftingAccessoryItem, closeButton);
        });

        craftButtonGold.onClick.RemoveAllListeners();
        craftButtonGold.onClick.AddListener(() =>
        {
            //SetItemCraftingTime(craftIndex);
            Player.localPlayer.playerModularBuilding.CmdAddToCraft(runtimeBuildCraftItem, 0, craftAccessory.netIdentity, craftIndex, Convert.ToInt32(slider.value));
        });

        craftButtonCoin.onClick.RemoveAllListeners();
        craftButtonCoin.onClick.AddListener(() =>
        {
            //SetItemCraftingTime(craftIndex);
            Player.localPlayer.playerModularBuilding.CmdAddToCraft(runtimeBuildCraftItem, 1, craftAccessory.netIdentity, craftIndex, Convert.ToInt32(slider.value));
        });

        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(delegate { Craft(craftIndex); });

        if (craftAccessory) titleText.text = craftAccessory.craftingAccessoryItem.name;


        closeButton.image.enabled = true;
        player = Player.localPlayer;
        closeButton.image.raycastTarget = true;
        panel.gameObject.SetActive(true);
        SpawnCraftAtBegins(false, false);
        SpawnEndCraftAtBegins();
    }

    public void SpawnCraftAtBegins(bool close, bool ignore)
    {
        crafts = new List<ItemCrafting>();
        crafts = player.playerCharacterCreation.sex == 1 && craftAccessory.craftingAccessoryItem.itemtoCraftFemale.Count > 0 ? craftAccessory.craftingAccessoryItem.itemtoCraftFemale : craftAccessory.craftingAccessoryItem.itemtoCraft;
        UIUtils.BalancePrefabs(newSlot, crafts.Count, content);
        for (int i = 0; i < crafts.Count; i++)
        {
            int index = i;
            BuyBoostSlot slot = content.GetChild(index).GetComponent<BuyBoostSlot>();


            slot.boostImage.sprite = crafts[index].itemAndAmount.item.image;
            slot.boostImage.preserveAspect = true;
            slot.title.text = crafts[index].itemAndAmount.item.name;
            slot.coinImage.gameObject.SetActive(false);
            slot.coins.gameObject.SetActive(false);

            slot.boostButton.onClick.RemoveAllListeners();
            slot.boostButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                SetupForButton(index);
            });
        }
    }

    public void SpawnEndCraftAtBegins()
    {
        UIUtils.BalancePrefabs(craftSlot, craftAccessory.craftingItem.Count, contentFinished);
        for (int i = 0; i < craftAccessory.craftingItem.Count; i++)
        {
            int index = i;
            UICraftSlot slot = contentFinished.GetChild(i).GetComponent<UICraftSlot>();
            if (ScriptableItem.All.TryGetValue(craftAccessory.craftingItem[index].item.GetStableHashCode(), out ScriptableItem itemData))
            {
                slot.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, StatsManager.singleton.closeSize);
                slot.coinButton.gameObject.SetActive(false);
                slot.goldButton.gameObject.SetActive(false);
                slot.itemImage.sprite = itemData.image;
                slot.itemImage.preserveAspect = true;
                slot.itemName.text = craftAccessory.craftingItem[index].item;
                slot.itemAmountOverlay.gameObject.SetActive(craftAccessory.craftingItem[index].amount > 1);
                slot.itemAmount.text = craftAccessory.craftingItem[index].amount.ToString();
                //difference = DateTime.Parse(craftAccessory.craftingItem[index].timeEnd) - DateTime.UtcNow;
                secondsRemaining = TemperatureManager.singleton.CalculateSecondsRemaining(TemperatureManager.singleton.Totaldays, TemperatureManager.singleton.hours, TemperatureManager.singleton.minutes, TemperatureManager.singleton.seconds,
                                                                                          craftAccessory.craftingItem[index].totalDays, craftAccessory.craftingItem[index].hours, craftAccessory.craftingItem[index].minutes, craftAccessory.craftingItem[index].seconds);
                slot.claimButton.gameObject.SetActive(secondsRemaining <= 0);
                slot.timer.text = secondsRemaining <= 0 ? string.Empty : Utilities.ConvertToTimerLong(Convert.ToInt64(secondsRemaining));
                slot.scrollView.SetActive(false);
                slot.claimButton.onClick.RemoveAllListeners();
                slot.claimButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.playerModularBuilding.CmdClaimCraftedItem(index, craftAccessory.netIdentity);
                });
            }
        }
    }

    public void UpdateEndingItem()
    {
        if (Player.localPlayer && panel.activeInHierarchy)
        {
            for (int i = 0; i < contentFinished.childCount; i++)
            {
                int index = i;
                UICraftSlot slot = contentFinished.GetChild(index).GetComponent<UICraftSlot>();
                secondsRemaining = TemperatureManager.singleton.CalculateSecondsRemaining(TemperatureManager.singleton.Totaldays, TemperatureManager.singleton.hours, TemperatureManager.singleton.minutes, TemperatureManager.singleton.seconds,
                                                                                          craftAccessory.craftingItem[index].totalDays, craftAccessory.craftingItem[index].hours, craftAccessory.craftingItem[index].minutes, craftAccessory.craftingItem[index].seconds);
                slot.claimButton.gameObject.SetActive(secondsRemaining <= 0);
                slot.timer.text = secondsRemaining <= 0 ? string.Empty : Utilities.ConvertToTimerLong(Convert.ToInt64(secondsRemaining));
            }
        }
    }

    public void Close()
    {
        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
        panel.gameObject.SetActive(false);
        closeButton.image.raycastTarget = false;
        firstSelected = -1;
        craftIndex = -1;
        slider.value = 1;
        craftButtonGold.interactable = true;
        craftButtonCoin.interactable = true;
        craftPanel.SetActive(false);
        //UIUtils.BalancePrefabs(craftSlotChild, 0, ingredientCraftContent);
        goldText.text = string.Empty;
        coinText.text = string.Empty;
        closeButton.image.enabled = false;
        RemovePlayerFromBuildingAccessory(craftAccessory.netIdentity);
        BlurManager.singleton.Show();
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeath.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeath.Add(this);
    }

    public void RemovePlayerFromBuildingAccessory(NetworkIdentity identity)
    {
        Player.localPlayer.playerModularBuilding.CmdRemovePlayerInteractWithAccessory(identity);
    }
}
