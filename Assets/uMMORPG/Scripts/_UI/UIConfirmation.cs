using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIConfirmation : MonoBehaviour
{
    public static UIConfirmation singleton;
    public GameObject panel;
    public Button closeButton;
    public Button cancelButton;
    public TextMeshProUGUI messageText;
    public Button confirmButton;

    public UIConfirmation()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    public void Start()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });

    }

    public void Hide()
    {
        closeButton.image.enabled = false;
        closeButton.image.raycastTarget = false;
        panel.SetActive(false);
    }

    public void Show(string message, UnityAction onConfirm)
    {
        messageText.text = message;
        closeButton.image.enabled = true;
        closeButton.image.raycastTarget = true;
        confirmButton.onClick.SetListener(onConfirm);
        panel.SetActive(true);
    }
}