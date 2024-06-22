using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using DanielLochner.Assets.SimpleScrollSnap;
using System.Linq;

public partial class Player
{
    [HideInInspector] public List<ItemSlot> mallItems = new List<ItemSlot>();

    public bool CanAddItemMallObject(Item item, int amount)
    {
        // go through each slot
        for (int i = 0; i < mallItems.Count; ++i)
        {
            // empty? then subtract maxstack
            if (mallItems[i].amount == 0)
                amount -= item.maxStack;
            // not empty. same type too? then subtract free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            else if (mallItems[i].item.Equals(item))
                amount -= (mallItems[i].item.maxStack - mallItems[i].amount);

            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }

        // if we got here than amount was never <= 0
        return false;
    }


    public bool AddItemMallObject(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        if (CanAddItemMallObject(item, amount))
        {
            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing
            //  stack afterwards)
            for (int i = 0; i < mallItems.Count; ++i)
            {
                // not empty and same type? then add free amount (max-amount)
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (mallItems[i].amount > 0 && mallItems[i].item.Equals(item))
                {
                    ItemSlot temp = mallItems[i];
                    amount -= temp.IncreaseAmount(amount);
                    mallItems[i] = temp;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }

            // add to empty slots (if any)
            for (int i = 0; i < mallItems.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (mallItems[i].amount == 0)
                {
                    int add = Mathf.Min(amount, item.maxStack);
                    mallItems[i] = new ItemSlot(item, add);
                    amount -= add;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }
            // we should have been able to add all of them
            if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        }
        return false;
    }


    [Command]
    public void CmdAddItemMallItem(int categoryIndex,int itemMallIndex, string shopCode, int currencyUsed, int amount)
    {
        if(itemMallIndex > -1)
        {
            if(shopCode != string.Empty)
            {
                //Put here the stuff that has to been used for buy coins with real money
                playerNotification.TargetSpawnNotification("Premium buy");
            }
            else
            {
                mallItems.Clear();
                mallItems = inventory.slots.ToList();

                ItemMallChildItem premiumBuy = ItemMallManager.singleton.shopObject[categoryIndex].items[itemMallIndex];
                if(premiumBuy.items.Count > 0)
                {
                    bool canAdd = true;
                    for(int i = 0; i < premiumBuy.items.Count; i++)
                    {
                        if(!AddItemMallObject(new Item(premiumBuy.items[i].item), (premiumBuy.items[i].amount * amount)))
                        {
                            canAdd = false;
                        }
                    }

                    if(canAdd)
                    {
                        if ((currencyUsed == 1 && ((premiumBuy.gold * amount) == 0)) ||
                        (currencyUsed == 0 && ((premiumBuy.coin * amount) == 0)))
                        {
                            for (int i = 0; i < premiumBuy.items.Count; i++)
                            {
                                inventory.Add(new Item(premiumBuy.items[i].item), (premiumBuy.items[i].amount * amount));
                            }
                        }
                        else if ((currencyUsed == 1 && (gold < (premiumBuy.gold * amount) && premiumBuy.gold > 0)) ||
                           (currencyUsed == 0 && (itemMall.coins < (premiumBuy.coin * amount) && premiumBuy.coin > 0)))
                        {
                            playerNotification.TargetSpawnNotification("Insufficient founds.");
                        }
                        else
                        {
                            for (int i = 0; i < premiumBuy.items.Count; i++)
                            {
                                inventory.Add(new Item(premiumBuy.items[i].item), (premiumBuy.items[i].amount * amount));
                            }
                            if (currencyUsed == 1) gold -= (premiumBuy.gold * amount);
                            if (currencyUsed == 0) itemMall.coins -= (premiumBuy.coin * amount);
                        }
                    }
                    else
                    {
                        playerNotification.TargetSpawnNotification("Not enough space in inventory.");
                    }
                }
                else
                {
                    bool canAdd = true;
                    canAdd = inventory.CanAdd(new Item(premiumBuy.item), (premiumBuy.amount * amount));

                    if (canAdd)
                    {
                        if ((currencyUsed == 1 && ((premiumBuy.gold * amount) == 0)) ||
                        (currencyUsed == 0 && ((premiumBuy.coin * amount) == 0)))
                        {
                            for (int i = 0; i < premiumBuy.items.Count; i++)
                            {
                                inventory.Add(new Item(premiumBuy.items[i].item), (premiumBuy.items[i].amount * amount));
                            }
                        }
                        else if ((currencyUsed == 1 && (gold < (premiumBuy.gold * amount) && premiumBuy.gold > 0)) ||
                           (currencyUsed == 0 && (itemMall.coins < (premiumBuy.coin * amount) && premiumBuy.coin > 0)))
                        {
                            playerNotification.TargetSpawnNotification("Insufficient founds.");
                        }
                        else
                        {
                            inventory.Add(new Item(premiumBuy.item), (premiumBuy.amount * amount));
                            if (currencyUsed == 1) gold -= (premiumBuy.gold * amount);
                            if (currencyUsed == 0) itemMall.coins -= (premiumBuy.coin * amount);
                        }
                    }
                    else
                    {
                        playerNotification.TargetSpawnNotification("Not enough space in inventory.");
                    }
                }
            }
        }
    }
}

public class UIShop : MonoBehaviour
{
    public static UIShop singleton;
    public Transform premiumContent;
    public List<Transform> normalContent = new List<Transform>();
    public SimpleScrollSnap simpleScrollSnap;
    public ShopItem normalObjectToSpawn;
    public GameObject panel;
    public Button closeButton;
    public TextMeshProUGUI categoryText;
    public Button leftArrow;
    public Button rightArrow;
    public Button shopButton;
    public Button adsButton;
    public TextMeshProUGUI coinsForAdvsText;

    public int currentCategory;
    public int currentItem;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void Init()
    {
        for (int i = 0; i < premiumContent.childCount; i++)
        {
            int index_i = i;

            ShopItem slot = premiumContent.GetChild(index_i).GetComponent<ShopItem>();
            slot.itemName.text = ItemMallManager.singleton.premiumShopObject[index_i].items[0].item.name + " (" + ItemMallManager.singleton.premiumShopObject[index_i].items[0].coin + ")";
            slot.itemImage.sprite = ItemMallManager.singleton.premiumShopObject[index_i].items[0].item.image;
            slot.price.text = (2 * (index_i + 1)) + " $";
            slot.slotButton.onClick.RemoveAllListeners();
            slot.slotButton.onClick.AddListener(() =>
            {
                UISelectedIBuyItem.singleton.selectedIndex = index_i;
                UISelectedIBuyItem.singleton.Open(true);
            });
        }

        shopButton.onClick.RemoveAllListeners();
        shopButton.onClick.AddListener(() =>
        {
            Open();
        });

        coinsForAdvsText.text = "+" + ItemMallManager.singleton.coinsForAds.ToString();
        adsButton.onClick.RemoveAllListeners();
        adsButton.onClick.AddListener(() =>
        {
            Player.localPlayer.itemMall.CmdClickAds(DateTime.Now.ToString());
        });


        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            foreach(Transform t in normalContent)
            {
                t.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 1;
            }
            currentItem = 0;
            simpleScrollSnap.GoToPanel(0);
            premiumContent.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 1;
            closeButton.image.raycastTarget = false;
            panel.SetActive(false);
            closeButton.image.enabled = false;
            BlurManager.singleton.Show();
        });

        leftArrow.onClick.RemoveAllListeners();
        leftArrow.onClick.AddListener(() =>
        {
            currentCategory--;
            currentItem = 0;
            if (currentCategory < 0) currentCategory = 0;
            categoryText.text = ItemMallManager.singleton.shopObject[currentCategory].category.ToString();
        });

        rightArrow.onClick.RemoveAllListeners();
        rightArrow.onClick.AddListener(() =>
        {
            currentCategory++;
            currentItem = 0;
            if (currentCategory > ItemMallManager.singleton.shopObject.Count -1) currentCategory = ItemMallManager.singleton.shopObject.Count - 1;
            categoryText.text = ItemMallManager.singleton.shopObject[currentCategory].category.ToString();
        });
    }


    public void Open()
    {
        panel.SetActive(true);
        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;
        Spawn();
    }

    public void Spawn()
    {
        currentCategory = 0;
        categoryText.text = ItemMallManager.singleton.shopObject[0].category.ToString();
        for (int e = 0; e < normalContent.Count; e++)
        {
            int index_e = e;
            UIUtils.BalancePrefabs(normalObjectToSpawn.gameObject, ItemMallManager.singleton.shopObject[index_e].items.Count, normalContent[e]);
            for (int a = 0; a < ItemMallManager.singleton.shopObject[index_e].items.Count; a++)
            {
                int index_a = a;
                ShopItem slot = normalContent[index_e].GetChild(index_a).GetComponent<ShopItem>();
                if (ScriptableItem.All.TryGetValue(ItemMallManager.singleton.shopObject[index_e].items[index_a].item.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    slot.itemName.text = itemData.name + " ( " + ItemMallManager.singleton.shopObject[index_e].items[index_a].amount + " ) ";
                    slot.itemImage.sprite = itemData.image;
                    slot.itemImage.preserveAspect = true;
                    slot.coin.sprite = ImageManager.singleton.coin;
                    slot.gold.sprite = ImageManager.singleton.gold;
                    slot.coinText.text = ItemMallManager.singleton.shopObject[index_e].items[index_a].coin.ToString();
                    slot.goldText.text = ItemMallManager.singleton.shopObject[index_e].items[index_a].gold.ToString();

                    slot.slotButton.onClick.RemoveAllListeners();
                    slot.slotButton.onClick.AddListener(() =>
                    {
                        currentItem = index_a;

                        UISelectedIBuyItem.singleton.selectedItem = new PremiumBuy(ItemMallManager.singleton.shopObject[index_e]);
                        UISelectedIBuyItem.singleton.categoryIndex = currentCategory;
                        UISelectedIBuyItem.singleton.selectedIndex = currentItem;
                        UISelectedIBuyItem.singleton.gold = true;
                        UISelectedIBuyItem.singleton.Open(false);
                    });
                }
            }
        }

    }
}
