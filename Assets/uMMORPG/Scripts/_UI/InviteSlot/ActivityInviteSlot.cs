using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

[System.Serializable]
public struct InviteRequest
{
    public string title;
    public string description;
    public bool hasTimer;
    public int type;
    public string sender;
    public string target;

    public InviteRequest(string t, string d, bool tim, int ty, string mP, string tP)
    {
        title = t;
        description = d;
        hasTimer = tim;
        type = ty;
        sender = mP;
        target = tP;
    }
}

public class ActivityInviteSlot : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public static ActivityInviteSlot singleton;

    public GameObject panel;

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI acceptText;
    public Button acceptButton;
    public Button declineButton;
    public int timer = 20;
    public int actualTimer = 0;
    public int type = -1;

    public Player myPlayer;
    public Player target;

    private bool locked;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void Start()
    {
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            Player target = null;
            Player myPlayer = null;
            Player.onlinePlayers.TryGetValue(Player.localPlayer.playerScreenNotification.actualInviteRequest.sender, out target);
            Player.onlinePlayers.TryGetValue(Player.localPlayer.playerScreenNotification.actualInviteRequest.target, out myPlayer);

            if (target != null)
            {
                switch (type)
                {
                    case 0:
                        ResetInviteAndSetNew();
                        myPlayer.party.CmdAcceptInvite();
                        break;
                    case 1:
                        ResetInviteAndSetNew();
                        myPlayer.guild.CmdInviteAccept();
                        break;
                    case 2:
                        ResetInviteAndSetNew();
                        myPlayer.playerAlliance.CmdAcceptInviteToAlliance();
                        break;
                    case 3:
                        ResetInviteAndSetNew();
                        myPlayer.playerSpawnpoint.CmdAcceptResurrect(target.netIdentity);
                        break;
                    case 4:
                        ResetInviteAndSetNew();
                        myPlayer.trading.CmdAcceptRequest();
                        break;
                    case 5:
                        ResetInviteWithoutAction(type);
                        myPlayer.playerFriends.CmdAcceptFriends(target.name);
                        break;
                    case 6:
                        ResetInviteAndSetNew();
                        myPlayer.playerPartner.CmdAcceptInvitePartner();
                        break;
                }
            }
            else
            {
                Player.localPlayer.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "Cannot accept this request because sender is not online!");
                ResetInviteWithoutAction();
            }
        });


        declineButton.onClick.RemoveAllListeners();
        declineButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            ResetInviteWithoutAction(-1);
        });
    }

    public void ResetPanel()
    {
        panel.SetActive(false);
        declineButton.image.enabled = false;
        actualTimer = 0;
        type = -1;
    }

    public void ResetInviteAndSetNew()
    {
        CancelInvoke(nameof(DecreaseTimer));
        locked = false;
        type = -1;
        Player.localPlayer.playerScreenNotification.CmdAcceptAndRemoveFirstInvite();
    }

    public void ResetInviteWithoutAction(int type = -1)
    {
        CancelInvoke(nameof(DecreaseTimer));
        locked = false;
        type = -1;
        Player.localPlayer.playerScreenNotification.CmdRemoveCompleteFirstInvite(type);
    }

    public void Setup(string titleS, string descriptionS, bool hasTimer, int iType, string myPlayer, string target, bool insert)
    {
        Assign();

        panel.SetActive(true);
        declineButton.image.enabled = true;
        if (hasTimer)
        {
            if (!locked) actualTimer = timer;
            SetTimer(actualTimer);
            locked = true;
            Invoke(nameof(DecreaseTimer), 1.0f);
        }
        else
        {
            acceptText.text = "Accept";
        }

        title.text = titleS;
        description.text = descriptionS;
        type = iType;
    }

    public void DecreaseTimer()
    {
        if (actualTimer > 0)
        {
            SetTimer(actualTimer - 1);
            actualTimer--;
            Invoke(nameof(DecreaseTimer), 1.0f);
        }
        else
        {
            ResetInviteWithoutAction();
        }
    }

    public void SetTimer(int timer)
    {
        acceptText.text = "Accept (" + timer + ")";
    }

    public void Close()
    {
        foreach(InviteRequest req in Player.localPlayer.playerScreenNotification.invitation)
        {
            if(Player.localPlayer.playerScreenNotification.invitation.Count > 0)
                declineButton.onClick.Invoke();
        }
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }
}
