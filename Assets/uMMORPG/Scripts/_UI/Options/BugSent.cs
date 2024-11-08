using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BugSent : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public static BugSent singleton;
    public GameObject panel;
    public Button closeButton;
    public Button sendButton;
    public TMP_InputField inputField;
    public TextMeshProUGUI sendButtonText;
    public Image sendImage;
    private Player player;

    public Color waitingToSend;

    public void Start()
    {
        if(!singleton) singleton = this;
        sendButtonText.text = "Send!";

        sendButton.onClick.RemoveAllListeners();
        sendButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(18);
            player.playerOptions.CmdSaveIssue(player.name, "Bug", inputField.text);
            sendImage.color = Color.green;
            sendButtonText.text = "Thanks!";
        });

        closeButton.onClick.AddListener(() =>
        {
            Close();
        });
        inputField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void Open()
    {
        player = Player.localPlayer;
        if (!player) return;
        Assign();
        closeButton.image.enabled = true;
        closeButton.image.raycastTarget = true;
        sendButtonText.text = "Send!";
        panel.SetActive(true);
        sendImage.color = Color.yellow;
    }

    public void ValueChangeCheck()
    {
        sendButton.interactable = inputField.text != string.Empty;
        sendImage.color = inputField.text != string.Empty ? waitingToSend : Color.yellow;
    }

    public void Close()
    {
        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
        panel.SetActive(false);
        closeButton.image.raycastTarget = false;
        sendImage.color = Color.yellow;
        inputField.text = string.Empty;
        sendButtonText.text = "Send!";
        closeButton.image.enabled = false;
        BlurManager.singleton.Show();
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }
}
