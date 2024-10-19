using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPetStatusManagement : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public static UIPetStatusManagement singleton;

    public GameObject panel;

    public Button closeButton;
    public Image cureImage;
    public TextMeshProUGUI title;
    public TextMeshProUGUI actualMode;

    public Button attackMode;
    public Button defenceMode;
    public Button returnMode;

    public Button useButton;
    private int amount = 0;
    public Button helpButton;
    public GameObject helpObject;


    void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() =>
        {
            Player.localPlayer.petControl.CmdCurePet(true);
        });

        attackMode.onClick.RemoveAllListeners();
        attackMode.onClick.AddListener(() =>
        {
            if (Player.localPlayer.petControl.activePet != null)
            {
                Player.localPlayer.petControl.activePet.CmdSetAutoAttack();
            }
        });

        defenceMode.onClick.RemoveAllListeners();
        defenceMode.onClick.AddListener(() =>
        {
            if (Player.localPlayer.petControl.activePet != null)
            {
                Player.localPlayer.petControl.activePet.CmdSetDefendOwner();
            }
        });

        returnMode.onClick.RemoveAllListeners();
        returnMode.onClick.AddListener(() =>
        {
            if (Player.localPlayer.petControl.CanUnsummon())
                Player.localPlayer.petControl.CmdUnsummon();
        });

        //helpButton.onClick.RemoveAllListeners();
        helpButton.onClick.AddListener(() =>
        {
            helpObject.gameObject.SetActive(true);
        });
    }

    public void SyncPetMode()
    {
        if(Player.localPlayer.petControl.activePet != null)
        {
            if(Player.localPlayer.petControl.activePet.defendOwner)
            {
                actualMode.text = "Defence Mode";
            }
            else if(Player.localPlayer.petControl.activePet.autoAttack)
            {
                actualMode.text = "Auto Attack";
            }
        }
    }

    public void Open()
    {
        Assign();
        BlurManager.singleton.Hide();
        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;
        amount = FindAllCure();
        cureImage.sprite = PremiumItemManager.singleton.petFood.image;
        title.text = PremiumItemManager.singleton.petFood.name + " x " + amount;
        useButton.interactable = amount > 0;
        panel.SetActive(true);
        actualMode.text = Player.localPlayer.petControl.activePet.autoAttack ? "Auto attack" : "Defence mode";
    }

    public int FindAllCure()
    {
        return Player.localPlayer.inventory.FindItemInInventory(PremiumItemManager.singleton.petFood);
    }

    public void Close()
    {
        amount = 0;
        panel.SetActive(false);
        closeButton.image.raycastTarget = false;
        closeButton.image.enabled = false;
        BlurManager.singleton.Show();
        helpObject.gameObject.SetActive(false);
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }
}
