using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurManager : MonoBehaviour
{
    public static BlurManager singleton;
    public List<GameObject> toDeactivate = new List<GameObject>();
    public GameObject buildingPlacer;
    public GameObject blurObject;

    void Start()
    {
        if (!singleton) singleton = this;   
    }

    public void Hide()
    {
        blurObject.SetActive(true);
        for(int i = 0; i < toDeactivate.Count; i++)
        {
            toDeactivate[i].SetActive(false);
        }
        buildingPlacer.SetActive(false);
    }

    public void Show()
    {
        blurObject.SetActive(false);
        for (int i = 0; i < toDeactivate.Count; i++)
        {
            toDeactivate[i].SetActive(true);
        }
        buildingPlacer.SetActive(ModularBuildingManager.singleton.spawnedAccesssory ||
                                 ModularBuildingManager.singleton.spawnedBuilding ||
                                 ModularBuildingManager.singleton.spawnedWall);
    }
}
