using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public partial struct ItemMallChildItem
{
    public ScriptableItem item;
    public int amount;
    public string shopCode;
    public int coin;
    public int gold;
    public List<ChildFromMainItem> items;
}

[System.Serializable]
public partial struct ChildFromMainItem
{
    public ScriptableItem item;
    public int amount;
}


[System.Serializable]
public struct PremiumBuy
{
    public string category;
    public string shopCode;
    public List<ItemMallChildItem> items;

    public PremiumBuy (PremiumBuy buy)
    {
        category = buy.category;
        shopCode = buy.shopCode;
        items = buy.items;
    }
}

public class ItemMallManager : MonoBehaviour
{
    public static ItemMallManager singleton;

    public List<string> itemsCode;

    public List<PremiumBuy> premiumShopObject;
    public List<PremiumBuy> shopObject;
    
    [TextArea(5, 5)]
    public string inventoryMessage;
    [TextArea(5, 5)]
    public string currencyMessage;

    public long coinsForAds;

    void Start()
    {
        if (!singleton) singleton = this;
    }

}
