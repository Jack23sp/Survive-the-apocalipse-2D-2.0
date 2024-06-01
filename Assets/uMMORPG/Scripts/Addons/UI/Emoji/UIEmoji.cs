using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEmoji : MonoBehaviour
{
    public static UIEmoji singleton;
    public Transform content;
    public Button openCloseButton;
    public Animator emojiButton;
    public List<EmojiSlot> emojiAnimators;
    public int selectedEmoji;
    public UISelectedEmoji selectedEmojiPanel;

    void Awake()
    {
        if (!singleton) singleton = this;
        openCloseButton.onClick.AddListener(() =>
        {
            emojiButton.SetBool("OPEN", !emojiButton.GetBool("OPEN"));
            if (emojiButton.GetBool("OPEN"))
            {
                foreach (EmojiSlot anim in emojiAnimators)
                {
                    anim.CheckEmoji(true);
                }
            }
            else
            {
                foreach (EmojiSlot anim in emojiAnimators)
                {
                    anim.CheckEmoji(false);
                }
                content.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 1;
            }
        });
    }

    public void RefreshEmoji()
    {
        for (int i = 0; i < emojiAnimators.Count; i++)
        {
            int index = i;
            if (Player.localPlayer.playerEmoji.networkEmoji.Contains(emojiAnimators[index].gameObject.name))
            {
                emojiAnimators[index].padLock.gameObject.SetActive(false);
                emojiAnimators[index].manageEmojiButton.onClick.RemoveAllListeners();
                emojiAnimators[index].manageEmojiButton.onClick.AddListener(() =>
                {
                    //Spawn emoji
                    Player.localPlayer.playerEmoji.CmdSpawnEmoji(emojiAnimators[index].gameObject.name, Player.localPlayer.name);
                });
            }
            else
            {
                //Spawn emoji panel
                emojiAnimators[index].manageEmojiButton.onClick.RemoveAllListeners();
                emojiAnimators[index].manageEmojiButton.onClick.AddListener(() =>
                {
                    selectedEmoji = index;
                    selectedEmojiPanel.gameObject.SetActive(true);
                    if (selectedEmojiPanel.spawnedEmoji != null) Destroy(selectedEmojiPanel.spawnedEmoji);
                    selectedEmojiPanel.spawnedEmoji = Instantiate(emojiAnimators[index].gameObject, selectedEmojiPanel.spawnEmojiPointer);
                    selectedEmojiPanel.spawnedEmoji.GetComponent<RectTransform>().sizeDelta = new Vector2(20.0f, 20.0f);
                    selectedEmojiPanel.spawnedEmoji.GetComponent<EmojiSlot>().padLock.SetActive(false);
                });
            }
        }
    }
}
