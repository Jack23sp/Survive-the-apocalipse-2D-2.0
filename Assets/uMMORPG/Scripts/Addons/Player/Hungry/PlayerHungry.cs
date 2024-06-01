using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerHungry playerHungry;
}

public partial class Database
{
    class hungry
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentHungry { get; set; }
    }

    public void Connect_Hungry()
    {
        connection.CreateTable<hungry>();
    }

    public void SaveHungry(Player player)
    {
        PlayerHungry hungry = player.GetComponent<PlayerHungry>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM hungry WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new hungry
        {
            characterName = player.name,
            currentHungry = hungry.current
        });

    }
    public void LoadHungry(Player player)
    {
        PlayerHungry hungry = player.GetComponent<PlayerHungry>();

        foreach (hungry row in connection.Query<hungry>("SELECT * FROM hungry WHERE characterName=?", player.name))
        {
            hungry.current = row.currentHungry;
        }
    }
}

public class PlayerHungry : NetworkBehaviour
{
    Player player;
    [SyncVar(hook = (nameof(ManageHungry)))]
    public int current;
    [HideInInspector] public int max = 100;
    float cycleAmount = 60.0f;
    [HideInInspector] public int healthToRemove = 5;
    UIStatSlot hungrySlot;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerHungry = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        ManageHungry(current, current);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        cycleAmount = CoroutineManager.singleton.hungryInvoke;
        InvokeRepeating(nameof(DecreaseHungry), cycleAmount, cycleAmount);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Assign();
        if (UIFrontStats.singleton)
        {
            UIFrontStats.singleton.panel.SetActive(true);
            if (UIFrontStats.singleton.panel.activeInHierarchy)
            {
                for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
                {
                    if (UIFrontStats.singleton.stats[i].hungry)
                    {
                        hungrySlot = UIFrontStats.singleton.stats[i];
                        hungrySlot.gameObject.SetActive(true);
                        hungrySlot.image.sprite = ImageManager.singleton.hungryImage;
                        //hungrySlot.SpawnRestAmount(newValue >= oldValue ? newValue - oldValue : newValue - oldValue);
                        hungrySlot.amount.text = current + " / 100";
                        hungrySlot.intAmount = current;
                    }
                }
            }
        }
    }

    public void ManageHungry(int oldValue, int newValue)
    {
        Assign();
        if (!player.isLocalPlayer) return;

        if (UIKitchenSink.singleton) UIKitchenSink.singleton.SetHungryValue();
        if (UIBathroomSink.singleton) UIBathroomSink.singleton.SetHungryValue();
        if (UIWaterContainer.singleton) UIWaterContainer.singleton.SetHungryValue();
        if (UIFrontStats.singleton)
        {
            if (UIFrontStats.singleton.panel.activeInHierarchy)
            {
                for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
                {
                    if (UIFrontStats.singleton.stats[i].hungry)
                    {
                        hungrySlot = UIFrontStats.singleton.stats[i];
                        hungrySlot.gameObject.SetActive(true);
                        hungrySlot.image.sprite = ImageManager.singleton.hungryImage;
                        hungrySlot.SpawnRestAmount(newValue >= oldValue ? newValue - oldValue : newValue - oldValue);
                        hungrySlot.amount.text = newValue + " / 100";
                        hungrySlot.intAmount = newValue;
                    }
                }
            }
        }
    }

    public void DecreaseHungry()
    {
        Assign();
        if (player.playerHungry.current > 0) player.playerHungry.current--;
        if (player.playerHungry.current <= 0) player.health.current -= healthToRemove;
        if (player.health.current <= 0) player.health.current = 0;
    }

}
