using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIModularBuilding : MonoBehaviour
{
    public static UIModularBuilding singleton;
    public GameObject panel;
    public Button spawn;
    public Button up;
    public Button down;
    public Button left;
    public Button right;
    public Button cancel;
    public Button changePerspective;

    private void Awake()
    {
        if (!singleton) singleton = this;
    }

    private void Start()
    {
        spawn.onClick.RemoveAllListeners();
        spawn.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(4);
            ModularBuildingManager.singleton.SpawnBuilding();
        });

        up.onClick.RemoveAllListeners();
        up.onClick.AddListener(() =>
        {
            ModularBuildingManager.singleton.Up();
        });

        down.onClick.RemoveAllListeners();
        down.onClick.AddListener(() =>
        {
            ModularBuildingManager.singleton.Down();
        });

        left.onClick.RemoveAllListeners();
        left.onClick.AddListener(() =>
        {
            ModularBuildingManager.singleton.Left();
        });

        right.onClick.RemoveAllListeners();
        right.onClick.AddListener(() =>
        {
            ModularBuildingManager.singleton.Right();
        });

        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            if (Player.localPlayer.playerModularBuilding.fakeBuildingID != null)
            {
                Player.localPlayer.playerModularBuilding.CmdManageVisibilityOfObject(true);
                Player.localPlayer.playerModularBuilding.CmdRemoveFakeBuildingID(true);
                Player.localPlayer.playerModularBuilding.oldBuilding = null;
            }

            ModularBuildingManager.singleton.CancelBuildingMode();
            UIModularBuildingSelector.singleton.actualItem = null;
        });

        changePerspective.onClick.RemoveAllListeners();
        changePerspective.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            ModularBuildingManager.singleton.ChangePerspective();
        });
    }

}
