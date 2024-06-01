using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSpawnpoint : MonoBehaviour
{
    public Collider2D[] colliders = new Collider2D[0];
    private bool result = true;
    public Player player;

    public bool Check(Player pl)
    {
        result = true;
        player = pl;
        colliders = new Collider2D[0];
        colliders = Physics2D.OverlapCircleAll(transform.position, 1f, ModularBuildingManager.singleton.spawnpointLayerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            result = ModularBuildingManager.singleton.CanDoOtherActionFloor(colliders[i].GetComponentInParent<ModularBuilding>(), player);
            if (!result)
            {
                break;
            }
        }

        return result;
    }

}
