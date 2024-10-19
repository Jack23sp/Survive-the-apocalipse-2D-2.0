using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UISpawnpoint : MonoBehaviour
{
    private Player player;
    public static UISpawnpoint singleton;

    public TextMeshProUGUI description;
    public Button spawnHere;
    public Button spawnSomewhere;
    public GameObject createSpawnpoint;
    public GameObject setSpawnpoint;
    public Button buttonCreateSpawnpoint;
    public Button cancelSpawnpoint;
    public Button confirmSpawnpoint;
    public Button preferedSpawnpoint;
    public TMP_InputField inputFieldSpawnpoint;

    public GameObject spawnpoint;
    public Transform spawnpointContent;

    public int possibleSpawnpoint;
    public bool prefered;
    public int spawnpointAbility;

    public void Start()
    {
        if (!singleton) singleton = this;

        createSpawnpoint.SetActive(false);
        setSpawnpoint.SetActive(true);
        RefreshSpawnpoint();
        inputFieldSpawnpoint.onValueChanged.AddListener(delegate { RefreshText(); });

        spawnHere.onClick.RemoveAllListeners();
        spawnHere.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerSpawnpoint.CmdSpawnpointRevive(1.0f);
            ResetAfterSpawnpointClick();
        });

        spawnSomewhere.onClick.RemoveAllListeners();
        spawnSomewhere.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerSpawnpoint.CmdSpawnSomewhere();
            ResetAfterSpawnpointClick();
        });

        buttonCreateSpawnpoint.onClick.RemoveAllListeners();
        buttonCreateSpawnpoint.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            createSpawnpoint.SetActive(true);
            inputFieldSpawnpoint.text = string.Empty;
            preferedSpawnpoint.image.sprite = SpawnpointManager.singleton.notPrefered;
            setSpawnpoint.SetActive(false);
        });

        cancelSpawnpoint.onClick.RemoveAllListeners();
        cancelSpawnpoint.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            inputFieldSpawnpoint.text = string.Empty;
            preferedSpawnpoint.image.sprite = SpawnpointManager.singleton.notPrefered;
            createSpawnpoint.SetActive(false);
            setSpawnpoint.SetActive(true);
        });

        confirmSpawnpoint.onClick.RemoveAllListeners();
        confirmSpawnpoint.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            player.playerSpawnpoint.CmdSetSpawnpoint(inputFieldSpawnpoint.text, player.transform.position.x, player.transform.position.y, prefered, player.name);
            prefered = false;
            cancelSpawnpoint.onClick.Invoke();
        });

        preferedSpawnpoint.onClick.RemoveAllListeners();
        preferedSpawnpoint.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            prefered = !prefered;
            preferedSpawnpoint.image.sprite = prefered ? SpawnpointManager.singleton.prefered : SpawnpointManager.singleton.notPrefered;
        });
    }

    public void ResetAfterSpawnpointClick()
    {
        MenuButton.singleton.closeButton.interactable = true;
        MenuButton.singleton.closeButton.onClick.Invoke();
    }


    public void RefreshText()
    {
        confirmSpawnpoint.interactable = inputFieldSpawnpoint.text != string.Empty && possibleSpawnpoint > 0;
    }

    public void RefreshSpawnpoint()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        createSpawnpoint.SetActive(false);
        setSpawnpoint.SetActive(true);

        spawnpointAbility = Convert.ToInt32(AbilityManager.singleton.FindNetworkAbilityLevel(player.playerSpawnpoint.ability.name, player.name) / 10);
        possibleSpawnpoint = spawnpointAbility - player.playerSpawnpoint.spawnpoint.Count;

        spawnHere.interactable = player.health.current <= 0 && player.inventory.CountItem(new Item(SpawnpointManager.singleton.Instantresurrect)) > 0;
        spawnSomewhere.interactable = player.health.current <= 0;
        buttonCreateSpawnpoint.interactable = (possibleSpawnpoint > 0 && player.health.current > 0);

        if (prefered)
        {
            preferedSpawnpoint.image.sprite = SpawnpointManager.singleton.prefered;
        }
        else
        {
            preferedSpawnpoint.image.sprite = SpawnpointManager.singleton.notPrefered;
        }

        if (possibleSpawnpoint < 0) possibleSpawnpoint = 0;

        description.text = SpawnpointManager.singleton.messageInDescription + "\nYou can set other : " + possibleSpawnpoint + " spawnpoint.";

        UIUtils.BalancePrefabs(spawnpoint, player.playerSpawnpoint.spawnpoint.Count, spawnpointContent);
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            SpawnpointSlot slot = spawnpointContent.GetChild(index).GetComponent<SpawnpointSlot>();
            Color newColor;

            if (ColorUtility.TryParseHtmlString(player.playerSpawnpoint.spawnpoint[index].color, out newColor))
            {
                slot.GetComponent<Image>().color = newColor;
            }
            slot.prefered = player.playerSpawnpoint.spawnpoint[index].prefered;
            slot.preferButton.image.sprite = player.playerSpawnpoint.spawnpoint[index].prefered ? SpawnpointManager.singleton.prefered : SpawnpointManager.singleton.notPrefered;
            slot.preferButton.onClick.RemoveAllListeners();
            slot.preferButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                if (!player.isServer) player.playerSpawnpoint.CmdSetPrefered(player.playerSpawnpoint.spawnpoint[index].name);
                else player.playerSpawnpoint.SetPrefered(player.playerSpawnpoint.spawnpoint[index].name);
            });

            slot.spawnpointTitle.GetComponentInChildren<TextMeshProUGUI>().text = player.playerSpawnpoint.spawnpoint[index].name;
            slot.spawnpointTitle.interactable = (player.health.current == 0);
            slot.spawnpointTitle.onClick.RemoveAllListeners();
            slot.spawnpointTitle.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                player.playerSpawnpoint.CmdSpawnAtPoint(player.playerSpawnpoint.spawnpoint[index].spawnPositionx, player.playerSpawnpoint.spawnpoint[index].spawnPositiony);
                MenuButton.singleton.closeButton.onClick.Invoke();
            });
            slot.deleteButton.onClick.RemoveAllListeners();
            slot.deleteButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
                player.playerSpawnpoint.CmdDeleteSpawnpoint(player.playerSpawnpoint.spawnpoint[index].name);
            });
        }

    }
}
