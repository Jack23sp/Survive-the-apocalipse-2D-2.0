using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIBookPanel : MonoBehaviour
{
    public static UIBookPanel singleton;
    public GameObject panel;
    public Image bookImage;
    public TextMeshProUGUI bookTitle;
    public TextMeshProUGUI bookTime;
    public int seconds = 0;
    public int originalSeconds = 0;
    public string title;

    void Start()
    {
        if (!singleton) singleton = this;    
    }

    public void Monitoring(string tit)
    {
        title = tit;
        if (ScriptableBook.dict.TryGetValue(title.GetStableHashCode(), out ScriptableBook itemData))
        {
            originalSeconds = Convert.ToInt32(itemData.timerIncreasePerPointAbility);
            seconds = Convert.ToInt32(itemData.timerIncreasePerPointAbility);
            bookImage.sprite = itemData.image;
            bookImage.preserveAspect = true;
            bookTitle.text = title.ToString();
            bookTime.text = Utilities.ConvertToTimerMinuteAndSeconds(seconds);
            panel.SetActive(true);
            Invoke(nameof(RefreshOnlyTime), 1.0f);
        }
    }

    public void RefreshOnlyTime()
    {
        seconds--;
        bookTime.text = Utilities.ConvertToTimerMinuteAndSeconds(seconds);
        if(seconds == 0)
        {
            seconds = originalSeconds;
        }
        Invoke(nameof(RefreshOnlyTime), 1.0f);
    }

    public void ClosePanel()
    {
        bookImage.sprite = null;
        bookTitle.text = string.Empty;
        bookTime.text = string.Empty;
        title = string.Empty;
        seconds = 0;
        originalSeconds = 0;
        panel.SetActive(false);
    }
}
