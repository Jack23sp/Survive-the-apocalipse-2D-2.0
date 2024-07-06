using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Combat
{
    [Server]
    public virtual void DealDamage(Entity victim, int amount, DamageType damageType)
    {
        Combat victimCombat = victim.combat;

        // don't deal any damage if entity is invincible
        if (!victimCombat.invincible)
        {
            // deal the damage

            if (victim is Player)
            {
                Player player = (Player)victim;
                for (int i = 0; i < player.playerEquipment.slots.Count; i++)
                {
                    ItemSlot slot = player.playerEquipment.slots[i];
                    if (slot.amount == 0) continue;
                    if (amount > 0 && slot.item.currentArmor > 0)
                    {
                        int damage = Mathf.Min(slot.item.currentArmor, amount);
                        slot.item.currentArmor -= damage;
                        player.playerEquipment.slots[i] = slot;
                        amount -= damage;
                    }
                }
                if (amount > 0)
                {
                    victim.health.current = Mathf.Max(victim.health.current - amount, 0);
                }

            }
            else
            {
                if (victim.health.current < amount) victim.health.current = 0;
                else
                    victim.health.current -= amount;
            }

            // call OnServerReceivedDamage event on the target
            // -> can be used for monsters to pull aggro
            // -> can be used by equipment to decrease durability etc.
            victimCombat.onServerReceivedDamage.Invoke(entity, amount);

            // stun?

            // call OnDamageDealtTo / OnKilledEnemy events
            onDamageDealtTo.Invoke(victim);
            if (victim.health.current == 0)
                onKilledEnemy.Invoke(victim);
        }

        // let's make sure to pull aggro in any case so that archers
        // are still attacked if they are outside of the aggro range
        victim.OnAggro(entity);

        // show effects on clients
        victimCombat.RpcOnReceivedDamaged(amount, damageType);

        // reset last combat time for both
        entity.lastCombatTime = NetworkTime.time;
        victim.lastCombatTime = NetworkTime.time;
    }

}

public class DamagableObject : MonoBehaviour
{
    public bool DamagableByRange;
    public bool DamagableByMelee;
    public bool DamagableByExplosion;

    [HideInInspector] public Tree tree;
    [HideInInspector] public Rock rock;
    [HideInInspector] public ModularBuilding modularBuilding;
    [HideInInspector] public BuildingAccessory buildingAccessory;
    [HideInInspector] public Player player;
    [HideInInspector] public Monster zombie;
    [HideInInspector] public WallManager wall;

    [HideInInspector] public Player caster;
    private int rand;
    public float precisionRand;


    public void TakeDamage(Player caster, int damage, bool melee, bool explosion)
    {
        if (damage == 0)
        {
            if (melee)
            {
                if (tree)
                    caster.playerNotification.TargetSpawnNotification("Cannot hit tree with current weapon");
                if (rock)
                    caster.playerNotification.TargetSpawnNotification("Cannot hit rock with current weapon");
                //else
                //    caster.playerNotification.TargetSpawnNotification("Cannot hit this with current weapon");
            }
            return;
        }

        ItemSlot weapon = caster.equipment.slots[caster.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Weapon"))];
        if (weapon.amount == 0) return;

        if (melee)
        {
            if (weapon.item.currentDurability == 0 && ((WeaponItem)weapon.item.data).maxDurability.baseValue > 0)
            {
                caster.playerNotification.TargetSpawnNotification("Adjust your weapon to attack");
                return;
            }
        }

        float weaponBonus = ((WeaponItem)weapon.item.data).weaponAbility ? ((WeaponItem)weapon.item.data).CalculateWeaponDamage(caster) : 0.0f;
        if (tree != null)
        {
            if ((DamagableByRange && !melee) || (DamagableByMelee && melee) || (DamagableByExplosion && explosion))
            {
                if (DamagableByMelee && melee)
                {
                    caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("TREE CHOP"));
                }
                tree.health -= damage;

                int scrAmount = ResourceManager.singleton.GetTreeRewards(caster);
                GameObject g = Instantiate(ResourceManager.singleton.objectDrop.gameObject, tree.transform.position, Quaternion.identity);
                g.GetComponent<CurvedMovement>().startEntity = tree.transform;
                g.GetComponent<CurvedMovement>().SpawnAtPosition(new Item(ResourceManager.singleton.woodItem), scrAmount, -1, -1);

                if (tree.health <= 0)
                {
                    NetworkServer.Destroy(tree.gameObject);
                }

                int abIndex = AbilityManager.singleton.FindNetworkAbility("Woodcutter", caster.name);
                if (abIndex > -1)
                {
                    Ability ab = caster.playerAbility.networkAbilities[abIndex];
                    int max = AbilityManager.singleton.FindNetworkAbilityMaxLevel("Woodcutter", caster.name);
                    float next = Mathf.Min(ab.level + AbilityManager.singleton.increaseAbilityOnAction, max);
                    if (next > max) next = max;
                    float attrNext = (float)Math.Round(next, 2);
                    ab.level = attrNext;
                    caster.playerAbility.networkAbilities[abIndex] = ab;
                }
            }
        }
        else if (rock != null)
        {
            if ((DamagableByRange && !melee) || (DamagableByMelee && melee) || (DamagableByExplosion && explosion))
            {
                if (DamagableByMelee && melee)
                {
                    caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("ROCK SMASH"));
                }
                rock.health -= damage;

                ScriptableItem scr = ResourceManager.singleton.GetRockRewards(caster);
                GameObject g = Instantiate(ResourceManager.singleton.objectDrop.gameObject, rock.transform.position, Quaternion.identity);
                g.GetComponent<CurvedMovement>().startEntity = rock.transform;
                g.GetComponent<CurvedMovement>().SpawnAtPosition(new Item(scr), 1, -1, -1);

                if (rock.health <= 0)
                {
                    NetworkServer.Destroy(rock.gameObject);
                }

                int abIndex = AbilityManager.singleton.FindNetworkAbility("Miner", caster.name);
                if (abIndex > -1)
                {
                    Ability ab = caster.playerAbility.networkAbilities[abIndex];
                    int max = AbilityManager.singleton.FindNetworkAbilityMaxLevel("Miner", caster.name);
                    float next = Mathf.Min(ab.level + AbilityManager.singleton.increaseAbilityOnAction, max);
                    if (next > max) next = max;
                    float attrNext = (float)Math.Round(next,2);
                    ab.level = attrNext;
                    caster.playerAbility.networkAbilities[abIndex] = ab;
                }
            }
        }
        else if (modularBuilding != null)
        {

        }
        else if (buildingAccessory != null)
        {
            if ((DamagableByRange && !melee) || (DamagableByMelee && melee) || (DamagableByExplosion && explosion))
            {
                if (!ModularBuildingManager.singleton.CanDoOtherActionForniture(buildingAccessory, caster))
                {
                    if (DamagableByMelee && melee)
                    {
                        caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel(CalculateSoundToPlay(buildingAccessory.craftingAccessoryItem, (WeaponItem)weapon.item.data)));
                    }
                    buildingAccessory.health -= ((float)damage + (float)((damage / 100) * weaponBonus));
                    if (buildingAccessory)
                    {
                        if (buildingAccessory.health <= 0)
                        {
                            NetworkServer.Destroy(buildingAccessory.gameObject);
                        }
                    }
                }
            }
        }
        else if (player != null)
        {
            if (player.combat.invincible) return;
            if (player.health.health.current == 0) return;

            if (!melee)
            {
                if (player.health.current > 0)
                {
                    if (damage > 0)
                    {
                        rand = UnityEngine.Random.Range(1, 101);
                        if (rand <= caster.playerShootPrecision.Calculate())
                        {
                            caster.combat.DealDamage(player, damage, DamageType.Normal);
                            player.playerBlood.Spawn(2.0f);
                        }
                        else
                        {
                            caster.combat.DealDamage(player, 0, DamageType.Miss);
                        }
                        caster.petControl.OnDamageDealtTo(player);
                    }
                }
            }
            else
            {
                float dodgeAbilityValue = caster.playerMiss._current;

                float precisionValue = player.playerAccuracy._accuracy;

                ScriptableAbility casterAbility = player.playerAbility.abilities[AbilityManager.singleton.FindNetworkAbility("Soldier", player.name)];
                float casterAbilityLevel = AbilityManager.singleton.FindNetworkAbilityLevel("Soldier", player.name);
                float casterAbilityValue = (casterAbility.bonus * casterAbilityLevel) + player.playerBoost.FindBoostPercent("Soldier");


                int damageP = (damage + (int)((damage / 100) * weaponBonus));

                float rand = UnityEngine.Random.Range(0.0f, (100.0f + precisionValue));

                if (rand <= dodgeAbilityValue)
                {
                    caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("DODGE"));
                    caster.combat.DealDamage(player, 0, DamageType.Miss);
                }
                else
                {
                    float drill = UnityEngine.Random.Range(0.0f, 100.0f);
                    if (drill <= casterAbilityValue)
                    {
                        caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("SHIELD"));
                        caster.combat.DealDamage(player, damageP / 2, DamageType.Resist);
                    }
                    else
                    {
                        caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("BLOOD"));
                        caster.combat.DealDamage(player, damageP, DamageType.Normal);
                        player.playerBlood.Spawn(2.0f);
                    }
                }
                caster.petControl.OnDamageDealtTo(player);
            }

            if (player.health.health.current <= 0 && !player.playerNames.Contains(caster.name))
            {
                player.playerNames.Add(caster.name);
                caster.quests.SyncKillOnServer(new DetailOfQuest(zombie.name.Replace("Player", ""), 1));
                caster.playerPoints.playerKill++;
            }
        }
        else if (zombie != null)
        {
            if (zombie.health.health.current <= 0 && !zombie.playerNames.Contains(caster.name))
            {
                zombie.playerNames.Add(caster.name);
                if (zombie.isMonster)
                {
                    caster.quests.SyncKillOnServer(new DetailOfQuest(zombie.name.Replace("Zombie_",""), 1));
                    caster.playerPoints.monsterKill++;
                }
                else
                {
                    caster.quests.SyncKillOnServer(new DetailOfQuest(zombie.name.Replace("Animal_", ""), 1));
                    caster.playerPoints.animalKill++;
                }
            }
            
            if (zombie.health.health.current == 0) return;

            if (!zombie.target && !zombie.isNeutral)
            {

                zombie.followDistance = Vector2.Distance(zombie.transform.position, caster.transform.position) + 2;
                if (!zombie.settedManualTarget)
                {
                    zombie.target = caster;
                    Invoke(nameof(zombie.CancelFollow), 10.0f);
                }
            }

            if (!melee)
            {
                rand = UnityEngine.Random.Range(1, 101);
                if (rand <= caster.playerShootPrecision.Calculate())
                {
                    zombie.combat.DealDamage(zombie, (damage) + (int)((damage / 100) * weaponBonus), DamageType.Normal);
                    caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("BLOOD"));
                    ResourceManager.singleton.SpawnDropBlood(UnityEngine.Random.Range(1, 10), zombie.transform, 2.0f);
                }
                else
                {
                    zombie.combat.DealDamage(zombie, 0, DamageType.Miss);
                }

            }
            else
            {
                zombie.combat.DealDamage(zombie, (damage) + (int)((damage / 100) * weaponBonus), DamageType.Normal);
                caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("BLOOD"));
                ResourceManager.singleton.SpawnDropBlood(UnityEngine.Random.Range(1, 10), zombie.transform, 2.0f);
            }
            caster.petControl.OnDamageDealtTo(zombie);

            if (zombie.isNeutral) zombie.OnAggroEscape(caster);
        }
        else if (wall != null)
        {
            if ((DamagableByRange && !melee) || (DamagableByMelee && melee) || (DamagableByExplosion && explosion))
            {
                float damageB = damage + (damage + (int)((damage / 100) * weaponBonus)) + caster.playerBoost.FindBoostPercent("Destroyer");
                if (wall.up)
                {
                    if (wall.wall)
                    {
                        float amount = Mathf.Min(wall.modularBuilding.wallHealthUp, damageB);
                        wall.modularBuilding.wallHealthUp -= amount;
                        if (wall.modularBuilding.wallHealthUp <= 0) wall.modularBuilding.serverUpBasementDecoration = -1;
                    }
                    else
                    {
                        float amount = Mathf.Min(wall.modularBuilding.doorHealthUp, damageB);
                        wall.modularBuilding.doorHealthUp -= amount;
                        if (wall.modularBuilding.doorHealthUp <= 0) wall.modularBuilding.serverUpBasementDecoration = -1;

                    }
                }
                else if (wall.down)
                {
                    if (wall.wall)
                    {
                        float amount = Mathf.Min(wall.modularBuilding.wallHealthDown, damageB);
                        wall.modularBuilding.wallHealthDown -= damage;
                        if (wall.modularBuilding.wallHealthDown <= 0) wall.modularBuilding.serverDownBasementDecoration = -1;

                    }
                    else
                    {
                        float amount = Mathf.Min(wall.modularBuilding.doorHealthDown, damageB);
                        wall.modularBuilding.doorHealthDown -= damage;
                        if (wall.modularBuilding.doorHealthDown <= 0) wall.modularBuilding.serverDownBasementDecoration = -1;
                    }
                }
                else if (wall.left)
                {
                    if (wall.wall)
                    {
                        float amount = Mathf.Min(wall.modularBuilding.wallHealthSx, damageB);
                        wall.modularBuilding.wallHealthSx -= damage;
                        if (wall.modularBuilding.wallHealthSx <= 0) wall.modularBuilding.serverLeftBasementDecoration = -1;
                    }
                    else
                    {
                        float amount = Mathf.Min(wall.modularBuilding.doorHealthSx, damageB);
                        wall.modularBuilding.doorHealthSx -= damage;
                        if (wall.modularBuilding.doorHealthSx <= 0) wall.modularBuilding.serverLeftBasementDecoration = -1;
                    }
                }
                else if (wall.right)
                {
                    if (wall.wall)
                    {
                        float amount = Mathf.Min(wall.modularBuilding.wallHealthDx, damageB);
                        wall.modularBuilding.wallHealthDx -= damage;
                        if (wall.modularBuilding.wallHealthDx <= 0) wall.modularBuilding.serverRightBasementDecoration = -1;
                    }
                    else
                    {
                        float amount = Mathf.Min(wall.modularBuilding.doorHealthDx, damageB);
                        wall.modularBuilding.doorHealthDx -= damage;
                        if (wall.modularBuilding.doorHealthDx <= 0) wall.modularBuilding.serverRightBasementDecoration = -1;
                    }
                }
                if (DamagableByMelee && melee)
                {
                    caster.playerResources.RpcPlaySound(SoundManager.singleton.FindSoundByLabel("WALL"));
                }
            }
        }

        if (weapon.item.data.maxDurability.baseValue > 0)
        {
            if (weapon.item.currentDurability > 0)
            {
                ItemSlot equi = caster.playerEquipment.slots[0];
                if (melee)
                {
                    int rand = UnityEngine.Random.Range(0, 5);
                    if (rand >= 2)
                    {
                        equi.item.currentDurability--;
                        caster.playerEquipment.slots[0] = equi;
                    }
                }
                else
                {
                    int rand = UnityEngine.Random.Range(0, 10);
                    if (rand < 2)
                    {
                        equi.item.currentDurability--;
                        caster.playerEquipment.slots[0] = equi;
                    }

                }
            }
        }

    }

    public string CalculateSoundToPlay(ScriptableBuildingAccessory scriptableBuildingAccessory, WeaponItem weaponItem)
    {
        if (scriptableBuildingAccessory.isWood && weaponItem.isWood)
            return "WoodvsWood";
        if (scriptableBuildingAccessory.isMetal && weaponItem.isWood)
            return "MetalvsWood";
        if (scriptableBuildingAccessory.isConcrete && weaponItem.isWood)
            return "ConcretevsWood";
        if (scriptableBuildingAccessory.isStone && weaponItem.isWood)
            return "StonevsWood";

        if (scriptableBuildingAccessory.isWood && weaponItem.isMetal)
            return "WoodvsMetal";
        if (scriptableBuildingAccessory.isMetal && weaponItem.isMetal)
            return "MetalvsMetal";
        if (scriptableBuildingAccessory.isConcrete && weaponItem.isMetal)
            return "ConcretevsMetal";
        if (scriptableBuildingAccessory.isStone && weaponItem.isMetal)
            return "StonevsMetal";

        return string.Empty;
    }
}
