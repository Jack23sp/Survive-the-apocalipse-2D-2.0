using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerBelt : MonoBehaviour
{
    public static UIPlayerBelt singleton;
    public Transform content;
    private Player player;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void OnEnable()
    {
        CheckSkillbar();
    }

    public void CheckSkillbar()
    {
        if (!player)
            player = Player.localPlayer;

        if (player != null)
        {
            // refresh all
            for (int i = 0; i < player.playerBelt.belt.Count; i++)
            {
                int index = i;
                UISkillbarSlot slot = content.GetChild(index).GetComponent<UISkillbarSlot>();
                slot.dragAndDropable.name = index.ToString(); // drag and drop index
                ItemSlot itemSlot = player.playerBelt.belt[index];

                if (itemSlot.amount > 0)
                {
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.white;
                    //slot.image.sprite = itemSlot.item.image;
                    slot.image.sprite = itemSlot.item.data.skinImages.Count > 0
                    && itemSlot.item.skin > -1 ? itemSlot.item.data.skinImages[itemSlot.item.skin] : itemSlot.item.data.image;
                    slot.image.preserveAspect = true;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(true);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.registerItem.index = index;
                    slot.durabilitySlider.fillAmount = itemSlot.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot.item.currentDurability / (float)itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)) : 0;
                    slot.unsanitySlider.fillAmount = itemSlot.item.data.maxUnsanity > 0 ? ((float)itemSlot.item.currentUnsanity / (float)itemSlot.item.data.maxUnsanity) : 0;
                    slot.registerItem.skillSlot = true;
                    slot.registerItem.equip = true;
                    slot.registerItem.delete = true;
                    slot.registerItem.use = !(itemSlot.item.data is WaterBottleItem);
                }
                else
                {
                    // refresh empty slot
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                    slot.registerItem.skillSlot = false;
                    slot.registerItem.index = -1;
                    slot.registerItem.equip = false;
                    slot.registerItem.delete = false;
                    slot.registerItem.use = false;
                    slot.durabilitySlider.fillAmount = 0;
                    slot.unsanitySlider.fillAmount = 0;


                }
            }
        }
    }
}
