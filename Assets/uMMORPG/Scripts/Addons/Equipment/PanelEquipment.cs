using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UIEquipmentSlot
{
    public Button button;
}

public partial class PanelEquipment : MonoBehaviour
{
    public static PanelEquipment singleton;

    private Player player;

    public UIEquipmentSlot slotPrefab;
    public Transform content;

    void OnEnable()
    {
        if (!singleton) singleton = this;
        player = Player.localPlayer;
        RefreshEquipment();

    }

    public void RefreshEquipment()
    {
        if (!player) return;
        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.equipment.slots.Count, content);

        // refresh all
        for (int i = 0; i < player.equipment.slots.Count; ++i)
        {
            UIEquipmentSlot slot = content.GetChild(i).GetComponent<UIEquipmentSlot>();
            slot.dragAndDropable.name = i.ToString(); // drag and drop slot
            ItemSlot itemSlot = player.equipment.slots[i];

            // set category overlay in any case. we use the last noun in the
            // category string, for example EquipmentWeaponBow => Bow
            // (disabled if no category, e.g. for archer shield slot)
            EquipmentInfo slotInfo = ((PlayerEquipment)player.equipment).slotInfo[i];
            slot.categoryOverlay.SetActive(slotInfo.requiredCategory != "");
            string overlay = Utils.ParseLastNoun(slotInfo.requiredCategory);
            slot.categoryText.text = overlay != "" ? overlay : "?";

            if (itemSlot.amount > 0)
            {
                // refresh valid item

                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                slot.tooltip.enabled = true;
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = itemSlot.ToolTip();
                slot.dragAndDropable.dragable = true;
                slot.button.interactable = true;
                // use durability colors?
                /*if (itemSlot.item.maxDurability > 0)
                {
                    if (itemSlot.item.durability == 0)
                        slot.image.color = brokenDurabilityColor;
                    else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                        slot.image.color = lowDurabilityColor;
                    else
                        slot.image.color = Color.white;
                }
                else*/
                slot.image.color = Color.white; // reset for no-durability items
                slot.image.sprite = itemSlot.item.image;

                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                // refresh invalid item
                slot.button.interactable = false;
                slot.tooltip.enabled = false;
                slot.dragAndDropable.dragable = false;
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }
    }
}

