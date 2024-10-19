using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


[Serializable]
public partial struct ItemMallCategory
{
    public string category;
    public ScriptableItem[] items;
}

[Serializable]
public class ConsumableItem
{
    public string Name;
    public string Id;
    public string desc;
    public float price;
}

[Serializable]
public struct AdsConfig
{
    public string lastClick;
    public string lastClickServer;
    public int actualClick;

    public AdsConfig(string lastClickClient,string lastClickServer, int actualClick)
    {
        this.lastClick = lastClickClient;
        this.lastClickServer = lastClickServer;
        this.actualClick = actualClick;
    }
}

public partial class Database
{
    class Ads
    {
        public string characterName { get; set; }
        public string lastClickServer { get; set; }
        public string lastClick { get; set; }
        public int actualClick { get; set; }
    }

    public void SaveAds(Player player)
    {
        PlayerItemMall playerItemMall = player.GetComponent<PlayerItemMall>();

        connection.Execute("DELETE FROM Ads WHERE characterName=?", player.name);

        connection.InsertOrReplace(new Ads
        {
            characterName = player.name,
            lastClickServer = playerItemMall.adsConfig.lastClickServer,
            lastClick = playerItemMall.adsConfig.lastClick,
            actualClick = playerItemMall.adsConfig.actualClick
        });
    }

    public void LoadAds(Player player)
    {
        PlayerItemMall playerItemMall = player.GetComponent<PlayerItemMall>();

        foreach (Ads row in connection.Query<Ads>("SELECT * FROM Ads WHERE characterName=?", player.name))
        {
            playerItemMall.adsConfig = new AdsConfig()
            {
                lastClick = row.lastClick,
                lastClickServer = row.lastClickServer,
                actualClick = row.actualClick
            };
        }
    }
}

[RequireComponent(typeof(PlayerChat))]
[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerItemMall : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerChat chat;
    public PlayerInventory inventory;

    [Header("Item Mall")]
    public ScriptableItemMall config;
    [SyncVar(hook = (nameof(CheckCoin)))] public long coins = 0;
    public float couponWaitSeconds = 3;

    public int maxAmountOfDailyClick;
    public AdsConfig adsConfig;


    public override void OnStartServer()
    {
        InvokeRepeating(nameof(ProcessCoinOrders), 5, 5);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        UIShop.singleton.shopButton.gameObject.SetActive(true);
        UIShop.singleton.Init();
    }

    public override void OnStartClient()
    {
        if(adsConfig.lastClick == null) 
            adsConfig = new AdsConfig(string.Empty, string.Empty, 0);
        base.OnStartClient();
    }

    public void CheckCoin(long oldCoin, long newCoin)
    {
        if (!player.isLocalPlayer) return;
        if (UIFrontStats.singleton)
        {
            for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
            {
                if (UIFrontStats.singleton.stats[i].coin)
                {
                    UIFrontStats.singleton.stats[i].amount.text = newCoin.ToString();
                      UIFrontStats.singleton.stats[i].SpawnRestAmount((int)newCoin - (int)oldCoin);
                }
            }
        }
    }

    // item mall ///////////////////////////////////////////////////////////////
    [Command]
    public void CmdEnterCoupon(string coupon)
    {
        // only allow entering one coupon every few seconds to avoid brute force
        if (NetworkTime.time >= player.nextRiskyActionTime)
        {
            // YOUR COUPON VALIDATION CODE HERE
            // coins += ParseCoupon(coupon);
            Debug.Log("coupon: " + coupon + " => " + name + "@" + NetworkTime.time);
            player.nextRiskyActionTime = NetworkTime.time + couponWaitSeconds;
        }
    }

    [Command]
    public void CmdUnlockItem(int categoryIndex, int itemIndex)
    {
        // validate: only if alive so people can't buy resurrection potions
        // after dieing in a PvP fight etc.
        if (player.health.current > 0 &&
            0 <= categoryIndex && categoryIndex <= config.categories.Length &&
            0 <= itemIndex && itemIndex <= config.categories[categoryIndex].items.Length)
        {
            Item item = new Item(config.categories[categoryIndex].items[itemIndex]);
            if (0 < item.itemMallPrice && item.itemMallPrice <= coins)
            {
                // try to add it to the inventory, subtract costs from coins
                if (inventory.Add(item, 1))
                {
                    coins -= item.itemMallPrice;
                    Debug.Log(name + " unlocked " + item.name);

                    // NOTE: item mall purchases need to be persistent, yet
                    // resaving the player here is not necessary because if the
                    // server crashes before next save, then both the inventory
                    // and the coins will be reverted anyway.
                }
            }
        }
    }

    // coins can't be increased by an external application while the player is
    // ingame. we use an additional table to store new orders in and process
    // them every few seconds from here. this way we can even notify the player
    // after his order was processed successfully.
    //
    // note: the alternative is to keep player.coins in the database at all
    // times, but then we need RPCs and the client needs a .coins value anyway.
    [Server]
    void ProcessCoinOrders()
    {
        List<long> orders = Database.singleton.GrabCharacterOrders(name);
        foreach (long reward in orders)
        {
            coins += reward;
            Debug.Log("Processed order for: " + name + ";" + reward);
            chat.TargetMsgInfo("Processed order for: " + reward);
        }
    }

    [Command]
    public void CmdClickAds(string clientClick)
    {
        DateTime serverClick = DateTime.Parse(adsConfig.lastClickServer == string.Empty ? DateTime.UtcNow.ToString() : adsConfig.lastClickServer);
        DateTime tclientClik = DateTime.Parse(clientClick);
        //TimeSpan differenceClient;
        //TimeSpan differenceServer;
        //differenceClient = DateTime.Parse(adsConfig.lastClick == string.Empty ? clientClick : adsConfig.lastClick) - tclientClik;
        //differenceServer = serverClick - DateTime.UtcNow;

        if(adsConfig.actualClick < AdsManager.singleton.maxAmountOfDailyClick)
        {

            if (tclientClik.Day == DateTime.Parse(adsConfig.lastClick == string.Empty ? clientClick : adsConfig.lastClick).Day)
            {
                //adsConfig.actualClick++;
                //adsConfig.lastClickServer = DateTime.UtcNow.ToString();
                //adsConfig.lastClick = clientClick;
                //Target
                TargetAdsConfig();
            }
        }
        else
        {
            if (tclientClik.Day > DateTime.Parse(adsConfig.lastClick == string.Empty ? clientClick : adsConfig.lastClick).Day)
            {
                //adsConfig.actualClick = 1;
                //adsConfig.lastClickServer = DateTime.UtcNow.ToString();
                //adsConfig.lastClick = clientClick;
                //Target
                TargetAdsConfig();
            }
        }
    }

    [Command]
    public void CmdSetAdsConfig(string clientDate)
    {
        adsConfig.actualClick++;
        adsConfig.lastClickServer = DateTime.UtcNow.ToString();
        adsConfig.lastClick = clientDate;
        coins += ItemMallManager.singleton.coinsForAds;
    }

    [TargetRpc]
    public void TargetAdsConfig()
    {
         AdsManager.singleton.ShowAd();
    }
}
