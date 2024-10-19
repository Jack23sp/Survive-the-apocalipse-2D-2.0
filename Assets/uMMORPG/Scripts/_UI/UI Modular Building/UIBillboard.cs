using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UIBillboard : MonoBehaviour, IUIScript
{
    public static UIBillboard singleton;
    public GameObject panel;

    public Button setButton;
    public TMP_InputField inputField;
    public Button closeButton;
    public Button manageButton;

    public Billboard billboard;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    public void ValueChangeCheck()
    {
        setButton.GetComponentInChildren<TextMeshProUGUI>().text = "Set!";
    }

    public void Open(Billboard Billboard)
    {
        panel.SetActive(true);
        billboard = Billboard;
        Assign();

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(billboard, Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(billboard.netIdentity, billboard.craftingAccessoryItem, closeButton);
        });

        inputField.onValueChanged.RemoveAllListeners();
        inputField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        inputField.text = billboard.message;


        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            panel.SetActive(false);
            inputField.text = string.Empty;
            closeButton.image.raycastTarget = false;
            closeButton.image.enabled = false;
            BlurManager.singleton.Show();
        });

        setButton.onClick.RemoveAllListeners();
        setButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player.localPlayer.CmdSetMessage(inputField.text, Player.localPlayer.netIdentity, billboard.netIdentity);
        });
    }

    public void Close()
    {
        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
        panel.SetActive(false);
        inputField.text = string.Empty;
        closeButton.image.raycastTarget = false;
        closeButton.image.enabled = false;
        RemovePlayerFromBuildingAccessory(billboard.netIdentity);
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
