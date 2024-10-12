using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [Command]
    public void CmdAddWeaponToWeaponStorage(int inventoryIndex, int type, NetworkIdentity identity)
    {
        WeaponStorage weaponStorage = identity.GetComponent<WeaponStorage>();
        if (weaponStorage)
        {
            if (weaponStorage)
            {
                if (type == 0)
                {
                    if (weaponStorage.InventoryCanAdd(inventory.slots[inventoryIndex].item, 1, 5, 0, true))
                    {
                        weaponStorage.InventoryAdd(inventory.slots[inventoryIndex].item, 1, 5, inventoryIndex, this, 0, true);
                        return;
                    }
                }
                else if (type == 1)
                {
                    if (weaponStorage.InventoryCanAdd(inventory.slots[inventoryIndex].item, 1, 15, 5, true))
                    {
                        weaponStorage.InventoryAdd(inventory.slots[inventoryIndex].item, 1, 15, inventoryIndex, this, 5, true);
                        return;
                    }
                }
                else
                {
                    if (weaponStorage.InventoryCanAdd(inventory.slots[inventoryIndex].item, inventory.slots[inventoryIndex].amount, 36, 15, false))
                    {
                        weaponStorage.InventoryAdd(inventory.slots[inventoryIndex].item, inventory.slots[inventoryIndex].amount, 36, inventoryIndex, this, 15, false);
                        return;
                    }
                }
            }
        }
    }

    [Command]
    public void CmdAddToInventoryFromWeaponStorage(int index, NetworkIdentity identity)
    {
        WeaponStorage weaponStorage = identity.GetComponent<WeaponStorage>();

        if (weaponStorage)
        {
            if (weaponStorage.weapon.Count >= index)
            {
                if (inventory.CanAddItem(weaponStorage.weapon[index].item, weaponStorage.weapon[index].amount))
                {
                    inventory.AddItem(weaponStorage.weapon[index].item, weaponStorage.weapon[index].amount);
                    weaponStorage.weapon[index] = new ItemSlot();
                }
            }
        }
    }
}

public partial class Database
{
    class weaponstorage_item_accessories
    {
        public int buildingItem { get; set; }
        public int myIndex { get; set; }
        public string character { get; set; }
        public string accessoryName { get; set; }
        public int bulletsRemaining { get; set; }
        public int skin { get; set; }
    }

    class weaponstorage_item
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

    public void SaveWeaponStorage(int index)
    {
        for (int i = 0; i < ((WeaponStorage)ModularBuildingManager.singleton.buildingAccessories[index]).weapon.Count; i++)
        {
            ItemSlot slot = ((WeaponStorage)ModularBuildingManager.singleton.buildingAccessories[index]).weapon[i];
            if (slot.amount > 0)
            {
                connection.InsertOrReplace(new weaponstorage_item
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
                        connection.InsertOrReplace(new weaponstorage_item_accessories
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

    public void LoadWeaponStorage(int index, WeaponStorage weaponstorage)
    {
        for (int i = 0; i < 36; i++)
            weaponstorage.weapon.Add(new ItemSlot());

        foreach (weaponstorage_item row in connection.Query<weaponstorage_item>("SELECT * FROM weaponstorage_item WHERE myIndex=?", index))
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

                int am = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM weaponstorage_item_accessories WHERE buildingItem=? AND myIndex=?", index, row.slot);
                item.accessories = new Item[am];
                int count = 0;
                foreach (weaponstorage_item_accessories row2 in connection.Query<weaponstorage_item_accessories>("SELECT * FROM weaponstorage_item_accessories WHERE buildingItem=? AND myIndex=?", index, row.slot))
                {
                    if (ScriptableItem.All.TryGetValue(row2.accessoryName.GetStableHashCode(), out ScriptableItem accessory))
                    {
                        Item item2 = new Item(accessory);
                        item2.bulletsRemaining = row2.bulletsRemaining;
                        item.accessories[count] = item2;
                        count++;
                    }
                }

                weaponstorage.weapon[row.slot] = new ItemSlot(item, row.amount);
            }
        }
    }
}

public class WeaponStorage : BuildingAccessory
{
    public SyncList<ItemSlot> weapon = new SyncList<ItemSlot>();

    public GameObject otherPlayerAreInteractWithThisAccessory;

    public List<GameObject> doorsOpen = new List<GameObject>();
    public List<GameObject> doorsClosed = new List<GameObject>();

    public new void Start()
    {
        base.Start();
        if (weapon.Count == 0)
        {
            for (int i = 0; i < 36; i++)
            {
                weapon.Add(new ItemSlot());
            }
        }
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.weaponStorages.Contains(this)) ModularBuildingManager.singleton.weaponStorages.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.weaponStorages.Contains(this)) ModularBuildingManager.singleton.weaponStorages.Remove(this);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        weapon.Callback += OnWeaponInventoryChanged;
        playerThatInteractWhitThis.Callback += PlayerInteraction;
        InteractionCall();
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
        InteractionCall();
    }


    public void InteractionCall()
    {
        otherPlayerAreInteractWithThisAccessory.SetActive(playerThatInteractWhitThis.Count > 0);
        foreach (GameObject open in doorsOpen)
        {
            open.SetActive(playerThatInteractWhitThis.Count > 0);
        }

        foreach (GameObject open in doorsClosed)
        {
            open.SetActive(playerThatInteractWhitThis.Count == 0);
        }
    }

    void OnWeaponInventoryChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        //if (!(op == SyncList<ItemSlot>.Operation.OP_ADD))
        //{
            if (UIWeaponStorage.singleton && UIWeaponStorage.singleton.panel.activeInHierarchy)
            {
                if (ModularBuildingManager.singleton.buildingAccessory && UIWeaponStorage.singleton.weaponStorage && ModularBuildingManager.singleton.buildingAccessory.netId == UIWeaponStorage.singleton.weaponStorage.netId)
                {
                    UIWeaponStorage.singleton.Open(UIWeaponStorage.singleton.weaponStorage, UIWeaponStorage.singleton.isReadOnly);
                }
            }
        //}
    }

    // For weapon make one lost for each one
    public bool InventoryCanAdd(Item item, int amount, int final, int initial = 0, bool unique = true)
    {
        int initialAMount = amount;
        for (int i = initial; i < final; i++)
        {
            if (amount > 0)
            {
                if (unique)
                {
                    if (weapon[i].amount == 0)
                        return true;
                }
                else
                {
                    if (weapon[i].amount == 0)
                        return true;
                    else if (weapon[i].item.data.name == item.data.name)
                        amount -= (weapon[i].item.maxStack - weapon[i].amount);
                }
            }
            else
            {
                return true;
            }
        }

        return initialAMount > amount;
    }

    public bool InventoryAdd(Item item, int amount, int final, int inventoryIndex, Player player, int initial = 0, bool unique = true)
    {
        int initialAmount = amount;
        int remaining = 0;
        if (InventoryCanAdd(item, amount, final, initial, unique))
        {
            if (unique == false)
            {
                for (int i = initial; i < final; i++)
                {
                    int index = i;
                    if (weapon[index].amount > 0 && weapon[index].item.data.name == item.name)
                    {
                        ItemSlot temp = weapon[index];


                        remaining = temp.item.data.maxStack - temp.amount;
                        if (amount > 0)
                        {
                            if (remaining > 0)
                            {
                                if (remaining >= amount)
                                {
                                    temp.IncreaseAmount(amount);

                                    ItemSlot invItem = player.inventory.slots[inventoryIndex];
                                    invItem.DecreaseAmount(amount);
                                    player.inventory.slots[inventoryIndex] = invItem;
                                    remaining -= amount;
                                    amount = 0;
                                    weapon[index] = temp;
                                }
                                else
                                {
                                    temp.IncreaseAmount(remaining);

                                    ItemSlot invItem = player.inventory.slots[inventoryIndex];
                                    invItem.DecreaseAmount(remaining);
                                    player.inventory.slots[inventoryIndex] = invItem;
                                    remaining = 0;
                                    amount -= remaining;
                                    weapon[index] = temp;
                                }
                            }
                        }
                    }
                    else if (weapon[index].amount  == 0)
                    {
                        if (amount > 0)
                        {
                            ItemSlot newItem = new ItemSlot(item, amount > item.maxStack ? item.maxStack : amount);
                            weapon[index] = newItem;

                            ItemSlot inventoryItem = player.inventory.slots[inventoryIndex];
                            inventoryItem.DecreaseAmount(amount > item.maxStack ? item.maxStack : amount);
                            player.inventory.slots[inventoryIndex] = inventoryItem;

                            amount -= item.maxStack > amount ? item.maxStack : amount;
                        }
                    }
                }
            }
            else
            {
                for (int i = initial; i < final; i++)
                {
                    int index = i;
                    if (weapon[index].amount == 0)
                    {
                        ItemSlot newItem = new ItemSlot(item, amount > item.maxStack ? item.maxStack : amount);
                        weapon[index] = newItem;

                        ItemSlot inventoryItem = player.inventory.slots[inventoryIndex];
                        inventoryItem.DecreaseAmount(amount > item.maxStack ? item.maxStack : amount);
                        player.inventory.slots[inventoryIndex] = inventoryItem;

                        amount -= item.maxStack > amount ? item.maxStack : amount;
                        return true;
                    }
                }
            }
            if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        }
        return initialAmount > amount;
    }
}
