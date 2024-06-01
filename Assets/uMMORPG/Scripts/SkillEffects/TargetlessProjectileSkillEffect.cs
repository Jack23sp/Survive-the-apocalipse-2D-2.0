// Target Projectile skill effects like arrows, flaming fire balls, etc. that
// fly towards a direction without a target, and deal damage if anything hit.
//
// Note: we could move it on the server and use NetworkTransform to synchronize
// the position to all clients, which is the easy method. But we just move it on
// the server AND on the client to save bandwidth. Same result.
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using JetBrains.Annotations;
using System;

// needs a Rigidbody and collider (trigger!) for OnTriggerEnter
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class TargetlessProjectileSkillEffect : SkillEffect
{
    [Header("Components")]
    public Animator animator;

    [Header("Properties")]
    public float speed = 25;
    public bool melee = false;
    [HideInInspector] public int damage = 1; // set by skill
    [HideInInspector] public int damageToRock = 1; // set by skill
    [HideInInspector] public int damageToTree = 1; // set by skill
    [HideInInspector] public int damageToWall = 1; // set by skill
    [HideInInspector] public int damageToForniture = 1; // set by skill
    [HideInInspector] public float stunChance; // set by skill
    [HideInInspector] public float stunTime; // set by skill
    public string type;

    public GameObject light;

    // client player.lookDirection might be different from server lookDirection
    // when firing. so we need to sync the true direction to the client to move
    // the projectile the same way as on the server.
    // prevents issues like https://github.com/vis2k/uMMORPG2D/issues/3

    // fly direction
    [SyncVar, HideInInspector] public Vector2 direction;

    // if a targetless projectile doesn't hit anything, it should still be
    // destroyed after a while so it doesn't hang around forever.
    // destroying after 'distance' instead of 'time' is more accurate.
    // => fireballs should only fly X meters far
    // => instead of say '10 seconds' of flying across the whole map
    public float autoDestroyDistance = 20;

    Vector2 initialPosition;

    // effects like a trail or particles need to have their initial positions
    // corrected too. simply connect their .Clear() functions to the event.
    public UnityEvent onSetInitialPosition;

    public void DisableLight()
    {
        if(light) light.SetActive(false);
    }

    void Start()
    {
        // remember start position for distance checks
        initialPosition = transform.position;

        if (isClient && !melee)
        {
            if(light) light.SetActive(true);
            Invoke(nameof(DisableLight), 0.3f);
        }

        // move via Rigidbody into synced direction on server & client
        GetComponent<Rigidbody2D>().velocity = direction.normalized * speed;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetInitialPosition();
        SpawnSounds();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ((Player)caster).playerWeapon.TargetNearest();
    }

    public void SpawnSounds()
    {
        float heading = Mathf.Atan2(((Player)caster).playerMove.tempVector.x, ((Player)caster).playerMove.tempVector.y);
        if(light) light.transform.localRotation = Quaternion.Euler(0f, (heading * Mathf.Rad2Deg), 0);

        if (Player.localPlayer && !Player.localPlayer.playerOptions.blockSound)
            caster.GetComponent<PlayerSounds>().PlaySounds(type, "1");
    }

    void SetInitialPosition()
    {
        // the projectile should always start at the effectMount position.
        // -> server doesn't run animations, so it will never spawn it exactly
        //    where the effectMount is on the client by the time the packet
        //    reaches the client.
        // -> the best solution is to correct it here once
        if (caster != null)
        {
            transform.position = caster.skills.effectMount.position;
            onSetInitialPosition.Invoke();
        }
    }

    // fixedupdate on client and server to simulate the same effect without
    // using a NetworkTransform
    void FixedUpdate()
    {
        // caster still around?
        // note: we keep flying towards it even if it died already, because
        //       it looks weird if fireballs would be canceled inbetween.
        if (caster != null)
        {
            // server: did we fly further than auto destroy distance?
            if (isServer && Vector2.Distance(initialPosition, transform.position) >= autoDestroyDistance)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
        else if (isServer) NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D co)
    {
        if (co.gameObject.name != caster.name)
        {
            DamagableObject damagableObject = co.GetComponent<DamagableObject>();
            if (damagableObject)
            {
                if ((damagableObject.tree || damagableObject.rock) && !melee) { }
                else
                {
                    int dam = 0;
                    if (damagableObject.GetComponent<Entity>()) dam = damage;
                    else if (damagableObject.GetComponent<WallManager>()) dam = damageToWall;
                    else if (damagableObject.GetComponent<Tree>()) dam = damageToTree;
                    else if (damagableObject.GetComponent<Rock>()) dam = damageToRock;
                    else if (damagableObject.GetComponent<BuildingAccessory>()) dam = damageToForniture;

                    damagableObject.TakeDamage(((Player)caster), dam, melee, false);
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }
}
