// Quick slash in front of us that hits anything standing there.
//
// => Useful for hack & slash / action based combat skills/games without target.
//    (set one of the skillbar slots to SPACEBAR key for the ultimate effect)
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Skill/Slash Damage", order=999)]
public class SlashDamageSkill : DamageSkill
{
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
        // cast a box or circle into look direction and try to attack anything
        // that is attackable
        //float range = castRange.Get(skillLevel);
        //Vector2 center = (Vector2)caster.transform.position + caster.lookDirection;
        //Vector2 size = new Vector2(range, range);
        //Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0);
        RaycastHit2D[] colliders = Physics2D.LinecastAll(caster.transform.GetChild(0).transform.position, ((Vector2)caster.transform.GetChild(0).transform.position + caster.lookDirection) * castRange.Get(skillLevel), JoystickManager.singleton.meleeDetector);
        for(int i = 0; i < colliders.Length; i++)
        {
            DamagableObject damagableObject = colliders[i].collider.GetComponent<DamagableObject>();
            if (damagableObject && damagableObject.player != ((Player)caster))
            {
                if (damagableObject)
                {
                    int dam = 0;
                    if (damagableObject.GetComponent<Entity>()) dam = damage.Get(skillLevel);
                    else if (damagableObject.GetComponent<WallManager>()) dam = damageToWall.Get(skillLevel);
                    else if (damagableObject.GetComponent<Tree>()) dam = damageToTree.Get(skillLevel);
                    else if (damagableObject.GetComponent<Rock>()) dam = damageToRock.Get(skillLevel);
                    else if (damagableObject.GetComponent<BuildingAccessory>()) dam = damageToForniture.Get(skillLevel);

                    damagableObject.TakeDamage(((Player)caster), dam, true, false);
                    return;
                }
            }
        }
    }
}
