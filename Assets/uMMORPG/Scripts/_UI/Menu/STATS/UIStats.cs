using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIStats : MonoBehaviour
{
    public static UIStats singleton;
    public Transform content;
    public VerticalLayoutGroup verticalLayout;

    void OnEnable()
    {
        if (!singleton) singleton = this;
        SpawnStatsAtBegins();
        Setsize();
        verticalLayout.enabled = true;
    }

    void OnDisable()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            int index = i;
            UIStatsSlot slot = content.GetChild(i).GetComponent<UIStatsSlot>();
            slot.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, StatsManager.singleton.closeSize);
            slot.description.gameObject.SetActive(false);
        }
        verticalLayout.enabled = true;
    }

    public void SpawnStatsAtBegins()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            int index = i;
            UIStatsSlot slot = content.GetChild(index).GetComponent<UIStatsSlot>();
            StatsManager.singleton.ManageStatSlot(slot);
            //slot.button.onClick.RemoveAllListeners();
            slot.panelButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                foreach (UIStatsSlot slots in content.GetComponentsInChildren<UIStatsSlot>())
                {
                    slots.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, StatsManager.singleton.closeSize);
                    slots.description.gameObject.SetActive(false);
                }
                if (slot.rectTransform.sizeDelta.y > 30.0f)
                {
                    slot.description.gameObject.SetActive(false);
                    slot.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, StatsManager.singleton.closeSize);
                }
                else
                {
                    slot.description.gameObject.SetActive(true);
                    slot.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, StatsManager.singleton.openSize);
                }
                verticalLayout.enabled = true;
            });
        }
    }

    public void Setsize()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            int index = i;
            UIStatsSlot slot = content.GetChild(index).GetComponent<UIStatsSlot>();
            slot.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, StatsManager.singleton.closeSize);
        }

    }
}
