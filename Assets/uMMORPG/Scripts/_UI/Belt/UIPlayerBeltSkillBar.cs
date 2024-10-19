using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerBeltSkillBar : MonoBehaviour
{
    public static UIPlayerBeltSkillBar singleton;
    public Transform content;

    private Player player;
    private UISelectedItem selectedItem;
    public UIEquipmentSlotCustom equipmentSlot;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void OnEnable()
    {
        CheckSkillbar();
        CheckEquipment();
    }

    public void CheckEquipment()
    {
        if (!player)
            player = Player.localPlayer;

        if (player != null && equipmentSlot)
        {
            equipmentSlot.button.interactable = true;
            equipmentSlot.amountOverlay.SetActive(false);
            equipmentSlot.categoryOverlay.SetActive(false);
            equipmentSlot.cooldownCircle.fillAmount = 0;
            equipmentSlot.dragAndDropable.dragable = equipmentSlot.dragAndDropable.dropable = false;
            if (player.equipment.slots[0].amount > 0)
            {
                equipmentSlot.image.color = Color.white; // reset for no-durability items
                equipmentSlot.image.sprite = player.equipment.slots[0].item.data.image;
            }
            else
            {
                equipmentSlot.image.color = Color.clear;
                equipmentSlot.image.sprite = null;
            }
        }
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
                UISkillbarSlot slot = content.GetChild(i).GetComponent<UISkillbarSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                ItemSlot itemSlot = player.playerBelt.belt[index];

                if (itemSlot.amount > 0)
                {
                    slot.button.onClick.SetListener(() =>
                    {
                        if (GameObjectSpawnManager.singleton.spawnedSelectedItem == null)
                        {
                            GameObjectSpawnManager.singleton.spawnedSelectedItem = Instantiate(GameObjectSpawnManager.singleton.selectedItem, slot.transform);
                            selectedItem = GameObjectSpawnManager.singleton.spawnedSelectedItem.GetComponent<UISelectedItem>();
                            selectedItem.itemImage.sprite = itemSlot.item.data.image;
                            selectedItem.nameText.text = itemSlot.item.data.name;
                            selectedItem.overlayAmountObject.SetActive(itemSlot.amount > 0);
                            selectedItem.overlayAmountText.text = itemSlot.amount.ToString();
                            selectedItem.description.text = itemSlot.item.ToolTip();
                            selectedItem.closeButton.onClick.RemoveAllListeners();
                            selectedItem.closeButton.onClick.AddListener(() =>
                            {
                                Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
                            });
                            selectedItem.deleteButton.onClick.RemoveAllListeners();
                            selectedItem.deleteButton.onClick.AddListener(() =>
                            {
                                player.playerBelt.CmdDeleteBeltItem(index);
                                Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
                            });
                            selectedItem.useButton.onClick.RemoveAllListeners();
                            selectedItem.useButton.onClick.AddListener(() =>
                            {
                                if (itemSlot.item.data is UsableItem usable &&
                                    usable.CanUse(player, index))
                                {
                                    if (itemSlot.item.data is ScriptableBuilding || itemSlot.item.data is ScriptableWall || itemSlot.item.data is ScriptableDoor)
                                    {
                                        ModularBuildingManager.singleton.isInventory = false;
                                        usable.Use(player, index);
                                    }
                                    else
                                        player.playerBelt.CmdUseBeltItem(index);
                                }
                                if (itemSlot.item.data is EquipmentItem) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
                            });
                        }
                        else
                        {
                            Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
                            GameObjectSpawnManager.singleton.spawnedSelectedItem = Instantiate(GameObjectSpawnManager.singleton.selectedItem, slot.transform);
                            selectedItem = GameObjectSpawnManager.singleton.spawnedSelectedItem.GetComponent<UISelectedItem>();
                            selectedItem.itemImage.sprite = itemSlot.item.data.image;
                            selectedItem.nameText.text = itemSlot.item.data.name;
                            selectedItem.overlayAmountObject.SetActive(itemSlot.amount > 0);
                            selectedItem.overlayAmountText.text = itemSlot.amount.ToString();
                            selectedItem.description.text = itemSlot.item.ToolTip();
                            selectedItem.closeButton.onClick.RemoveAllListeners();
                            selectedItem.closeButton.onClick.AddListener(() =>
                            {
                                Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
                            });
                            selectedItem.deleteButton.onClick.RemoveAllListeners();
                            selectedItem.deleteButton.onClick.AddListener(() =>
                            {
                                player.playerBelt.CmdDeleteBeltItem(index);
                                Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
                            });
                            selectedItem.useButton.onClick.RemoveAllListeners();
                            selectedItem.useButton.onClick.AddListener(() =>
                            {
                                if (itemSlot.item.data is UsableItem usable &&
                                    usable.CanUse(player, index))
                                    player.playerBelt.CmdUseBeltItem(index);
                                if (itemSlot.item.data is EquipmentItem) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
                            });
                        }
                    });
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = false;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
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
                }
            }
        }
    }
}
