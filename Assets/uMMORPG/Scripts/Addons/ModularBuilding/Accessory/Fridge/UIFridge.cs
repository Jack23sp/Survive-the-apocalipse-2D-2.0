using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIFridge : MonoBehaviour, IUIScript
{
    public static UIFridge singleton;
    public GameObject panel;
    public Transform fridgeContainer;
    public Transform inventoryContainer;

    public GameObject objectToSpawn;
    [HideInInspector] public Player player;

    public Button closeButton;
    public Button manageButton;

    public Fridge fridge;


    void Start()
    {
        if (!singleton) singleton = this;
    }


    public void Open(Fridge Fridge)
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;
        fridge = Fridge;
        Assign();

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.SetListener(() =>
        {
            Close();
        });

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(fridge, Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(fridge.netIdentity, fridge.craftingAccessoryItem, closeButton);
        });

        closeButton.image.enabled = true;
        panel.SetActive(true);
        closeButton.image.raycastTarget = true;

        UIUtils.BalancePrefabs(objectToSpawn, player.inventory.slots.Count, inventoryContainer);
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            int icopy = i;
            UIInventorySlot slot = inventoryContainer.GetChild(icopy).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory.slots[icopy];

            slot.image.preserveAspect = true;
            slot.dragAndDropable.enabled = false;
            slot.tooltip.enabled = false;
            if (itemSlot.amount > 0)
            {
                //slot.registerItem.index = icopy;
                slot.durabilitySlider.fillAmount = itemSlot.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot.item.currentDurability / (float)itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = itemSlot.item.data.maxUnsanity > 0 ? ((float)itemSlot.item.currentUnsanity / (float)itemSlot.item.data.maxUnsanity) : 0;

                if (player.inventory.slots[icopy].item.data.canUseFridge)
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
                        player.CmdAddToFridge(icopy, -1, fridge.netIdentity);

                });
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.data.skinImages.Count > 0 && itemSlot.item.skin > -1 ?
                                    itemSlot.item.data.skinImages[itemSlot.item.skin] :
                                    itemSlot.item.data.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                //slot.registerItem.index = -1; 
                slot.durabilitySlider.fillAmount = 0;
                slot.unsanitySlider.fillAmount = 0;


                slot.button.onClick.RemoveAllListeners();
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }

        UIUtils.BalancePrefabs(objectToSpawn, fridge.maxSlotAmount, fridgeContainer);
        for (int a = 0; a < fridgeContainer.childCount; a++)
        {
            UIInventorySlot slot2 = fridgeContainer.GetChild(a).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot2 = fridge.slots[a];
            slot2.image.preserveAspect = true;
            slot2.dragAndDropable.enabled = false;
            slot2.tooltip.enabled = false;

            if (itemSlot2.amount > 0)
            {
                int icopy = a;
                //slot2.registerItem.index = icopy;
                //slot2.registerItem.fridgeSlot = true;
                slot2.button.onClick.RemoveAllListeners();
                slot2.button.onClick.SetListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    if (!UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
                        player.CmdAddToFridge(-1, icopy, fridge.netIdentity);

                });
                slot2.image.color = Color.white;
                slot2.image.sprite = itemSlot2.item.data.skinImages.Count > 0 && itemSlot2.item.skin > -1 ?
                                     itemSlot2.item.data.skinImages[itemSlot2.item.skin] :
                                     itemSlot2.item.data.image;
                slot2.cooldownCircle.fillAmount = 0;
                slot2.amountOverlay.SetActive(itemSlot2.amount > 1);
                slot2.amountText.text = itemSlot2.amount.ToString();
            }
            else
            {
                slot2.button.onClick.RemoveAllListeners();
                slot2.image.color = Color.clear;
                slot2.image.sprite = null;
                slot2.cooldownCircle.fillAmount = 0;
                slot2.amountOverlay.SetActive(false);
                //slot2.registerItem.index = -1;
                //slot2.registerItem.fridgeSlot = false;

            }
        }
    }
    public void Close()
    {
        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
        closeButton.image.raycastTarget = false;
        panel.SetActive(false);
        closeButton.image.enabled = false;
        RemovePlayerFromBuildingAccessory(fridge.netIdentity);
        BlurManager.singleton.Show();
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeath.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeath.Add(this);
    }

    public void RemovePlayerFromBuildingAccessory(NetworkIdentity identity)
    {
        Player.localPlayer.playerModularBuilding.CmdRemovePlayerInteractWithAccessory(identity);
    }
}
