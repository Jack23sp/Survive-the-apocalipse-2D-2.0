using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class UILibrary : MonoBehaviour, IUIScript
{
    public static UILibrary singleton;
    public GameObject panel;
    public Transform libraryContainer;
    public Transform inventoryContainer;

    public GameObject objectToSpawn;
    [HideInInspector] public Player player;

    public Button closeButton;
    public Button manageButton;

    public Library library;


    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Open(Library libraryPar)
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;
        library = libraryPar;
        Assign();

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.SetListener(() =>
        {
            Close();
        });

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(library, Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(library.netIdentity, library.craftingAccessoryItem, closeButton);
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

            if (itemSlot.amount > 0)
            {
                slot.registerItem.index = icopy;
                slot.durabilitySlider.fillAmount = player.inventory.slots[icopy].item.data.maxDurability.baseValue > 0 ? ((float)player.inventory.slots[icopy].item.currentDurability / (float)player.inventory.slots[icopy].item.data.maxDurability.Get(player.inventory.slots[icopy].item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = player.inventory.slots[icopy].item.data.maxUnsanity > 0 ? ((float)player.inventory.slots[icopy].item.currentUnsanity / (float)player.inventory.slots[icopy].item.data.maxUnsanity) : 0;

                if (player.inventory.slots[icopy].item.data.canUseLibrary)
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
                        player.CmdAddToLibrary(icopy, -1, library.netIdentity);
                });

                slot.dragAndDropable.enabled = false;
                slot.tooltip.enabled = false;
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

        UIUtils.BalancePrefabs(objectToSpawn, library.amount, libraryContainer);
        for (int a = 0; a < libraryContainer.childCount; a++)
        {
            int index = a;
            UIInventorySlot slot2 = libraryContainer.GetChild(index).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot2 = library.slots[index];
            slot2.image.preserveAspect = true;
            slot2.dragAndDropable.enabled = false;

            if (itemSlot2.amount > 0)
            {
                int icopy = a;
                slot2.registerItem.index = index;
                slot2.durabilitySlider.fillAmount = itemSlot2.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot2.item.currentDurability / (float)itemSlot2.item.data.maxDurability.Get(itemSlot2.item.durabilityLevel)) : 0;
                slot2.unsanitySlider.fillAmount = itemSlot2.item.data.maxUnsanity > 0 ? ((float)itemSlot2.item.currentUnsanity / (float)itemSlot2.item.data.maxUnsanity) : 0;


                slot2.button.onClick.RemoveAllListeners();
                slot2.button.onClick.SetListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    if (!UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
                        player.CmdAddToLibrary(-1, icopy, library.netIdentity);
                });
                slot2.dragAndDropable.enabled = false;
                slot2.tooltip.enabled = false;
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
                slot2.registerItem.index = -1;
                slot2.durabilitySlider.fillAmount = 0;
                slot2.unsanitySlider.fillAmount = 0;

                slot2.dragAndDropable.enabled = false;
                slot2.tooltip.enabled = false;
                slot2.button.onClick.RemoveAllListeners();
                slot2.image.color = Color.clear;
                slot2.image.sprite = null;
                slot2.cooldownCircle.fillAmount = 0;
                slot2.amountOverlay.SetActive(false);
            }
        }
    }

    public void Close()
    {
        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
        closeButton.image.raycastTarget = false;
        panel.SetActive(false);
        closeButton.image.enabled = false;
        RemovePlayerFromBuildingAccessory(library.netIdentity);
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
