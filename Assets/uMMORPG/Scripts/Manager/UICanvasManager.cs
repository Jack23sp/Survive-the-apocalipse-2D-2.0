using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICanvasManager : MonoBehaviour
{
    public static UICanvasManager singleton;
    public Button menu;
    public GameObject menuObject;
    public GameObject skillbar;
    public GameObject manager;


    void Start()
    {
        if (!singleton) singleton = this;
        Invoke(nameof(CheckActivation),1.0f);
        menu.onClick.SetListener(() =>
        {
            menuObject.SetActive(!menuObject.activeInHierarchy);
            menuObject.GetComponent<Button>().image.raycastTarget = menuObject.activeInHierarchy;
        });
    }

    public void Update()
    {
        if (!manager.activeInHierarchy) manager.SetActive(true);
    }

    void CheckActivation()
    {
        if (Player.localPlayer)
        {
            menu.gameObject.SetActive(true);
            skillbar.SetActive(true);
        }
        else
        {
            Invoke(nameof(CheckActivation), 1.0f);
        }
    }
}
