using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Monster
{
    public float initialFollowDistance;
    public MonsterComponentManager monsterComponentManager;
    public List<string> playerNames = new List<string>();
    public bool settedManualTarget;
    public bool isMonster;

    public void Awake()
    {
        initialFollowDistance = followDistance;
    }

    public void CancelFollow()
    {
        if (target != null)
            if (Vector2.Distance(target.transform.position, transform.position) > initialFollowDistance)
            {
                followDistance = initialFollowDistance;
                settedManualTarget = false;
            }
            else
                Invoke(nameof(CancelFollow), 5.0f);
        else
        {
            followDistance = initialFollowDistance;
            settedManualTarget = false;
        }
    }
}

public class MonsterComponentManager : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public NetworkIdentity identity;

    public Monster monster;
    public MonsterInventory inventory;
    public MonsterSkills skill;
    public NavMeshAgent2D navMeshAgent;
    public NetworkNavMeshAgent2D networkNavMesh;
    public RegularNavMeshMovement2D regularNavMesh;
    public SortByDepth sortByDepth;
    public Animator animator;
    public AudioSource audio;
    public Rigidbody2D rigidbody2D;

    public bool disabled = true;

    public override void OnStartServer()
    {
        base.OnStartServer();
        //InvokeRepeating(nameof(Check), 0.0f, UnityEngine.Random.Range(0.1f, 1.0f));
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        //InvokeRepeating(nameof(Check), 0.0f, UnityEngine.Random.Range(0.1f, 0.5f));
    }

    void Check()
    {
        if (disabled && spriteRenderer.enabled)
        {
            monster.enabled = true;
            animator.enabled = true;
            audio.enabled = true;
            inventory.enabled = true;
            skill.enabled = true;
            navMeshAgent.enabled = true;
            networkNavMesh.enabled = true;
            regularNavMesh.enabled = true;
            sortByDepth.enabled = true;
            if (rigidbody2D) rigidbody2D.simulated = true;
            disabled = false;
        }
        if (!disabled && !spriteRenderer.enabled)
        {
            monster.enabled = false;
            animator.enabled = false;
            audio.enabled = false;
            inventory.enabled = false;
            skill.enabled = false;
            navMeshAgent.enabled = false;
            networkNavMesh.enabled = false;
            regularNavMesh.enabled = false;
            sortByDepth.enabled = false;
            if (rigidbody2D) rigidbody2D.simulated = false;
            disabled = true;
        }
    }

    public void ForceAble(float distance, Player player)
    {
        monster.CancelInvoke(nameof(monster.CancelFollow));
        monster.followDistance = distance;
        monster.target = player;
        monster.Invoke(nameof(monster.CancelInvoke), 100.0f);
    }
}
