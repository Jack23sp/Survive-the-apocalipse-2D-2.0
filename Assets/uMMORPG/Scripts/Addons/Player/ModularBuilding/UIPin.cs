using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPin : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public static UIPin singleton;
    public List<PinButtonSlot> pinButton = new List<PinButtonSlot>();
    public Button unlockButton;
    public TMP_InputField codeText;
    public string code;
    public CentralManager centralManager;
    public Button closeButton;
    public GameObject panel;
    public Image panelImage;
    public Transform colliderHit;
    public Animation anim;

    void Start()
    {
        if (!singleton) singleton = this;
        codeText.text = string.Empty;

        for (int i = 0; i < pinButton.Count; i++)
        {
            int index = i;
            pinButton[index].button.onClick.RemoveAllListeners();
            pinButton[index].button.onClick.AddListener(() =>
            {
                if (!pinButton[index].cancel)
                {
                    code = code + pinButton[index].buttonValue.text;
                    codeText.text = code;
                }
                else
                {
                    if(code.Length > 0) code = code.Substring(0, code.Length - 1);
                    codeText.text = code;
                }

                codeText.ForceLabelUpdate();
            });
        }

        unlockButton.onClick.RemoveAllListeners();
        unlockButton.onClick.AddListener(() =>
        {
            if (code == centralManager.modularBuilding.GetPin())
            {
                ModularBuildingManager.singleton.DoorManager(colliderHit,code);
                closeButton.onClick.Invoke();
            }
            else
                anim.Play();
        });

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => {
            Close();
        });
    }

    public void Open(Player player, CentralManager centralManager, Transform collider)
    {
        this.centralManager = centralManager;
        Assign();

        colliderHit = collider;
        panelImage.raycastTarget = true;
        panelImage.enabled = true;
        panel.SetActive(true);
        code = string.Empty;
        codeText.text = code;
    }

    public void Close()
    {
        code = string.Empty;
        codeText.text = code;
        panelImage.raycastTarget = false;
        panelImage.enabled = false;
        panel.SetActive(false);
        colliderHit = null;
        centralManager = null;
        BlurManager.singleton.Show();
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }

}
