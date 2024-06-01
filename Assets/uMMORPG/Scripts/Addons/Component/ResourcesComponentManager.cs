using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesComponentManager : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Rock rock;
    public Tree tree;
    public NavMeshObstacle2DCustom navMeshObstacle2D;
    public ScaleAnimation scaleAnimation;
    public Rigidbody2D rigidbody2D;

    public bool disabled = true;

    public void Awake()
    {
        InvokeRepeating(nameof(Check), 0.0f, UnityEngine.Random.Range(0.1f, 1.0f));
    }

    void Check()
    {
        if (disabled && spriteRenderer.enabled)
        {
            if (rock) rock.enabled = true;
            if (tree) tree.enabled = true;
            if (navMeshObstacle2D) navMeshObstacle2D.enabled = true;
            if (rigidbody2D) rigidbody2D.simulated = true;
            scaleAnimation.enabled = true;
            disabled = false;
        }
        if (!disabled && !spriteRenderer.enabled)
        {
            if (rock) rock.enabled = false;
            if (tree) tree.enabled = false;
            if (navMeshObstacle2D) navMeshObstacle2D.enabled = false;
            if (rigidbody2D) rigidbody2D.simulated = false;
            scaleAnimation.enabled = false;
            disabled = true;
        }
    }
}
