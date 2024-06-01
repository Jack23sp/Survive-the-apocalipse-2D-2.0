using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Gate : BuildingAccessory
{
    public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

    public int firstToStart = -1;

    public Animator animator;

    [SyncVar(hook = (nameof(ManageGate)))]
    public int playerInside;

    public new void Start()
    {
        base.Start();
        spriteRenderers[0].enabled = true;
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.gates.Contains(this)) ModularBuildingManager.singleton.gates.Add(this);
        }
    }
    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.gates.Contains(this)) ModularBuildingManager.singleton.gates.Remove(this);
        }
    }

    public void OnEnable()
    {
        spriteRenderers[0].enabled = isClient;
    }

    public void ManageGate(int oldValue, int newValue)
    {
        Player.localPlayer.playerModularBuilding.CmdManageGate(netIdentity, newValue <= 0 ? true : false);
    }

    public void ManageSprite(int index)
    {
        if (firstToStart <= 0)
        {
            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                int index_i = i;
                //spriteRenderers[index_i].enabled = (index_i == index);
                spriteRenderers[index_i].color = index_i == index ? new Color(255,255,255,255) : new Color(255, 255, 255, 0);
                renderer = spriteRenderers[index_i];
                firstToStart = index_i;
                if (index_i == spriteRenderers.Count - 1) navMeshObstacle2D.enabled = false;
            }
        }
        else
        {
            for (int i = spriteRenderers.Count - 1; i >= 0; i--)
            {
                int index_i = i;
                spriteRenderers[index_i].color = index_i == index ? new Color(255, 255, 255, 255) : new Color(255, 255, 255, 0);
                renderer = spriteRenderers[index_i];
                firstToStart = index_i;
                if (index_i == 0) navMeshObstacle2D.enabled = true;
            }
        }
    }

}
