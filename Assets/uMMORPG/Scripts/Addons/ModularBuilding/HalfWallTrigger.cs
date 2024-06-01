using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfWallTrigger : MonoBehaviour
{
    public ModularBuilding modularBuilding;
    public GameObject mask;
    public WallManager WallManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!WallManager.gameObject.activeInHierarchy)
        {
            mask.SetActive(false); return;
        }
        if (modularBuilding.isClient)
        {
            if (collision.CompareTag("Player"))
            {
                if (collision.GetComponent<NetworkIdentity>().isLocalPlayer && 
                   (ModularBuildingManager.singleton.CanDoOtherActionFloor(modularBuilding, Player.localPlayer) || 
                    AbilityManager.singleton.FindNetworkAbilityLevel("Thief", Player.localPlayer.name) >= modularBuilding.level))

                {
                    mask.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!WallManager.gameObject.activeInHierarchy)
        {
            mask.SetActive(false); return;
        }
        if (modularBuilding.isClient)
        {
            if (collision.CompareTag("Player"))
            {
                if (collision.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    mask.SetActive(false);
                }
            }
        }
    }

}
