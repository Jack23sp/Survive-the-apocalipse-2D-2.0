using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBoosts : MonoBehaviour
{
    public static UIBoosts singleton;
    public Button addBoost;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!singleton) singleton = this;

        addBoost.onClick.AddListener(() =>
        {
            Player.localPlayer.playerBoost.CmdAddBoost("Velocity");
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
