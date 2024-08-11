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

    public Collider2D leftCollider1;
    public Collider2D leftCollider2;
    public Collider2D leftCollider3;
    public Collider2D rightCollider1;
    public Collider2D rightCollider2;
    public Collider2D rightCollider3;

    public Collider2D leftColliderGate;
    public Collider2D rightColliderGate;

    [SyncVar(hook = nameof(SyncTextLevel))]
    public int level = 1;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (leftCollider1) leftCollider1.gameObject.SetActive(true);
        if (leftCollider2) leftCollider2.gameObject.SetActive(true);
        if (leftCollider3) leftCollider3.gameObject.SetActive(true);
        if (rightCollider1) rightCollider1.gameObject.SetActive(true);
        if (rightCollider2) rightCollider2.gameObject.SetActive(true);
        if (rightCollider3) rightCollider3.gameObject.SetActive(true);
        if (leftColliderGate) leftColliderGate.gameObject.SetActive(true);
        if (rightColliderGate) rightColliderGate.gameObject.SetActive(true);
    }

    public void SyncTextLevel(int oldValue, int newValue)
    {
        if (UIBuildingAccessoryManager.singleton)
        {
            UIBuildingAccessoryManager.singleton.SyncLevelText();
        }
    }

    public void LevelDoAction(Player player)
    {
        ExecuteActionLevelUp(player);

        if (leftCollider1) PropagateActionLevelUp(leftCollider1, player);
        if (leftCollider2) PropagateActionLevelUp(leftCollider2, player);
        if (leftCollider3) PropagateActionLevelUp(leftCollider3, player);

        if (rightCollider1) PropagateActionLevelUp(rightCollider1, player);
        if (rightCollider2) PropagateActionLevelUp(rightCollider2, player);
        if (rightCollider3) PropagateActionLevelUp(rightCollider3, player);

        if (leftColliderGate) PropagateActionLevelUp(leftColliderGate, player);
        if (rightColliderGate) PropagateActionLevelUp(rightColliderGate, player);

    }

    public void ExecuteActionLevelUp(Player player)
    {
        level++;
    }

    public void PropagateActionLevelUp(Collider2D collider, Player player)
    {
        if (collider != null)
        {
            RaycastHit2D[] hit = Physics2D.RaycastAll(collider.transform.position, Vector2.zero);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider != null)
                {
                    Fence adjacentObject = hit[i].collider.GetComponentInParent<Fence>();
                    if (adjacentObject != null && (adjacentObject.owner != player.name || adjacentObject.group != player.guild.guild.name))
                    {
                        adjacentObject.LevelDoAction(player);
                    }

                    Gate adjacentObject1 = hit[i].collider.GetComponentInParent<Gate>();
                    if (adjacentObject1 != null && (adjacentObject1.owner != player.name || adjacentObject1.group != player.guild.guild.name))
                    {
                        adjacentObject1.LevelDoAction(player);
                    }
                }
            }
        }
    }

    public void ClaimDoAction(Player player)
    {
        ExecuteActionLevelUp(player);

        if (leftCollider1) PropagateActionClaim(leftCollider1, player);
        if (leftCollider2) PropagateActionClaim(leftCollider2, player);
        if (leftCollider3) PropagateActionClaim(leftCollider3, player);

        if (rightCollider1) PropagateActionClaim(rightCollider1, player);
        if (rightCollider2) PropagateActionClaim(rightCollider2, player);
        if (rightCollider3) PropagateActionClaim(rightCollider3, player);

        if (leftColliderGate) PropagateActionClaim(leftColliderGate, player);
        if (rightColliderGate) PropagateActionClaim(rightColliderGate, player);
    }

    public void ExecuteActionClaim(Player player)
    {
        owner = player.name;
        group = player.guild.guild.name;
    }

    public void PropagateActionClaim(Collider2D collider, Player player)
    {
        if (collider != null)
        {
            RaycastHit2D[] hit = Physics2D.RaycastAll(collider.transform.position, Vector2.zero);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider != null)
                {
                    Fence adjacentObject = hit[i].collider.GetComponentInParent<Fence>();
                    if (adjacentObject != null && (adjacentObject.owner != player.name || adjacentObject.group != player.guild.guild.name))
                    {
                        adjacentObject.ClaimDoAction(player);
                    }

                    Gate adjacentObject1 = hit[i].collider.GetComponentInParent<Gate>();
                    if (adjacentObject1 != null && (adjacentObject1.owner != player.name || adjacentObject1.group != player.guild.guild.name))
                    {
                        adjacentObject1.ClaimDoAction(player);
                    }
                }
            }
        }
    }


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
