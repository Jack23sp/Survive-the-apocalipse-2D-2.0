using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDeleteWall : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public WallManager wallManager;
    public int positioning;
    public Button panelCancelButton;
    public Button cancelButton;
    public Button confirmButton;

    void Start()
    {
        Assign();
        panelCancelButton.onClick.AddListener(() =>
        {
            BlurManager.singleton.Show();
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            Destroy(this.gameObject);
        });

        cancelButton.onClick.AddListener(() =>
        {
            BlurManager.singleton.Show();
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            Destroy(this.gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            BlurManager.singleton.Show();
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (ModularBuildingManager.singleton.CanDoOtherActionFloor(wallManager.modularBuilding, Player.localPlayer))
                Player.localPlayer.playerModularBuilding.CmdDeleteWall(wallManager.modularBuilding.identity, positioning);
            panelCancelButton.onClick.Invoke();
        });
    }

    public void Close()
    {
        Destroy(this.gameObject);
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }

}
