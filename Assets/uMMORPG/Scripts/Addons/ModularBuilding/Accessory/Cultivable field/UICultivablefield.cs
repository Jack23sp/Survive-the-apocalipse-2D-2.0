using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UICultivablefield : MonoBehaviour, IUIScript
{
    public static UICultivablefield singleton;
    public CuiltivableField cultivableField;
    public Transform inventoryContent;
    public UIInventorySlot toInstantiate;
    public Button objectSelected;
    public TextMeshProUGUI description;
    public Button plantVegetablesButton;
    public Button plantsButtonOnMain;
    public Button cancelButtonOnMain;
    public string selectedItem;
    public Button closeButton;
    public GameObject panelCanvas;
    public GameObject panelCanvasSelected;


    void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });

        plantVegetablesButton.onClick.RemoveAllListeners();
        plantVegetablesButton.onClick.AddListener(() =>
        {

            BlurManager.singleton.Show();
            if (!string.IsNullOrEmpty(selectedItem))
            {
                Player.localPlayer.playerHungry.objectToPlant = selectedItem;
                ScriptableItem itm;
                if (ScriptableItem.All.TryGetValue(selectedItem.GetStableHashCode(), out itm))
                {
                    plantsButtonOnMain.image.sprite = itm.image;
                }
            }

            closeButton.image.raycastTarget = false;
            closeButton.image.enabled = false;
            selectedItem = string.Empty;
            objectSelected.image = null;
            description.text = string.Empty;
            panelCanvas.SetActive(false);
            panelCanvasSelected.SetActive(true);
        });

        plantVegetablesButton.interactable = false;

        cancelButtonOnMain.onClick.RemoveAllListeners();
        cancelButtonOnMain.onClick.AddListener(() =>
        {
            Player.localPlayer.playerHungry.objectToPlant = string.Empty;
            panelCanvasSelected.SetActive(false);
        });
    }

    public void Open(CuiltivableField cuiltivable)
    {
        cultivableField = cuiltivable;
        Assign();

        objectSelected.gameObject.SetActive(false);
        plantVegetablesButton.interactable = false;
        closeButton.image.enabled = true;
        closeButton.image.raycastTarget = true;
        closeButton.interactable = true;
        description.text = string.Empty;
        panelCanvas.SetActive(true);

        Player player = Player.localPlayer;
        if (!player) return;

        UIUtils.BalancePrefabs(toInstantiate.gameObject, Player.localPlayer.inventory.slots.Count, inventoryContent);
        for(int i = 0; i  < Player.localPlayer.inventory.slots.Count; i++)
        {
            int index = i;
            UIInventorySlot slot = inventoryContent.GetChild(index).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory.slots[index];

            slot.image.preserveAspect = true;
            slot.dragAndDropable.enabled = false;

            if (itemSlot.amount > 0)
            {
                slot.registerItem.index = -1;
                slot.durabilitySlider.fillAmount = player.inventory.slots[index].item.data.maxDurability.baseValue > 0 ? ((float)player.inventory.slots[index].item.currentDurability / (float)player.inventory.slots[index].item.data.maxDurability.Get(player.inventory.slots[index].item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = player.inventory.slots[index].item.data.maxUnsanity > 0 ? ((float)player.inventory.slots[index].item.currentUnsanity / (float)player.inventory.slots[index].item.data.maxUnsanity) : 0;

                if (player.inventory.slots[index].item.data is FoodItem && ((FoodItem)player.inventory.slots[index].item.data).seasonToGrown != Seasons.Nothing)
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
                    objectSelected.gameObject.SetActive(true);
                    plantVegetablesButton.interactable = true;

                    selectedItem = itemSlot.item.name;
                    objectSelected.image.sprite = itemSlot.item.image;
                    description.text = "Season to growth : " + ((FoodItem)player.inventory.slots[index].item.data).seasonToGrown.ToString() + "\n" +
                                       "Return amounts : between 2 and " + ((FoodItem)player.inventory.slots[index].item.data).maxAmountToReturn; //+ "\n" +
                                       //"Time to growth : " + Utilities.ConvertToTimerMinuteAndSeconds(((FoodItem)player.inventory.slots[index].item.data).timeToGrowth).ToString();
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

    }

    public void Close()
    {
        BlurManager.singleton.Show();
        closeButton.image.raycastTarget = false;
        closeButton.image.enabled = false;
        selectedItem = string.Empty;
        objectSelected.image = null;
        description.text = string.Empty;
        RemovePlayerFromBuildingAccessory(cultivableField.netIdentity);
        panelCanvas.SetActive(false);
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
