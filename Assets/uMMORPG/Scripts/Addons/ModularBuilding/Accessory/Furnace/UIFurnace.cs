using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIFurnace : MonoBehaviour
{
    public static UIFurnace singleton;

    public Furnace furnace;

    public GameObject panel;

    public Button closeButton;
    public Button manageButton;

    public UIInventorySlot wood;
    public GameObject objectToSpawn;

    public List<UIInventorySlot> elements = new List<UIInventorySlot>();
    public List<UIInventorySlot> results = new List<UIInventorySlot>();

    public Transform inventoryContent;
    public Button onButton;


    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void RefreshFurnaceElements()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            panel.SetActive(false);
            closeButton.image.raycastTarget = false;
            closeButton.image.enabled = false;
            BlurManager.singleton.Show();
        });

        onButton.onClick.RemoveAllListeners();
        onButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(16);
            Player.localPlayer.CmdManageFurnace(furnace.netIdentity);
        });

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(furnace, Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(furnace.netIdentity, furnace.craftingAccessoryItem, closeButton);
        });


        onButton.GetComponentInChildren<TextMeshProUGUI>().text = furnace.on ? "ON" : "OFF";

        for (int i = 0; i < elements.Count; i++)
        {
            int icopy = i;
            UIInventorySlot slot = elements[icopy];
            ItemSlot itemSlot = furnace.elements[icopy];

            slot.image.preserveAspect = true;
            slot.dragAndDropable.enabled = false;

            if (itemSlot.amount > 0)
            {
                //slot.registerItem.index = icopy;
                slot.durabilitySlider.fillAmount = itemSlot.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot.item.currentDurability / (float)itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = itemSlot.item.data.maxUnsanity > 0 ? ((float)itemSlot.item.currentUnsanity / (float)itemSlot.item.data.maxUnsanity) : 0;

                slot.button.interactable = true;

                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.SetListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    if (!UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
                    {
                        player.CmdAddElementsToInventory(icopy, furnace.netIdentity);
                    }
                });

                slot.dragAndDropable.enabled = false;
                slot.tooltip.enabled = false;
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                slot.button.interactable = false;

                slot.registerItem.index = -1;
                slot.durabilitySlider.fillAmount = 0;
                slot.unsanitySlider.fillAmount = 0;
                slot.dragAndDropable.enabled = false;
                slot.tooltip.enabled = false;
                slot.button.onClick.RemoveAllListeners();
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }

        for (int i = 0; i < results.Count; i++)
        {
            int icopy = i;
            UIInventorySlot slot = results[icopy];
            ItemSlot itemSlot = furnace.results[icopy];

            slot.image.preserveAspect = true;
            slot.dragAndDropable.enabled = false;

            if (itemSlot.amount > 0)
            {
                //slot.registerItem.index = icopy;
                slot.durabilitySlider.fillAmount = itemSlot.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot.item.currentDurability / (float)itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = itemSlot.item.data.maxUnsanity > 0 ? ((float)itemSlot.item.currentUnsanity / (float)itemSlot.item.data.maxUnsanity) : 0;

                slot.button.interactable = true;

                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.SetListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    if (!UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
                    {
                        player.CmdAddResultsToInventory(icopy, furnace.netIdentity);
                    }
                });

                slot.dragAndDropable.enabled = false;
                slot.tooltip.enabled = false;
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                slot.button.interactable = false;

                slot.registerItem.index = -1;
                slot.durabilitySlider.fillAmount = 0;
                slot.unsanitySlider.fillAmount = 0;
                slot.dragAndDropable.enabled = false;
                slot.tooltip.enabled = false;
                slot.button.onClick.RemoveAllListeners();
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }

        UIInventorySlot woodSlot = wood;
        ItemSlot woods = furnace.wood[0];
        if (woods.amount > 0)
        {
            //woodSlot.registerItem.index = 0;
            woodSlot.durabilitySlider.fillAmount = 0;
            woodSlot.unsanitySlider.fillAmount = 0;

            woodSlot.button.interactable = true;

            woodSlot.button.onClick.RemoveAllListeners();
            woodSlot.button.onClick.SetListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(17);
                if (!UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
                {
                    player.CmdAddWoodToInventory(furnace.netIdentity);
                }
            });

            woodSlot.dragAndDropable.enabled = false;
            woodSlot.tooltip.enabled = false;
            woodSlot.image.color = Color.white;
            woodSlot.image.sprite = woods.item.image;
            woodSlot.cooldownCircle.fillAmount = 0;
            woodSlot.amountOverlay.SetActive(woods.amount > 1);
            woodSlot.amountText.text = woods.amount.ToString();
        }
        else
        {
            woodSlot.button.interactable = false;

            woodSlot.registerItem.index = -1;
            woodSlot.durabilitySlider.fillAmount = 0;
            woodSlot.unsanitySlider.fillAmount = 0;
            woodSlot.dragAndDropable.enabled = false;
            woodSlot.tooltip.enabled = false;
            woodSlot.button.onClick.RemoveAllListeners();
            woodSlot.image.color = Color.clear;
            woodSlot.image.sprite = null;
            woodSlot.cooldownCircle.fillAmount = 0;
            woodSlot.amountOverlay.SetActive(false);
        }

    }

    public void Open(Furnace Furnace)
    {
        Player player = Player.localPlayer;
        if (!player) return;
        furnace = Furnace;

        panel.SetActive(true);
        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;

        RefreshFurnaceElements();

        UIUtils.BalancePrefabs(objectToSpawn, player.inventory.slots.Count, inventoryContent);
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            int icopy = i;
            UIInventorySlot slot = inventoryContent.GetChild(icopy).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory.slots[icopy];

            slot.image.preserveAspect = true;
            slot.dragAndDropable.enabled = false;

            if (itemSlot.amount > 0)
            {
                slot.registerItem.index = icopy;
                slot.durabilitySlider.fillAmount = player.inventory.slots[icopy].item.data.maxDurability.baseValue > 0 ? ((float)player.inventory.slots[icopy].item.currentDurability / (float)player.inventory.slots[icopy].item.data.maxDurability.Get(player.inventory.slots[icopy].item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = player.inventory.slots[icopy].item.data.maxUnsanity > 0 ? ((float)player.inventory.slots[icopy].item.currentUnsanity / (float)player.inventory.slots[icopy].item.data.maxUnsanity) : 0;

                if (FurnaceManager.singleton && FurnaceManager.singleton.FindItemInAllowedForFurnace(player.inventory.slots[icopy].item.name))
                {
                    slot.button.interactable = true;
                }
                else
                {
                    slot.button.interactable = false;
                }
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.SetListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    if (!UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
                    {
                        if (player.inventory.slots[icopy].item.data.name == "Wood")
                        {
                            player.CmdAddWood(icopy, furnace.netIdentity);
                        }
                        else
                        {
                            player.CmdAddElements(icopy, furnace.netIdentity);
                        }
                    }
                });

                slot.dragAndDropable.enabled = false;
                slot.tooltip.enabled = false;
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                slot.registerItem.index = -1;
                slot.durabilitySlider.fillAmount = 0;
                slot.unsanitySlider.fillAmount = 0;
                slot.dragAndDropable.enabled = false;
                slot.tooltip.enabled = false;
                slot.button.onClick.RemoveAllListeners();
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }

    }
}
