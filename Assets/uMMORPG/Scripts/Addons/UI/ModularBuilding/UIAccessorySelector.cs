using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAccessorySelector : MonoBehaviour
{
    public static UIAccessorySelector singleton;
    public Button confirmButton;
    public Button closeButton;
    public Transform mainAccessoryContent;
    public Transform accessoriesContent;
    public GameObject objectToSpawn;

    public BuildingAccessory selected;

    void Start()
    {
        if (!singleton) singleton = this;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(this.gameObject);
        });

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            if(selected)
            {
                ModularBuildingManager.singleton.SelectorClicked(selected, closeButton);
                closeButton.onClick.Invoke();
            }
        });
    }

    public void Open(BuildingAccessory mainAccessory)
    {
        UIUtils.BalancePrefabs(objectToSpawn, 1, mainAccessoryContent);
        UIUtils.BalancePrefabs(objectToSpawn, mainAccessory.accessoriesInThisForniture.Count, accessoriesContent);

        #region main
        UIAccessorySelectorSlot slot = mainAccessoryContent.GetChild(0).GetComponent<UIAccessorySelectorSlot>();
        slot.accessoryImage.sprite = mainAccessory.craftingAccessoryItem.image;
        slot.accessoryImage.preserveAspect = true;
        slot.accessoryName.text = mainAccessory.craftingAccessoryItem.name;
        slot.clickButton.onClick.RemoveAllListeners();
        slot.clickButton.onClick.AddListener(() =>
        {
            slot.selectedObject.SetActive(!slot.selectedObject.activeInHierarchy);
            if(slot.selectedObject.activeInHierarchy)
            {
                selected = mainAccessory;
            }

            for(int e = 0; e < accessoriesContent.childCount; e++)
            {
                UIAccessorySelectorSlot slot1 = accessoriesContent.GetChild(e).GetComponent<UIAccessorySelectorSlot>();
                slot1.selectedObject.SetActive(false);
            }
            confirmButton.interactable = slot.selectedObject.activeInHierarchy;
        });
        slot.manageAccessoriesButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(mainAccessory.netIdentity, mainAccessory.craftingAccessoryItem, closeButton);
        });
        #endregion

        #region accessories
        for (int a = 0; a < mainAccessory.accessoriesInThisForniture.Count; a++)
        {
            int index_a = a;
            UIAccessorySelectorSlot accSlot = accessoriesContent.GetChild(index_a).GetComponent<UIAccessorySelectorSlot>();
            accSlot.accessoryImage.sprite = mainAccessory.accessoriesInThisForniture[index_a].craftingAccessoryItem.image;
            accSlot.accessoryImage.preserveAspect = true;
            accSlot.accessoryName.text = mainAccessory.accessoriesInThisForniture[index_a].craftingAccessoryItem.name;
            accSlot.clickButton.onClick.RemoveAllListeners();
            accSlot.clickButton.onClick.AddListener(() =>
            {
                accSlot.selectedObject.SetActive(!accSlot.selectedObject.activeInHierarchy);
                slot.selectedObject.SetActive(false);

                if (accSlot.selectedObject.activeInHierarchy)
                {
                    selected = mainAccessory.accessoriesInThisForniture[index_a];
                }

                for (int e = 0; e < accessoriesContent.childCount; e++)
                {
                    if (e != index_a)
                    {
                        UIAccessorySelectorSlot slot2 = accessoriesContent.GetChild(e).GetComponent<UIAccessorySelectorSlot>();
                        slot2.selectedObject.SetActive(false);
                    }
                }
                confirmButton.interactable = accSlot.selectedObject.activeInHierarchy;
            });
            accSlot.manageAccessoriesButton.onClick.AddListener(() =>
            {
                GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
                g.GetComponent<UIBuildingAccessoryManager>().Init(mainAccessory.accessoriesInThisForniture[index_a].netIdentity, mainAccessory.accessoriesInThisForniture[index_a].craftingAccessoryItem, closeButton);
            });

        }
        #endregion
    }
}
