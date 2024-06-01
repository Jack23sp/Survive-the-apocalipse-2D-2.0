using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public partial class UIChat : MonoBehaviour
{
    public static UIChat singleton;
    public GameObject panel;
    public InputField messageInput;
    public Button sendButton;
    public Transform content;
    public ScrollRect scrollRect;
    public KeyCode[] activationKeys = { KeyCode.Return, KeyCode.KeypadEnter };
    public int keepHistory = 100; // only keep 'n' messages

    public PlayerChat chat;

    bool eatActivation;

    public List<Button> categoryButton;

    public int selectedChat = 0;

    public List<BubbleMessage> bubbleMessages = new List<BubbleMessage>();

    public bool isOpen = true;
    public Button openButton;
    public Animator animator;

    public void Start()
    {
        if (!singleton) singleton = this;

        Invoke(nameof(SearchPlayer), 0.1f);
    }

    public void SearchPlayer()
    {
        if (Player.localPlayer)
            SetActions();
        else
            Invoke(nameof(SearchPlayer), 0.1f);
    }

    public void SetActions()
    {
        chat = Player.localPlayer.chat;
        messageInput.characterLimit = chat.maxLength;

        messageInput.onEndEdit.RemoveAllListeners();
        messageInput.onEndEdit.SetListener((value) =>
        {
            // submit key pressed? then submit and set new input text
            if (Utils.AnyKeyDown(activationKeys))
            {
                string newinput = chat.OnSubmit(value);
                messageInput.text = newinput;
                messageInput.MoveTextEnd(false);
                eatActivation = true;
            }

            // unfocus the whole chat in any case. otherwise we would scroll or
            // activate the chat window when doing wsad movement afterwards
            UIUtils.DeselectCarefully();
        });

        // send button
        sendButton.onClick.RemoveAllListeners();
        sendButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(18);
            // submit and set new input text
            string newinput = chat.OnSubmit(messageInput.text);
            messageInput.text = newinput;
            messageInput.MoveTextEnd(false);

            // unfocus the whole chat in any case. otherwise we would scroll or
            // activate the chat window when doing wsad movement afterwards
            UIUtils.DeselectCarefully();
        });
        panel.SetActive(true);

        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            isOpen = !isOpen;
            animator.SetBool("OPEN", isOpen);
        });

        for (int i = 0; i < categoryButton.Count; i++)
        {
            int index = i;
            categoryButton[index].onClick.RemoveAllListeners();
            categoryButton[index].onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                Display(index);
            });
        }
        openButton.onClick.Invoke();

        Display(0);
    }


    public void Display(int whichDisplay)
    {
        selectedChat = whichDisplay;
        if (!isOpen)
        {
            isOpen = true;
            animator.SetBool("OPEN", isOpen);
        }
        switch (whichDisplay)
        {
            case 0:
                AddMessageSeparated(Player.localPlayer.chat.infoChat.ToList(), 0);
                if (messageInput.text.Contains("/w"))
                    messageInput.text.Replace("/w", "");
                chat.infoToSee = 0;
                bubbleMessages[0].gameObject.SetActive(false);
                break;
            case 1:
                AddMessageSeparated(Player.localPlayer.chat.localChat.ToList(), 1);
                if (messageInput.text.Contains("/w"))
                    messageInput.text.Replace("/w","");
                bubbleMessages[1].gameObject.SetActive(false);
                chat.localToSee = 0;
                break;
            case 2:
                AddMessageSeparated(Player.localPlayer.chat.whisperChat.ToList(), 2);
                if (!messageInput.text.Contains("/w")) messageInput.text = "/w" + messageInput.text;
                chat.whisperToSee = 0;
                bubbleMessages[2].gameObject.SetActive(false);
                break;
            case 3:
                AddMessageSeparated(Player.localPlayer.chat.partyChat.ToList(), 3);
                if (messageInput.text.Contains("/w"))
                    messageInput.text.Replace("/w", "");
                chat.partyToSee = 0;
                bubbleMessages[3].gameObject.SetActive(false);
                break;
            case 4:
                AddMessageSeparated(Player.localPlayer.chat.guildChat.ToList(), 4);
                if (messageInput.text.Contains("/w"))
                    messageInput.text.Replace("/w", "");
                chat.guildToSee = 0;
                bubbleMessages[4].gameObject.SetActive(false);
                break;
            case 5:
                AddMessageSeparated(Player.localPlayer.chat.allyChat.ToList(), 5);
                if (messageInput.text.Contains("/w"))
                    messageInput.text.Replace("/w", "");
                chat.allyToSee = 0;
                bubbleMessages[5].gameObject.SetActive(false);
                break;
        }
    }

    void AutoScroll()
    {
        // update first so we don't ignore recently added messages, then scroll
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void AddMessage(ChatMessage message)
    {
        //// delete old messages so the UI doesn't eat too much performance.
        //// => every Destroy call causes a lag because of a UI rebuild
        //// => it's best to destroy a lot of messages at once so we don't
        ////    experience that lag after every new chat message
        if (content.childCount >= keepHistory)
        {
            for (int i = 0; i < content.childCount / 2; ++i)
                Destroy(content.GetChild(i).gameObject);
        }

        // instantiate and initialize text prefab
        //GameObject go = Instantiate(message.textPrefab, content.transform, false);
        //go.GetComponent<Text>().text = message.Construct();
        //go.GetComponent<UIChatEntry>().message = message;

        AutoScroll();
    }

    public GameObject SearchChatPrefab(int type)
    {
        return Player.localPlayer.chat.textPrefab[type];
    }

    public void AddMessageSeparated(List<ChatMessage> message, int type)
    {
        UIUtils.BalancePrefabs(SearchChatPrefab(type), message.Count, content.transform);
        for (int i = 0; i < message.Count; i++)
        {
            int index = i;
            GameObject go = content.transform.GetChild(index).transform.gameObject;
            go.GetComponent<Text>().text = message[index].Construct();
            go.GetComponent<Text>().color = SearchChatPrefab(type).GetComponent<Text>().color;
            go.GetComponent<UIChatEntry>().message = message[index];
        }

        AutoScroll();
    }

    // called by chat entries when clicked
    public void OnEntryClicked(UIChatEntry entry)
    {
        // any reply prefix?
        if (!string.IsNullOrWhiteSpace(entry.message.replyPrefix))
        {
            // set text to reply prefix
            messageInput.text = entry.message.replyPrefix;

            // activate
            messageInput.Select();

            // move cursor to end (doesn't work in here, needs small delay)
            Invoke(nameof(MoveTextEnd), 0.1f);
        }
    }

    void MoveTextEnd()
    {
        messageInput.MoveTextEnd(false);
    }
}
