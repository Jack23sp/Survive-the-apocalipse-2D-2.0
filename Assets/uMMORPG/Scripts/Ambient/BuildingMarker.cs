using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMarker : MonoBehaviour
{
    public ModularBuilding modularBuilding;
    public GameObject marker;
    public SpriteRenderer markerRenderer;

    void Start()
    {
        if (modularBuilding.isClient)
        {
            InvokeRepeating(nameof(Check), 1.0f,2.0f);
        }
    }

    public void Check()
    {
        if (modularBuilding.isClient)
        {
            if (modularBuilding.main)
            {
                marker.transform.parent = null;
                int mod = ModularBuildingManager.singleton.CanDoOtherActionFloorInt(modularBuilding, Player.localPlayer);

                switch (mod)
                {
                    case -2:
                        marker.SetActive(false);
                        break;
                    case -1:
                        marker.SetActive(false);
                        break;
                    case 0:
                        marker.SetActive(true);
                        markerRenderer.color = Color.green;
                        break;
                    case 1:
                        marker.SetActive(true);
                        markerRenderer.color = Color.yellow;
                        break;
                    case 2:
                        marker.SetActive(true);
                        markerRenderer.color = Color.blue;
                        break;

                }
            }
            else
            {
                marker.SetActive(false);
            }
        }
        else
        {
            marker.SetActive(false);
        }
    }
}
