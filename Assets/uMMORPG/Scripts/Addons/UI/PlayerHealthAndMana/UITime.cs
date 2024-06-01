using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITime : MonoBehaviour
{
    public static UITime singleton;
    public GameObject panel;
    public TextMeshProUGUI timeText;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Open()
    {
        panel.SetActive(true);
    }

    public void Set(string time)
    { 
        timeText.text = time;
    }
}
