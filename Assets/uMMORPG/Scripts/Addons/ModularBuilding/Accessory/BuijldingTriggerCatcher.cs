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
;        {
            if (collision.CompareTag("Wheat"))
            {
                SpawnedObject so = collision.GetComponent<SpawnedObject>();
                if (!so) return;
                so.hasOverlay = true;
                IrregularColliderSpawner irr = so.parent.GetComponent<IrregularColliderSpawner>();
                if (!irr) return;
                AmbientDecoration dec = irr.spawnedObjects[so.index];
                so.hasOverlay = true;
                irr.spawnedObjects[so.index] = dec;
            }
            //if (collision.CompareTag("VegatationSpawner"))
            //{
            //    PolygonCollider2D polygonCollider = collision.GetComponent<PolygonCollider2D>();
            //    spawn = polygonCollider.gameObject.GetComponent<IrregularColliderSpawner>();
            //    // Ottieni i bounds del BoxCollider2D
            //    Bounds boxBounds = GetComponent<BoxCollider2D>().bounds;

            //    // Trova i vertici del BoxCollider2D
            //    Vector2[] boxVertices = new Vector2[4];
            //    boxVertices[0] = new Vector2(boxBounds.min.x, boxBounds.min.y); // Bottom Left
            //    boxVertices[1] = new Vector2(boxBounds.max.x, boxBounds.min.y); // Bottom Right
            //    boxVertices[2] = new Vector2(boxBounds.max.x, boxBounds.max.y); // Top Right
            //    boxVertices[3] = new Vector2(boxBounds.min.x, boxBounds.max.y); // Top Left

            //    for (int i = 0; i < spawn.spawnedObjects.Count; i++)
            //    {
            //        AmbientDecoration dec = spawn.spawnedObjects[i];
            //        // Verifica se l'oggetto non è un trigger o collider
            //        if (dec.obj != null && !dec.overlay)
            //        {

            //            // Verifica se la posizione dell'oggetto è all'interno del PolygonCollider2D
            //            if (thisCollider.OverlapPoint(dec.position))
            //            {
            //                dec.overlay = true;
            //                dec.obj.GetComponent<SpawnedObject>().hasOverlay = true;
            //                spawn.spawnedObjects[i] = dec;
            //            }
            //        }
            //    }
            //}
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
