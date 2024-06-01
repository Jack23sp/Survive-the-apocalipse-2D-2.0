using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFrontStats : MonoBehaviour
{
    public static UIFrontStats singleton;
    public GameObject panel;

    public GameObject toSpawn;

    public List<UIStatSlot> stats;

    void Awake()
    {
        if (!singleton) singleton = this;

        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].button != null)
            {
                if (stats[i].torch)
                {
                    stats[i].button.onClick.AddListener(() =>
                    {
                        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                        Player.localPlayer.playerTorch.CmdSetTorch();
                    });
                }
            }
        }
    }

}
