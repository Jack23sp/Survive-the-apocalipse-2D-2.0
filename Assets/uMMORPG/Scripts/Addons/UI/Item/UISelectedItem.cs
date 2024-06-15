using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Mirror;
using System;

public partial class ItemContainer
{
    public int FirstFreeSlot()
    {
        Player player = Player.localPlayer;
        int free = -1;
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            int index = i;
            if (free == -1)
            {
                if (player.inventory.slots[index].amount == 0)
                    free = index;
            }
        }
        return free;
    }
}

public partial class Player
{
    [Command]
    public void CmdDeleteItem(int index, int type)
    {
        if (type == 0)
        {
            inventory.slots[index] = new ItemSlot();
        }
        else if (type == 1)
        {
            playerBelt.belt[index] = new ItemSlot();
        }
        else
        {
            equipment.slots[index] = new ItemSlot();
        }
    }

    [Command]
    public void CmdMunitionToInventory(string munitionName, int amount, int index, bool isInventory)
    {
        if (ScriptableItem.All.TryGetValue(munitionName.GetStableHashCode(), out ScriptableItem itemData))
        {
            if (inventory.CanAddItem(new Item(itemData), amount))
            {
                ItemSlot slot = isInventory ? inventory.slots[index] : playerBelt.belt[index];
                if (slot.item.bulletsRemaining - amount >= 0)
                {
                    inventory.AddItem(new Item(itemData), amount);
                    slot.item.bulletsRemaining -= amount;
                    if (isInventory)
                    {
                        inventory.slots[index] = slot;
                    }
                    else
                    {
                        playerBelt.belt[index] = slot;
                    }
                }
                else
                {
                    inventory.AddItem(new Item(itemData), slot.item.bulletsRemaining);
                    slot.item.bulletsRemaining = 0;
                    if (isInventory)
                    {
                        inventory.slots[index] = slot;
                    }
                    else
                    {
                        playerBelt.belt[index] = slot;
                    }
                }
            }
        }
    }

    [Command]
    public void CmdMunitionToMagazine(string munitionName, int amount, int index, bool isInventory)
    {
        if (ScriptableItem.All.TryGetValue(munitionName.GetStableHashCode(), out ScriptableItem itemData))
        {
            if (inventory.CountItem(new Item(itemData)) >= amount)
            {
                ItemSlot slot = isInventory ? inventory.slots[index] : playerBelt.belt[index];
                if (slot.item.bulletsRemaining + amount <= ((WeaponItem)slot.item.data).maxMunition)
                {
                    inventory.RemoveItem(new Item(itemData), amount);
                    slot.item.bulletsRemaining += amount;
                    if (isInventory)
                    {
                        inventory.slots[index] = slot;
                    }
                    else
                    {
                        playerBelt.belt[index] = slot;
                    }
                }
                else
                {
                    int remaining = ((WeaponItem)slot.item.data).maxMunition - slot.item.bulletsRemaining;
                    if(remaining == 0) return;

                    inventory.RemoveItem(new Item(itemData), remaining);
                    slot.item.bulletsRemaining += remaining;
                    if (isInventory)
                    {
                        inventory.slots[index] = slot;
                    }
                    else
                    {
                        playerBelt.belt[index] = slot;
                    }
                }
            }
            else
            {
                int count = inventory.CountItem(new Item(itemData));
                ItemSlot slot = isInventory ? inventory.slots[index] : playerBelt.belt[index];
                if (slot.item.bulletsRemaining + count <= ((WeaponItem)slot.item.data).maxMunition)
                {
                    slot.item.bulletsRemaining += count;
                    inventory.RemoveItem(new Item(itemData), count);
                    if (isInventory)
                    {
                        inventory.slots[index] = slot;
                    }
                    else
                    {
                        playerBelt.belt[index] = slot;
                    }
                }               
            }
        }
    }
}

public partial class PlayerPetControl
{
    [Command]
    public void CmdCurePet(bool isInventory)
    {
        int foodCure = -1;
        if (isInventory)
        {
            foodCure = player.inventory.FindFirst(PremiumItemManager.singleton.petFood.name);
        }
        else
        {
            foodCure = player.playerBelt.FindFirst(PremiumItemManager.singleton.petFood.name);
        }

        PlayerPetControl playerPetControl = GetComponent<PlayerPetControl>();

        if (playerPetControl.activePet != null && foodCure > -1)
        {
            if (Vector3.Distance(playerPetControl.activePet.transform.position, player.transform.GetChild(0).transform.position) < 2)
            {
                player.playerScreenNotification.CmdSpawnNotification(player.netIdentity, "Go closer to your pet");
                return;
            }
            playerPetControl.activePet.health.current += (PremiumItemManager.singleton.petFood.petHealthToAdd);
            playerPetControl.SetState("IDLE");
            if (isInventory)
            {
                ItemSlot slot = inventory.slots[foodCure];
                slot.amount--;
                inventory.slots[foodCure] = slot;
            }
            else
            {
                ItemSlot slot = player.playerBelt.belt[foodCure];
                slot.amount--;
                player.playerBelt.belt[foodCure] = slot;
            }
        }
    }
}

public class UISelectedItem : MonoBehaviour
{
    public static UISelectedItem singleton;

    public Image panel;
    public Image itemImage;
    public GameObject overlayAmountObject;
    public TextMeshProUGUI overlayAmountText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI description;
    public Button closeButton;
    public Button deleteButton;
    public Button useButton;
    public Button equipButton;
    public Button addAccessory;
    public Button itemDetails;

    public ItemSlot ItemSlot;
    public Item munitionItem;
    public bool delete;
    public bool use;
    public bool equip;
    public bool skillSlot;
    public bool inventorySlot;
    public bool equipmentSlot;
    public bool warehouseSlot;
    public bool fridgeSlot;
    public bool librarySlot;

    public Slider slider;
    public TextMeshProUGUI sliderValue;
    public Image sliderItemImage;
    public TextMeshProUGUI inInventory;
    public TextMeshProUGUI inAccessory;
    public Button manageMunitionInventory;
    public TextMeshProUGUI manageMunitionInventoryText;

    public GameObject weaponAccessoryObject;
    public Transform weaponAccessoryContent;
    public GameObject weaponAccessoryObjectToSpawn;

    public GameObject accessoryPanel;
    public Transform accessoryPanelContent;
    public GameObject objectToSpawn;

    public GameObject skinPanel;
    public Transform skinPanelContent;
    public GameObject skinObjectToSpawn;

    public TextMeshProUGUI precisionText;

    public int index;

    public Button dropButton;

    public void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            panel.gameObject.SetActive(false);
            closeButton.image.raycastTarget = false;
            Reset(false);
        });

        dropButton.onClick.AddListener(() =>
        {
            ConfirmDropItem.singleton.Manage(true);
        });

        slider.onValueChanged.AddListener(delegate { CheckValue(); });
    }

    public void CheckValue()
    {
        if (ItemSlot.amount > 0)
        {
            sliderValue.text = slider.value.ToString();

            if (ItemSlot.item.data is WaterBottleItem)
            {
                manageMunitionInventoryText.text = "Drink";
                int waterInside = ItemSlot.item.honeyContainer > 0 ? ItemSlot.item.honeyContainer : ItemSlot.item.waterContainer;
                if (slider.value > waterInside)
                {
                    manageMunitionInventory.interactable = false;
                    sliderValue.color = Color.red;
                }
                else
                {
                    sliderValue.color = Color.black;
                    manageMunitionInventory.interactable = true;
                    manageMunitionInventory.onClick.RemoveAllListeners();
                    manageMunitionInventory.onClick.AddListener(() =>
                    {
                        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                        if (ItemSlot.item.CanAddWater())
                            Player.localPlayer.playerThirsty.CmdDrinkWater(Convert.ToInt32(slider.value), index, inventorySlot);
                        else if (ItemSlot.item.CanAddHoney())
                            Player.localPlayer.playerThirsty.CmdDrinkHoney(Convert.ToInt32(slider.value), index, inventorySlot);
                    });

                }
            }
            else if (ItemSlot.item.data is EquipmentItem && ItemSlot.item.data.maxMunition > 0)
            {
                if (slider.value <= ItemSlot.item.bulletsRemaining)
                {
                    manageMunitionInventoryText.text = slider.value + " to inventory";
                    manageMunitionInventory.onClick.RemoveAllListeners();
                    manageMunitionInventory.onClick.AddListener(() =>
                    {
                        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                        Player.localPlayer.CmdMunitionToInventory(((WeaponItem)ItemSlot.item.data).requiredAmmo.name, Convert.ToInt32(slider.value), index, inventorySlot);
                    });
                }
                else
                {
                    manageMunitionInventoryText.text = (slider.value - ItemSlot.item.bulletsRemaining) + " into Magazine";
                    manageMunitionInventory.onClick.RemoveAllListeners();
                    manageMunitionInventory.onClick.AddListener(() =>
                    {
                        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                        Player.localPlayer.CmdMunitionToMagazine(((WeaponItem)ItemSlot.item.data).requiredAmmo.name, Convert.ToInt32(slider.value - ItemSlot.item.bulletsRemaining), index, inventorySlot);
                    });
                }
            }
            else if (ItemSlot.item.data is FoodItem && ((FoodItem)ItemSlot.item.data).maxBlood > 0)
            {
                manageMunitionInventory.onClick.RemoveAllListeners();
                manageMunitionInventory.onClick.AddListener(() =>
                {
                    Player.localPlayer.playerBlood.CmdAddBlood(inventorySlot, index, Convert.ToInt32(slider.value));
                });
            }
        }
    }

    public void CallInvokeToCheck()
    {
        Invoke(nameof(CheckSpawnItem), 0.5f);
    }

    public void CheckSpawnItem()
    {
        if (index > -1)
        {
            if (inventorySlot) ItemSlot = Player.localPlayer.inventory.slots[index];
            if (skillSlot) ItemSlot = Player.localPlayer.playerBelt.belt[index];
            if (equipmentSlot) ItemSlot = Player.localPlayer.equipment.slots[index];
            if (fridgeSlot) ItemSlot = UIFridge.singleton.fridge.slots[index];
            if (warehouseSlot) ItemSlot = UIWarehouse.singleton.warehouse.slots[index];
            if (librarySlot) ItemSlot = UILibrary.singleton.library.slots[index];
            if (fridgeSlot || warehouseSlot || librarySlot)
            {
                delete = false;
                use = false;
                equip = false;
                skillSlot = false;
                inventorySlot = false;
                equipmentSlot = false;
            }
        }

        if (ItemSlot.amount > 0)
        {
            Setup(ItemSlot, delete, use, equip);
        }
    }

    public void Setup(ItemSlot itemSlot, bool delete, bool use, bool equip)
    {
        panel.gameObject.SetActive(true);
        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;

        itemImage.sprite = itemSlot.item.data.skinImages.Count > 0
                           && itemSlot.item.skin > -1 ? itemSlot.item.data.skinImages[itemSlot.item.skin] : itemSlot.item.data.image;
        itemImage.preserveAspect = true;
        overlayAmountObject.SetActive(itemSlot.amount > 0);
        overlayAmountText.text = itemSlot.amount.ToString();
        nameText.text = itemSlot.item.data.name;
        description.text = itemSlot.ToolTip();
        munitionItem = new Item();
        slider.gameObject.SetActive(false);
        sliderItemImage.gameObject.SetActive(false);
        inAccessory.gameObject.SetActive(false);
        inInventory.gameObject.SetActive(false);
        manageMunitionInventory.gameObject.SetActive(false);
        dropButton.gameObject.SetActive(inventorySlot || skillSlot);
        weaponAccessoryObject.SetActive(false);
        skinPanel.SetActive(false);
        addAccessory.gameObject.SetActive(itemSlot.item.data is WeaponItem && ((WeaponItem)itemSlot.item.data).accessoryToAdd.Count > 0);
        precisionText.gameObject.SetActive(addAccessory.gameObject.activeInHierarchy);

        precisionText.text = "Aim ability : " + Player.localPlayer.playerShootPrecision.Calculate() + " + <color=#24F337>(" + Player.localPlayer.playerShootPrecision.CalculateWeapon(itemSlot) + ") </color>" + "%";

        ConfirmDropItem.singleton.indexSlot = index;
        if (equipmentSlot || warehouseSlot || fridgeSlot || librarySlot)
            ConfirmDropItem.singleton.inventory = false;
        if (inventorySlot)
            ConfirmDropItem.singleton.inventory = true;
        else if (skillSlot)
            ConfirmDropItem.singleton.inventory = false;

        if (itemSlot.amount > 0 && (inventorySlot || skillSlot))
        {
            if (itemSlot.item.data is WaterBottleItem)
            {
                sliderItemImage.sprite = ImageManager.singleton.waterImage;
                sliderItemImage.preserveAspect = true;
                inAccessory.text = (itemSlot.item.honeyContainer > 0 ? "Honey " : "Water ") + "inside : " + (itemSlot.item.honeyContainer > 0 ? itemSlot.item.honeyContainer : itemSlot.item.waterContainer);
                inInventory.text = "";
                slider.gameObject.SetActive(true);
                sliderItemImage.gameObject.SetActive(true);
                inAccessory.gameObject.SetActive(true);
                inInventory.gameObject.SetActive(true);
                slider.value = 0.0f;
                sliderValue.text = "0";
                slider.minValue = 0;
                slider.maxValue = ((WaterBottleItem)itemSlot.item.data).maxWater;
                sliderValue.color = Color.black;
                manageMunitionInventory.gameObject.SetActive(true);
                manageMunitionInventoryText.text = "Drink";
                use = false;
            }
            else
            {
                slider.gameObject.SetActive(false);
                sliderItemImage.gameObject.SetActive(false);
                inAccessory.gameObject.SetActive(false);
                inInventory.gameObject.SetActive(false);
                use = true;
            }


            if (itemSlot.item.data is EquipmentItem && itemSlot.item.data.maxMunition > 0)
            {
                slider.gameObject.SetActive(true);
                use = false;
                sliderItemImage.gameObject.SetActive(true);
                inAccessory.gameObject.SetActive(true);
                inInventory.gameObject.SetActive(true);
                munitionItem = itemSlot.item;
                sliderItemImage.sprite = itemSlot.item.data.image;
                sliderItemImage.preserveAspect = true;
                inAccessory.text = "In magazine : " + itemSlot.item.bulletsRemaining + " / " + itemSlot.item.data.maxMunition;
                inInventory.text = "In inventory : " + Player.localPlayer.inventory.CountItem(new Item(((WeaponItem)itemSlot.item.data).requiredAmmo));
                slider.value = 0.0f;
                sliderValue.text = "0";
                slider.minValue = 0;
                slider.maxValue = itemSlot.item.data.maxMunition;
                manageMunitionInventory.gameObject.SetActive(true);
                manageMunitionInventoryText.text = "Select";
            }

            if (itemSlot.item.data is FoodItem && ((FoodItem)itemSlot.item.data).maxBlood > 0)
            {
                slider.gameObject.SetActive(true);
                use = false;
                sliderItemImage.gameObject.SetActive(true);
                inAccessory.gameObject.SetActive(true);
                inInventory.gameObject.SetActive(true);
                munitionItem = itemSlot.item;
                sliderItemImage.sprite = itemSlot.item.data.image;
                sliderItemImage.preserveAspect = true;
                inAccessory.text = "";
                inInventory.text = "";
                slider.value = 0.0f;
                sliderValue.text = "0";
                slider.minValue = 0;
                slider.maxValue = ItemSlot.item.currentBlood > (Player.localPlayer.playerBlood.max - Player.localPlayer.playerBlood.current) ?
                                  (((FoodItem)itemSlot.item.data).maxBlood - ItemSlot.item.currentBlood) : ItemSlot.item.currentBlood;
                manageMunitionInventory.gameObject.SetActive(true);
                manageMunitionInventoryText.text = "Select";
            }
        }
        else
        {
            slider.gameObject.SetActive(false);
            sliderItemImage.gameObject.SetActive(false);
            inAccessory.gameObject.SetActive(false);
            inInventory.gameObject.SetActive(false);
        }

        if (itemSlot.amount > 0)
        {
            if (itemSlot.item.data is WeaponItem)
            {
                if (itemSlot.item.accessories.Length > 0)
                {
                    weaponAccessoryObject.SetActive(true);
                    slider.gameObject.SetActive(false);
                    sliderItemImage.gameObject.SetActive(false);
                    inAccessory.gameObject.SetActive(false);
                    inInventory.gameObject.SetActive(false);
                    manageMunitionInventory.gameObject.SetActive(false);

                    UIUtils.BalancePrefabs(weaponAccessoryObjectToSpawn, itemSlot.item.accessories.Length, weaponAccessoryContent);
                    for (int i = 0; i < itemSlot.item.accessories.Length; i++)
                    {
                        int index_i = i;
                        UIWeaponAccessory slot = weaponAccessoryContent.GetChild(index_i).GetComponent<UIWeaponAccessory>();
                        slot.accessoryImage.sprite = itemSlot.item.accessories[index_i].data.skinImages.Count > 0 && itemSlot.item.skin > -1 ?
                                                     itemSlot.item.accessories[index_i].data.skinImages[itemSlot.item.accessories[index_i].skin] :
                                                     itemSlot.item.accessories[index_i].data.image;
                        slot.accessoryImage.preserveAspect = true;
                        slot.accessoryName.text = itemSlot.item.accessories[index_i].name;
                        slot.accessoryPrecision.text = ((WeaponItem)itemSlot.item.accessories[index_i].data).weaponPrecision == 0 ? "" : "Precision: \n" + ((WeaponItem)itemSlot.item.accessories[index_i].data).weaponPrecision.ToString() + "%";
                        slot.buttonRemoveAccessory.gameObject.SetActive(((WeaponItem)itemSlot.item.accessories[index_i].data).weaponPrecision > 0);
                        slot.buttonRemoveAccessory.onClick.RemoveAllListeners();
                        slot.buttonRemoveAccessory.onClick.AddListener(() =>
                        {
                            Player.localPlayer.playerUpgrade.CmdUnequipAccessories(index, index_i, inventorySlot == true ? 0 : equipmentSlot == true ? 1 : 2);
                        });
                    }
                }

                if (itemSlot.item.data.skinImages.Count > 0)
                {
                    skinPanel.SetActive(true);
                    slider.gameObject.SetActive(false);
                    sliderItemImage.gameObject.SetActive(false);
                    inAccessory.gameObject.SetActive(false);
                    inInventory.gameObject.SetActive(false);
                    manageMunitionInventory.gameObject.SetActive(false);

                    UIUtils.BalancePrefabs(skinObjectToSpawn, SkinManager.singleton.skins.Count, skinPanelContent);
                    for (int i = 0; i < SkinManager.singleton.skins.Count; i++)
                    {
                        int index_i = i;
                        UISkinSlot slot = skinPanelContent.GetChild(index_i).GetComponent<UISkinSlot>();
                        slot.image.sprite = itemSlot.item.data.skinImages[index_i];
                        slot.image.preserveAspect = true;

                        if (itemSlot.item.skin == index_i)
                        {
                            slot.button.interactable = false;
                            slot.button.onClick.RemoveAllListeners();
                            slot.buttonName.text = "Current";
                            slot.currencyContainer.SetActive(false);
                        }
                        else
                        {
                            PlayerUpgrade upgrade = Player.localPlayer.playerUpgrade;
                            if (upgrade)
                            {
                                if (index_i != 0)
                                {
                                    slot.button.interactable = true;
                                    slot.buttonName.text = "";
                                    slot.currencyContainer.SetActive(true);
                                    slot.currencyImage.sprite = SkinManager.singleton.gold ? ImageManager.singleton.gold : ImageManager.singleton.coin;
                                    slot.currencyImage.preserveAspect = true;
                                    slot.cost.text = SkinManager.singleton.costToBuySkin.ToString();

                                    for (int e = 0; e < upgrade.weaponSkins.Count; e++)
                                    {
                                        if (upgrade.weaponSkins[e].itemName == itemSlot.item.data.name)
                                        {
                                            for (int a = 0; a < upgrade.weaponSkins[e].buyedSkin.Length; a++)
                                            {
                                                if (upgrade.weaponSkins[e].buyedSkin.ToList().Contains(index_i))
                                                {
                                                    slot.button.interactable = true;
                                                    slot.buttonName.text = "Paint";
                                                    slot.currencyContainer.SetActive(false);

                                                    slot.button.onClick.RemoveAllListeners();
                                                    slot.button.onClick.AddListener(() =>
                                                    {
                                                        Player.localPlayer.playerUpgrade.CmdSelectSkin(index, index_i, itemSlot.item.data.name, inventorySlot == true ? 0 : equipmentSlot == true ? 1 : 2);
                                                        return;
                                                    });
                                                }
                                                else
                                                {
                                                    slot.button.interactable = true;
                                                    slot.buttonName.text = "";
                                                    slot.currencyContainer.SetActive(true);
                                                }
                                            }
                                        }
                                    }

                                    slot.button.onClick.RemoveAllListeners();
                                    slot.button.onClick.AddListener(() =>
                                    {
                                        Player.localPlayer.playerUpgrade.CmdSetSkin(index, index_i, itemSlot.item.data.name, inventorySlot == true ? 0 : equipmentSlot == true ? 1 : 2);
                                    });
                                }
                                else
                                {
                                    slot.button.interactable = true;
                                    slot.buttonName.text = "Paint";
                                    slot.currencyContainer.SetActive(false);

                                    slot.button.onClick.RemoveAllListeners();
                                    slot.button.onClick.AddListener(() =>
                                    {
                                        Player.localPlayer.playerUpgrade.CmdSelectSkin(index, index_i, itemSlot.item.data.name, inventorySlot == true ? 0 : equipmentSlot == true ? 1 : 2);
                                    });
                                }                              
                            }
                        }
                    }
                }
            }
        }

        deleteButton.gameObject.SetActive(delete);
        useButton.gameObject.SetActive(use);
        equipButton.gameObject.SetActive(equip);

        if (skillSlot || equipmentSlot)
        {
            equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "To Inventory!";
        }
        if (inventorySlot)
        {
            equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "To Belt!";
        }

        itemDetails.interactable = UIUtils.FindWhereTheItemIsCrafted(itemSlot.item.data) != null;
        itemDetails.onClick.RemoveAllListeners();
        itemDetails.onClick.AddListener(() =>
        {
            UIItemDetails.singleton.Open(itemSlot.item.data, UIUtils.FindWhereTheItemIsCrafted(itemSlot.item.data));
        });

        addAccessory.onClick.RemoveAllListeners();
        addAccessory.onClick.AddListener(() =>
        {

            accessoryPanel.gameObject.SetActive(true);

            if (itemSlot.item.data is WeaponItem && ((WeaponItem)itemSlot.item.data).accessoryToAdd.Count > 0)
            {
                WeaponObject weaponObject = null;
                List<int> toDelete = new List<int>();

                for (int e = 0; e < Player.localPlayer.playerWeaponIK.weaponsHolder.Count; e++)
                {
                    if (Player.localPlayer.playerWeaponIK.weaponsHolder[e].weaponHolder.weaponObject.name == ((WeaponItem)itemSlot.item.data).weaponToSpawn.name)
                    {
                        weaponObject = Player.localPlayer.playerWeaponIK.weaponsHolder[e].weaponHolder.weaponObject.GetComponent<WeaponObject>();
                    }
                }
                // Spawn new items
                UIUtils.BalancePrefabs(objectToSpawn, Player.localPlayer.inventory.slots.Count, accessoryPanelContent);
                for (int i = 0; i < Player.localPlayer.inventory.slots.Count; i++)
                {
                    int index_i = i;
                    ItemSlot local = Player.localPlayer.inventory.slots[index_i];
                    UIInventorySlot slot = accessoryPanelContent.GetChild(index_i).GetComponent<UIInventorySlot>();
                    if (Player.localPlayer.inventory.slots[index_i].amount > 0)
                    {
                        Destroy(slot.registerItem);
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = true;

                        slot.durabilitySlider.fillAmount = local.item.data.maxDurability.baseValue > 0 ? ((float)local.item.currentDurability / (float)local.item.data.maxDurability.Get(local.item.durabilityLevel)) : 0;
                        slot.unsanitySlider.fillAmount = local.item.data.maxUnsanity > 0 ? ((float)local.item.currentUnsanity / (float)local.item.data.maxUnsanity) : 0;
                        slot.image.color = Color.white; // reset for no-durability items
                        slot.image.sprite = local.item.data.skinImages.Count > 0 && local.item.skin > -1 ?
                                            local.item.data.skinImages[local.item.skin] :
                                            local.item.data.image;
                        slot.image.preserveAspect = true;

                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(local.amount > 1);
                        slot.amountText.text = local.amount.ToString();

                        slot.button.onClick.RemoveAllListeners();
                        slot.button.onClick.AddListener(() =>
                        {
                            Player.localPlayer.playerUpgrade.CmdEquipAccessory(index, (int)Player.localPlayer.inventory.slots[index_i].item.data.accessoriesType, index_i, Player.localPlayer.inventory.slots[index_i].item.data.name, itemSlot.item.data.name, inventorySlot == true ? 0 : equipmentSlot == true ? 1 : 2);
                            accessoryPanel.gameObject.SetActive(false);
                        });
                    }
                    else
                    {
                        // refresh invalid item
                        slot.button.onClick.RemoveAllListeners();
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(false);
                        Destroy(slot.registerItem);
                        slot.durabilitySlider.fillAmount = 0;
                        slot.unsanitySlider.fillAmount = 0;
                        slot.outline.enabled = false;
                    }

                    if (weaponObject && Player.localPlayer.inventory.slots[i].amount > 0)
                    {
                        bool findIt = false;

                        for (int a = 0; a < weaponObject.weaponPieces.Count; a++)
                        {
                            if (weaponObject.weaponPieces[a].item.name == Player.localPlayer.inventory.slots[i].item.data.name)
                            {
                                findIt = true;
                            }
                        }

                        if (!findIt)
                        {
                            toDelete.Add(i);
                        }
                    }
                    else
                    {
                        toDelete.Add(i);
                    }
                }

                toDelete.Reverse();
                for (int o = 0; o < toDelete.Count; o++)
                {
                    Destroy(accessoryPanelContent.GetChild(toDelete[o]).gameObject);
                }
            }

        });

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(15);
            if (itemSlot.amount > 0 && index > -1)
            {
                if (equipmentSlot)
                {
                    Reset(false);
                    Player.localPlayer.CmdDeleteItem(index, 2);
                }
                else if (inventorySlot)
                {
                    Reset(false);
                    Player.localPlayer.CmdDeleteItem(index, 0);
                }
                else if (skillSlot)
                {
                    Reset(false);
                    Player.localPlayer.CmdDeleteItem(index, 1);
                }
            }
        });

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (itemSlot.amount > 0 && index > -1)
            {

                if (equipmentSlot)
                {
                    Reset(false);
                    Player.localPlayer.playerEquipment.CmdSwapInventoryEquip(Player.localPlayer.inventory.FirstFreeSlot(), index);
                }
                else if (inventorySlot)
                {
                    Reset(false);
                    Player.localPlayer.playerBelt.CmdSwapInventoryBelt(index);
                }
                else if (skillSlot)
                {
                    Reset(false);
                    Player.localPlayer.playerBelt.CmdSwapBeltInventory(index);
                }
            }
        });

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (itemSlot.amount > 0 && index > -1)
            {
                //if (equipmentSlot)
                //{
                //    Player.localPlayer.inventory.CmdUseItem(index);
                //}
                //else 
                if (inventorySlot)
                {
                    ManageBuilding(true);

                    Reset(true);
                    if (Player.localPlayer.inventory.slots[index].item.data is ScriptableBuilding)
                    {
                        ((ScriptableBuilding)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableDoor)
                    {
                        ((ScriptableDoor)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableFence)
                    {
                        ((ScriptableFence)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableGate)
                    {
                        ((ScriptableGate)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableLight)
                    {
                        ((ScriptableLight)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableConcrete)
                    {
                        ((ScriptableConcrete)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableFurnace)
                    {
                        ((ScriptableFurnace)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableWaterContainer)
                    {
                        ((ScriptableWaterContainer)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableWall)
                    {
                        ((ScriptableWall)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableBillboard)
                    {
                        ((ScriptableBillboard)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableFlag)
                    {
                        ((ScriptableFlag)Player.localPlayer.inventory.slots[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableBuildingAccessory)
                    {
                        ((ScriptableBuildingAccessory)Player.localPlayer.inventory.slots[index].item.data).Spawn(Player.localPlayer, ModularBuildingManager.singleton.isInventory, index, 0, Player.localPlayer.transform.position);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableBook)
                    {
                        if (Player.localPlayer.playerAdditionalState.additionalState == "" && Player.localPlayer.inventory.InventoryOperationsAllowed())
                            Player.localPlayer.playerAdditionalState.CmdUseBook(Player.localPlayer, index, true);
                    }
                    else if (Player.localPlayer.inventory.slots[index].item.data is ScriptableDumbbell)
                    {
                        ((ScriptableDumbbell)Player.localPlayer.inventory.slots[index].item.data).Spawn(Player.localPlayer, ModularBuildingManager.singleton.isInventory, index, 0, Player.localPlayer.transform.position);
                    }
                    else
                    {
                        Player.localPlayer.inventory.CmdUseItem(index);
                    }
                }
                else if (skillSlot)
                {
                    ManageBuilding(false);

                    Reset(true);
                    if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableBuilding)
                    {
                        ((ScriptableBuilding)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableDoor)
                    {
                        ((ScriptableDoor)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableFence)
                    {
                        ((ScriptableFence)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableGate)
                    {
                        ((ScriptableGate)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableLight)
                    {
                        ((ScriptableLight)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableConcrete)
                    {
                        ((ScriptableConcrete)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableFurnace)
                    {
                        ((ScriptableFurnace)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableWaterContainer)
                    {
                        ((ScriptableWaterContainer)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableWall)
                    {
                        ((ScriptableWall)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableBillboard)
                    {
                        ((ScriptableBillboard)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableFlag)
                    {
                        ((ScriptableFlag)Player.localPlayer.playerBelt.belt[index].item.data).Use(Player.localPlayer, index);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableBuildingAccessory)
                    {
                        ((ScriptableBuildingAccessory)Player.localPlayer.playerBelt.belt[index].item.data).Spawn(Player.localPlayer, ModularBuildingManager.singleton.isInventory, index, 0, Player.localPlayer.transform.position);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableBook)
                    {
                        if (Player.localPlayer.playerAdditionalState.additionalState == "" && Player.localPlayer.playerBelt.InventoryOperationsAllowed())
                            Player.localPlayer.playerAdditionalState.CmdUseBook(Player.localPlayer, index, false);
                    }
                    else if (Player.localPlayer.playerBelt.belt[index].item.data is ScriptableDumbbell)
                    {
                        ((ScriptableDumbbell)Player.localPlayer.playerBelt.belt[index].item.data).Spawn(Player.localPlayer, ModularBuildingManager.singleton.isInventory, index, 0, Player.localPlayer.transform.position);
                    }
                    else
                    {
                        Player.localPlayer.playerBelt.CmdUseBeltItem(index);
                    }
                }
            }
        });
    }

    public void ManageBuilding(bool selector)
    {
        ModularBuildingManager.singleton.isInventory = selector;
        if (ModularBuildingManager.singleton.scriptableBuilding) Destroy(ModularBuildingManager.singleton.scriptableBuilding);
        if (ModularBuildingManager.singleton.scriptableBuildingAccessory) Destroy(ModularBuildingManager.singleton.scriptableBuildingAccessory);
        if (ModularBuildingManager.singleton.scriptableDoor) Destroy(ModularBuildingManager.singleton.scriptableDoor);
        if (ModularBuildingManager.singleton.scriptableExternal) Destroy(ModularBuildingManager.singleton.scriptableExternal);
        if (ModularBuildingManager.singleton.scriptableFence) Destroy(ModularBuildingManager.singleton.scriptableFence);
        if (ModularBuildingManager.singleton.scriptableGate) Destroy(ModularBuildingManager.singleton.scriptableGate);
        if (ModularBuildingManager.singleton.scriptableWall) Destroy(ModularBuildingManager.singleton.scriptableWall);

    }

    public void Reset(bool considerBuilding)
    {
        use = false;
        delete = false;
        equip = false;
        skillSlot = false;
        inventorySlot = false;
        equipmentSlot = false;
        warehouseSlot = false;
        librarySlot = false;
        fridgeSlot = false;
        //index = -1;
        panel.gameObject.SetActive(false);
        closeButton.image.raycastTarget = false;
        description.text = string.Empty;
        deleteButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(false);
        closeButton.image.enabled = false;
        if (considerBuilding)
        {
            if (MenuButton.singleton.panels[0].gameObject.activeInHierarchy)
            {
                if (ItemSlot.amount > 0 && ItemSlot.item.data is ScriptableBillboard
                   || ItemSlot.item.data is ScriptableBuilding
                   || ItemSlot.item.data is ScriptableBuildingAccessory
                   || ItemSlot.item.data is ScriptableConcrete
                   || ItemSlot.item.data is ScriptableFence
                   || ItemSlot.item.data is ScriptableFlag
                   || ItemSlot.item.data is ScriptableFurnace
                   || ItemSlot.item.data is ScriptableGate
                   || ItemSlot.item.data is ScriptableLight
                   || ItemSlot.item.data is ScriptableWall
                   || ItemSlot.item.data is ScriptableDoor
                   || ItemSlot.item.data is ScriptableWaterContainer)
                {
                    MenuButton.singleton.Close();
                }
            }
        }
        ItemSlot = new ItemSlot();
    }
}
