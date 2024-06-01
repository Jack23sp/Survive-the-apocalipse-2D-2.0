using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementBase : MonoBehaviour
{
    private BoxCollider2D collider;
    private SpriteRenderer renderer;
    public ModularBuilding modularBuilding;
    public bool left, up, right, down;

    public Collider2D[] colliderHits = new Collider2D[0];

    void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (modularBuilding.isServer)
        {
            if (collision.CompareTag("Basement"))
            {
                if (ModularBuildingManager.singleton.CanDoOtherActionFloorWithouPlayer(collision.GetComponent<ModularBuilding>(), modularBuilding.owner, modularBuilding.group))
                {
                    if(left)
                    {
                        modularBuilding.leftBuilding = collision.GetComponent<ModularBuilding>();
                        collision.GetComponent<ModularBuilding>().modularIndex = modularBuilding.modularIndex;
                    }
                    if (up)
                    {
                        modularBuilding.upBuilding = collision.GetComponent<ModularBuilding>();
                        collision.GetComponent<ModularBuilding>().modularIndex = modularBuilding.modularIndex;
                    }
                    if (right)
                    {
                        modularBuilding.rightBuilding   = collision.GetComponent<ModularBuilding>();
                        collision.GetComponent<ModularBuilding>().modularIndex = modularBuilding.modularIndex;
                    }
                    if (down)
                    {
                        modularBuilding.downBuilding = collision.GetComponent<ModularBuilding>();
                        collision.GetComponent<ModularBuilding>().modularIndex = modularBuilding.modularIndex;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (modularBuilding.isServer)
        {
            if (collision.CompareTag("Basement"))
            {
                if (ModularBuildingManager.singleton.CanDoOtherActionFloorWithouPlayer(collision.GetComponent<ModularBuilding>(), modularBuilding.owner, modularBuilding.group))
                {
                    if (left)
                    {
                        modularBuilding.leftBuilding = null;
                    }
                    if (up)
                    {
                        modularBuilding.upBuilding = null;
                    }
                    if (right)
                    {
                        modularBuilding.rightBuilding = null;
                    }
                    if (down)
                    {
                        modularBuilding.downBuilding = null;
                    }
                }
            }
        }

    }




    public void Manage(bool condition)
    {
        colliderHits = Physics2D.OverlapBoxAll(transform.position, new Vector2(collider.size.x, collider.size.y), 0.0f, ModularBuildingManager.singleton.basementLayerMask);
        if (condition)
        {
            if(colliderHits.Length > 0)
            {
                //collider.enabled = false;
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.0f);
            }
            else
            {
                //collider.enabled = condition;
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 120.0f);
            }
        }
        else
        {
            //collider.enabled = condition;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.0f);
        }
    }
}
