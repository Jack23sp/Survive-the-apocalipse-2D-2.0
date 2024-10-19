using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnpointButton : MonoBehaviour
{
    public Button openSpawnpointPanel;
    public Image panelImage;
    public GameObject panel;

    public void Start()
    {
        openSpawnpointPanel.onClick.AddListener(() =>
        {
            panel.SetActive(true);
            panelImage.raycastTarget = true;
        });
    }
}
