using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemDetails : MonoBehaviour
{
    public static UIItemDetails singleton;
    public Image mainImage;
    public Button closeButton;
    public GameObject panel;
    
    public TextMeshProUGUI itemName;
    public Image buildingImage;
    public TextMeshProUGUI buildingName;
    public Transform content;
    public GameObject objectToSpawn;
    private ItemCrafting itemCrafting;

    void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>{
            panel.SetActive(false);
            mainImage.raycastTarget = false;
            mainImage.enabled = false;
            closeButton.interactable = false;
            itemCrafting = new ItemCrafting();
        });
    }


    public void Open(ScriptableItem item, ScriptableBuildingAccessory buildingAccessory)
    {
        panel.SetActive(true);
        mainImage.raycastTarget = true;
        mainImage.enabled = true;
        closeButton.interactable = true;

        itemName.text = item.name;

        buildingImage.sprite = buildingAccessory.image;
        buildingName.text = buildingAccessory.name;

        itemCrafting = UIUtils.FindTheItemIsCrafted(item);

        UIUtils.BalancePrefabs(objectToSpawn, itemCrafting.ingredients.Count, content);
        for (int i = 0; i < itemCrafting.ingredients.Count; i++)
        {
            int index = i;
            BuyBoostSlot slot = content.GetChild(index).GetComponent<BuyBoostSlot>();
            slot.title.text = itemCrafting.ingredients[index].item.name + " (" + itemCrafting.ingredients[index].amount + ")";
            slot.boostImage.sprite = itemCrafting.ingredients[index].item.image;
            slot.coinImage.gameObject.SetActive(false);
            slot.coins.gameObject.SetActive(false);
        }

    }
}
