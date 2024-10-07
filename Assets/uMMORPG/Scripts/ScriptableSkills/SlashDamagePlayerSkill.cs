// for targetless projectiles that are fired into a general direction.
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Skill/Slash Player Skill", order=999)]
public class SlashDamagePlayerSkill : DamageSkill
{
    [Header("Projectile")]
    public TargetlessProjectileSkillEffect projectile; // Arrows, Bullets, Fireballs, ...
    public AudioClip aimSound;


    public override bool CheckSelf(Entity caster, int skillLevel)
    {
        // check base and ammo
        return base.CheckSelf(caster, skillLevel);
    }

    public override bool CheckTarget(Entity caster)
    {
        // no target necessary, but still set to self so that LookAt(target)
        // doesn't cause the player to look at a target that doesn't even matter
        caster.target = caster;
        return true;
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
    {
        // can cast anywhere
        destination = (Vector2)caster.transform.position + caster.lookDirection;
        return true;
    }

    public override void Apply(Entity caster, int skillLevel, Vector2 direction)
    {

        // spawn the skill effect. this can be used for anything ranging from
        // blood splatter to arrows to chain lightning.
        // -> we need to call an RPC anyway, it doesn't make much of a diff-
        //    erence if we use NetworkServer.Spawn for everything.
        // -> we try to spawn it at the weapon's projectile mount
        if (projectile != null)
        {
            if (caster is Player)
            {
                if (((Player)caster).playerTired.tired > 0)
                {
                    if (((Player)caster).playerTired.tired <= ((Player)caster).playerTired.tiredLimitForAim && ((Player)caster).mana.current > 0)
                    {
                        ((Player)caster).mana.current--;

                        GameObject go = Instantiate(projectile.gameObject, caster.skills.effectMount.position, caster.skills.effectMount.rotation);
                        TargetlessProjectileSkillEffect effect = go.GetComponent<TargetlessProjectileSkillEffect>();
                        effect.target = caster.target;
                        effect.caster = caster;
                        effect.damage = damage.Get(skillLevel);
                        effect.damageToWall = damageToWall.Get(skillLevel);
                        effect.damageToTree = damageToTree.Get(skillLevel);
                        effect.damageToRock = damageToRock.Get(skillLevel);
                        effect.damageToForniture = damageToForniture.Get(skillLevel);
                        effect.stunChance = stunChance.Get(skillLevel);
                        effect.stunTime = stunTime.Get(skillLevel);
                        // always fly into caster's look direction.
                        // IMPORTANT: use the parameter. DON'T use entity.direction.
                        // we want the exact direction that was passed in CmdUse()!
                        effect.direction = direction;
                        NetworkServer.Spawn(go);
                    }
                    else
                    {
                        GameObject go = Instantiate(projectile.gameObject, caster.skills.effectMount.position, caster.skills.effectMount.rotation);
                        TargetlessProjectileSkillEffect effect = go.GetComponent<TargetlessProjectileSkillEffect>();
                        effect.target = caster.target;
                        effect.caster = caster;
                        effect.damage = damage.Get(skillLevel);
                        effect.damageToWall = damageToWall.Get(skillLevel);
                        effect.damageToTree = damageToTree.Get(skillLevel);
                        effect.damageToRock = damageToRock.Get(skillLevel);
                        effect.damageToForniture = damageToForniture.Get(skillLevel);
                        effect.stunChance = stunChance.Get(skillLevel);
                        effect.stunTime = stunTime.Get(skillLevel);
                        // always fly into caster's look direction.
                        // IMPORTANT: use the parameter. DON'T use entity.direction.
                        // we want the exact direction that was passed in CmdUse()!
                        effect.direction = direction;
                        NetworkServer.Spawn(go);
                    }
                }
            }
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
}
