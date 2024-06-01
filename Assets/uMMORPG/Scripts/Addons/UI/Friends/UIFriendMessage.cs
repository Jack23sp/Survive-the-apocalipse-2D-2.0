using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIFriendMessage : MonoBehaviour
{
    public static UIFriendMessage singleton;

    public GameObject panel;

    public TMP_InputField messageInputText;
    public Button sendButton;
    public TextMeshProUGUI sendButtonText;
    public Button closeButton;

    public TextMeshProUGUI title;

    private Player selectedPlayer;
    public string playerName;

    void Start()
    {
        if (!singleton) singleton = this;
        messageInputText.onValueChanged.AddListener(delegate { CheckChange(); });

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            panel.SetActive(false);
            closeButton.image.raycastTarget = false;
            selectedPlayer = null;
            playerName = string.Empty;
            closeButton.image.enabled = false;
            BlurManager.singleton.Show();
        });

        sendButton.onClick.RemoveAllListeners();
        sendButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(18);
            if (messageInputText.text != string.Empty)
            {
                if (Player.onlinePlayers.TryGetValue(playerName, out Player selectedPlayer))
                {
                    Player.localPlayer.chat.CmdMsgWhisper(playerName, messageInputText.text);
                    Player.localPlayer.playerNotification.SpawnNotification(ImageManager.singleton.message, "Message sent to : " + playerName);
                }
                else
                {
                    Player.localPlayer.playerNotification.SpawnNotification(ImageManager.singleton.message, "Cannot send message because " + playerName + " is not online");
                }
            }
            closeButton.onClick.Invoke();
        });

    }

    public void Open(string friendName)
    {
        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;
        panel.SetActive(true);
        playerName = friendName;

        title.text = "Message to : " + playerName;

        messageInputText.text = string.Empty;
        sendButtonText.text = "Waiting!";
        sendButton.interactable = false;
    }

    public void CheckChange()
    {
        sendButton.interactable = sendButtonText.text != string.Empty;
        sendButtonText.text = messageInputText.text != string.Empty ? "Send!" : "Waiting!";
    }
}
