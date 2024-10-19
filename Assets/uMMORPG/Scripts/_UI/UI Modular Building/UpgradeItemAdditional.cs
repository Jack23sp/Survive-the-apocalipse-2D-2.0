using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AccessoriesType
{
    notApplicable = -1,
    silencer = 0,
    scope = 1,
    foregrip = 2,
    magazine = 3,
    Barell = 4,
    Body = 5,
    Bottom = 6,
    stock = 7
}

public partial struct Item
{
    public Item[] accessories;


    public int currentDurability;
    public int durabilityLevel;
    public int bagLevel;

    public int skin;
    public int bulletsRemaining;
    public bool alreadyAddedAccessory;
}

[System.Serializable]
public partial struct AccessoriesRecap
{
    public string accessorytype;
    public int accessoryIndex;
    public int accessorySkin;
}

public partial class ScriptableItem
{
    [Header("Repair")]
    public List<CustomItem> repairItems;
    public List<CustomItem> upgradeItems;

    [Header("Armor")]
    public LinearInt armor;
    public int maxArmorLevel;

    [Header("Cook")]
    public FoodItem productAfterCooking;

    //[Header("Cover temperature")]
    //public float coverTemperature;

    [Header("Bag")]
    public LinearInt additionalSlot;
    public LinearInt protectedSlot;
    public int maxBagLevel;

    [Header("Weight")]
    public LinearFloat maxWeight;

    [Header("Weapon Upgrade")]
    public List<Sprite> skinImages;
    public GameObject weaponToSpawn;
    public Sprite munitionImage;
    public int maxMunition;
    public float attackRange = 1;
    public bool needMunitionInMagazine = false;
    public LayerMask attackLayer;

    [Header("Skill required")]
    public ScriptableSkill requiredSkill;

    [Header("Unsanity")]
    public int maxUnsanity;


    [Header("Durability")]
    public LinearInt maxDurability;
    public int maxDurabilityLevel;

    [Header("Building Stuff")]
    public bool canUseFurnace = false;
    public bool canUseWarehouse = false;
    public bool canUseCabinet = false;
    public bool canUseWeaponStorage = false;
    public bool canUseUpgrade = false;
    public bool canUseCook = false;
    public bool canUseFridge = false;
    public bool canUseLibrary = false;
    public List<ItemCrafting> itemtoCraftBuilding;
    public List<ItemCrafting> itemtoCraft;
    public List<ItemCrafting> itemtoCraftFemale;
    public bool continuePlacing;

    [Header("Weapon")]
    public bool canAddSkin;
    public AccessoriesType accessoriesType;
    public List<ScriptableItem> accessoryToAdd;
}

