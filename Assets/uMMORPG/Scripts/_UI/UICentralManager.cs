using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICentralManager : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public static UICentralManager singleton;

    public Button closeButton;

    public GameObject panel;

    public GameObject floorTexturePanel;
    public GameObject upgradePanel;
    public GameObject deletePanel;
    public GameObject repairPanel;

    public GameObject buttonContainer;
    public Button upgradeButton;
    public Button floorButton;
    public Button deleteButton;
    public Button repairButton;
    public TextMeshProUGUI floorButtonText;

    public Transform upgradeContent;
    public Transform floorContent;
    public Transform floorRepairItemContent;

    public ScrollRect upgradeScrollRect;
    public ScrollRect floorScrollRect;

    public GameObject floorPrefab;
    public GameObject upgradePrefab;
    public GameObject objectToCreate;

    public Button confirmUpgradeButton;
    public Button confirmFloorTextureButton;
    public Button confirmDeleteButton;
    public Button confirmRepairButton;

    public Button changePinButton;
    public Button firstPinModeButton;
    public Button secondPinModeButton;
    public TMP_InputField firstPinInputField;
    public TMP_InputField secondPinInputField;
    public TextMeshProUGUI firstPinText;
    public TextMeshProUGUI secondPinText;
    public TextMeshProUGUI errorText;
    public GameObject messageObject;
    public TextMeshProUGUI messageText;
    public GameObject pinPanel;

    public int selectedTexture = 0;

    public Image preview;

    public TextMeshProUGUI levelText;

    public ScriptableBuilding scriptableBuilding;

    public ModularBuilding modularBuilding;

    [HideInInspector]
    public int mode = -1;
    [HideInInspector]
    public int hasAllItemToRepair = -1;
    [HideInInspector]
    public int selectedPanel = 0;

    public void Start()
    {
        if (!singleton) singleton = this;

        firstPinInputField.onValueChanged.AddListener(OnInputValueChanged1);
        secondPinInputField.onValueChanged.AddListener(OnInputValueChanged2);
    }

    void OnInputValueChanged1(string newValue)
    {
        if (!IsInteger(newValue))
        {
            firstPinInputField.text = RemoveLastCharacter(newValue);
        }
    }

    void OnInputValueChanged2(string newValue)
    {
        if (!IsInteger(newValue))
        {
            secondPinInputField.text = RemoveLastCharacter(newValue);
        }
    }

    bool IsInteger(string value)
    {
        return int.TryParse(value, out _);
    }

    string RemoveLastCharacter(string value)
    {
        if(value.Length > 0)
            return value.Substring(0, value.Length - 1);
        else
            return value;
    }

    public void Reset()
    {
        selectedPanel = 0;
        upgradeScrollRect.verticalNormalizedPosition = 0;
        floorScrollRect.verticalNormalizedPosition = 0;
        upgradePanel.SetActive(false);
        deletePanel.SetActive(false);
        repairPanel.SetActive(false);
        floorTexturePanel.SetActive(true);
        selectedTexture = 0;
        confirmFloorTextureButton.interactable = false;
        confirmUpgradeButton.interactable = false;
        panel.SetActive(false);
        closeButton.image.raycastTarget = false;
        BlurManager.singleton.Show();
        preview.gameObject.SetActive(false);
        changePinButton.interactable = false;
        pinPanel.SetActive(false);
        mode = -1;
    }

    public void SetPreview(int index)
    {
        preview.gameObject.SetActive(true);
        preview.sprite = FloorManager.singleton.floorSprites[index];
        preview.preserveAspect = true;
        selectedTexture = index;
        if (modularBuilding.floorTexture != index) confirmFloorTextureButton.interactable = true;
        else confirmFloorTextureButton.interactable = false;
    }

    public void RefreshModularBuildingLevel()
    {
        levelText.text = "Building level : " + modularBuilding.level.ToString();
    }

    public void RefreshItemsToUpgrade()
    {
        UIUtils.BalancePrefabs(upgradePrefab, modularBuilding.building.upgradeItems.Count, upgradeContent);
        for (int i = 0; i < modularBuilding.building.upgradeItems.Count; i++)
        {
            int index = i;
            UICraftSlotChild slot = upgradeContent.GetChild(index).GetComponent<UICraftSlotChild>();
            int has = Player.localPlayer.inventory.CountItem(new Item(modularBuilding.building.upgradeItems[index].items));
            slot.ingredientsAmount.text = has + " / " + modularBuilding.building.upgradeItems[index].amount;
            slot.ingredientsAmount.color = has < modularBuilding.building.upgradeItems[index].amount ? Color.red : Color.black;
            if (has < modularBuilding.building.upgradeItems[index].amount) confirmUpgradeButton.interactable = false;
            slot.ingredientImage.sprite = modularBuilding.building.upgradeItems[index].items.image;
            slot.ingredientImage.preserveAspect = true;
        }
    }

    public void RefreshPanelView(int mod)
    {
        switch(mod)
        {
            case 1:
                repairButton.onClick.Invoke();
                break;
            case 3:
                upgradeButton.onClick.Invoke();
                break;
        }
    }

    public void Open(ModularBuilding modular, int mod)
    {
        Assign();
        closeButton.image.enabled = true;
        panel.SetActive(true);
        deletePanel.SetActive(false);
        selectedTexture = 0;
        preview.gameObject.SetActive(false);
        floorButton.interactable = true;
        deleteButton.interactable = true;
        upgradeButton.interactable = true;
        repairButton.interactable = true;
        confirmFloorTextureButton.interactable = false;
        confirmUpgradeButton.interactable = false;
        confirmRepairButton.interactable = true;
        closeButton.image.raycastTarget = true;
        mode = mod;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });

        if (modular)
        {
            modularBuilding = modular;
            levelText.text = "Building level : " + modularBuilding.level.ToString();

            if (mod == 0)
            {
                upgradePanel.SetActive(true);
                deletePanel.SetActive(false);
                floorTexturePanel.SetActive(false);
                repairPanel.SetActive(false);

                pinPanel.gameObject.SetActive(false);
                buttonContainer.SetActive(true);

                confirmFloorTextureButton.onClick.RemoveAllListeners();
                confirmFloorTextureButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    if (selectedTexture > -1)
                    {
                        Player.localPlayer.playerModularBuilding.CmdSetFloor(modularBuilding.netIdentity, selectedTexture);
                    }
                });

                confirmDeleteButton.onClick.RemoveAllListeners();
                confirmDeleteButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    Player.localPlayer.playerModularBuilding.CmdSetFakeBuildingID(modularBuilding.netIdentity, new BuildingAccessory[0]);
                    Player.localPlayer.playerModularBuilding.CmdDeleteAccessory();
                    closeButton.onClick.Invoke();
                });

                confirmUpgradeButton.onClick.RemoveAllListeners();
                confirmUpgradeButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    Player.localPlayer.playerModularBuilding.CmdAddLevel(modularBuilding.netIdentity);
                });

                confirmRepairButton.onClick.RemoveAllListeners();
                confirmRepairButton.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    Player.localPlayer.playerModularBuilding.CmdRepairModular(modularBuilding.netIdentity);
                });

                floorButton.onClick.RemoveAllListeners();
                floorButton.onClick.AddListener(() =>
                {
                    selectedPanel = 0;
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    floorTexturePanel.SetActive(true);
                    deletePanel.SetActive(false);
                    upgradePanel.SetActive(false);
                    repairPanel.SetActive(false);
                    UIUtils.BalancePrefabs(floorPrefab, FloorManager.singleton.floorSprites.Count, floorContent);
                    for (int i = 0; i < FloorManager.singleton.floorSprites.Count; i++)
                    {
                        int index = i;
                        FloorSlot slot = floorContent.GetChild(index).GetComponent<FloorSlot>();
                        slot.textureButton.onClick.RemoveAllListeners();
                        slot.textureButton.onClick.AddListener(() =>
                        {
                            confirmUpgradeButton.interactable = true;
                            floorButtonText.text = "OK!";
                            SetPreview(index);
                        });
                        slot.textureImage.sprite = FloorManager.singleton.floorSprites[index];
                        slot.textureImage.preserveAspect = true;
                    }
                });

                repairButton.onClick.RemoveAllListeners();
                repairButton.onClick.AddListener(() =>
                {
                    selectedPanel = 1;
                    floorTexturePanel.SetActive(false);
                    deletePanel.SetActive(false);
                    upgradePanel.SetActive(false);
                    repairPanel.SetActive(true);
                    UIUtils.BalancePrefabs(objectToCreate, modularBuilding.building.repairItems.Count, floorRepairItemContent);
                    for (int i = 0; i < modularBuilding.building.repairItems.Count; i++)
                    {
                        int index = i;
                        hasAllItemToRepair = 0;
                        UICraftSlotChild slot = floorRepairItemContent.GetChild(index).gameObject.GetComponent<UICraftSlotChild>();
                        slot.ingredientImage.sprite = modularBuilding.building.repairItems[index].items.image;
                        slot.ingredientImage.preserveAspect = true;
                        int hasItem = Player.localPlayer.inventory.CountItem(new Item(modularBuilding.building.repairItems[index].items));
                        if (hasAllItemToRepair > 0) 
                                hasAllItemToRepair = Player.localPlayer.inventory.CountItem(new Item(modularBuilding.building.repairItems[index].items));
                        else 
                                confirmRepairButton.interactable = false;
                        slot.ingredientsAmount.color = hasItem < modularBuilding.building.repairItems[index].amount ? Color.red : Color.white;
                        slot.ingredientsAmount.text = modularBuilding.building.repairItems[index].amount + "/" + Player.localPlayer.inventory.CountItem(new Item(modularBuilding.building.repairItems[index].items));
                    }
                    hasAllItemToRepair = 0;
                });

                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() =>
                {
                    selectedPanel = 2;
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    floorTexturePanel.SetActive(false);
                    deletePanel.SetActive(true);
                    upgradePanel.SetActive(false);
                    repairPanel.SetActive(false);
                    confirmDeleteButton.interactable = true;
                });

                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() =>
                {
                    selectedPanel = 3;
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    floorTexturePanel.SetActive(false);
                    deletePanel.SetActive(false);
                    upgradePanel.SetActive(true);
                    repairPanel.SetActive(false);
                    confirmUpgradeButton.interactable = true;
                    RefreshItemsToUpgrade();
                });

                floorButton.onClick.Invoke();
            }
            else
            {
                upgradePanel.SetActive(false);
                floorTexturePanel.SetActive(false);
                pinPanel.gameObject.SetActive(true);
                buttonContainer.SetActive(false);

                if (modular.GetPin() == "0000")
                {
                    if(ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) >= 1)
                    {
                        if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) == 4)
                        {
                            messageObject.SetActive(true);
                            messageText.text = "To take control of this building you need ability Thief at least at level " + modularBuilding.level;
                            secondPinText.gameObject.SetActive(false);
                            secondPinInputField.gameObject.SetActive(false);
                        }
                        else if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) == 3)
                        {
                            messageObject.SetActive(true);
                            messageText.text = "You can take control of this building";
                            secondPinText.gameObject.SetActive(true);
                            secondPinInputField.gameObject.SetActive(false);
                        }
                        else
                        {
                            messageObject.SetActive(false);
                            messageText.text = "To change or view PIN you need to be a master or vice of the group.";
                            messageText.gameObject.SetActive(false);
                            secondPinText.gameObject.SetActive(true);
                            secondPinInputField.gameObject.SetActive(true);
                        }
                        firstPinModeButton.interactable = secondPinModeButton.interactable = true;
                        firstPinInputField.text = modularBuilding.GetPin();
                        secondPinInputField.text = string.Empty;
                        changePinButton.interactable = true;
                        firstPinText.text = "Insert PIN";
                        firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        firstPinInputField.readOnly = true;
                        secondPinText.text = "Repeat PIN";
                        secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        secondPinInputField.readOnly = false;

                        firstPinModeButton.onClick.RemoveAllListeners();
                        firstPinModeButton.onClick.AddListener(() =>
                        {
                            if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) != 3)
                            {
                                if (firstPinInputField.contentType == TMP_InputField.ContentType.Pin)
                                    firstPinInputField.contentType = TMP_InputField.ContentType.Standard;
                                else
                                    firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                                firstPinInputField.ForceLabelUpdate();
                            }
                        });

                        secondPinModeButton.onClick.RemoveAllListeners();
                        secondPinModeButton.onClick.AddListener(() =>
                        {
                            if (secondPinInputField.contentType == TMP_InputField.ContentType.Pin)
                                secondPinInputField.contentType = TMP_InputField.ContentType.Standard;
                            else
                                secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                            secondPinInputField.ForceLabelUpdate();
                        });

                        changePinButton.onClick.RemoveAllListeners();
                        changePinButton.onClick.AddListener(() =>
                        {
                            if (firstPinInputField.text != secondPinInputField.text 
                                && firstPinInputField.text != string.Empty 
                                && secondPinInputField.text != string.Empty 
                                && firstPinInputField.text == modularBuilding.GetPin())
                            {
                                Player.localPlayer.playerModularBuilding.CmdSetPin(modularBuilding.netIdentity, Player.localPlayer.netIdentity, firstPinInputField.text, secondPinInputField.text);
                            }
                            else
                            {
                                errorText.text = "Error in PIN insert";
                            }

                        });

                    }
                    else
                    {
                        messageText.text = "To change or view PIN you need to be a master or vice of the group.";
                        firstPinModeButton.interactable = secondPinModeButton.interactable = false;
                        firstPinInputField.text = modularBuilding.GetPin();
                        secondPinInputField.text = modularBuilding.GetPin();
                        changePinButton.interactable = false;
                        firstPinText.text = "Lock PIN";
                        firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        firstPinInputField.readOnly = true;
                        secondPinInputField.gameObject.SetActive(false);
                        secondPinText.gameObject.SetActive(false);
                        secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        secondPinInputField.readOnly = true;
                        messageObject.SetActive(true);

                        firstPinModeButton.onClick.RemoveAllListeners();
                        firstPinModeButton.onClick.AddListener(() =>
                        {
                            firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                            firstPinInputField.ForceLabelUpdate();
                        });

                        secondPinModeButton.onClick.RemoveAllListeners();
                        secondPinModeButton.onClick.AddListener(() =>
                        {
                            secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                            secondPinInputField.ForceLabelUpdate();
                        });

                        changePinButton.onClick.RemoveAllListeners();
                    }
                }
                else
                {
                    if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) >= 1)
                    {
                        if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) == 4)
                        {
                            messageObject.SetActive(true);
                            messageText.text = "To take control of this building you need ability Thief at least at level " + modularBuilding.level;
                            firstPinModeButton.interactable = false;
                            secondPinText.gameObject.SetActive(false);
                            secondPinInputField.gameObject.SetActive(false);
                        }
                        else if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) == 3)
                        {
                                messageObject.SetActive(true);
                                messageText.text = "You can take control of this building";
                                firstPinModeButton.interactable = false;
                                secondPinText.gameObject.SetActive(true);
                                secondPinInputField.gameObject.SetActive(false);
                        }
                        else if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) == 2)
                        {
                            messageObject.SetActive(true);
                            messageText.text = "";
                            firstPinModeButton.interactable = true;
                            secondPinText.gameObject.SetActive(true);
                            secondPinInputField.gameObject.SetActive(true);
                        }
                        else
                        {
                            messageObject.SetActive(false);
                            messageText.text = "To change or view PIN you need to be a master or vice of the group.";
                            firstPinModeButton.interactable = true;
                            messageText.gameObject.SetActive(false);
                            secondPinText.gameObject.SetActive(true);
                            secondPinInputField.gameObject.SetActive(true);
                        }
                        //firstPinModeButton.interactable = secondPinModeButton.interactable = true;
                        firstPinInputField.text = modularBuilding.GetPin();
                        secondPinInputField.text = string.Empty;
                        changePinButton.interactable = true;
                        firstPinText.text = "Current PIN";
                        firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        firstPinInputField.readOnly = false;
                        secondPinText.text = "Insert new PIN";
                        secondPinInputField.gameObject.SetActive(true);
                        secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        secondPinInputField.readOnly = false;

                        firstPinModeButton.onClick.RemoveAllListeners();
                        firstPinModeButton.onClick.AddListener(() =>
                        {
                            if (ModularBuildingManager.singleton.CanSetPIN(modularBuilding, Player.localPlayer.name) != 3)
                            {
                                if (firstPinInputField.contentType == TMP_InputField.ContentType.Pin)
                                    firstPinInputField.contentType = TMP_InputField.ContentType.Standard;
                                else
                                    firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                                firstPinInputField.ForceLabelUpdate();
                            }
                        });

                        secondPinModeButton.onClick.RemoveAllListeners();
                        secondPinModeButton.onClick.AddListener(() =>
                        {
                            if (secondPinInputField.contentType == TMP_InputField.ContentType.Pin)
                                secondPinInputField.contentType = TMP_InputField.ContentType.Standard;
                            else
                                secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                            secondPinInputField.ForceLabelUpdate();
                        });

                        changePinButton.onClick.RemoveAllListeners();
                        changePinButton.onClick.AddListener(() =>
                        {
                            if (firstPinInputField.text != string.Empty && 
                                secondPinInputField.text != string.Empty && 
                                firstPinInputField.text == modularBuilding.GetPin() &&
                                secondPinInputField.text != modularBuilding.GetPin())
                            {
                                Player.localPlayer.playerModularBuilding.CmdSetPin(modularBuilding.netIdentity, Player.localPlayer.netIdentity, firstPinInputField.text, secondPinInputField.text);
                            }
                            else
                            {
                                errorText.text = "Error in PIN insert";
                            }

                        });
                    }
                    else
                    {
                        messageText.text = "To change or view PIN you need to be a master or vice of the group.";
                        firstPinModeButton.interactable = secondPinModeButton.interactable = false;
                        firstPinText.text = "Lock PIN";
                        firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        firstPinInputField.readOnly = true;
                        firstPinInputField.text = modularBuilding.GetPin();
                        secondPinInputField.text = modularBuilding.GetPin();
                        changePinButton.interactable = false;
                        secondPinInputField.gameObject.SetActive(false);
                        secondPinText.gameObject.SetActive(false);
                        secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                        secondPinInputField.readOnly = true;
                        messageObject.SetActive(true);

                        firstPinModeButton.onClick.RemoveAllListeners();
                        firstPinModeButton.onClick.AddListener(() =>
                        {
                            firstPinInputField.contentType = TMP_InputField.ContentType.Pin;
                            firstPinInputField.ForceLabelUpdate();
                        });

                        secondPinModeButton.onClick.RemoveAllListeners();
                        secondPinModeButton.onClick.AddListener(() =>
                        {
                            secondPinInputField.contentType = TMP_InputField.ContentType.Pin;
                            secondPinInputField.ForceLabelUpdate();
                        });

                        changePinButton.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }

    public void Close()
    {
        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
        Reset();
        BlurManager.singleton.Show();
        closeButton.image.enabled = false;
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }
}
