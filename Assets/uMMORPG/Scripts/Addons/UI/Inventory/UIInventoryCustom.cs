// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UIInventoryCustom : MonoBehaviour
{
    public static UIInventoryCustom singleton;
    public UIInventorySlot slotPrefab;
    public Transform content;
    public Button mergeButton;
    public Button splitButton;

    public int operationType = -1;
    public List<string> slotToManage = new List<string>();
    public List<int> indexToManage = new List<int>();

    public void Start()
    {
        if (!singleton) singleton = this;

        mergeButton.onClick.AddListener(() =>
        {
            SearchItemToManage(0,false);
        });

        splitButton.onClick.AddListener(() =>
        {
            SearchItemToManage(1,false);
        });
    }

    public int FindNext(string itemName, int index)
    {
        for (int i = 0; i < Player.localPlayer.inventory.slots.Count; i++)
        {
            int index_i = i;
            if (Player.localPlayer.inventory.slots[index_i].amount > 0 && Player.localPlayer.inventory.slots[index_i].item.data.maxStack > Player.localPlayer.inventory.slots[index_i].amount)
            {
                if (Player.localPlayer.inventory.slots[index_i].item.data.name == itemName && index_i != index)
                {
                    return index_i;
                }
            }
        }
        return -1;
    }

    public int CountItemslotAmount(string itemName)
    {
        int amount = 0;
        for (int i = 0; i < Player.localPlayer.inventory.slots.Count; i++)
        {
            int index_i = i;
            if (Player.localPlayer.inventory.slots[index_i].amount > 0)
            {
                if (Player.localPlayer.inventory.slots[index_i].item.data.name == itemName)
                {
                    amount++;
                }
            }
        }
        return amount;
    }

    public void SearchItemToManage(int type, bool ignore)
    {
        slotToManage.Clear();
        indexToManage.Clear();
        if (!ignore)
        {
            if (type == 0)
            {
                if (operationType == 0)
                {
                    operationType = -1;
                }
                else
                {
                    operationType = type;
                }
            }
            else
            {
                if (operationType == 1)
                {
                    operationType = -1;
                }
                else
                {
                    operationType = type;
                }
            }
        }

        if (operationType == -1)
        {
            // Deactivate highlight of item
            ResetAspectOfItemOutline();
        }
        else if (operationType == 0)
        {
            //merge
            ResetAspectOfItemOutline();

            for (int e = 0; e < Player.localPlayer.inventory.slots.Count; e++)
            {
                int index_e = e;

                if (Player.localPlayer.inventory.slots[index_e].amount > 0)
                {
                    if (Player.localPlayer.inventory.slots[index_e].item.data.maxStack > 1 && 
                        Player.localPlayer.inventory.slots[index_e].item.data.maxStack > Player.localPlayer.inventory.slots[index_e].amount &&
                        CountItemslotAmount(Player.localPlayer.inventory.slots[index_e].item.data.name) > 1)
                    {
                        content.GetChild(index_e).GetComponent<UIInventorySlot>().registerItem.index = -1;

                        slotToManage.Add(Player.localPlayer.inventory.slots[index_e].item.data.name);
                        indexToManage.Add(index_e);
                        content.GetChild(index_e).GetComponent<UIInventorySlot>().outline.enabled = true;
                    }
                }
            }
        }
        else
        {
            //split
            ResetAspectOfItemOutline();

            if (Player.localPlayer.inventory.FirstFreeSlot() == -1) return;

            for (int e = 0; e < Player.localPlayer.inventory.slots.Count; e++)
            {
                int index_e = e;

                if (Player.localPlayer.inventory.slots[index_e].amount > 1)
                {
                    if (Player.localPlayer.inventory.slots[index_e].item.data.maxStack > 1)
                    {
                        content.GetChild(index_e).GetComponent<UIInventorySlot>().registerItem.index = -1;
                        slotToManage.Add(Player.localPlayer.inventory.slots[index_e].item.data.name);
                        indexToManage.Add(index_e);
                        content.GetChild(index_e).GetComponent<UIInventorySlot>().outline.enabled = true;
                    }
                }
            }
        }
        PartialRefresh();
    }

    public void ResetAspectOfItemOutline()
    {
        for (int i = 0; i < Player.localPlayer.inventory.slots.Count; i++)
        {
            int index = i;
            content.GetChild(index).GetComponent<UIInventorySlot>().outline.enabled = false;
            content.GetChild(index).GetComponent<UIInventorySlot>().registerItem.index = index;
        }

    }

    public void PartialRefresh()
    {
        Player player = Player.localPlayer;
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            int index = i;
            UIInventorySlot slot = content.GetChild(index).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory.slots[index];

            if (itemSlot.amount > 0)
            {
                if (operationType == -1 || !indexToManage.Contains(index))
                {
                    slot.registerItem.index = index;
                    slot.button.onClick.RemoveAllListeners();
                }
                else
                {
                    slot.registerItem.index = -1;
                    slot.button.onClick.RemoveAllListeners();
                    slot.button.onClick.AddListener(() =>
                    {
                        if (operationType == 0 && indexToManage.Contains(index))
                        {
                            Player.localPlayer.inventory.CmdInventoryMergeItem(index, FindNext(itemSlot.item.data.name, index));
                        }
                        else if (operationType == 1 && indexToManage.Contains(index))
                        {
                            if (Player.localPlayer.inventory.FirstFreeSlot() > -1)
                                Player.localPlayer.inventory.CmdInventorySplit(index, Player.localPlayer.inventory.FirstFreeSlot());
                        }
                    });
                }
            }
            else
            {
                // refresh invalid item
                slot.outline.enabled = false;
            }
        }
    }

    public void Open()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.slots.Count, content);

            for (int i = 0; i < player.inventory.slots.Count; i++)
            {
                int index = i;
                UIInventorySlot slot = content.GetChild(index).GetComponent<UIInventorySlot>();
                //slot.dragAndDropable.name = index.ToString(); // drag and drop index
                ItemSlot itemSlot = player.inventory.slots[index];

                if (itemSlot.amount > 0)
                {
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = true;
                    slot.registerItem.use = true;
                    slot.registerItem.equip = true;
                    slot.registerItem.delete = true;
                    slot.registerItem.inventorySlot = true;
                    if (operationType == -1 || !indexToManage.Contains(index))
                    {
                        slot.registerItem.index = index;
                        slot.button.onClick.RemoveAllListeners();
                    }
                    else
                    {
                        slot.registerItem.index = -1;
                        slot.button.onClick.RemoveAllListeners();
                        slot.button.onClick.AddListener(() =>
                        {
                            if (operationType == 0 && indexToManage.Contains(index))
                            {
                                Player.localPlayer.inventory.CmdInventoryMergeItem(index, FindNext(itemSlot.item.data.name, index));
                            }
                            else if (operationType == 1 && indexToManage.Contains(index))
                            {
                                if (Player.localPlayer.inventory.FirstFreeSlot() > -1)
                                    Player.localPlayer.inventory.CmdInventorySplit(index, Player.localPlayer.inventory.FirstFreeSlot());
                            }
                        });
                    }
                    slot.durabilitySlider.fillAmount = itemSlot.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot.item.currentDurability / (float)itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)) : 0;
                    slot.unsanitySlider.fillAmount = itemSlot.item.data.maxUnsanity > 0 ? ((float)itemSlot.item.currentUnsanity / (float)itemSlot.item.data.maxUnsanity) : 0;
                    slot.image.color = Color.white; // reset for no-durability items
                    slot.image.sprite = itemSlot.item.data.skinImages.Count > 0
                                        && itemSlot.item.skin > -1 ? itemSlot.item.data.skinImages[itemSlot.item.skin] : itemSlot.item.data.image;
                    slot.image.preserveAspect = true;

                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
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
                    slot.registerItem.inventorySlot = false;
                    slot.registerItem.index = -1;
                    slot.durabilitySlider.fillAmount = 0;
                    slot.unsanitySlider.fillAmount = 0;
                    slot.outline.enabled = false;
                }
            }
            if (operationType > -1)
            {
                int opType = operationType;
                SearchItemToManage(opType,false);
                //SearchItemToManage(opType,false);
            }
        }
    }
}
