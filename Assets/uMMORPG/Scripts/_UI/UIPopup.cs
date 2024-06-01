using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPopup : MonoBehaviour
{
    public static UIPopup singleton;
    public GameObject panel;
    public TextMeshProUGUI messageText;
    public Button closeButton;
    public Button internalClosebutton;

    public UIPopup()
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

        internalClosebutton.onClick.RemoveAllListeners();
        internalClosebutton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    public void Show(string message)
    {
        // append error if visible, set otherwise. then show it.
        if (panel.activeSelf) messageText.text += ";\n" + message;
        else messageText.text = message;
        panel.SetActive(true);
        closeButton.image.enabled = true;
    }

    public void Hide()
    {
        closeButton.image.enabled = true;
        panel.SetActive(false);
        closeButton.image.enabled = false;
    }
}
