using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MenuButton : MonoBehaviour
{
    public static MenuButton singleton;
    public List<ScrollRect> contentFirstPanel = new List<ScrollRect>();
    public List<ScrollRect> contentSecondPanel = new List<ScrollRect>();
    public List<ScrollRect> contentThirdPanel = new List<ScrollRect>();
    public List<ScrollRect> contentFourthPanel = new List<ScrollRect>();
    public List<ScrollRect> contentFifthPanel = new List<ScrollRect>();
    public List<ScrollRect> contentSixthPanel = new List<ScrollRect>();
    public List<ScrollRect> contentSeventhPanel = new List<ScrollRect>();
    public List<ScrollRect> contentEightPanel = new List<ScrollRect>();
    public List<ScrollRect> contentNinthPanel = new List<ScrollRect>();

    public List<GameObject> panels = new List<GameObject>();
    public GameObject panel;

    public Button inventoryButton;
    public Button abilitiesButton;
    public Button groupButton;
    public Button questsButton;
    public Button spawnpointButton;
    public Button mapButton;
    public Button boostsButton;
    public Button friendButton;
    public Button optionsButton;


    public Button closeButton;
    public Button openPanel;

    public UIInventoryCustom inventory;
    public UIEquipment equipment;
    public UIStats stats;
    public UIAbilities abilities;
    public UISpawnpoint spawnpoint;
    public UIGroup group;
    public UIFriends friends;
    public UIOptions options;
    public UIBoost boosts;
    public UIQuests quests;
    private Camera minimapCamera;
    public RawImage minimapRaw;
    public TextMeshProUGUI fatText;

    public void Update()
    {
        if(minimapCamera && minimapCamera.enabled)
        {
            minimapRaw.texture = minimapCamera.activeTexture;
        }
    }

    void Start()
    {
        if (!singleton) singleton = this;

        Invoke(nameof(CheckPlayer), 0.5f);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            ManageClose();
        });

        

        openPanel.onClick.RemoveAllListeners();
        openPanel.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            if (!minimapCamera) minimapCamera = RenderTextureManager.singleton.minimapCamera;
            ManageClose();
            panels[0].gameObject.SetActive(true);
            inventory.Open();
            equipment.Open();
            panel.SetActive(true);
            closeButton.image.raycastTarget = true;
            closeButton.image.enabled = true;
            SpawnpointManageButtonForSpawnpoint(true);
            BlurManager.singleton.Hide();
            RefreshFatText();
        });

        inventoryButton.onClick.RemoveAllListeners();
        inventoryButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 0)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentFirstPanel.Count; e++)
                    {
                        contentFirstPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }
            inventory.Open();
            equipment.Open();
            stats.SpawnStatsAtBegins();
            minimapCamera.enabled = false;
        });

        abilitiesButton.onClick.RemoveAllListeners();
        abilitiesButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 1)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentSecondPanel.Count; e++)
                    {
                        contentSecondPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }

            abilities.Spawn();
            abilities.content.GetChild(0).GetComponent<AbilitySlot>().button.onClick.Invoke();
            abilities.content.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
            minimapCamera.enabled = false;
        });

        groupButton.onClick.RemoveAllListeners();
        groupButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 2)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentThirdPanel.Count; e++)
                    {
                        contentThirdPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }
            group.RefreshGuild();
            minimapCamera.enabled = false;
        });

        questsButton.onClick.RemoveAllListeners();
        questsButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 3)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentFourthPanel.Count; e++)
                    {
                        contentFourthPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }
            quests.Open();
            minimapCamera.enabled = false;
        });

        spawnpointButton.onClick.RemoveAllListeners();
        spawnpointButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 4)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentFifthPanel.Count; e++)
                    {
                        contentFifthPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }

            spawnpoint.RefreshSpawnpoint();
            minimapCamera.enabled = false;
        });

        mapButton.onClick.RemoveAllListeners();
        mapButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 5)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentSixthPanel.Count; e++)
                    {
                        contentSixthPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }
            minimapCamera.enabled = true;
        });

        boostsButton.onClick.RemoveAllListeners();
        boostsButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 6)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentSeventhPanel.Count; e++)
                    {
                        contentSeventhPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }
            minimapCamera.enabled = false;
            boosts.UpdateBoost();
        });

        friendButton.onClick.RemoveAllListeners();
        friendButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 7)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentEightPanel.Count; e++)
                    {
                        contentEightPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }
            friends.Open();
            minimapCamera.enabled = false;
        });

        optionsButton.onClick.RemoveAllListeners();
        optionsButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(9);
            if (UIInventoryCustom.singleton)
            {
                UIInventoryCustom.singleton.operationType = -1;
                UIInventoryCustom.singleton.ResetAspectOfItemOutline();
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (i != 8)
                {
                    panels[i].gameObject.SetActive(false);
                }
                else
                {
                    panels[i].gameObject.SetActive(true);
                    for (int e = 0; e < contentNinthPanel.Count; e++)
                    {
                        contentNinthPanel[e].verticalNormalizedPosition = 1;
                    }
                }
            }
            minimapCamera.enabled = false;
            options.Open();
        });
    }

    public void RefreshFatText()
    {
       if(Player.localPlayer) fatText.text = "Fat gained : " + (Player.localPlayer.playerCharacterCreation.fat / 30).ToString("F2");
    }

    public void Close()
    {
        closeButton.image.raycastTarget = false;
        closeButton.image.enabled = false;
        for (int i = 0; i < panels.Count; i++)
        {
            if (i > 0)
            {
                panels[i].gameObject.SetActive(false);
            }
            else
            {
                panels[i].gameObject.SetActive(true);
                for (int e = 0; e < contentFirstPanel.Count; e++)
                {
                    contentFirstPanel[e].verticalNormalizedPosition = 1;
                }
            }
        }
        panel.SetActive(false);
        minimapCamera.enabled = false;
        BlurManager.singleton.Show();
        if (UIInventoryCustom.singleton)
        {
            UIInventoryCustom.singleton.operationType = -1;
            UIInventoryCustom.singleton.ResetAspectOfItemOutline();
        }
    }

    public void ManageClose()
    {
        closeButton.image.raycastTarget = false;
        closeButton.image.enabled = false;
        for (int i = 0; i < panels.Count; i++)
        {
            if (i > 0)
            {
                panels[i].gameObject.SetActive(false);
            }
            else
            {
                panels[i].gameObject.SetActive(true);
                for (int e = 0; e < contentFirstPanel.Count; e++)
                {
                    contentFirstPanel[e].verticalNormalizedPosition = 1;
                }
            }
        }
        panel.SetActive(false);
        SpawnpointManageButtonForSpawnpoint(true);
        minimapCamera.enabled = false;
        BlurManager.singleton.Show();

    }

    public void CheckPlayer()
    {
        if (Player.localPlayer)
        {
            openPanel.image.enabled = true;
            openPanel.enabled = true;
        }
        else
        {
            Invoke(nameof(CheckPlayer), 0.5f);
        }
    }


    public void SpawnpointManageButtonForSpawnpoint(bool condition)
    {
        if (condition)
        {
            inventoryButton.interactable =
            abilitiesButton.interactable =
            groupButton.interactable =
            questsButton.interactable =
            spawnpointButton.interactable =
            mapButton.interactable =
            boostsButton.interactable =
            friendButton.interactable =
            optionsButton.interactable =
            closeButton.interactable = condition;
        }
        else
        {
            inventoryButton.interactable =
            abilitiesButton.interactable =
            groupButton.interactable =
            questsButton.interactable =
            closeButton.interactable =
            mapButton.interactable =
            boostsButton.interactable =
            friendButton.interactable =
            optionsButton.interactable = condition;
        }
    }
}

