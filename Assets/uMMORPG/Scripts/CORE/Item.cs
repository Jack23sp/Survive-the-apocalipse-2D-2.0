// The Item struct only contains the dynamic item properties, so that the static
// properties can be read from the scriptable object.
//
// Items have to be structs in order to work with SyncLists.
//
// Use .Equals to compare two items. Comparing the name is NOT enough for cases
// where dynamic stats differ. E.g. two pets with different levels shouldn't be
// merged.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Data;

[Serializable]
public partial struct Item
{
    // hashcode used to reference the real ScriptableItem (can't link to data
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;

    // dynamic stats (cooldowns etc. later)
    public NetworkIdentity summoned; // summonable that's currently summoned
    public float summonedHealth; // stored in item while summonable unsummoned
    public int summonedLevel; // stored in item while summonable unsummoned
    public long summonedExperience; // stored in item while summonable unsummoned

    // constructors
    public Item(ScriptableItem data)
    {
        hash = data.name.GetStableHashCode();
        summoned = null;
        summonedHealth = data is SummonableItem ? ((SummonableItem)data).summonPrefab.health.max : 0;
        summonedLevel = data is SummonableItem ? 1 : 0;
        summonedExperience = 0;

        armorLevel = 1;
        accuracyLevel = 1;
        durabilityLevel = 1;
        bagLevel = 1;
        batteryLevel = 1;

        weight = data.weight;

        waterContainer = data is WaterBottleItem ? ((WaterBottleItem)data).maxWater : 0;
        gasContainer = 0;
        honeyContainer = 0;

        coverTemperature = 0;

        radioCurrentBattery = 0;
        torchCurrentBattery = 0;

        currentUnsanity = data.maxUnsanity;
        currentDurability = durabilityLevel > 0 ? data.maxDurability.Get(durabilityLevel) : 0;
        currentArmor = armorLevel > 0 ? data.armor.Get(armorLevel) : 0;
        torchCurrentBattery = data is EquipmentItem ? ((EquipmentItem)data).battery.Get(batteryLevel) : 0;
        currentBlood = data is FoodItem ? ((FoodItem)data).maxBlood : 0;
        wet = 0;

        skin = 0;
        bulletsRemaining = 0;
        accessories = new Item[0];
        alreadyAddedAccessory = false;
        AddItem();
    }

    public bool CanUpgradeOrRepair()
    {
        return ((data.maxArmorLevel > 0 && armorLevel < data.maxArmorLevel) ||
            (data.maxBagLevel > 0 && bagLevel < data.maxBagLevel) ||
            (data.maxDurabilityLevel > 0 && durabilityLevel < data.maxDurabilityLevel) ||
            (data.maxDurabilityLevel > 0 && durabilityLevel < data.maxDurabilityLevel) ||
            (data.maxDurabilityLevel > 0 && currentDurability < data.maxDurability.Get(durabilityLevel)) || (data is WeaponItem)) ;
    }

    public void AddItem()
    {
        if (data.accessoryToAdd.Count > 0 && data.weaponToSpawn && !alreadyAddedAccessory)
        {
            for (int i = 0; i < data.accessoryToAdd.Count; i++)
            {
                int index_i = i;
                bool contains = false;
                for (int e = 0; e < accessories.Length; e++)
                {
                    int index_e = e;
                    if (accessories[index_e].name == data.accessoryToAdd[index_i].name)
                    {
                        contains = true;
                    }
                }
                if (!contains)
                {
                    List<Item> acc = accessories.ToList();
                    acc.Add(new Item(data.accessoryToAdd[index_i]));
                    accessories = acc.ToArray();
                }
            }
            alreadyAddedAccessory = true;
        }

    }

    // wrappers for easier access
    public ScriptableItem data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableItem.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableItem.All.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableItem with hash=" + hash + ". Make sure that all ScriptableItems are in the Resources folder so they are loaded properly.");
            return ScriptableItem.All[hash];
        }
    }
    public string name => data.name;
    public int maxStack => data.maxStack;
    public long buyPrice => data.buyPrice;
    public long sellPrice => data.sellPrice;
    public long itemMallPrice => data.itemMallPrice;
    public bool sellable => data.sellable;
    public bool tradable => data.tradable;
    public bool destroyable => data.destroyable;
    public Sprite image => data.image;

    // tooltip
    public string ToolTip()
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip());
        tip.Replace("{SUMMONEDHEALTH}", summonedHealth.ToString());
        tip.Replace("{SUMMONEDLEVEL}", summonedLevel.ToString());
        tip.Replace("{SUMMONEDEXPERIENCE}", summonedExperience.ToString());

        // addon system hooks
        Utils.InvokeMany(typeof(Item), this, "ToolTip_", tip);

        return tip.ToString();
    }
}
