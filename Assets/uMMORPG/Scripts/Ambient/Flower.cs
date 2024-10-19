using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : NetworkBehaviour
{
    public ScriptableItem itemToAdd;
    [HideInInspector] public Collider2D zoneCollider;

    public void OnDestroy()
    {
        if (isServer)
        {
            SpawnManager.singleton.RemoveFromZone(zoneCollider, this.gameObject);
        }
    }
}
