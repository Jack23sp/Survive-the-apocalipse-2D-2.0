using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;
using System.Reflection;

public class UIBuildingAccessoryManager : MonoBehaviour
{
    public static UIBuildingAccessoryManager singleton;
    public Button closeButton;
    public Button repairButton;
    public Button moveButton;
    public Button deleteButton;
    public Button confirmButton;
    private Button parentButton;

    public GameObject objectToCreate;
    public GameObject scrollRect;
    public Transform content;

    public TextMeshProUGUI repairText;
    public TextMeshProUGUI moveText;
    public TextMeshProUGUI deleteText;

    public NetworkIdentity accessoryIdentity;
    public ScriptableBuildingAccessory accessoryToAdd;

    private BuildingAccessory buildingAccessory;
    [HideInInspector] public bool repair;
    [HideInInspector] public bool delete;
    private int hasAllItemToRepair;
    public int inventoryIndex;

    public Vector3 position;

    public void Init(NetworkIdentity identity, ScriptableBuildingAccessory scriptableBuildingAccessory, Button UIParentButton = null)
    {
        if (!singleton)
        {
            Destroy(singleton);
            singleton = this;
        }
        else
        {
            singleton = this;
        }

        accessoryIdentity = identity;
        accessoryToAdd = scriptableBuildingAccessory;
        buildingAccessory = identity.GetComponent<BuildingAccessory>();
        if (UIParentButton) parentButton = UIParentButton;
        position = buildingAccessory.gameObject.transform.position;
        closeButton.image.raycastTarget = true;
        moveText.gameObject.SetActive(false);


        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            CloseButton(false);
        });

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() =>
        {
            delete = true;
            DeleteButton();
        });

        repairButton.onClick.RemoveAllListeners();
        repairButton.onClick.AddListener(() =>
        {
            delete = false;
            RepairButton();
        });

        moveButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(() =>
        {
            delete = false;
            Movebutton();
        });

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            ConfirmButton();
        });

        RepairButton();
    }

    public void ConfirmButton()
    {
        if (!delete && !repair)
        {
            Player.localPlayer.playerModularBuilding.oldBuilding = buildingAccessory;
            Player.localPlayer.playerModularBuilding.fakeBuilding = buildingAccessory.craftingAccessoryItem.GetType();
            Player.localPlayer.playerModularBuilding.CmdSetFakeBuildingID(buildingAccessory.netIdentity);
            Player.localPlayer.playerModularBuilding.CmdManageVisibilityOfObject(false);
            UseItem(buildingAccessory, buildingAccessory.gameObject);
        }
        else if(delete)
        {
            Player.localPlayer.playerModularBuilding.CmdDeleteAccessory(buildingAccessory.netIdentity);
        }
        else
        {
            Player.localPlayer.playerModularBuilding.CmdRepairAccessory(buildingAccessory.netIdentity);
        }
        CloseButton(true);
    }

    void UseItem(BuildingAccessory buildingAccessory, GameObject objectToInstantiate)
    {
        ModularBuildingManager.singleton.spawnedAccesssory = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
        ModularBuildingManager.singleton.activeBuildingModeWall = false;
        ModularBuildingManager.singleton.activeBuildingModeDoor = false;
        ModularBuildingManager.singleton.activeBasementPositioning = false;
        ModularBuildingManager.singleton.activeFencePositioning = false;
        ModularBuildingManager.singleton.activeGatePositioning = false;

        switch (buildingAccessory.craftingAccessoryItem.GetType().ToString())
        {
            case "ScriptableBuildingAccessory":
                ModularBuildingManager.singleton.scriptableBuildingAccessory = buildingAccessory.craftingAccessoryItem;
                break;
            case "ScriptableFence":
                ModularBuildingManager.singleton.scriptableFence = (ScriptableFence)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.activeFencePositioning = true;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
            case "ScriptableGate":
                ModularBuildingManager.singleton.scriptableGate = (ScriptableGate)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.activeGatePositioning = true;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
            case "ScriptableConcrete":
                ModularBuildingManager.singleton.scriptableExternal = (ScriptableConcrete)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
            case "ScriptableBillboard":
                ModularBuildingManager.singleton.scriptableExternal = (ScriptableBillboard)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
            case "ScriptableFlag":
                ModularBuildingManager.singleton.scriptableExternal = (ScriptableFlag)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
            case "ScriptableLight":
                ModularBuildingManager.singleton.scriptableExternal = (ScriptableLight)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
            case "ScriptableFurnace":
                ModularBuildingManager.singleton.scriptableExternal = (ScriptableFurnace)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
            case "ScriptableWaterContainer":
                ModularBuildingManager.singleton.scriptableExternal = (ScriptableWaterContainer)buildingAccessory.craftingAccessoryItem;
                ModularBuildingManager.singleton.spawnedBuilding = Instantiate(objectToInstantiate.GetComponent<BuildingAccessory>().craftingAccessoryItem.buildingList[objectToInstantiate.GetComponent<BuildingAccessory>().oldPositioning].buildingObject, buildingAccessory.transform.position, Quaternion.identity);
                Destroy(ModularBuildingManager.singleton.spawnedAccesssory);
                break;
        }

        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        if(ModularBuildingManager.singleton.scriptableExternal) 
            ModularBuildingManager.singleton.uIModularBuilding.changePerspective.interactable = (ModularBuildingManager.singleton.scriptableExternal && ((ScriptableBuildingAccessory)ModularBuildingManager.singleton.scriptableExternal).buildingList.Count > 1);
        if (ModularBuildingManager.singleton.scriptableBuildingAccessory) 
            ModularBuildingManager.singleton.uIModularBuilding.changePerspective.interactable = (ModularBuildingManager.singleton.scriptableBuildingAccessory && ModularBuildingManager.singleton.scriptableBuildingAccessory.buildingList.Count > 1);
        if (ModularBuildingManager.singleton.scriptableBuilding) 
            ModularBuildingManager.singleton.uIModularBuilding.changePerspective.interactable = (ModularBuildingManager.singleton.scriptableBuilding && ModularBuildingManager.singleton.scriptableBuilding.buildingList.Count > 1);
        if (ModularBuildingManager.singleton.scriptableFence) 
            ModularBuildingManager.singleton.uIModularBuilding.changePerspective.interactable = (ModularBuildingManager.singleton.scriptableFence && ModularBuildingManager.singleton.scriptableFence.buildingList.Count > 1);
        if (ModularBuildingManager.singleton.scriptableGate) 
            ModularBuildingManager.singleton.uIModularBuilding.changePerspective.interactable = (ModularBuildingManager.singleton.scriptableGate && ModularBuildingManager.singleton.scriptableGate.buildingList.Count > 1);
        ModularBuildingManager.singleton.uIModularBuilding.panel.SetActive(true);
        ModularBuildingManager.singleton.uIModularBuilding.up.interactable = true;
        ModularBuildingManager.singleton.uIModularBuilding.left.interactable = true;
        ModularBuildingManager.singleton.uIModularBuilding.down.interactable = true;
        ModularBuildingManager.singleton.uIModularBuilding.right.interactable = true;
        ModularBuildingManager.singleton.skillbarObject.SetActive(false);

        ModularBuildingManager.singleton.inventoryIndex = -1;
    }

    public void Movebutton()
    {
        ManageUI(false);
    }

    public void RepairButton()
    {
        ManageUI(true);
    }

    public void DeleteButton()
    {
        ManageUI(false);
    }

    public void CloseButton(bool close)
    {
        if (close)
        {
            if (parentButton) 
                parentButton.onClick.Invoke();
        }
        Destroy(singleton);
        Destroy(this.gameObject);
    }

    public void ManageUI(bool condition)
    {
        repair = condition;
        scrollRect.gameObject.SetActive(condition);
        repairText.gameObject.SetActive(condition);
        moveText.gameObject.SetActive(!condition);
        if (delete)
        {
            moveText.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
        }
        deleteText.gameObject.SetActive(delete);

        if (!delete)
        {
            if (repair)
            {
                if (accessoryToAdd && accessoryIdentity)
                {
                    hasAllItemToRepair = 1;
                    UIUtils.BalancePrefabs(objectToCreate, accessoryToAdd.repairItems.Count, content);
                    for (int i = 0; i < accessoryToAdd.repairItems.Count; i++)
                    {
                        int index = i;
                        UICraftSlotChild slot = content.GetChild(index).gameObject.GetComponent<UICraftSlotChild>();
                        slot.ingredientImage.sprite = accessoryToAdd.repairItems[index].items.image;
                        slot.ingredientImage.preserveAspect = true;
                        int hasItem = Player.localPlayer.inventory.CountItem(new Item(accessoryToAdd.repairItems[index].items));
                        if (hasAllItemToRepair > 0) hasAllItemToRepair = Player.localPlayer.inventory.CountItem(new Item(accessoryToAdd.repairItems[index].items));
                        slot.ingredientsAmount.color = hasItem < accessoryToAdd.repairItems[index].amount ? Color.red : Color.white;
                        slot.ingredientsAmount.text = accessoryToAdd.repairItems[index].amount + "/" + Player.localPlayer.inventory.CountItem(new Item(accessoryToAdd.repairItems[index].items));
                    }
                    confirmButton.interactable = hasAllItemToRepair > 0 && Player.localPlayer.inventory.CanAddItem(new Item(accessoryToAdd), 1) && buildingAccessory.health < buildingAccessory.maxHealth;
                }
            }
            else
            {
                confirmButton.interactable = true;
            }
        }
        else
        {
            confirmButton.interactable = true;
        }
    }
}
