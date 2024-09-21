using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResourceGathered : MonoBehaviour, IUIScriptNoBuildingRelated
{
    public static UIResourceGathered singleton;
    public GameObject panel;
    public ResourceGathered resource;
    public GameObject toSpawn;
    public Transform content;
    public TextMeshProUGUI title;
    public Button closeButton;


    void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => {
            Close();
        });
    }

    public void Open(ResourceGathered resourceGathered)
    {
        Assign();
        resource = resourceGathered;
        panel.SetActive(true);
        title.text = resource.buildingType;
        closeButton.image.raycastTarget = true;
        closeButton.image.enabled = true;

        UIUtils.BalancePrefabs(toSpawn, resource.slots.Count, content);
        for(int i = 0; i  < resource.slots.Count; i++)
        {
            int index = i;
            ResourceSlot slot = content.GetChild(index).GetComponent<ResourceSlot>();
            slot.itemImage.sprite = resource.slots[index].item.data.image;
            slot.itemName.text = resource.slots[index].item.data.name;
            slot.itemAmount.text = resource.slots[index].amount.ToString();
            slot.takeButton.onClick.RemoveAllListeners();
            slot.takeButton.onClick.AddListener(() => {
                Player.localPlayer.CmdAddGatheredResorce(index, resource.netIdentity);
            });
        }
    }

    public void Close()
    {
        closeButton.image.raycastTarget = false;
        panel.SetActive(false);
        closeButton.image.enabled = false;
        BlurManager.singleton.Show();
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeathNoBuilding.Add(this);
    }
}
