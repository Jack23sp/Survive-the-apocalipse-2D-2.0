using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISelectedEmoji : MonoBehaviour
{
    public Button buyEmojiButton;
    public TextMeshProUGUI buyEmojiText;
    public Button closeEmojiPanel;
    public Transform spawnEmojiPointer;
    public GameObject spawnedEmoji;

    public void OnEnable()
    {
        buyEmojiText.text = Player.localPlayer.playerEmoji.FindCoinEmojiByName(UIEmoji.singleton.emojiAnimators[UIEmoji.singleton.selectedEmoji].name).ToString();
        if (Player.localPlayer.itemMall.coins < Player.localPlayer.playerEmoji.FindCoinEmojiByName(UIEmoji.singleton.emojiAnimators[UIEmoji.singleton.selectedEmoji].name))
        {
            buyEmojiText.color = Color.red;
        }
        else
        {
            buyEmojiText.color = Color.white;
        }
        buyEmojiButton.onClick.RemoveAllListeners();
        buyEmojiButton.onClick.AddListener(() =>
        {
            if(Player.localPlayer.itemMall.coins >= Player.localPlayer.playerEmoji.FindCoinEmojiByName(UIEmoji.singleton.emojiAnimators[UIEmoji.singleton.selectedEmoji].name))
            {
                Player.localPlayer.playerEmoji.CmdAddEmoji(UIEmoji.singleton.emojiAnimators[UIEmoji.singleton.selectedEmoji].name, 0);
            }
            Close();
        });
        closeEmojiPanel.onClick.RemoveAllListeners();
        closeEmojiPanel.onClick.AddListener(() =>
        {
            Close();
        });

    }

    public void Close()
    {
        Destroy(spawnedEmoji);
        this.gameObject.SetActive(false);
        UIEmoji.singleton.selectedEmoji = -1;
    }
}
