using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;

public partial class Player
{
    [Command]
    public void CmdAddToWarehouse(int inventorySlots, int warehouseSlot, NetworkIdentity identity)
    {
        Warehouse warehouse = identity.GetComponent<Warehouse>();
        if (inventorySlots > -1)
        {
            if (inventory.slots[inventorySlots].amount == 0) return;

            int add = warehouse.Add(inventory.slots[inventorySlots].item, inventory.slots[inventorySlots].amount);

            if (add == -10000)
                inventory.slots[inventorySlots] = new ItemSlot();
            else
            {
                ItemSlot slot = inventory.slots[inventorySlots];
                slot.amount = add;
                inventory.slots[inventorySlots] = slot;
            }
        }
        if (warehouseSlot > -1)
        {
            if (warehouse.slots[warehouseSlot].amount == 0) return;
            int add = inventory.AddNew(warehouse.slots[warehouseSlot].item, warehouse.slots[warehouseSlot].amount);

            if (add == -10000)
                warehouse.slots[warehouseSlot] = new ItemSlot();
            else
            {
                ItemSlot slot = warehouse.slots[warehouseSlot];
                slot.amount = add;
                warehouse.slots[warehouseSlot] = slot;
            }
        }
    }

}

public partial class Database
{
    class warehouse_item_accessories
    {
        public int buildingItem { get; set; }
        public int myIndex { get; set; }
        public string character { get; set; }
        public string accessoryName { get; set; }
        public int bulletsRemaining { get; set; }
        public int skin { get; set; }
    }

    class warehouse_item
    {
        public int myIndex { get; set; }
        public string character { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public float summonedHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; } // TODO does long work?
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int currentDurability { get; set; }
        public int skin { get; set; }
        public int accuracyLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int armorLevel { get; set; }
        public int bagLevel { get; set; }
        public int batteryLevel { get; set; }
        public int currentBlood { get; set; }
        public int gasContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public float weight { get; set; }
        public float wet { get; set; }
        public int bulletsRemaining { get; set; }
    }

    public void SaveWarehouse(int index)
    {
        for (int i = 0; i < ((Warehouse)ModularBuildingManager.singleton.buildingAccessories[index]).slots.Count; i++)
        {
            ItemSlot slot = ((Warehouse)ModularBuildingManager.singleton.buildingAccessories[index]).slots[i];
            if (slot.amount > 0)
            {
                connection.InsertOrReplace(new warehouse_item
                {
                    myIndex = index,
                    character = "",
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    summonedHealth = slot.item.summonedHealth,
                    summonedLevel = slot.item.summonedLevel,
                    summonedExperience = slot.item.summonedExperience,
                    currentArmor = slot.item.currentArmor,
                    currentUnsanity = slot.item.currentUnsanity,
                    radioCurrentBattery = slot.item.radioCurrentBattery,
                    torchCurrentBattery = slot.item.torchCurrentBattery,
                    currentDurability = slot.item.currentDurability,
                    skin = slot.item.skin,
                    accuracyLevel = slot.item.accuracyLevel,
                    durabilityLevel = slot.item.durabilityLevel,
                    armorLevel = slot.item.armorLevel,
                    bagLevel = slot.item.bagLevel,
                    batteryLevel = slot.item.batteryLevel,
                    currentBlood = slot.item.currentBlood,
                    gasContainer = slot.item.gasContainer,
                    honeyContainer = slot.item.honeyContainer,
                    waterContainer = slot.item.waterContainer,
                    weight = slot.item.weight,
                    bulletsRemaining = slot.item.bulletsRemaining,
                    wet = slot.item.wet
                });

                if (slot.item.data is WeaponItem && slot.item.accessories.Length > 0)
                {
                    for (int y = 0; y < slot.item.accessories.Length; y++)
                    {
                        connection.InsertOrReplace(new warehouse_item_accessories
                        {
                            buildingItem = index,
                            myIndex = y,
                            character = "",
                            accessoryName = slot.item.accessories[y].name,
                            bulletsRemaining = slot.item.accessories[y].bulletsRemaining,
                            skin = slot.item.skin
                        });
                    }
                }
            }
        }
    }

    public void LoadWarehouse(int index, Warehouse warehouse)
    {
        for (int i = 0; i < warehouse.amount; i++)
            warehouse.slots.Add(new ItemSlot());

        foreach (warehouse_item row in connection.Query<warehouse_item>("SELECT * FROM warehouse_item WHERE myIndex=?", index))
        {
            if (ScriptableItem.All.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                item.summonedHealth = row.summonedHealth;
                item.summonedLevel = row.summonedLevel;
                item.summonedExperience = row.summonedExperience;
                item.currentArmor = row.currentArmor;
                item.currentUnsanity = row.currentUnsanity;
                item.radioCurrentBattery = row.radioCurrentBattery;
                item.torchCurrentBattery = row.torchCurrentBattery;
                item.currentDurability = row.currentDurability;
                item.skin = row.skin;
                item.accuracyLevel = row.accuracyLevel;
                item.durabilityLevel = row.durabilityLevel;
                item.armorLevel = row.armorLevel;
                item.bagLevel = row.bagLevel;
                item.batteryLevel = row.batteryLevel;
                item.currentBlood = row.currentBlood;
                item.gasContainer = row.gasContainer;
                item.honeyContainer = row.honeyContainer;
                item.waterContainer = row.waterContainer;
                item.weight = row.weight;
                item.wet = row.wet;

                int am = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM warehouse_item_accessories WHERE buildingItem=? AND myIndex=?", index,row.slot);
                item.accessories = new Item[am];
                int count = 0;
                foreach (warehouse_item_accessories row2 in connection.Query<warehouse_item_accessories>("SELECT * FROM warehouse_item_accessories WHERE buildingItem=? AND myIndex=?", index, row.slot))
                {
                    if (ScriptableItem.All.TryGetValue(row2.accessoryName.GetStableHashCode(), out ScriptableItem accessory))
                    {
                        Item item2 = new Item(accessory);
                        item2.bulletsRemaining = row2.bulletsRemaining;
                        item.accessories[count] = item2;
                        count++;
                    }
                }

                warehouse.slots[row.slot] = new ItemSlot(item, row.amount);
            }
        }
    }

}

public class Warehouse : BuildingAccessory
{
    public SyncList<ItemSlot> slots = new SyncList<ItemSlot>();
    public int amount;

    public List<GameObject> warehouseNameObjects = new List<GameObject>(); 
    public TextMeshProUGUI warehouseNameText;

    public readonly SyncList<string> playerThatInteractWhitThis = new SyncList<string>();

    private Player plInteractCheck;


    new void Start()
    {
        base.Start();

        if (isServer)
        {
            if (slots.Count == 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    slots.Add(new ItemSlot());
                }
            }
            Invoke(nameof(CheckUnsanity), CoroutineManager.singleton.unsanityInvoke * 3);
        }
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.warehouses.Contains(this)) ModularBuildingManager.singleton.warehouses.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.warehouses.Contains(this)) ModularBuildingManager.singleton.warehouses.Remove(this);
        }
    }

    public new void ManageName (string oldvalue, string newValue)
    {
        if (playerThatInteractWhitThis.Count == 0)
        {
            for (int i = 0; i < warehouseNameObjects.Count; i++)
            {
                warehouseNameObjects[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < warehouseNameObjects.Count; i++)
            {
                warehouseNameObjects[i].SetActive(newValue != string.Empty);
            }
        }

        if(newValue != string.Empty)
        {
            warehouseNameText.text = newName;
        }
        else
        {
            warehouseNameText.text = string.Empty;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerThatInteractWhitThis.Callback += PlayerInteraction;
        slots.Callback += OnWarehouseInventoryChanged;
        ManageName(base.newName, base.newName);
        for (int i = 0; i < warehouseNameObjects.Count; i++)
        {
            warehouseNameObjects[i].SetActive(newName != string.Empty);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Invoke(nameof(CheckPlayer), 3.0f);
    }
    public void CheckPlayer()
    {
        if (netIdentity.observers != null && netIdentity.observers.Count > 0)
        {
            for (int i = playerThatInteractWhitThis.Count - 1; i >= 0; i--)
            {
                if (!Player.onlinePlayers.TryGetValue(playerThatInteractWhitThis[i], out plInteractCheck))
                {
                    playerThatInteractWhitThis.RemoveAt(i);
                }
            }
        }
        Invoke(nameof(CheckPlayer), 3.0f);
    }

    public override void AddPlayerThatAreInteract(string playerName)
    {
        base.AddPlayerThatAreInteract(playerName);
        if (!playerThatInteractWhitThis.Contains(playerName)) playerThatInteractWhitThis.Add(playerName);
    }

    public override void RemovePlayerThatAreInteract(string playerName)
    {
        base.RemovePlayerThatAreInteract(playerName);
        if (playerThatInteractWhitThis.Contains(playerName)) playerThatInteractWhitThis.Remove(playerName);
    }


    void PlayerInteraction(SyncList<string>.Operation op, int index, string oldSlot, string newSlot)
    {
        if (playerThatInteractWhitThis.Count == 0)
        {
            renderer.sprite = ImageManager.singleton.warehouseClosed[oldPositioning];
            for (int i = 0; i < warehouseNameObjects.Count; i++)
            {
                warehouseNameObjects[i].SetActive(true);
            }
        }
        else
        {
            renderer.sprite = ImageManager.singleton.warehouseOpen[oldPositioning];
            for (int i = 0; i < warehouseNameObjects.Count; i++)
            {
                warehouseNameObjects[i].SetActive(false);
            }
        }
    }


    void OnWarehouseInventoryChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        //if (!(op == SyncList<ItemSlot>.Operation.OP_ADD))
        //{
            if (UIWarehouse.singleton && UIWarehouse.singleton.panel.activeInHierarchy)
            {
                if (ModularBuildingManager.singleton.buildingAccessory && UIWarehouse.singleton.warehouse && UIWarehouse.singleton.panel.activeInHierarchy)
                {
                    UIWarehouse.singleton.Open(UIWarehouse.singleton.warehouse, UIWarehouse.singleton.isReadOnly);
                }
            }
        //}
    }
    public void CheckUnsanity()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i;
            if (slots[index].amount > 0)
            {
                ItemSlot slot = slots[index];
                slot.item.currentUnsanity--;
                if (slot.item.currentUnsanity <= 0)
                {
                    slots[index] = new ItemSlot();
                }
                else
                {
                    slots[index] = slot;
                }
            }
        }
        Invoke(nameof(CheckUnsanity), CoroutineManager.singleton.unsanityInvoke * 3);
    }

    public int SlotsFree()
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int free = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount == 0)
                ++free;
        return free;
    }

    // helper function to calculate the occupied slots
    public int SlotsOccupied()
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int occupied = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0)
                ++occupied;
        return occupied;
    }

    // helper function to calculate the total amount of an item type in inventory
    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
    public int Count(Item item)
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int amount = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.name == item.name)
                amount += slot.amount;
        return amount;
    }

    // helper function to remove 'n' items from the inventory
    public bool Remove(Item item, int amount)
    {
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (slot.amount > 0 && slots[i].item.name == item.name)
            {
                // take as many as possible
                amount -= slot.DecreaseAmount(amount);
                slots[i] = slot;

                // are we done?
                if (amount == 0) return true;
            }
        }

        // if we got here, then we didn't remove enough items
        return false;
    }

    // helper function to check if the inventory has space for 'n' items of type
    // -> the easiest solution would be to check for enough free item slots
    // -> it's better to try to add it onto existing stacks of the same type
    //    first though
    // -> it could easily take more than one slot too
    // note: this checks for one item type once. we can't use this function to
    //       check if we can add 10 potions and then 10 potions again (e.g. when
    //       doing player to player trading), because it will be the same result
    public bool CanAdd(Item item, int amount)
    {
        // go through each slot
        for (int i = 0; i < slots.Count; ++i)
        {
            // empty? then subtract maxstack
            if (slots[i].amount == 0)
                amount -= item.maxStack;
            // not empty. same type too? then subtract free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            else if (slots[i].item.name == item.name)
                amount -= (slots[i].item.maxStack - slots[i].amount);

            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }

        // if we got here than amount was never <= 0
        return false;
    }

    // helper function to put 'n' items of a type into the inventory, while
    // trying to put them onto existing item stacks first
    // -> this is better than always adding items to the first free slot
    // -> function will only add them if there is enough space for all of them
    public int Add(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        //if (CanAdd(item, amount))
        //{
        // add to same item stacks first (if any)
        // (otherwise we add to first empty even if there is an existing
        //  stack afterwards)
        for (int i = 0; i < slots.Count; ++i)
        {
            // not empty and same type? then add free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (slots[i].amount > 0 && slots[i].item.name == item.name)
            {
                ItemSlot temp = slots[i];
                amount -= temp.IncreaseAmount(amount);
                slots[i] = temp;
            }

            // were we able to fit the whole amount already? then stop loop
            if (amount <= 0) return -10000;
        }

        // add to empty slots (if any)
        for (int i = 0; i < slots.Count; ++i)
        {
            // empty? then fill slot with as many as possible
            if (slots[i].amount == 0)
            {
                int add = Mathf.Min(amount, item.maxStack);
                slots[i] = new ItemSlot(item, add);
                amount -= add;
            }

            // were we able to fit the whole amount already? then stop loop
            if (amount <= 0) return -10000;
        }
        // we should have been able to add all of them
        if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        //}
        return amount;

    }
}
