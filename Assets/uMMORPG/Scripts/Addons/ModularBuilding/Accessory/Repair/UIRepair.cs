using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIRepair : MonoBehaviour
{
    public static UIRepair singleton;
    private Upgrade repair;
    public GameObject panel;
    public GameObject objectToSpawn;
    public Transform content;
    public Button closeButton;
    public Button repairButton;
    public Image imageObjectToRepair;
    public TextMeshProUGUI description;
    public Button manageButton;

    public GameObject durabilityContainer;
    public TextMeshProUGUI minDurability;
    public TextMeshProUGUI maxDurability;
    public TextMeshProUGUI currentDurability;
    public TextMeshProUGUI repairButtonText;
    public Image slider;
    public Transform ingredientContent;
    public GameObject ingredientSlot;
    

    private Player player;
    private int selectedItem;
    private string itemName;
    private int itemCount;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Open(Upgrade upgradeBuilding)
    {
        player = Player.localPlayer;
        if (player)
        {
            repair = upgradeBuilding;

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                selectedItem = -1;
                itemName = string.Empty;
                durabilityContainer.SetActive(false);
                closeButton.image.enabled = false;
                BlurManager.singleton.Show();
                panel.SetActive(false);
            });

            repairButton.onClick.RemoveAllListeners();
            repairButton.onClick.AddListener(() =>
            {
                player.playerUpgrade.CmdRepairItem(selectedItem, itemName);
            });

            manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(repair, Player.localPlayer));
            manageButton.onClick.RemoveAllListeners();
            manageButton.onClick.AddListener(() =>
            {
                GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
                g.GetComponent<UIBuildingAccessoryManager>().Init(repair.netIdentity, repair.craftingAccessoryItem, closeButton);
            });


            panel.SetActive(true);
            closeButton.image.enabled = true;

            UIUtils.BalancePrefabs(objectToSpawn, Player.localPlayer.inventory.slots.Count, content);
            for(int i = 0; i < player.inventory.slots.Count; i++)
            {
                int index = i;
                UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                ItemSlot itemSlot = player.inventory.slots[index];

                if (itemSlot.amount > 0)
                {
                    if(itemSlot.item.data.maxDurability.baseValue == 0)
                    {
                        slot.button.interactable = false;
                    }
                    else
                    {
                        slot.button.onClick.RemoveAllListeners();
                        slot.button.onClick.AddListener(() =>
                        {
                            selectedItem = index;
                            itemName = itemSlot.item.data.name;

                            durabilityContainer.SetActive(true);
                            description.text = itemSlot.ToolTip();
                            imageObjectToRepair.sprite = itemSlot.item.data.image;
                            imageObjectToRepair.preserveAspect = true;
                            minDurability.text = "0";
                            maxDurability.text = itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel).ToString();
                            currentDurability.text = itemSlot.item.currentDurability.ToString() + "%";
                            slider.fillAmount = (float)itemSlot.item.currentDurability / itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel);

                            UIUtils.BalancePrefabs(ingredientSlot, itemSlot.item.data.repairItems.Count, ingredientContent);
                            for (int i = 0; i < itemSlot.item.data.repairItems.Count; i++)
                            {
                                UIRepairSlot slot = ingredientContent.GetChild(i).GetComponent<UIRepairSlot>();
                                slot.registerItem.use = false;
                                slot.registerItem.equip = false;
                                slot.registerItem.delete = false;
                                slot.registerItem.inventorySlot = false;
                                slot.image.sprite = itemSlot.item.data.repairItems[i].items.image;
                                slot.image.preserveAspect = true;
                                itemCount = player.inventory.CountItem(new Item(itemSlot.item.data.repairItems[i].items));
                                slot.quantity.text = itemCount + " / " + itemSlot.item.data.repairItems[i].amount;
                                slot.quantity.color = itemCount > itemSlot.item.data.repairItems[i].amount ? Color.white : Color.red;
                            }
                        });
                    }

                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.registerItem.use = false;
                    slot.registerItem.equip = false;
                    slot.registerItem.delete = false;
                    slot.registerItem.inventorySlot = false;
                    slot.registerItem.index = -1;
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
        }
    }
}
