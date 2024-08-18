using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class UISelectedIBuyItem : MonoBehaviour
{
    public static UISelectedIBuyItem singleton;
    public GameObject panel;
    public Transform childContent;
    public GameObject selectedItemChildObject; // Shop Child
    public Slider slider;
    public TextMeshProUGUI sliderValue;
    public TextMeshProUGUI currency;
    public TextMeshProUGUI description;
    public Image currencyImage;
    public Button buyButton;
    public Button closeButton;
    public TextMeshProUGUI itemName;
    public Image itemImage;
    public TextMeshProUGUI itemAmount;
    public PremiumBuy selectedItem = new PremiumBuy();
    public int categoryIndex;
    public int selectedIndex;
    public bool gold;
    public Button switchCurrency;
    public Image switchCurrencyAlternativePayment;
    public GameObject alertObject;
    public TextMeshProUGUI alertText;
    public bool premium;
    public GameObject panelOne;
    public GameObject panelTwo;
    public GameObject panelThree;


    void Start()
    {
        if (!singleton) singleton = this;

        slider.onValueChanged.AddListener(delegate { ChangeAspect(false); });

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            if(!premium) 
                Player.localPlayer.CmdAddItemMallItem(categoryIndex, selectedIndex, "", Convert.ToInt32(gold), Convert.ToInt32(slider.value));
            else
                Player.localPlayer.CmdAddItemMallItem(0, 0, ItemMallManager.singleton.premiumShopObject[selectedIndex].items[0].shopCode, 0, 1);
            closeButton.onClick.Invoke();
        });

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            gold = false;
            //categoryIndex = -1;
            //selectedIndex = -1;
            panel.SetActive(false);
            slider.value = 1;
            sliderValue.text = "1";
            currency.text = String.Empty;
            itemName.text = String.Empty;
            closeButton.image.raycastTarget = false;
            switchCurrencyAlternativePayment.sprite = ImageManager.singleton.coin;
            alertObject.SetActive(false);
            premium = false;
            closeButton.image.enabled = false;
            BlurManager.singleton.Show();
        });

        switchCurrency.onClick.RemoveAllListeners();
        switchCurrency.onClick.AddListener(() =>
        {
            gold = !gold;
            sliderValue.text = "1";
            slider.value = 1;
            ChangeAspect(false);
        });
    }

    public void ChangeAspect(bool prem)
    {
        slider.gameObject.SetActive(!prem);
        switchCurrency.gameObject.SetActive(!prem);


        sliderValue.text = Convert.ToInt32(slider.value).ToString();
        if (!prem)
        {
            description.text = string.Empty;
            description.text = new ItemSlot(new Item(selectedItem.items[selectedIndex].item), 1).ToolTip();
            //Debug.Log("Type : " + selectedItem.items[selectedIndex].item.GetType().ToString());
            panelOne.SetActive(true);
            panelTwo.SetActive(true);
            panelThree.SetActive(true);
            currencyImage.gameObject.SetActive(true);
            currency.text = gold ? (selectedItem.items[selectedIndex].gold * Convert.ToInt32(slider.value)).ToString() : (selectedItem.items[selectedIndex].coin * Convert.ToInt32(slider.value)).ToString();
            itemName.text = selectedItem.items[selectedIndex].item.name;
            itemImage.sprite = selectedItem.items[selectedIndex].item.image;
            itemImage.preserveAspect = true;
            itemAmount.text = (selectedItem.items[selectedIndex].amount * Convert.ToInt32(slider.value)).ToString();
            buyButton.interactable = true;
        }
        else
        {
            description.text = string.Empty;
            panelOne.SetActive(true);
            panelTwo.SetActive(false);
            panelThree.SetActive(true);
            PremiumBuy slot = ItemMallManager.singleton.premiumShopObject[selectedIndex];
            currencyImage.gameObject.SetActive(false);
            currency.text = UIShop.singleton.premiumContent.GetChild(selectedIndex).GetComponent<ShopItem>().price.text;
            itemName.text = slot.items[0].item.name;
            itemImage.sprite = slot.items[0].item.image;
            itemImage.preserveAspect = true;
            itemAmount.text = slot.items[0].coin.ToString();
            buyButton.interactable = true;
        }

        alertObject.gameObject.SetActive(false);

        if (!prem)
        {
            Player.localPlayer.mallItems.Clear();
            Player.localPlayer.mallItems = Player.localPlayer.inventory.slots.ToList();
            if (selectedItem.items[selectedIndex].items.Count > 0)
            {

                for (int i = 0; i < selectedItem.items[selectedIndex].items.Count; i++)
                {
                    if (!Player.localPlayer.AddItemMallObject(new Item(selectedItem.items[selectedIndex].items[i].item), selectedItem.items[selectedIndex].items[i].amount))
                    {
                        buyButton.interactable = false;
                        alertObject.gameObject.SetActive(true);
                        alertText.text = ItemMallManager.singleton.inventoryMessage;
                    }
                }


                //for (int i = 0; i < selectedItem.items[selectedIndex].items.Count; i++)
                //{
                //    if (!Player.localPlayevr.inventory.CanAdd(new Item(selectedItem.items[selectedIndex].items[i].item), selectedItem.items[selectedIndex].items[i].amount))
                //    {
                //        buyButton.interactable = false;
                //        alertObject.gameObject.SetActive(true);
                //        alertText.text = ItemMallManager.singleton.inventoryMessage;
                //    }
                //}
            }
            //else
            //{
            //    if (!Player.localPlayer.inventory.CanAdd(new Item(selectedItem.items[selectedIndex].item), selectedItem.items[selectedIndex].amount))
            //    {
            //        buyButton.interactable = false;
            //        alertObject.gameObject.SetActive(true);
            //        alertText.text = ItemMallManager.singleton.inventoryMessage;
            //    }
            //}

            if (gold && Player.localPlayer.gold < (selectedItem.items[selectedIndex].gold * Convert.ToInt32(slider.value)))
            {
                alertObject.SetActive(true);
                alertText.text = ItemMallManager.singleton.currencyMessage;
            }

            if (!gold && Player.localPlayer.itemMall.coins < (selectedItem.items[selectedIndex].coin * Convert.ToInt32(slider.value)))
            {
                alertObject.SetActive(true);
                alertText.text = ItemMallManager.singleton.currencyMessage;
            }

        }
        else
        {
            alertObject.SetActive(false);
        }

        currencyImage.sprite = gold ? ImageManager.singleton.gold : ImageManager.singleton.coin;
        switchCurrencyAlternativePayment.sprite = gold ? ImageManager.singleton.gold : ImageManager.singleton.coin;
        switchCurrencyAlternativePayment.preserveAspect = true;
        currencyImage.preserveAspect = true;
        SpawnObject();
    }

    public void Open(bool prem)
    {
        premium = prem;
        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;
        buyButton.interactable = false;
        panel.SetActive(true);
        ChangeAspect(prem);
    }

    public void SpawnObject()
    {
        if (selectedItem.items[selectedIndex].items.Count > 0)
        {
            UIUtils.BalancePrefabs(selectedItemChildObject, selectedItem.items[selectedIndex].items.Count, childContent);
            for (int i = 0; i < selectedItem.items[selectedIndex].items.Count; i++)
            {
                ChildShopItem slot = childContent.GetChild(i).GetComponent<ChildShopItem>();
                slot.itemImage.sprite = selectedItem.items[selectedIndex].items[i].item.image;
                slot.itemImage.preserveAspect = true;
                slot.itemAmount.text = (selectedItem.items[selectedIndex].items[i].amount * Convert.ToInt32(slider.value)).ToString();
                slot.itemName.text = selectedItem.items[selectedIndex].items[i].item.name;
            }
        }
        else
        {
            UIUtils.BalancePrefabs(selectedItemChildObject, 1, childContent);
            ChildShopItem slot = childContent.GetChild(0).GetComponent<ChildShopItem>();
            slot.itemImage.sprite = selectedItem.items[selectedIndex].item.image;
            slot.itemImage.preserveAspect = true;
            slot.itemAmount.text = (1 * Convert.ToInt32(slider.value)).ToString();
            slot.itemName.text = selectedItem.items[selectedIndex].item.name;

        }

        if(premium)
        {
            UIUtils.BalancePrefabs(selectedItemChildObject, 0, childContent);
        }
    }
}
