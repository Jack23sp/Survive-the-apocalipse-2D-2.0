using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class BasementTrigger : MonoBehaviour
{
    public ModularBuilding modularBuilding;
    public GameObject roof;
    public GameObject light;
    public BoxCollider2D collider;
    public List<ModularBuilding> modularBuildings = new List<ModularBuilding>();
    public List<Collider2D> obstacles = new List<Collider2D>();
    private List<int> layer = new List<int>();
    public Collider2D[] colliders;
    public LayerMask obstacleChecker;
    public LayerMask playersLayer;


    public void OnEnable()
    {
        if (modularBuilding.netIdentity.netId == 0) roof.SetActive(false);
        layer = Utilities.LayerMaskToList(obstacleChecker);
    }

    public bool Check()
    {
        return obstacles.Count < 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (modularBuilding.isClient)
        {
            if (modularBuilding.CheckRoof())
            {
                colliders = Physics2D.OverlapBoxAll(modularBuilding.transform.position, GetComponent<Collider2D>().bounds.size, 0f, playersLayer);

                if (colliders.Length > 0)
                {
                    if (colliders.ToList().Contains(Player.localPlayer.collider) &&
                       ModularBuildingManager.singleton.IsOverlapPercentageAboveThreshold(collider, ((CapsuleCollider2D)Player.localPlayer.collider),0.6f))
                    {
                        roof.SetActive(false);
                    }
                    else
                    {
                        roof.SetActive(true);
                    }
                }
                else
                {
                    roof.SetActive(true);
                }
            }
            else
            {
                roof.SetActive(false);
            }

            if (collision.CompareTag("WallMarker"))
            {
                collision.gameObject.GetComponent<WallManager>().CheckWall();
            }
        }
        if (modularBuilding.isClient || (!modularBuilding.isClient && !modularBuilding.isServer))
        {
            if (collision.CompareTag("FloorPositioning"))
            {
                collision.GetComponent<PlacementBase>().Manage(false);
            }
            if (collision.CompareTag("Basement"))
            {
                if (!modularBuildings.Contains(collision.GetComponent<ModularBuilding>())) modularBuildings.Add(collision.GetComponent<ModularBuilding>());
            }
        }

        if (layer.Contains(collision.gameObject.layer))
        {
            if(LayerMask.LayerToName(collision.gameObject.layer) == "Tree")
            {
                if (!obstacles.Contains(collision)) obstacles.Add(collision);
            }
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Rock")
            {
                if (!obstacles.Contains(collision)) obstacles.Add(collision);
            }
            if (LayerMask.LayerToName(collision.gameObject.layer) == "ResourceGathered")
            {
                if (!obstacles.Contains(collision)) obstacles.Add(collision);
            }
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Basement" && collision != collider && collider.GetComponent<ModularBuilding>())
            {
                if (!obstacles.Contains(collision)) obstacles.Add(collision);
            }
        }

        if (modularBuilding.isServer)
        {
            IrregularColliderSpawner spawn = null;
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

    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (modularBuilding.isClient)
        {
            if (modularBuilding.CheckRoof())
            {
                colliders = Physics2D.OverlapBoxAll(modularBuilding.transform.position, GetComponent<Collider2D>().bounds.size, 0f, playersLayer);

                if (colliders.Length > 0)
                {
                    if (colliders.ToList().Contains(Player.localPlayer.collider) &&
                        ModularBuildingManager.singleton.IsOverlapPercentageAboveThreshold(collider, ((CapsuleCollider2D)Player.localPlayer.collider), 0.6f))
                    {
                        roof.SetActive(false);
                    }
                    else
                    {
                        roof.SetActive(true);
                    }
                }
                else
                {
                    roof.SetActive(true);
                }
            }
            else
            {
                roof.SetActive(false);
            }

            if (collision.CompareTag("WallMarker"))
            {
                collision.gameObject.GetComponent<WallManager>().CheckWallOnExit();
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (modularBuilding.isClient)
        {
            //if (modularBuilding.CheckRoof())
            //{
            //    colliders = Physics2D.OverlapBoxAll(modularBuilding.transform.position, GetComponent<Collider2D>().bounds.size, 0f, playersLayer);

            //    if (colliders.Length > 0)
            //    {
            //        if (colliders.ToList().Contains(Player.localPlayer.collider))
            //        {
            //            switch (ModularBuildingManager.singleton.GetCollisionDirection(collider, ((CapsuleCollider2D)Player.localPlayer.collider)))
            //            {
            //                case CollisionDirection.Top:
            //                    if (ModularBuildingManager.singleton.IsOverlapPercentageAboveThreshold(collider, ((CapsuleCollider2D)Player.localPlayer.collider), 0.8f))
            //                        roof.SetActive(false);
            //                    break;
            //                default:
            //                    if (ModularBuildingManager.singleton.IsOverlapPercentageAboveThreshold(collider, ((CapsuleCollider2D)Player.localPlayer.collider), 0.6f))
            //                        roof.SetActive(false);
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            roof.SetActive(true);
            //        }
            //    }
            //    else
            //    {
            //        roof.SetActive(true);
            //    }
            //}
            //else
            //{
            //    roof.SetActive(false);
            //}

            if (collision.CompareTag("WallMarker"))
            {
                collision.gameObject.GetComponent<WallManager>().CheckWallOnExit();
            }

        }
        if (modularBuilding.isClient || (!modularBuilding.isClient && !modularBuilding.isServer))
        {
            if (collision.CompareTag("FloorPositioning"))
            {
                collision.GetComponent<PlacementBase>().Manage(true);
            }
            if (collision.CompareTag("Basement"))
            {
                if (modularBuildings.Contains(collision.GetComponent<ModularBuilding>())) modularBuildings.Remove(collision.GetComponent<ModularBuilding>());
            }
        }

        if (obstacles.Contains(collision)) obstacles.Remove(collision);

    }
}
