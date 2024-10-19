using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChildObject : MonoBehaviour
{
    public Player player;
    public List<GameObject> childObjects = new List<GameObject>();

    public PlayerSmokeParticles particle;

    public void Manage(bool isLocalPlayer, Player pl)
    {
        particle.player = pl;
        childObjects.Clear();
        childObjects = DisplayChildren(transform, isLocalPlayer);
    }

    List<GameObject> DisplayChildren(Transform trans, bool isLocalPlayer)
    {
        var childs = new List<GameObject>();
        foreach (Transform child in trans)
        {
            child.transform.tag = "PlayerParts";
            if (child.childCount == 0)
            {
                childs.Add(child.gameObject);
            }

            if (!isLocalPlayer)
            {
                child.gameObject.layer = LayerMask.NameToLayer("NotPersonalPlayer");
            }
            else
            {
                child.gameObject.layer = LayerMask.NameToLayer("PersonalPlayer");
            }
        }
        return childs;
    }
}
