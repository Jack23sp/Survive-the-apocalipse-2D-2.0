// Catches the Aggro Sphere's OnTrigger functions and forwards them to the
// Entity. Make sure that the aggro area's layer is IgnoreRaycast, so that
// clicking on the area won't select the entity.
//
// Note that a player's collider might be on the pelvis for animation reasons,
// so we need to use GetComponentInParent to find the Entity script.
//
// IMPORTANT: Monster.OnTriggerEnter would catch it too. But this way we don't
//            need to add OnTriggerEnter code to all the entity types that need
//            an aggro area. We can just reuse it.
//            (adding it to Entity.OnTriggerEnter would be strange too, because
//             not all entity types should react to OnTriggerEnter with aggro!)
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))] // aggro area trigger
public class AggroArea : MonoBehaviour
{
    public Entity owner; // set in the inspector

    // same as OnTriggerStay
    void OnTriggerEnter2D(Collider2D co)
    {
        Entity entity = co.GetComponentInParent<Entity>();
        if (entity)
        {
            owner.OnAggro(entity);
            if (owner is Monster)
            {
                ((Monster)owner).OnAggroEscape(entity);
            }
        }
    }

    void OnTriggerStay2D(Collider2D co)
    {
        Entity entity = co.GetComponentInParent<Entity>();
        if (entity) 
        {
            owner.OnAggro(entity);
            if(owner is Monster)
            {
                if(((Monster)owner).isNeutral)
                {
                    if (((Monster)owner).alwaysRunAway)
                    {
                        if(((Monster)owner).state == "IDLE")
                        {
                            ((Monster)owner).OnAggroEscape(entity);
                        }
                    }
                }
            }
        }
    }
}
