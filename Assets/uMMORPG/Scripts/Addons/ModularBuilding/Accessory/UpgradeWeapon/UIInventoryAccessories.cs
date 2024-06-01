using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryAccessories : MonoBehaviour
{
    private Player player;
    public static UIInventoryAccessories singleton;
    public int accessoryType = -1;
    public int weaponIndex = -1;
    public int accessorySelected = -1;
    public string itemName = string.Empty;
    public Item permanentAccessory;
    public string accessoryName;
    public List<int> inventoryIndex = new List<int>();
    public GameObject panel;
    public Transform content;
    public GameObject objectToSpawn;
    public Button closeButton;

    public Transform skinContent;
    public GameObject skinObjectToSpawn;

    public int selectedAccessoryToBuy;
    public int selectedSkinToBuy;
    public string selectedAccessoryName;

    public int contentWeaponSelected;

    [HideInInspector] public int previous;

    void Start()
    {
        if (!singleton) singleton = this;

        //UIUpgrade.singleton.buttonBuySkin.GetComponentInChildren<TextMeshProUGUI>().text = SkinManager.singleton.costToBuySkin.ToString();
    }

    public void SpawnItemAccessoryInInventory()
    {
        //    if (!player) player = Player.localPlayer;
        //    if (!player) return;

        //    UIUtils.BalancePrefabs(objectToSpawn, inventoryIndex.Count, content);
        //    for (int e = 0; e < inventoryIndex.Count; e++)
        //    {
        //        int index_e = e;
        //        AccessoriesSlot slot = content.GetChild(index_e).GetComponent<AccessoriesSlot>();
        //        slot.button.onClick.RemoveAllListeners();
        //        slot.button.onClick.AddListener(() =>
        //        {
        //            UIUpgrade.singleton.selectedAccessoriesFromInventory = inventoryIndex[index_e];
        //            UIUpgrade.singleton.skinContent.SetActive(true);
        //            UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(true);
        //            UIUpgrade.singleton.buttonBuySkin.interactable = player.itemMall.coins >= SkinManager.singleton.costToBuySkin;
        //            UIUpgrade.singleton.contentAttibuteToUpgrade.GetChild(accessorySelected).GetComponent<UIUpgradeItemDetails>().equip.interactable = true;
        //            slot.ManageShadow(true);


        //            // SKIN MANAGER
        //            skinContent.parent.parent.gameObject.SetActive(true);
        //            UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(false);
        //            UIUpgrade.singleton.SetSkin.gameObject.SetActive(true);
        //            UIUtils.BalancePrefabs(skinObjectToSpawn, player.inventory.slots[inventoryIndex[index_e]].item.data.skinImages.Count, skinContent);
        //            skinContent.GetChild(previous).GetComponent<AccessorySkin>().ManageShadow(true);
        //            previous = player.inventory.slots[accessoryType].item.personalAccessoryAttribute.actualIndex;
        //            for (int y = 0; y < player.inventory.slots[inventoryIndex[index_e]].item.data.skinImages.Count; y++)
        //            {
        //                int index_y = y;
        //                AccessorySkin skin = skinContent.GetChild(index_y).GetComponent<AccessorySkin>();
        //                skin.button.image.sprite = SkinManager.singleton.skins[index_y];
        //                skin.button.interactable = true;
        //                skin.button.onClick.RemoveAllListeners();
        //                skin.button.onClick.AddListener(() =>
        //                {
        //                    skinContent.GetChild(previous).GetComponent<AccessorySkin>().ManageShadow(false);
        //                    previous = index_y;
        //                    skin.ManageShadow(true);

        //                    UIUpgrade.singleton.selectedAccessoryToBuy = inventoryIndex[index_e];
        //                    UIUpgrade.singleton.selectedSkinToBuy = index_y;
        //                    UIUpgrade.singleton.selectedAccessoryFromPanel = index_e;
        //                    UIUpgrade.singleton.selectedAccessoryName = accessoryName;

        //                    if (player.inventory.slots[inventoryIndex[index_e]].amount > 0 && player.inventory.slots[inventoryIndex[index_e]].item.data.name == accessoryName)
        //                    {
        //                        if (player.inventory.slots[inventoryIndex[index_e]].item.personalAccessoryAttribute.skinIndex.Contains(index_y))
        //                        {
        //                            //CMD SKIN
        //                            UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(false);
        //                            UIUpgrade.singleton.SetSkin.gameObject.SetActive(true);
        //                            UIUpgrade.singleton.SetSkin.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = player.inventory.slots[inventoryIndex[index_e]].item.personalAccessoryAttribute.actualIndex == index_y ? "Setted" : "Set";

        //                            UIUpgrade.singleton.SetSkin.onClick.RemoveAllListeners();
        //                            UIUpgrade.singleton.SetSkin.onClick.AddListener(() =>
        //                            {
        //                                player.playerUpgrade.CmdSetSkin(inventoryIndex[index_e], index_y, player.inventory.slots[inventoryIndex[index_e]].item.data.name, index_e);
        //                                //UIUpgrade.singleton.ResetParameters();
        //                            });

        //                        }
        //                        else
        //                        {
        //                            //CMD BUY SKIN
        //                            UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(true);
        //                            UIUpgrade.singleton.SetSkin.gameObject.SetActive(false);

        //                            UIUpgrade.singleton.buttonBuySkin.onClick.RemoveAllListeners();
        //                            UIUpgrade.singleton.buttonBuySkin.onClick.AddListener(() =>
        //                            {
        //                                player.playerUpgrade.CmdBuySkin(inventoryIndex[index_e], index_y, player.inventory.slots[inventoryIndex[index_e]].item.data.name, index_e);
        //                                //UIUpgrade.singleton.ResetParameters();
        //                            });

        //                        }
        //                    }
        //                    else
        //                    {
        //                        UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(false);
        //                        UIUpgrade.singleton.SetSkin.gameObject.SetActive(false);
        //                        //UIUpgrade.singleton.ResetParameters();
        //                    }
        //                });
        //            }

        //            for (int a = 0; a < content.childCount; a++)
        //            {
        //                int index_a = a;
        //                if (index_a != index_e)
        //                    content.GetChild(index_a).GetComponent<AccessoriesSlot>().ManageShadow(false);
        //            }
        //        });
        //        slot.image.sprite = player.inventory.slots[inventoryIndex[index_e]].item.data.skinImages[player.inventory.slots[inventoryIndex[index_e]].item.personalAccessoryAttribute.actualIndex];
        //    }
        //}

        //public void SpawnItemAccessoryInInventoryStandard()
        //{
        //    if (!player) player = Player.localPlayer;
        //    if (!player) return;

        //    UIUtils.BalancePrefabs(objectToSpawn, inventoryIndex.Count, content);
        //    for (int e = 0; e < inventoryIndex.Count; e++)
        //    {
        //        int index_e = e;
        //        AccessoriesSlot slot = content.GetChild(index_e).GetComponent<AccessoriesSlot>();
        //        slot.image.sprite = player.inventory.slots[inventoryIndex[index_e]].item.data.skinImages[player.inventory.slots[inventoryIndex[index_e]].item.personalAccessoryAttribute.actualIndex];
        //        slot.button.onClick.RemoveAllListeners();
        //        slot.button.onClick.AddListener(() =>
        //        {
        //            UIUpgrade.singleton.selectedAccessoriesFromInventory = inventoryIndex[index_e];
        //            UIUpgrade.singleton.contentAttibuteToUpgrade.GetChild(accessorySelected).GetComponent<UIUpgradeItemDetails>().equip.interactable = true;
        //            slot.ManageShadow(true);


        //            // SKIN MANAGER
        //            skinContent.parent.parent.gameObject.SetActive(true);

        //            ClickButtonAccessoryStandard(index_e);
        //        });
        //    }

        //}

        //public void CloseButtonEventsAssign()
        //{
        //    closeButton.onClick.RemoveAllListeners();
        //    closeButton.onClick.AddListener(() =>
        //    {
        //        panel.SetActive(false);
        //        UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(false);
        //        foreach (Transform t in skinContent)
        //        {
        //            t.GetComponent<AccessorySkin>().ManageShadow(false);
        //            Destroy(t.gameObject);
        //        }
        //        permanentAccessory = new Item();
        //        weaponIndex = -1;
        //        if (UIUpgrade.singleton.contentAttibuteToUpgrade.GetChild(accessorySelected).GetComponentInChildren<UIUpgradeItemDetails>().equip.GetComponentInChildren<TextMeshProUGUI>().text == "Equip")
        //        {
        //            UIUpgrade.singleton.contentAttibuteToUpgrade.GetChild(accessorySelected).GetComponent<UIUpgradeItemDetails>().equip.interactable = false;
        //        }
        //        inventoryIndex.Clear();
        //    });


        //}

        //public void ClearSkinContents()
        //{
        //    foreach (Transform t in skinContent)
        //    {
        //        t.GetComponent<AccessorySkin>().ManageShadow(false);
        //        Destroy(t.gameObject);
        //    }
        //}

        //public void Open()
        //{
        //    panel.SetActive(true);

        //    CacheInventoryIndexOfAccessory();
        //    CloseButtonEventsAssign();
        //    ClearSkinContents();
        //    SpawnItemAccessoryInInventory();
        //}

        //public void ClickButtonAccessoryStandard(int index_e)
        //{
        //    if (player.inventory.slots[inventoryIndex[index_e]].item.data.skinImages.Count > 1)
        //    {
        //        skinContent.parent.parent.gameObject.SetActive(true);

        //        UIUtils.BalancePrefabs(skinObjectToSpawn, player.inventory.slots[inventoryIndex[index_e]].item.data.skinImages.Count, skinContent);
        //        skinContent.GetChild(previous).GetComponent<AccessorySkin>().ManageShadow(true);
        //        previous = player.inventory.slots[accessoryType].item.personalAccessoryAttribute.actualIndex;
        //        for (int y = 0; y < player.inventory.slots[inventoryIndex[index_e]].item.data.skinImages.Count; y++)
        //        {

        //            int index_y = y;
        //            AccessorySkin skin = skinContent.GetChild(index_y).GetComponent<AccessorySkin>();
        //            skin.button.image.sprite = SkinManager.singleton.skins[index_y];
        //            skin.button.interactable = true;
        //            skin.button.onClick.RemoveAllListeners();
        //            skin.button.onClick.AddListener(() =>
        //            {
        //                int slot = -1;
        //                skinContent.GetChild(previous).GetComponent<AccessorySkin>().ManageShadow(false);
        //                previous = index_y;
        //                skin.ManageShadow(true);
        //                if (permanentAccessory.name != string.Empty)
        //                {
        //                    if (player.inventory.slots[weaponIndex].amount > 0 && player.inventory.slots[weaponIndex].item.data.name == permanentAccessory.name)
        //                    {
        //                        for (int i = 0; i < player.inventory.slots[weaponIndex].item.actualEquipment.Count; i++)
        //                        {
        //                            int index = i;
        //                            if (player.inventory.slots[weaponIndex].item.actualEquipment[index].accessoryName == accessoryName &&
        //                                player.inventory.slots[weaponIndex].item.actualEquipment[index].skinIndex.Contains(index_y))
        //                            {
        //                                slot = index;
        //                            }
        //                        }
        //                        UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(slot == -1);
        //                        UIUpgrade.singleton.SetSkin.gameObject.SetActive(slot != -1);

        //                        if (slot == -1)
        //                        {
        //                            //CMD BUY SKIN
        //                            UIUpgrade.singleton.buttonBuySkin.onClick.RemoveAllListeners();
        //                            UIUpgrade.singleton.buttonBuySkin.onClick.AddListener(() =>
        //                            {
        //                                player.playerUpgrade.CmdBuySkinForStandard(weaponIndex, index_y, itemName, accessoryName, contentWeaponSelected, accessorySelected, inventoryIndex[index_e]);
        //                                //UIUpgrade.singleton.ResetParameters();
        //                            });
        //                        }
        //                        else
        //                        {
        //                            UIUpgrade.singleton.SetSkin.GetComponentInChildren<TextMeshProUGUI>().text = player.inventory.slots[weaponIndex].item.actualEquipment[slot].actualIndex == index_y ? "Setted" : "Set";
        //                            //CMD SET SKIN
        //                            UIUpgrade.singleton.SetSkin.onClick.RemoveAllListeners();
        //                            UIUpgrade.singleton.SetSkin.onClick.AddListener(() =>
        //                            {
        //                                player.playerUpgrade.CmdSetSkinForStandard(weaponIndex, index_y, itemName, accessoryName, contentWeaponSelected, accessorySelected, inventoryIndex[index_e]);
        //                                //UIUpgrade.singleton.ResetParameters();
        //                            });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(false);
        //                    UIUpgrade.singleton.SetSkin.gameObject.SetActive(false);
        //                }
        //            });
        //        }
        //    }
        //    else
        //    {
        //        skinContent.parent.parent.gameObject.SetActive(false);
        //        UIUpgrade.singleton.buttonBuySkin.gameObject.SetActive(false);
        //        UIUpgrade.singleton.SetSkin.gameObject.SetActive(false);

        //    }

        //}

        //public void OpenStandard()
        //{
        //    if (!player) player = Player.localPlayer;
        //    if (!player) return;

        //    panel.SetActive(true);

        //    foreach (Transform t in skinContent)
        //    {
        //        t.GetComponent<AccessorySkin>().ManageShadow(false);
        //    }

        //    CacheInventoryIndexOfAccessory();
        //    CloseButtonEventsAssign();
        //    ClearSkinContents();
        //    SpawnItemAccessoryInInventoryStandard();
        //}
    }
}
