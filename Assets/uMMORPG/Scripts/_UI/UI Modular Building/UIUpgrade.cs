using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class UIUpgrade : MonoBehaviour
{
    private Player player;
    public static UIUpgrade singleton;
    public GameObject repairText;
    public GameObject panel;
    public Button closeButton;
    public Transform contentInventory;
    public Transform skinContent;

    public GameObject durabilityObject;
    public Image durabilityImage;
    public TextMeshProUGUI nameAndDurability;
    public Transform contentDurability;
    public Button addDurability;
    public Button manageButton;
    public UIInventorySlot inventorySlot;
    public Button equipButton;


    public Image munitionImage;
    public GameObject weaponImage;

    public RawImage weaponCameraImage;
    public Button buyButton;
    public GameObject buyObject;
    public TextMeshProUGUI skinPrice;

    private Item durabilityItem;

    public int selectedItemFromInventory;
    public int selectedSkin = -1;
    public int selectedAccessory = -1;

    public int selectedUIIndex = -1;
    public int selectedInventorySkin = -1;
    public List<GameObject> actualAccessories = new List<GameObject>();

    public Camera weaponCamera;

    private Upgrade upgrade;

    public void Start()
    {
        if (!singleton) singleton = this;
        munitionImage.gameObject.SetActive(false);

        player = Player.localPlayer;

        ManageVisibilityOFUIAccessories(false);

        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            panel.SetActive(false);
            for (int i = 0; i < skinContent.childCount; i++)
            {
                int index = i;
                Destroy(skinContent.GetChild(index).gameObject);
            }
            for (int i = 0; i < contentInventory.childCount; i++)
            {
                int index = i;
                Destroy(contentInventory.GetChild(index).gameObject);
            }
            weaponImage.SetActive(false);
            closeButton.image.raycastTarget = false;
            durabilityObject.gameObject.SetActive(false);
            repairText.gameObject.SetActive(false);
            munitionImage.gameObject.SetActive(false);

            for (int i = 0; i < WeaponHolder.singleton.weaponPresentation.Count; i++)
            {
                int index_i = i;
                WeaponObject weaponObject = WeaponHolder.singleton.weaponPresentation[index_i];
                weaponObject.Reset();
            }
            weaponCamera.enabled = false;
            BlurManager.singleton.Show();
        });

        //buyButton.onClick.RemoveAllListeners();
        //buyButton.onClick.AddListener(() =>
        //{
        //    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
        //    if (selectedItemFromInventory > -1 && selectedSkin > -1)
        //    {
        //        player.playerUpgrade.CmdSetSkin(selectedAccessory, selectedAccessory, selectedSkin, player.inventory.slots[selectedAccessory].item.data.name);
        //    }
        //});

        addDurability.onClick.RemoveAllListeners();
        addDurability.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerUpgrade.CmdRepairItem(selectedItemFromInventory, player.inventory.slots[selectedItemFromInventory].item.data.name);
        });


        //equipButton.onClick.RemoveAllListeners();
        //equipButton.onClick.AddListener(() =>
        //{
        //    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
        //    if (selectedItemFromInventory > -1 && selectedAccessory > -1)
        //    {
        //        player.playerUpgrade.CmdEquipAccessory(selectedItemFromInventory, Convert.ToInt32(player.inventory.slots[selectedAccessory].item.data.accessoriesType), selectedAccessory, player.inventory.slots[selectedItemFromInventory].item.data.name);
        //    }
        //});

        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(upgrade.netIdentity, upgrade.craftingAccessoryItem, closeButton);
        });
    }

    public void RefreshInventory(int indexInventory, bool firstInteraction)
    {
        weaponCamera.enabled = true;
        ManageVisibilityOFUIAccessories(indexInventory > -1 && (player.inventory.slots[indexInventory].item.data is WeaponItem && ((WeaponItem)player.inventory.slots[indexInventory].item.data).weaponToSpawn != null));
        UIUtils.BalancePrefabs(inventorySlot.gameObject, player.inventory.slots.Count, contentInventory);

        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            int index_i = i;
            UIInventorySlot slot = contentInventory.GetChild(index_i).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory.slots[index_i];

            if (firstInteraction)
            {
                slot.button.interactable = itemSlot.amount > 0 && ((itemSlot.item.data is WeaponItem && ((WeaponItem)itemSlot.item.data).weaponToSpawn != null) ||
                                           (itemSlot.item.currentDurability != itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)));
            }
            slot.tooltip.enabled = slot.button.interactable;
            slot.image.sprite = itemSlot.amount > 0 ? (itemSlot.item.data.skinImages.Count > 0 && itemSlot.item.skin > -1 ?
                                itemSlot.item.data.skinImages[itemSlot.item.skin] : itemSlot.item.data.image) : null;
            slot.image.color = itemSlot.amount > 0 ? Color.white : Color.clear;
            slot.image.preserveAspect = true;
            slot.tooltip.enabled = false;
            slot.dragAndDropable.enabled = false;
            slot.amountOverlay.SetActive(false);
            slot.amountText.text = itemSlot.amount.ToString();
            slot.cooldownCircle.gameObject.SetActive(false);
            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                equipButton.gameObject.SetActive(false);
                weaponImage.SetActive(true);
                if (player.inventory.slots[index_i].item.data is WeaponItem && ((WeaponItem)player.inventory.slots[index_i].item.data).weaponToSpawn != null)
                {
                    selectedItemFromInventory = index_i;
                    RefreshInventoryAfterItemClick(index_i);
                    EnableCorrectWeapon(((WeaponItem)player.inventory.slots[index_i].item.data).weaponToSpawn.name);
                    ManageUIAccessories(index_i);
                    RefreshWeaponView(selectedItemFromInventory);
                }
                if (player.inventory.slots[index_i].item.currentDurability != player.inventory.slots[index_i].item.data.maxDurability.Get(player.inventory.slots[index_i].item.durabilityLevel))
                {
                    RefreshDurabilityPanel(index_i);
                }
            });
        }
        if (indexInventory > -1)
        {
            contentInventory.GetChild(indexInventory).GetComponent<UIInventorySlot>().button.onClick.Invoke();
            equipButton.gameObject.SetActive(false);
        }

    }


    public void EnableCorrectWeapon(string weaponName)
    {
        for (int i = 0; i < WeaponHolder.singleton.weaponPresentation.Count; i++)
        {
            int index_i = i;
            WeaponHolder.singleton.weaponPresentation[index_i].gameObject.SetActive(WeaponHolder.singleton.weaponPresentation[index_i].name == weaponName);
        }
    }

    public void RefreshInventoryAfterItemClick(int inventoryIndex)
    {
        if (player.inventory.slots[inventoryIndex].amount > 0)
        {
            for (int i = 0; i < contentInventory.childCount; i++)
            {
                contentInventory.GetChild(i).GetComponent<Outline>().enabled = false;
                contentInventory.GetChild(i).GetComponent<Outline>().effectColor = Color.green;
            }

            UIInventorySlot selectedSlot = contentInventory.GetChild(inventoryIndex).GetComponent<UIInventorySlot>();
            selectedSlot.GetComponent<Outline>().enabled = true;
            selectedSlot.GetComponent<Outline>().effectColor = Color.green;

            ItemSlot slot = player.inventory.slots[inventoryIndex];
            string weaponName = ((WeaponItem)slot.item.data).weaponToSpawn.name;

            if (WeaponHolder.singleton)
            {
                for (int e = 0; e < WeaponHolder.singleton.weaponPresentation.Count; e++)
                {
                    int index_e = e;
                    if (WeaponHolder.singleton.weaponPresentation[index_e].name == weaponName)
                    {
                        for (int a = 0; a < player.inventory.slots.Count; a++)
                        {
                            int index_a = a;
                            if (player.inventory.slots[index_a].amount == 0) continue;
                            for (int u = 0; u < WeaponHolder.singleton.weaponPresentation[index_e].weaponPieces.Count; u++)
                            {
                                int index_u = u;
                                if (player.inventory.slots[index_a].item.data.name == WeaponHolder.singleton.weaponPresentation[index_e].weaponPieces[index_u].item.name)
                                {
                                    contentInventory.GetChild(index_a).GetComponent<Outline>().enabled = true;
                                    UIInventorySlot invSlot = contentInventory.GetChild(index_a).GetComponent<UIInventorySlot>();
                                    invSlot.button.interactable = true;
                                    contentInventory.GetChild(index_a).GetComponent<Outline>().effectColor = Color.yellow;
                                    invSlot.button.onClick.RemoveAllListeners();
                                    invSlot.button.onClick.AddListener(() =>
                                    {
                                        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                                        selectedAccessory = index_a;
                                        if (player.inventory.slots[index_a].item.data.skinImages.Count > 0)
                                        {
                                            buyButton.interactable = false;
                                            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Setted";
                                            buyObject.SetActive(false);
                                        }
                                        SpawnSkin(index_a, -1);
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void SpawnSkin(int inventoryIndex, int previousSelected)
    {
        bool alreadyPresent = false;
        ItemSlot slot = player.inventory.slots[inventoryIndex];
        equipButton.gameObject.SetActive(true);

        if (previousSelected <= -1)
        {
            UIUtils.BalancePrefabs(inventorySlot.gameObject, slot.item.data.skinImages.Count, skinContent);
            for (int a = 0; a < slot.item.data.skinImages.Count; a++)
            {
                int index_a = a;
                UIInventorySlot slot2 = skinContent.GetChild(index_a).GetComponent<UIInventorySlot>();

                if (!alreadyPresent) alreadyPresent = player.playerUpgrade.CheckIfAlreadyBuySkinForThisAccessory(inventoryIndex, index_a);
                else
                {
                    alreadyPresent = false;
                    if (!alreadyPresent) alreadyPresent = player.playerUpgrade.CheckIfAlreadyBuySkinForThisAccessory(inventoryIndex, index_a);
                }
                slot2.amountOverlay.SetActive(false);
                slot2.image.sprite = alreadyPresent || index_a == 0 ? player.inventory.slots[inventoryIndex].item.data.skinImages[index_a] : SkinManager.singleton.skins[index_a];
                slot2.image.color = Color.white;
                slot2.image.preserveAspect = true;
                slot2.tooltip.enabled = false;
                slot2.dragAndDropable.enabled = false;
                slot2.cooldownCircle.gameObject.SetActive(false);
                slot2.button.onClick.RemoveAllListeners();
                slot2.button.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    selectedSkin = index_a;
                    if (player.inventory.slots[inventoryIndex].item.skin == index_a)
                    {
                        buyButton.interactable = false;
                        buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Setted";
                        buyObject.SetActive(false);
                    }
                    else
                    {
                        bool alreadyPurchase = player.playerUpgrade.CheckIfAlreadyBuySkinForThisAccessory(inventoryIndex, index_a);
                        selectedInventorySkin = index_a;
                        buyButton.interactable = true;
                        buyButton.GetComponentInChildren<TextMeshProUGUI>().text = alreadyPurchase ? "Set" : "Buy";
                        if (index_a == 0)
                        {
                            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Set";
                            buyObject.SetActive(false);
                        }
                        else
                        {
                            buyObject.SetActive(!alreadyPurchase);
                        }
                        skinPrice.text = SkinManager.singleton.costToBuySkin.ToString();
                    }
                });
            }
        }
        else
        {
            UIInventorySlot skinSlot = skinContent.GetChild(previousSelected).GetComponent<UIInventorySlot>();
            skinSlot.image.sprite = player.inventory.slots[inventoryIndex].item.data.skinImages[previousSelected];

            if (player.inventory.slots[inventoryIndex].item.skin == previousSelected)
            {
                buyButton.interactable = false;
                buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Setted";
                buyObject.SetActive(false);
            }
            else
            {
                bool alreadyPurchase = player.playerUpgrade.CheckIfAlreadyBuySkinForThisAccessory(inventoryIndex, previousSelected);
                buyButton.interactable = true;
                buyButton.GetComponentInChildren<TextMeshProUGUI>().text = alreadyPurchase ? "Set" : "Buy";
                buyObject.SetActive(!alreadyPurchase);
                skinPrice.text = SkinManager.singleton.costToBuySkin.ToString();
            }
        }
    }


    public void RefreshDurabilityPanel(int refresh)
    {
        bool canRepair = true;
        if (player.inventory.slots[refresh].item.currentDurability < player.inventory.slots[refresh].item.data.maxDurability.Get(player.inventory.slots[refresh].item.durabilityLevel))
        {
            durabilityImage.sprite = player.inventory.slots[refresh].item.data.skinImages.Count > 0 ? player.inventory.slots[refresh].item.data.skinImages[player.inventory.slots[refresh].item.skin] : player.inventory.slots[refresh].item.data.image;
            durabilityImage.preserveAspect = true;
            durabilityObject.SetActive(true);
            repairText.gameObject.SetActive(true);
            UIUtils.BalancePrefabs(inventorySlot.gameObject, player.inventory.slots[refresh].item.data.repairItems.Count, contentDurability);
            for (int e = 0; e < player.inventory.slots[refresh].item.data.repairItems.Count; e++)
            {
                int index_e = e;
                UIInventorySlot slot1 = contentDurability.GetChild(index_e).GetComponent<UIInventorySlot>();
                slot1.image.sprite = player.inventory.slots[refresh].item.data.repairItems[index_e].items.image;
                slot1.image.preserveAspect = true;
                durabilityItem = new Item(player.inventory.slots[refresh].item.data.repairItems[index_e].items);
                slot1.amountOverlay.SetActive(player.inventory.CountItem(durabilityItem) > 0);
                slot1.amountText.text = player.inventory.CountItem(durabilityItem).ToString() + " / " + player.inventory.slots[refresh].item.data.repairItems[index_e].amount;
                slot1.cooldownCircle.gameObject.SetActive(false);
                slot1.registerItem.index = refresh;
                slot1.durabilitySlider.fillAmount = player.inventory.slots[refresh].item.data.maxDurability.baseValue > 0 ? ((float)player.inventory.slots[refresh].item.currentDurability / (float)player.inventory.slots[refresh].item.data.maxDurability.Get(player.inventory.slots[refresh].item.durabilityLevel)) : 0;
                slot1.unsanitySlider.fillAmount = player.inventory.slots[refresh].item.data.maxUnsanity > 0 ? ((float)player.inventory.slots[refresh].item.currentUnsanity / (float)player.inventory.slots[refresh].item.data.maxUnsanity) : 0;



                if (canRepair) canRepair = player.inventory.CountItem(durabilityItem) >= player.inventory.slots[refresh].item.data.repairItems[index_e].amount;

                if (player.inventory.CountItem(durabilityItem) >= player.inventory.slots[refresh].item.data.repairItems[index_e].amount)
                {
                    slot1.amountText.color = Color.white;
                }
                else
                {
                    slot1.amountText.color = Color.red;
                }
            }
            addDurability.interactable = canRepair;
        }
        else
        {
            durabilityObject.SetActive(false);
            repairText.gameObject.SetActive(false);
            UIUtils.BalancePrefabs(inventorySlot.gameObject, 0, contentDurability);
        }
    }

    public void Open(Upgrade upgrades)
    {
        player = Player.localPlayer;
        if (!player) return;
        upgrade = upgrades;

        munitionImage.gameObject.SetActive(false);
        panel.gameObject.SetActive(true);
        closeButton.image.raycastTarget = true;

        RefreshInventory(-1, true);
    }

    public void DestroyContentObject(Transform content)
    {
        for (int i = 0; i < content.childCount; i++)
        {
            int index_i = i;
            Destroy(content.GetChild(index_i).gameObject);
        }
    }

    public void ManageVisibilityOFUIAccessories(bool condition)
    {
        for (int i = 0; i < actualAccessories.Count; i++)
        {
            int index_i = i;
            actualAccessories[index_i].SetActive(condition);
        }
    }

    public void ManageUIAccessories(int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        for (int i = 0; i < actualAccessories.Count; i++)
        {
            int index_i = i;
            actualAccessories[index_i].SetActive((index_i + 1) <= slot.item.accessories.Length);

            if (actualAccessories[index_i].activeInHierarchy)
            {
                UIInventorySlot invSlot = actualAccessories[index_i].GetComponent<UIInventorySlot>();
                invSlot.image.sprite = slot.item.accessories[index_i].data.skinImages.Count > 0 ? slot.item.accessories[index_i].data.skinImages[slot.item.accessories[index_i].skin] : slot.item.accessories[index_i].data.image;
                invSlot.image.preserveAspect = true;
                invSlot.amountOverlay.SetActive(slot.item.accessories[index_i].bulletsRemaining > 0);
                invSlot.amountText.text = slot.item.accessories[index_i].bulletsRemaining.ToString();

                invSlot.image.color = Color.white;

                invSlot.tooltip.enabled = false;
                invSlot.dragAndDropable.enabled = false;

                invSlot.cooldownCircle.gameObject.SetActive(false);

                invSlot.button.onClick.RemoveAllListeners();
                invSlot.button.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    //player.playerUpgrade.CmdUnequipAccessories(inventoryIndex, index_i);
                });
            }
        }
    }

    public void RefreshWeaponView(int inventoryIndex)
    {
        if (WeaponHolder.singleton)
        {
            for (int i = 0; i < WeaponHolder.singleton.weaponPresentation.Count; i++)
            {
                int index_i = i;
                WeaponObject weaponObject = WeaponHolder.singleton.weaponPresentation[index_i];
                if (weaponObject.name == player.inventory.slots[inventoryIndex].item.data.weaponToSpawn.name)
                {
                    for (int j = 0; j < WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces.Count; j++)
                    {
                        int index_j = j;
                        if (WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].isPreset)
                        {
                            if (ItemContainsItem(inventoryIndex, WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].item.name) == -1)
                            {
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.gameObject.SetActive(true);
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.material = SkinManager.singleton.weaponAccessoryMaterials[0];
                            }
                            else
                            {
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.gameObject.SetActive(true);
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.material =
                                SkinManager.singleton.weaponAccessoryMaterials[player.inventory.slots[inventoryIndex].item.accessories[ItemContainsItem(inventoryIndex, WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].item.name)].skin];
                            }
                        }
                        else
                        {
                            if (ItemContainsItem(inventoryIndex, WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].item.name) == -1)
                            {
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.gameObject.SetActive(false);
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.material = SkinManager.singleton.weaponAccessoryMaterials[0];
                            }
                            else
                            {
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.gameObject.SetActive(true);
                                WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].renderer.material =
                                SkinManager.singleton.weaponAccessoryMaterials[player.inventory.slots[inventoryIndex].item.accessories[ItemContainsItem(inventoryIndex, WeaponHolder.singleton.weaponPresentation[index_i].weaponPieces[index_j].item.name)].skin];
                            }

                        }
                    }
                }
            }
        }
    }

    public int ItemContainsItem(int inventoryIndex, string accessoryName)
    {
        int contain = -1;
        for (int i = 0; i < player.inventory.slots[inventoryIndex].item.accessories.Length; i++)
        {
            int index_i = i;
            if (player.inventory.slots[inventoryIndex].item.accessories[index_i].name == accessoryName)
            {
                contain = index_i;
            }
        }

        return contain;
    }
}
