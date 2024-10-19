using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;

public partial class Inventory
{
    public int FindItemInInventory(ScriptableItem item)
    {
        int amountToReturn = 0;
        for (int i = 0; i < Player.localPlayer.inventory.slots.Count; i++)
        {
            int index_i = i;
            if (Player.localPlayer.inventory.slots[index_i].amount == 0) continue;
            if (Player.localPlayer.inventory.slots[index_i].item.data.name == item.name)
            {
                amountToReturn += Player.localPlayer.inventory.slots[index_i].amount;
            }
        }
        return amountToReturn;
    }
}

public partial class Player
{
    [Command]
    public void CmdCraftBuilding(string itemToManage, int amount, int index)
    {
        Item itm = new Item();

        if (ScriptableItem.All.TryGetValue(itemToManage.GetStableHashCode(), out ScriptableItem itemData))
        {
            itm = new Item(itemData);
            if (inventory.CanAddItem(itm, amount))
            {
                for (int i = 0; i < itemData.itemtoCraftBuilding.Count; i++)
                {
                    int index_i = i;
                    itm = new Item(itemData.itemtoCraftBuilding[index_i].itemAndAmount.item);
                    if (inventory.CountItem(itm) < itemData.itemtoCraftBuilding[index_i].itemAndAmount.amount) return;
                }
            }

            for (int i = 0; i < itemData.itemtoCraftBuilding.Count; i++)
            {
                int index_i = i;
                itm = new Item(itemData.itemtoCraftBuilding[index_i].itemAndAmount.item);
                inventory.RemoveItem((itm), itemData.itemtoCraftBuilding[index_i].itemAndAmount.amount);
            }
            inventory.AddItem(new Item(itemData), amount);
            TargetRefreshPanelUIBuilinding(index);
        }
    }

    [TargetRpc]
    public void TargetRefreshPanelUIBuilinding(int index)
    {
        if (UIModularBuildingSelector.singleton)
        {
            UIModularBuildingSelector.singleton.SpawnObjectInPanel(UIModularBuildingSelector.singleton.selectedBuilding);
            UIModularBuildingSelector.singleton.itemSelectorContent.GetChild(index).GetComponent<BuildingSlot>().button.onClick.Invoke();
        }
    }
}

public class UIModularBuildingSelector : MonoBehaviour
{
    public static UIModularBuildingSelector singleton;
    public GameObject panel;


    public Building building;
    public Building accessory;
    public Building outdoor;

    public Button mainButton;
    public Button buildingButton;
    public Button accessoryButton;
    public Button outdoorButton;

    public GameObject selectorBuildingPanel;
    public Transform itemSelectorContent;
    public Transform itemSelectedContent;

    public GameObject selectedPanel;
    public TextMeshProUGUI buildingText;
    public Button useButton;
    public Button craftButton;
    public Slider slider;
    public TextMeshProUGUI sliderText;

    public Button closeButton;

    public BuildingSlot slot;

    private int selected = -1;
    private int inv = -1;

    public UsableItem selectedItem;
    public UsableItem selectedItemChildren;
    public int selectedItemMain = -1;
    public int selectedItemChild = -1;
    public int selectedBuilding = -1;
    public ScriptableItem actualItem;

    void Start()
    {
        if (!singleton) singleton = this;
        Invoke(nameof(CheckPanel), 1.0f);

        useButton.gameObject.SetActive(false);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            ResetChoice();
            selectorBuildingPanel.gameObject.SetActive(false);
        });

        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            ResetChoice();
            selectedPanel.SetActive(!selectedPanel.activeInHierarchy);
            UIUtils.BalancePrefabs(slot.gameObject, 0, itemSelectedContent);
        });
        buildingButton.onClick.RemoveAllListeners();
        buildingButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            selected = 0;
            selectorBuildingPanel.gameObject.SetActive(true);
            UIUtils.BalancePrefabs(slot.gameObject, 0, itemSelectedContent);
            selectedBuilding = 0;
            SpawnObjectInPanel(0);
        });
        accessoryButton.onClick.RemoveAllListeners();
        accessoryButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            selected = 1;
            selectorBuildingPanel.gameObject.SetActive(true);
            UIUtils.BalancePrefabs(slot.gameObject, 0, itemSelectedContent);
            selectedBuilding = 1;
            SpawnObjectInPanel(1);
        });
        outdoorButton.onClick.RemoveAllListeners();
        outdoorButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            selected = 2;
            selectorBuildingPanel.gameObject.SetActive(true);
            UIUtils.BalancePrefabs(slot.gameObject, 0, itemSelectedContent);
            selectedBuilding = 2;
            SpawnObjectInPanel(2);
        });

        slider.onValueChanged.AddListener(delegate
        {
            if (selectedItemChildren == null)
                ManageButton(selectedItem);
            else
                ManageButton(selectedItemChildren);
        });

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(2);
            if (selectedItemChildren)
            {
                ModularBuildingManager.singleton.ClearAllCache();
                Use(selectedItemChildren,true);
                ResetChoice();
            }
            else if (selectedItem)
            {
                ModularBuildingManager.singleton.ClearAllCache();
                Use(selectedItem,true);
                ResetChoice();
            }
            closeButton.onClick.Invoke();
        });

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(3);
            ScriptableItem sel = null;
            if (selectedItem != null && selectedItemChildren == null)
            {
                sel = selectedItem;
            }
            else if (selectedItem == null && selectedItemChildren != null)
            {
                sel = selectedItemChildren;
            }
            else
                sel = selectedItemChildren;

            Player.localPlayer.CmdCraftBuilding(sel.name, Convert.ToInt32(slider.value), selectedItemMain);
        });

    }

    public void ResetChoice()
    {
        selectedItem = null;
        selectedItemChildren = null;
        selectedItemMain = -1;
        selectedItemChild = -1;
        inv = -1;
        //selected = -1;

        buildingText.text = string.Empty;
        slider.value = 1;
        slider.gameObject.SetActive(false);
    }

    public void CheckPanel()
    {
        if (Player.localPlayer)
        {
            panel.SetActive(true);
        }
        else
        {
            Invoke(nameof(CheckPanel), 1.0f);
        }
    }

    public void ManageButton(UsableItem itm)
    {
        if (itm == null) return;
        Item item = new Item();
        bool canCraft = true;
        for (int i = 0; i < itm.itemtoCraftBuilding.Count; i++)
        {
            int index_i = i;
            item = new Item(itm.itemtoCraftBuilding[index_i].itemAndAmount.item);
            if (Player.localPlayer.inventory.CountItem(item) < (itm.itemtoCraftBuilding[index_i].itemAndAmount.amount * slider.value))
            {
                canCraft = false;
                BuildingSlot slot = itemSelectedContent.GetChild(index_i).GetComponent<BuildingSlot>();
                slot.overlayAmount.text = (itm.itemtoCraftBuilding[index_i].itemAndAmount.amount * slider.value).ToString();
                slot.overlayAmount.color = Color.red;
            }
            else
            {
                BuildingSlot slot = itemSelectedContent.GetChild(index_i).GetComponent<BuildingSlot>();
                slot.overlayAmount.text = (itm.itemtoCraftBuilding[index_i].itemAndAmount.amount * slider.value).ToString();
                slot.overlayAmount.color = Color.white;
            }
        }
        craftButton.gameObject.SetActive(canCraft);
        craftButton.interactable = Player.localPlayer.inventory.CanAddItem(new Item(itm), Convert.ToInt32(slider.value));
        useButton.gameObject.SetActive(false);
        sliderText.text = slider.value.ToString();
    }

    public void SpawnObjectInPanel(int sel)
    {
        craftButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
        Building build = SpawnFirstLineOfObject(sel);
        selectorBuildingPanel.gameObject.SetActive(true);
        selectedItem = null;
        UIUtils.BalancePrefabs(slot.gameObject, build.usableItems.Count, itemSelectorContent);
        for (int i = 0; i < build.usableItems.Count; i++)
        {
            int index = i;
            BuildingSlot slot = itemSelectorContent.GetChild(index).GetComponent<BuildingSlot>();
            slot.image.sprite = build.usableItems[index].item.image;
            slot.image.preserveAspect = true;
            slot.overlayObject.SetActive(build.usableItems[index].children.Count > 0);
            slot.overlayAmount.text = Player.localPlayer.inventory.FindItemInInventory(build.usableItems[index].item).ToString();
            useButton.gameObject.SetActive(false);
            craftButton.gameObject.SetActive(false);

            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() =>
            {
                selectedItem = null;
                selectedItemMain = -1;
                selectedItemChild = -1;
                selectedItemChildren = null;
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                slider.gameObject.SetActive(false);
                useButton.gameObject.SetActive(Player.localPlayer.inventory.FindItemInInventory(build.usableItems[index].item) > 0);
                craftButton.gameObject.SetActive(false);
                selectedItemMain = index;
                selectedItem = build.usableItems[index].item;
                buildingText.text = build.usableItems[index].item.name;

                if (build.usableItems[index].children.Count == 0)
                {
                    int amount = Player.localPlayer.inventory.FindItemInInventory(build.usableItems[index].item);
                    slider.value = 1;
                    selectedItemChildren = null;
                    slider.gameObject.SetActive(false);
                    //UIUtils.BalancePrefabs(slot.gameObject, 0, itemSelectedContent);
                    // Do stuff with object;
                    if (amount == 0)
                    {
                        slider.gameObject.SetActive(true);
                        slider.value = 1;
                        bool craft = true;
                        UIUtils.BalancePrefabs(slot.gameObject, build.usableItems[index].item.itemtoCraftBuilding.Count, itemSelectedContent);
                        for (int e = 0; e < build.usableItems[index].item.itemtoCraftBuilding.Count; e++)
                        {
                            int index_e = e;
                            slider.gameObject.SetActive(true);
                            BuildingSlot slot = itemSelectedContent.GetChild(index_e).GetComponent<BuildingSlot>();
                            slot.image.sprite = build.usableItems[index].item.itemtoCraftBuilding[index_e].itemAndAmount.item.image;
                            slot.image.preserveAspect = true;
                            slot.button.interactable = false;
                            slot.overlayObject.SetActive(true);
                            slot.overlayAmount.text = build.usableItems[index].item.itemtoCraftBuilding[index_e].itemAndAmount.amount.ToString();

                            inv = Player.localPlayer.inventory.FindItemInInventory(build.usableItems[index].item.itemtoCraftBuilding[index_e].itemAndAmount.item);
                            if (inv < (build.usableItems[index].item.itemtoCraftBuilding[index_e].itemAndAmount.amount * slider.value))
                            {
                                slot.overlayAmount.color = Color.red;
                                craft = false;
                            }
                            else
                            {
                                slot.overlayAmount.color = Color.white;
                            }
                        }
                        craftButton.gameObject.SetActive(craft);
                    }
                    else
                    {
                        UIUtils.BalancePrefabs(slot.gameObject, 0, itemSelectedContent);
                        slider.gameObject.SetActive(false);
                        useButton.gameObject.SetActive(true);
                    }
                }
                else
                {

                    slider.gameObject.SetActive(false);
                    UIUtils.BalancePrefabs(slot.gameObject, build.usableItems[index].children.Count, itemSelectedContent);
                    for (int e = 0; e < build.usableItems[index].children.Count; e++)
                    {
                        int index_e = e;
                        BuildingSlot slot = itemSelectedContent.GetChild(index_e).GetComponent<BuildingSlot>();
                        slot.image.sprite = build.usableItems[index].children[index_e].image;
                        slot.image.preserveAspect = true;
                        slot.button.interactable = true;
                        slot.overlayObject.SetActive(true);
                        int amount = Player.localPlayer.inventory.FindItemInInventory(build.usableItems[index].children[index_e]);
                        slot.overlayAmount.text = amount.ToString();
                        slot.overlayAmount.color = Color.white;
                        slot.button.onClick.RemoveAllListeners();
                        slot.button.onClick.AddListener(() =>
                        {
                            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                            buildingText.text = build.usableItems[index].children[index_e].name;
                            int amount = Player.localPlayer.inventory.FindItemInInventory(build.usableItems[index].children[index_e]);
                            selectedItemChild = index_e;
                            selectedItemChildren = build.usableItems[index].children[index_e];
                            if (amount == 0)
                            {
                                slider.gameObject.SetActive(true);
                                useButton.gameObject.SetActive(false);
                                slider.value = 1;
                                bool craft = true;
                                UIUtils.BalancePrefabs(slot.gameObject, build.usableItems[index].children[index_e].itemtoCraftBuilding.Count, itemSelectedContent);
                                for (int a = 0; a < build.usableItems[index].children[index_e].itemtoCraftBuilding.Count; a++)
                                {
                                    int index_a = a;
                                    slider.gameObject.SetActive(true);
                                    BuildingSlot slot = itemSelectedContent.GetChild(index_a).GetComponent<BuildingSlot>();
                                    slot.image.sprite = build.usableItems[index].children[index_e].itemtoCraftBuilding[index_a].itemAndAmount.item.image;
                                    slot.image.preserveAspect = true;
                                    slot.button.interactable = false;
                                    slot.overlayObject.SetActive(true);
                                    slot.overlayAmount.text = build.usableItems[index].children[index_e].itemtoCraftBuilding[index_a].itemAndAmount.amount.ToString();
                                    inv = Player.localPlayer.inventory.FindItemInInventory(build.usableItems[index].children[index_e].itemtoCraftBuilding[index_a].itemAndAmount.item);
                                    if (inv < (build.usableItems[index].children[index_e].itemtoCraftBuilding[index_a].itemAndAmount.amount * slider.value))
                                    {
                                        slot.overlayAmount.color = Color.red;
                                        craft = false;
                                    }
                                    else
                                    {
                                        slot.overlayAmount.color = Color.white;
                                    }
                                }

                                craftButton.gameObject.SetActive(craft);
                            }
                            else
                            {
                                UIUtils.BalancePrefabs(slot.gameObject, 0, itemSelectedContent);
                                slider.gameObject.SetActive(false);
                                useButton.gameObject.SetActive(true);
                            }
                        });
                    }
                }
            });
        }
    }

    public void Use(ScriptableItem item, bool isInventory)
    {
        ModularBuildingManager.singleton.isInventory = isInventory;
        actualItem = null;
        if (isInventory)
        {
            for (int i = 0; i < Player.localPlayer.inventory.slots.Count; i++)
            {
                if (Player.localPlayer.inventory.slots[i].amount == 0) continue;
                if (Player.localPlayer.inventory.slots[i].item.data.name == item.name)
                {
                    switch (Player.localPlayer.inventory.slots[i].item.data.GetType().ToString())
                    {
                        case "ScriptableBuilding":
                            ((ScriptableBuilding)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableDoor":
                            ((ScriptableDoor)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableWall":
                            ((ScriptableWall)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableBuildingAccessory":
                            ((ScriptableBuildingAccessory)Player.localPlayer.inventory.slots[i].item.data).Spawn(Player.localPlayer, true, i, 0, Player.localPlayer.transform.position);
                            break;
                        case "ScriptableFence":
                            ((ScriptableFence)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableGate":
                            ((ScriptableGate)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableLight":
                            ((ScriptableLight)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableConcrete":
                            ((ScriptableConcrete)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableBillboard":
                            ((ScriptableBillboard)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableFlag":
                            ((ScriptableFlag)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableFurnace":
                            ((ScriptableFurnace)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableWaterContainer":
                            ((ScriptableWaterContainer)Player.localPlayer.inventory.slots[i].item.data).Use(Player.localPlayer, i);
                            break;

                    }
                    actualItem = Player.localPlayer.inventory.slots[i].item.data;
                    return;
                }
            }
        }
        else
        {
            for (int i = 0; i < Player.localPlayer.playerBelt.belt.Count; i++)
            {
                if (Player.localPlayer.playerBelt.belt[i].amount == 0) continue;
                if (Player.localPlayer.playerBelt.belt[i].item.data.name == item.name)
                {
                    switch (Player.localPlayer.playerBelt.belt[i].item.data.GetType().ToString())
                    {
                        case "ScriptableBuilding":
                            ((ScriptableBuilding)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableDoor":
                            ((ScriptableDoor)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableWall":
                            ((ScriptableWall)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableBuildingAccessory":
                            ((ScriptableBuildingAccessory)Player.localPlayer.playerBelt.belt[i].item.data).Spawn(Player.localPlayer, true, i, 0, Player.localPlayer.transform.position);
                            break;
                        case "ScriptableFence":
                            ((ScriptableFence)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableGate":
                            ((ScriptableGate)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableLight":
                            ((ScriptableLight)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableConcrete":
                            ((ScriptableConcrete)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableBillboard":
                            ((ScriptableBillboard)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableFlag":
                            ((ScriptableFlag)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableFurnace":
                            ((ScriptableFurnace)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;
                        case "ScriptableWaterContainer":
                            ((ScriptableWaterContainer)Player.localPlayer.playerBelt.belt[i].item.data).Use(Player.localPlayer, i);
                            break;

                    }
                    actualItem = Player.localPlayer.playerBelt.belt[i].item.data;
                    return;
                }
            }

        }
    }

    public Building SpawnFirstLineOfObject(int sel)
    {
        return sel == 0 ? building : sel == 1 ? accessory : outdoor;
    }
}
