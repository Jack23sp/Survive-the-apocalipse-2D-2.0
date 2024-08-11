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
    public Button cleanButton;
    public Button claimButton;
    public Button levelUpButton;
    public Button confirmButton;
    private Button parentButton;

    public GameObject objectToCreate;
    public GameObject scrollRect;
    public Transform content;

    public TextMeshProUGUI repairText;
    public TextMeshProUGUI moveText;
    public TextMeshProUGUI deleteText;
    public TextMeshProUGUI cleanText;
    public TextMeshProUGUI claimText;
    public TextMeshProUGUI levelUpText;

    public NetworkIdentity accessoryIdentity;
    public ScriptableBuildingAccessory accessoryToAdd;

    private BuildingAccessory buildingAccessory;
    [HideInInspector] public bool repair;
    [HideInInspector] public bool delete;
    [HideInInspector] public bool clean;
    private int hasAllItemToRepair;
    [HideInInspector] public int selected;
    public Vector3 position;

    public void Init(NetworkIdentity identity, ScriptableBuildingAccessory scriptableBuildingAccessory, Button UIParentButton = null)
    {
        selected = 3;
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

        //float thief = Player.localPlayer.playerAbility.networkAbilities[AbilityManager.singleton.FindNetworkAbility("Thief", Player.localPlayer.name)].level;

        //claimButton.gameObject.SetActive(scriptableBuildingAccessory is ScriptableFence && ((Fence)buildingAccessory).level < thief);
        //claimButton.gameObject.SetActive(scriptableBuildingAccessory is ScriptableGate && ((Gate)buildingAccessory).level < thief);

        //levelUpButton.gameObject.SetActive(scriptableBuildingAccessory is ScriptableFence);
        //levelUpButton.gameObject.SetActive(scriptableBuildingAccessory is ScriptableGate);


        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            CloseButton(UIParentButton != null);
        });

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() =>
        {
            selected = 1;
            ManageUI(selected);
        });

        cleanButton.onClick.RemoveAllListeners();
        cleanButton.onClick.AddListener(() =>
        {
            selected = 2;
            ManageUI(selected);
        });

        repairButton.onClick.RemoveAllListeners();
        repairButton.onClick.AddListener(() =>
        {
            selected = 3;
            ManageUI(selected);
        });

        moveButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(() =>
        {
            selected = 4;
            ManageUI(selected);
        });

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            selected = 5;
            ManageUI(selected);
        });

        levelUpButton.onClick.RemoveAllListeners();
        levelUpButton.onClick.AddListener(() =>
        {
            selected = 6;
            ManageUI(selected);
        });


        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            ConfirmButton();
        });

        ManageUI(selected);
    }

    public void SyncLevelText()
    {
        levelUpText.text = "The level of this chain of this construction is " + (buildingAccessory.GetComponent<Fence>() ? buildingAccessory.GetComponent<Fence>().level.ToString() : buildingAccessory.GetComponent<Gate>().level.ToString());
    }

    public void ConfirmButton()
    {
        if (selected == 4)
        {
            Player.localPlayer.playerModularBuilding.oldBuilding = buildingAccessory;
            Player.localPlayer.playerModularBuilding.fakeBuilding = buildingAccessory.craftingAccessoryItem.GetType();
            Player.localPlayer.playerModularBuilding.accessoryInOldBuilding = buildingAccessory.accessoriesInThisForniture;
            Player.localPlayer.playerModularBuilding.CmdSetFakeBuildingID(buildingAccessory.netIdentity, Player.localPlayer.playerModularBuilding.accessoryInOldBuilding.ToArray());
            Player.localPlayer.playerModularBuilding.CmdManageVisibilityOfObject(false);
            UseItem(buildingAccessory, buildingAccessory.gameObject);
        }
        else if(selected == 1)
        {
            Player.localPlayer.playerModularBuilding.accessoryInOldBuilding = buildingAccessory.accessoriesInThisForniture;
            Player.localPlayer.playerModularBuilding.CmdSetFakeBuildingID(buildingAccessory.netIdentity, Player.localPlayer.playerModularBuilding.accessoryInOldBuilding.ToArray());
            Player.localPlayer.playerModularBuilding.CmdSetAccessoriesList(Player.localPlayer.playerModularBuilding.accessoryInOldBuilding.ToArray());
            Player.localPlayer.playerModularBuilding.CmdDeleteAccessory();
        }
        else if (selected == 3)
        {
            Player.localPlayer.playerModularBuilding.CmdRepairAccessory(buildingAccessory.netIdentity);
        }
        else if (selected == 2)
        {
            Player.localPlayer.CmdCleanAquarium(buildingAccessory.netIdentity);
        }
        else if (selected == 5)
        {
            Player.localPlayer.CmdlClaim(buildingAccessory.netIdentity);
        }
        else if (selected == 6)
        {
            Player.localPlayer.CmdLevelUp(buildingAccessory.netIdentity);
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
            case "ScriptableDumbbell":
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

    public void CloseButton(bool close)
    {
        if (close)
        {
            if (parentButton)
            {
                parentButton.onClick.Invoke();
                BlurManager.singleton.Show();
            }
        }
        Destroy(singleton);
        Destroy(this.gameObject);
    }

    public void ManageUI(int condition)
    {
        repair = condition == 3;
        scrollRect.gameObject.SetActive(condition == 3);
        repairText.gameObject.SetActive(condition == 3);
        moveText.gameObject.SetActive(condition == 4);
        cleanText.gameObject.SetActive(condition == 2);
        deleteText.gameObject.SetActive(condition == 1);
        claimText.gameObject.SetActive(condition == 5);
        levelUpText.gameObject.SetActive(condition == 6);

        claimText.text = string.Empty;

        if (condition == 5)
        {
            float thief = Player.localPlayer.playerAbility.networkAbilities[AbilityManager.singleton.FindNetworkAbility("Thief", Player.localPlayer.name)].level;
            int level = buildingAccessory.GetComponent<Fence>() ? buildingAccessory.GetComponent<Fence>().level : buildingAccessory.GetComponent<Gate>().level;
            if (thief < level)
                claimText.text = "To claim this chain of construction you need the ability Thief to level " + (buildingAccessory.GetComponent<Fence>() ? buildingAccessory.GetComponent<Fence>().level.ToString() : buildingAccessory.GetComponent<Gate>().level.ToString());
            else
                claimText.text = "Do you want to claim this chain of construction?";
        }

        if (condition == 6)
        {
            levelUpText.text = "Do you want level up this chain of construction?";
        }

        if (condition == 3)
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
        else if (condition == 4 || condition == 1 || condition == 2 || condition == 6)
        {
            confirmButton.interactable = true;
        }

        if (condition == 5 && claimText.text == "Do you want to claim this chain of construction?")
        {
            confirmButton.interactable = true;
        }
    }
}
