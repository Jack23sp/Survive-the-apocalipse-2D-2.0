using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BuijldingTriggerCatcher : MonoBehaviour
{
    public BuildingAccessory buildingAccessory;

    public List<Collider2D> avoidCollidersNotToCache = new List<Collider2D>();
    public List<string> tagToAvoid = new List<string>();

    public BoxCollider2D thisCollider;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!buildingAccessory) return;
        if ((buildingAccessory.necessaryForPositioning.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            if (!buildingAccessory.basementColliderHits.Contains(collision)) buildingAccessory.basementColliderHits.Add(collision);
        }
        if ((buildingAccessory.otherAccessory.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            if (!buildingAccessory.colliderHits.Contains(collision)) buildingAccessory.colliderHits.Add(collision);
        }
        if ((buildingAccessory.needToAvoid.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            if ((collision.GetComponent<NetworkIdentity>() && collision.GetComponent<NetworkIdentity>().netId > 0) || (collision.GetComponentInParent<NetworkIdentity>() && collision.GetComponentInParent<NetworkIdentity>().netId > 0))
            {
                if (!avoidCollidersNotToCache.Contains(collision) && !tagToAvoid.Contains(collision.tag))
                    if (!buildingAccessory.avoidHits.Contains(collision)) buildingAccessory.avoidHits.Add(collision);

            }
        }
        buildingAccessory.CheckPossibleSpawn();
        if (buildingAccessory.isServer)
        {
            for (int i = 0; i < buildingAccessory.basementColliderHits.Count; i++)
            {
                int index = i;
                if (buildingAccessory.basementColliderHits[index].CompareTag("Basement"))
                {
                    buildingAccessory.group = buildingAccessory.basementColliderHits[index].GetComponent<ModularBuilding>().group;
                    buildingAccessory.owner = buildingAccessory.basementColliderHits[index].GetComponent<ModularBuilding>().owner;
                }
            }
        }
        if (buildingAccessory.isServer)
;       {
            if (collision.CompareTag("Wheat"))
            {
                SpawnedObject so = collision.GetComponent<SpawnedObject>();
                if (!so) return;
                so.hasOverlay = true;
                IrregularColliderSpawner irr = so.parent.GetComponent<IrregularColliderSpawner>();
                if (!irr) return;
                if (so.index > irr.spawnedObjects.Count) NetworkServer.Destroy(so.gameObject);
                else
                {
                    AmbientDecoration dec = irr.spawnedObjects[so.index];
                    so.hasOverlay = true;
                    irr.spawnedObjects[so.index] = dec;
                }
            }
        }

        if (buildingAccessory.isServer || buildingAccessory.isClient) Destroy(this);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!buildingAccessory) return;
        if ((buildingAccessory.necessaryForPositioning.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            if (buildingAccessory.basementColliderHits.Contains(collision)) buildingAccessory.basementColliderHits.Remove(collision);
        }
        if ((buildingAccessory.otherAccessory.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            if (buildingAccessory.colliderHits.Contains(collision)) buildingAccessory.colliderHits.Remove(collision);
        }
        if ((buildingAccessory.needToAvoid.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            if (buildingAccessory.avoidHits.Contains(collision)) buildingAccessory.avoidHits.Remove(collision);
        }

        buildingAccessory.CheckPossibleSpawn();
    }
}
