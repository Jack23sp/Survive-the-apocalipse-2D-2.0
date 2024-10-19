using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAccessoryPanel : MonoBehaviour, IUIScriptNoBuildingRelated
{


    public Button closeButton;
    public GameObject panel;

    public void Start()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });
    }

    void OnEnable()
    {
        Assign();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }

}
