using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public struct TutorialSlot
{
    [TextArea(4, 4)]
    public string description;
    public RectTransform rectTransform;
    public Vector2 position;
}

public partial class TutorialElement : MonoBehaviour
{
    public bool isAble;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager singleton;
    public Material shineMaterial;
    public GameObject panel;
    public RectTransform rectTransform;
    public List<TutorialSlot> slots = new List<TutorialSlot>();
    public int index;
    public TextMeshProUGUI slotDescription;
    public Button forwardButton;
    public Button terminateTutorial;

    private Button cachedButton;
    private int forIndex = 0;

    public void Start()
    {
        if (!singleton) singleton = this;

        forwardButton.onClick.AddListener(() =>
        {
            Step(forwardButton);
        });

        if (slots.Count > 0) rectTransform.anchoredPosition = slots[0].position;

    }

    public void Move()
    {
        CancelInvoke(nameof(Move));
        if (rectTransform.anchoredPosition != slots[index].position)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, slots[index].position, Time.fixedDeltaTime * 3.5f);
            Invoke(nameof(Move), 0.01f);
        }
    }

    public void Step(Button btn)
    {
        bool ok = false;
        if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponent<Button>())
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].rectTransform.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    if (index == i)
                    {
                        btn.GetComponent<TutorialElement>().isAble = true;
                        ok = true;
                    }
                }
            }
        }

        if (!ok) return;

        if (btn)
        {
            int provIndex = index + 1;
            if (provIndex > slots.Count - 1)
            {
                forwardButton.gameObject.SetActive(false);
                slotDescription.text = "Congratulation you have terminate the tutorial!";
                btn.image.material = null;
                index = 0;
                return;
            }
            else
            {
                index++;
                Invoke(nameof(Init), 0.5f);
            }
        }
    }

    public void Setup()
    {
        int prv_index = 0;

        for (prv_index = 0; prv_index <= slots.Count - 1; prv_index++)
        {
            if (!slots[prv_index].rectTransform.GetComponent<TutorialElement>())
            {
                TutorialElement te = slots[prv_index].rectTransform.gameObject.AddComponent<TutorialElement>();
            }

            slots[prv_index].rectTransform.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (slots[index].rectTransform.GetComponent<TutorialElement>() && !slots[index].rectTransform.GetComponent<TutorialElement>().isAble)
                {
                    Step(slots[index].rectTransform.GetComponent<Button>());
                }
            });
        }
        panel.SetActive(true);
        Init();
    }

    public void Init()
    {

        Invoke(nameof(Move), 0.01f);


        for (int i = index; i >= 0; i--)
        {
            if (i < index)
            {
                if (slots[i].rectTransform.GetComponent<Image>())
                    slots[i].rectTransform.GetComponent<Image>().material = null;
            }
        }

        if (slots[index].rectTransform.GetComponent<Button>())
        {
            slotDescription.text = slots[index].description;
            forwardButton.gameObject.SetActive(false);
            cachedButton = slots[index].rectTransform.GetComponent<Button>();
            cachedButton.image.material = shineMaterial;
        }
        else
        {
            slotDescription.text = slots[index].description;
            slots[index].rectTransform.GetComponent<Image>().material = shineMaterial;
            forwardButton.gameObject.SetActive(true);
            forwardButton.onClick.RemoveListener(Init);
        }
        //index++;
    }
}
