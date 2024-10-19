// for targetless projectiles that are fired into a general direction.
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "uMMORPG Skill/MunitionSkill", order = 999)]
public class MunitionSkill : DamageSkill
{
    [Header("Projectile")]
    public TargetlessProjectileSkillEffect projectile; // Arrows, Bullets, Fireballs, ...
    public ItemSlot equip;


    bool HasRequiredWeaponAndAmmo(Entity caster)
    {
        // requires no weapon category?
        // then we can't find weapon and check ammo. just allow it.
        // (monsters have no weapon requirements and don't even have an
        //  equipment component)
        //if (string.IsNullOrWhiteSpace(requiredWeaponCategory))
        //    return true;

        if (caster.equipment.slots[0].amount > 0)
        {
            // no ammo required, or has that ammo equipped?
            WeaponItem itemData = (WeaponItem)caster.equipment.slots[0].item.data;
            return itemData.requiredAmmo == null ||
                   (itemData.needMunitionInMagazine && LookForRemaingBullets(((Player)caster)) > 0) || (!itemData.needMunitionInMagazine && itemData.requiredAmmo != null && SearchForMunitionOnInventory(((Player)caster)) > -1);
        }
        return false;
    }

    public int LookForRemaingBullets(Player player)
    {
        for(int i = 0; i < player.equipment.slots[0].item.accessories.Length; i++)
        {
            if (player.equipment.slots[0].item.accessories[i].data.accessoriesType == AccessoriesType.magazine)
            {
                return player.equipment.slots[0].item.accessories[i].bulletsRemaining;
            }
        }

        return 0;
    }

    public int SearchForMunitionOnInventory(Player player)
    {

        if (player.equipment.slots[0].amount == 0) return -1;
        int mun = -1;

        for(int i = 0; i < player.playerBelt.belt.Count; i++)
        {
            if (player.playerBelt.belt[i].amount == 0) continue;
            if (player.playerBelt.belt[i].item.data.name == ((WeaponItem)player.equipment.slots[0].item.data).requiredAmmo.name)
            {
                return 0;
            }
        }


        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            if (player.inventory.slots[i].amount == 0) continue;
            if (player.inventory.slots[i].item.data.name == ((WeaponItem)player.equipment.slots[0].item.data).requiredAmmo.name)
            {
                return 1;
            }
        }

        return mun;
    }

    public void RemoveAmmo(Player player)
    {

        if (player.equipment.slots[0].amount == 0) return;
        int mun = SearchForMunitionOnInventory(player);
        if (player.equipment.slots[0].item.data.needMunitionInMagazine)
        {
            for (int i = 0; i < player.equipment.slots[0].item.accessories.Length; i++)
            {
                if (player.equipment.slots[0].item.accessories[i].data.accessoriesType == AccessoriesType.magazine)
                {
                    Item item = player.equipment.slots[0].item.accessories[i];
                    if(item.bulletsRemaining > 0)
                    {
                        item.bulletsRemaining--;
                        player.equipment.slots[0].item.accessories[i] = item;
                        player.playerWeapon.shooted++;
                        //player.playerEquipment.Reassign();
                        //if (player.equipment.slots[0].item.bulletsRemaining == 0)
                        //    player.playerWeapon.ChargeMunition(player.equipment.slots[0].item.data.name);
                    }
                }
            }
        }
        else
        {
            if (mun == 0)
            {
                for (int i = 0; i < player.playerBelt.belt.Count; i++)
                {
                    if (player.playerBelt.belt[i].amount == 0) continue;
                    if (player.playerBelt.belt[i].item.data.name == ((WeaponItem)player.equipment.slots[0].item.data).requiredAmmo.name)
                    {
                        ItemSlot slot = player.playerBelt.belt[i];
                        slot.amount--;
                        player.playerBelt.belt[i] = slot;
                        //player.playerEquipment.Reassign();
                        return;
                    }
                }
            }
            else if (mun == 1)
            {
                for (int i = 0; i < player.inventory.slots.Count; i++)
                {
                    if (player.inventory.slots[i].amount == 0) continue;

                    if (player.inventory.slots[i].item.data.name == ((WeaponItem)player.equipment.slots[0].item.data).requiredAmmo.name)
                    {
                        ItemSlot slot = player.inventory.slots[i];
                        slot.amount--;
                        player.inventory.slots[i] = slot;
                        //player.playerEquipment.Reassign();
                        return;
                    }
                }
            }
        }
    }



    void ConsumeRequiredWeaponsAmmo(Player caster)
    {
        RemoveAmmo(caster);
    }

    public override bool CheckSelf(Entity caster, int skillLevel)
    {
        // check base and ammo
        return base.CheckSelf(caster, skillLevel) &&
               HasRequiredWeaponAndAmmo(((Player)caster));
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
        // consume ammo if needed
        ConsumeRequiredWeaponsAmmo(((Player)caster));
        
        // spawn the skill effect. this can be used for anything ranging from
        // blood splatter to arrows to chain lightning.
        // -> we need to call an RPC anyway, it doesn't make much of a diff-
        //    erence if we use NetworkServer.Spawn for everything.
        // -> we try to spawn it at the weapon's projectile mount
        if (projectile != null)
        {
            GameObject go = Instantiate(projectile.gameObject, caster.transform.position, caster.transform.rotation);
            TargetlessProjectileSkillEffect effect = go.GetComponent<TargetlessProjectileSkillEffect>();
            effect.target = caster.target;
            effect.caster = caster;
            effect.damage = damage.Get(skillLevel);
            effect.damageToRock = damageToRock.Get(skillLevel);
            effect.damageToTree = damageToTree.Get(skillLevel);
            effect.damageToWall = damageToWall.Get(skillLevel);
            effect.stunChance = stunChance.Get(skillLevel);
            effect.stunTime = stunTime.Get(skillLevel);
            // always fly into caster's look direction.
            // IMPORTANT: use the parameter. DON'T use entity.direction.
            // we want the exact direction that was passed in CmdUse()!
            effect.direction = direction;
            NetworkServer.Spawn(go);
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
}
