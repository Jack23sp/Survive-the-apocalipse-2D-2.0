using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial struct Item
{
    public int waterContainer;
    public int gasContainer;
    public int honeyContainer;
}

public partial class Player
{
    [HideInInspector] public PlayerThirsty playerThirsty;
}

public partial class Database
{
    class thirsty
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentThirsty { get; set; }
    }


    public void Connect_Thirsty()
    {
        connection.CreateTable<thirsty>();
    }

    public void SaveThirsty(Player player)
    {
        PlayerThirsty thirsty = player.GetComponent<PlayerThirsty>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM thirsty WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new thirsty
        {
            characterName = player.name,
            currentThirsty = thirsty.current
        });

    }
    public void LoadThirsty(Player player)
    {
        PlayerThirsty thirsty = player.GetComponent<PlayerThirsty>();

        foreach (thirsty row in connection.Query<thirsty>("SELECT * FROM thirsty WHERE characterName=?", player.name))
        {
            thirsty.current = row.currentThirsty;
        }

    }

}

public class PlayerThirsty : NetworkBehaviour
{
    Player player;
    [SyncVar(hook = (nameof(ManageThirsty)))]
    public int current = 100;
    [HideInInspector] public int max = 100;
    float cycleAmount = 60.0f;
    [HideInInspector] public int healthToRemove = 5;
    UIStatSlot thirstySlot;


    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerThirsty = this;
        max = 100;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        ManageThirsty(current, current);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        cycleAmount = CoroutineManager.singleton.thirstyInvoke;
        InvokeRepeating(nameof(DecreaseThirsty), cycleAmount, cycleAmount);
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
                    if (UIFrontStats.singleton.stats[i].thirsty)
                    {
                        thirstySlot = UIFrontStats.singleton.stats[i];
                        thirstySlot.gameObject.SetActive(true);
                        thirstySlot.image.sprite = ImageManager.singleton.thirstyImage;
                        //thirstySlot.SpawnRestAmount(newValue >= oldValue ? newValue - oldValue : newValue - oldValue);
                        thirstySlot.amount.text = current + " / 100";
                        thirstySlot.intAmount = current;
                    }
                }
            }
        }
    }

    public void ManageThirsty(int oldValue, int newValue)
    {
        Assign();
        if (player && !player.isLocalPlayer) return;

        if (UIKitchenSink.singleton.panel.activeInHierarchy)
        {
            UIKitchenSink.singleton.SetThirstyValue();
            UIKitchenSink.singleton.Refresh();
        }
        if (UIBathroomSink.singleton.panel.activeInHierarchy)
        {
            UIBathroomSink.singleton.SetThirstyValue();
            UIBathroomSink.singleton.Refresh();
        }
        if (UIWaterContainer.singleton.panel.activeInHierarchy)
        {
            UIWaterContainer.singleton.SetThirstyValue();
            UIWaterContainer.singleton.Refresh();
        }
        if (UIFrontStats.singleton.panel.activeInHierarchy)
        {
            for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
            {
                if (UIFrontStats.singleton.stats[i].thirsty)
                {
                    thirstySlot = UIFrontStats.singleton.stats[i];
                    thirstySlot.gameObject.SetActive(true);
                    thirstySlot.image.sprite = ImageManager.singleton.thirstyImage;
                    thirstySlot.SpawnRestAmount(newValue >= oldValue ? newValue - oldValue : newValue - oldValue);
                    thirstySlot.amount.text = newValue + " / 100";
                    thirstySlot.intAmount = newValue;
                }
            }
        }
    }

    [Command]
    public void CmdFillBottle(int index, NetworkIdentity aquiferIdentity)
    {
        Aquifer aquifer = aquiferIdentity.GetComponent<Aquifer>();
        int waterInAquifer = aquifer != null ? aquifer.actualWater : -1;
        if (player.inventory.slots[index].amount > 0 && player.inventory.slots[index].item.data is WaterBottleItem && waterInAquifer > 0)
        {
            if (player.inventory.slots[index].item.CanAddWater())
            {
                int waterToAdd = waterInAquifer < 10 ? waterInAquifer : 10;
                int waterDiff = ((WaterBottleItem)player.inventory.slots[index].item.data).maxWater - player.inventory.slots[index].item.waterContainer;
                if (waterDiff < waterToAdd)
                {
                    waterToAdd = waterDiff;
                }

                ItemSlot slot = player.inventory.slots[index];
                slot.item.waterContainer += waterToAdd;
                aquifer.actualWater -= waterToAdd;
                player.inventory.slots[index] = slot;
            }
        }
    }

    [Command]
    public void CmdFillBottleFromContainer(int index, NetworkIdentity aquiferIdentity)
    {
        WaterContainer aquifer = aquiferIdentity.GetComponent<WaterContainer>();
        int waterInAquifer = aquifer != null ? aquifer.water : -1;
        if (player.inventory.slots[index].amount > 0 && player.inventory.slots[index].item.data is WaterBottleItem && waterInAquifer > 0)
        {
            if (player.inventory.slots[index].item.CanAddWater())
            {
                int waterToAdd = waterInAquifer < 10 ? waterInAquifer : 10;
                int waterDiff = ((WaterBottleItem)player.inventory.slots[index].item.data).maxWater - player.inventory.slots[index].item.waterContainer;
                if (waterDiff < waterToAdd)
                {
                    waterToAdd = waterDiff;
                }

                ItemSlot slot = player.inventory.slots[index];
                slot.item.waterContainer += waterToAdd;
                aquifer.water -= waterToAdd;
                player.inventory.slots[index] = slot;
            }
        }
    }


    [Command]
    public void CmdDrinkWater(int amount, int inventoryIndex, bool isInventory)
    {
        ItemSlot slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];
        if (amount <= slot.item.waterContainer && slot.item.CanAddWater())
        {
            int diff = Mathf.Min(max - current,amount);
            current += diff;
            slot.item.waterContainer -= diff;
            if (isInventory) player.inventory.slots[inventoryIndex] = slot;
            else player.playerBelt.belt[inventoryIndex] = slot;

            if (slot.item.data is WaterBottleItem)
            {
                ((WaterBottleItem)slot.item.data).SetAnimation(player);
            }
            TargetRefreshSelectedItemSlider();
        }
    }

    [Command]
    public void CmdDrinkHoney(int amount, int inventoryIndex, bool isInventory)
    {
        ItemSlot slot = isInventory ? player.inventory.slots[inventoryIndex] : player.playerBelt.belt[inventoryIndex];
        if (amount <= slot.item.honeyContainer && slot.item.CanAddHoney())
        {
            int diff = Mathf.Min(max - current, amount);
            current += diff;
            slot.item.honeyContainer -= amount;
            if (isInventory) player.inventory.slots[inventoryIndex] = slot;
            else player.playerBelt.belt[inventoryIndex] = slot;
            TargetRefreshSelectedItemSlider();
        }
    }


    [TargetRpc]
    public void TargetRefreshSelectedItemSlider()
    {
        if (UISelectedItem.singleton)
        {
            UISelectedItem.singleton.slider.value = 0;
            UISelectedItem.singleton.sliderValue.text = "0";
            UISelectedItem.singleton.sliderValue.color = Color.black;
        }
    }

    [Command]
    public void CmdAddWater(int amount, NetworkIdentity aquifer)
    {
        int remaining = max - current;
        if (amount > remaining) return;
        else
        {
            Aquifer aquiferObject = aquifer.GetComponent<Aquifer>();
            if (aquiferObject.actualWater >= amount)
            {
                aquiferObject.actualWater -= amount;
                current += amount;
            }
            else
            {
                current += aquiferObject.actualWater;
                aquiferObject.actualWater = 0;
            }
        }
    }

    [Command]
    public void CmdAddWaterFromContainer(int amount, NetworkIdentity aquifer)
    {
        int remaining = max - current;
        if (amount > remaining) return;
        else
        {
            WaterContainer aquiferObject = aquifer.GetComponent<WaterContainer>();
            if (aquiferObject.water >= amount)
            {
                aquiferObject.water -= amount;
                current += amount;
            }
            else
            {
                current += aquiferObject.water;
                aquiferObject.water = 0;
            }
        }
    }



    public void DecreaseThirsty()
    {
        Assign();
        if (player.playerThirsty.current > 0) player.playerThirsty.current--;
        if (player.playerThirsty.current <= 0) player.health.current -= healthToRemove;
        if (player.health.current <= 0) player.health.current = 0;
    }

}
