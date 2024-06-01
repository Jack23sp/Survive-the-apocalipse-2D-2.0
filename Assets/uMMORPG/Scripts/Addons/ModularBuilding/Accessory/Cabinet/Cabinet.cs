using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public partial class Player
{
    [Command]
    public void CmdAddToCabinet(int inventorySlots, int warehouseSlot, NetworkIdentity identity)
    {
        Cabinet cabinet = identity.GetComponent<Cabinet>();
        if (inventorySlots > -1)
        {
            if (inventory.slots[inventorySlots].amount == 0) return;

            //if (fridge.CanAdd(inventory.slots[inventorySlots].item, inventory.slots[inventorySlots].amount))
            //{
            int add = cabinet.Add(inventory.slots[inventorySlots].item, inventory.slots[inventorySlots].amount);

            if (add == -10000)
                inventory.slots[inventorySlots] = new ItemSlot();
            else
            {
                ItemSlot slot = inventory.slots[inventorySlots];
                slot.amount = add;
                inventory.slots[inventorySlots] = slot;
            }
            //}
        }
        if (warehouseSlot > -1)
        {
            if (cabinet.inventory[warehouseSlot].amount == 0) return;
            //if (inventory.CanAdd(fridge.slots[warehouseSlot].item, fridge.slots[warehouseSlot].amount))
            //{
            int add = inventory.AddNew(cabinet.inventory[warehouseSlot].item, cabinet.inventory[warehouseSlot].amount);

            if (add == 0)
                cabinet.inventory[warehouseSlot] = new ItemSlot();
            else
            {
                ItemSlot slot = cabinet.inventory[warehouseSlot];
                slot.amount = add;
                cabinet.inventory[warehouseSlot] = slot;
            }
            //}
        }
    }
}

public partial class Database
{
    class cabinet_item_accessories
    {
        public int buildingItem { get; set; }
        public int myIndex { get; set; }
        public string character { get; set; }
        public string accessoryName { get; set; }
        public int bulletsRemaining { get; set; }
        public int skin { get; set; }
    }

    class cabinet_item
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

    public void SaveCabinet(int index)
    {
        for (int i = 0; i < ((Cabinet)ModularBuildingManager.singleton.buildingAccessories[index]).inventory.Count; i++)
        {
            ItemSlot slot = ((Cabinet)ModularBuildingManager.singleton.buildingAccessories[index]).inventory[i];
            if (slot.amount > 0)
            {
                connection.InsertOrReplace(new cabinet_item
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
                        connection.InsertOrReplace(new cabinet_item_accessories
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

    public void LoadCabinet(int index, Cabinet cabinet)
    {
        for (int i = 0; i < cabinet.maxSlotAmount; i++)
            cabinet.inventory.Add(new ItemSlot());

        foreach (cabinet_item row in connection.Query<cabinet_item>("SELECT * FROM cabinet_item WHERE myIndex=?", index))
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

                int am = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM cabinet_item_accessories WHERE buildingItem=? AND myIndex=?", index, row.slot);
                item.accessories = new Item[am];
                int count = 0;
                foreach (cabinet_item_accessories row2 in connection.Query<cabinet_item_accessories>("SELECT * FROM cabinet_item_accessories WHERE buildingItem=? AND myIndex=?", index, row.slot))
                {
                    if (ScriptableItem.All.TryGetValue(row2.accessoryName.GetStableHashCode(), out ScriptableItem accessory))
                    {
                        Item item2 = new Item(accessory);
                        item2.bulletsRemaining = row2.bulletsRemaining;
                        item.accessories[count] = item2;
                        count++;
                    }
                }
                cabinet.inventory[row.slot] = new ItemSlot(item, row.amount);
            }
        }
    }
}

public class Cabinet : BuildingAccessory
{
    public SyncList<ItemSlot> inventory = new SyncList<ItemSlot>();

    public int maxSlotAmount = 25;

    public new void Start()
    {
        base.Start();
        if (isServer)
        {
            if (inventory.Count != maxSlotAmount)
            {
                for (int i = inventory.Count; i < maxSlotAmount; i++)
                {
                    inventory.Add(new ItemSlot());
                }
            }
        }
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.cabinets.Contains(this)) ModularBuildingManager.singleton.cabinets.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.cabinets.Contains(this)) ModularBuildingManager.singleton.cabinets.Remove(this);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        inventory.Callback += OnInventoryChanged;
    }

    public void OnInventoryChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if (!(op == SyncList<ItemSlot>.Operation.OP_ADD))
        {
            if (UICabinet.singleton)
            {
                if (ModularBuildingManager.singleton.buildingAccessory && UICabinet.singleton.cabinet && ModularBuildingManager.singleton.buildingAccessory.netId == UICabinet.singleton.cabinet.netId)
                {
                    UICabinet.singleton.Open((Cabinet)ModularBuildingManager.singleton.buildingAccessory);
                }
            }
        }
    }

    public int Add(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        //if (CanAdd(item, amount))
        //{
        // add to same item stacks first (if any)
        // (otherwise we add to first empty even if there is an existing
        //  stack afterwards)
        for (int i = 0; i < inventory.Count; ++i)
        {
            // not empty and same type? then add free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (inventory[i].amount > 0 && inventory[i].item.name == item.name)
            {
                ItemSlot temp = inventory[i];
                amount -= temp.IncreaseAmount(amount);
                inventory[i] = temp;
            }

            // were we able to fit the whole amount already? then stop loop
            if (amount <= 0) return -10000;
        }

        // add to empty slots (if any)
        for (int i = 0; i < inventory.Count; ++i)
        {
            // empty? then fill slot with as many as possible
            if (inventory[i].amount == 0)
            {
                int add = Mathf.Min(amount, item.maxStack);
                inventory[i] = new ItemSlot(item, add);
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
