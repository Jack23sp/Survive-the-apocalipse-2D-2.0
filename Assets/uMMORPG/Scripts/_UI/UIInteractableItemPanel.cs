using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UIInteractableItemPanel : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public static UIInteractableItemPanel singleton;
    public GameObject panel;
    public TextMeshProUGUI description;
    public Button actionButton;
    public Button closeButton;
    public Button manageButton;

    void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.RemoveAllListeners();        
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });
    }

    public void Open(ScriptableItem item,NetworkIdentity identity)
    {
        Assign();
        panel.SetActive(true);
        closeButton.image.raycastTarget = true;
        closeButton.interactable = true;
        closeButton.image.enabled = true;
        description.text = "Do you want use " + item.name + " ?";

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(identity.GetComponent<BuildingAccessory>(), Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(identity.GetComponent<BuildingAccessory>().netIdentity, identity.GetComponent<BuildingAccessory>().craftingAccessoryItem, closeButton);
            closeButton.onClick.Invoke();
            BlurManager.singleton.Hide();
        });


        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() =>
        {
            if(item.name == "Dumbbell")
            {
                Player.localPlayer.playerAdditionalState.CmdUseDumbbell(Player.localPlayer, identity);
                closeButton.onClick.Invoke();
            }
        });
    }

    public void Close()
    {
        panel.SetActive(false);
        closeButton.image.raycastTarget = false;
        closeButton.interactable = false;
        closeButton.image.enabled = false;
        BlurManager.singleton.Show();
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }

}
