using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public enum DamageType { Normal, Block, Crit, Miss, Resist,Armor }

// inventory, attributes etc. can influence max health
public interface ICombatBonus
{
    int GetDamageBonus();
    int GetDefenseBonus();
    float GetCriticalChanceBonus();
    float GetBlockChanceBonus();
}

[Serializable] public class UnityEventIntDamageType : UnityEvent<int, DamageType> {}
[Serializable] public class UnityEventFloatDamageType : UnityEvent<float, DamageType> {}

[DisallowMultipleComponent]
public partial class Combat : NetworkBehaviour
{
    [Header("Components")]
    public Level level;
    public Entity entity;
    public new Collider2D collider;

    [Header("Stats")]
    [SyncVar] public bool invincible = false; // GMs, Npcs, ...
    public LinearInt baseDamage = new LinearInt{baseValue=1};
    public LinearInt baseDefense = new LinearInt{baseValue=1};
    public LinearFloat baseBlockChance;
    public LinearFloat baseCriticalChance;

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab;

    // events
    [Header("Events")]
    public UnityEventEntity onDamageDealtTo;
    public UnityEventEntity onKilledEnemy;
    public UnityEventEntityFloat onServerReceivedDamage;
    public UnityEventFloatDamageType onClientReceivedDamage;

    private List<int> equipWithArmor = new List<int>();
    private int selectedEquipment = -1;

    // cache components that give a bonus (attributes, inventory, etc.)
    ICombatBonus[] _bonusComponents;
    ICombatBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<ICombatBonus>());

    // calculate damage
    public int damage
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetDamageBonus();
            return baseDamage.Get(level.current) + bonus;
        }
    }

    // calculate defense
    public int defense
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetDefenseBonus();
            return baseDefense.Get(level.current) + bonus;
        }
    }

    // calculate block
    public float blockChance
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetBlockChanceBonus();
            return baseBlockChance.Get(level.current) + bonus;
        }
    }

    // calculate critical
    public float criticalChance
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetCriticalChanceBonus();
            return baseCriticalChance.Get(level.current) + bonus;
        }
    }

    // combat //////////////////////////////////////////////////////////////////
    // deal damage at another entity
    // (can be overwritten for players etc. that need custom functionality)
    [Server]
    public virtual void DealDamageAt(Entity victim, int amount, float stunChance=0, float stunTime=0)
    {
        Combat victimCombat = victim.combat;
        int damageDealt = 0;
        DamageType damageType = DamageType.Normal;

        // don't deal any damage if entity is invincible
        if (!victimCombat.invincible)
        {
            // block? (we use < not <= so that block rate 0 never blocks)
            if (UnityEngine.Random.value < victimCombat.blockChance)
            {
                damageType = DamageType.Block;
            }
            // deal damage
            else
            {
                // subtract defense (but leave at least 1 damage, otherwise
                // it may be frustrating for weaker players)
                damageDealt = Mathf.Max(amount - victimCombat.defense, 1);

                // critical hit?
                if (UnityEngine.Random.value < criticalChance)
                {
                    damageDealt *= 2;
                    damageType = DamageType.Crit;
                }

                // deal the damage
                //victim.health.current -= damageDealt;
                if (victim is Player)
                {
                    Player player = (Player)victim;

                    equipWithArmor.Clear();
                    selectedEquipment = -1;

                    for (int i = 0; i < player.playerEquipment.slots.Count; i++)
                    {
                        if (player.playerEquipment.slots[i].amount == 0) continue;
                        if (player.playerEquipment.slots[i].item.currentArmor > 0)
                        {
                            if (!equipWithArmor.Contains(i)) 
                                    equipWithArmor.Add(i);
                        }
                    }

                    if (equipWithArmor.Count > 0)
                    {
                        selectedEquipment = UnityEngine.Random.Range(0, equipWithArmor.Count - 1);

                        ItemSlot slot = player.playerEquipment.slots[equipWithArmor[selectedEquipment]];

                        if (damageDealt > 0 && slot.item.currentArmor > 0)
                        {
                            damageType = DamageType.Armor;
                            int damage = Mathf.Min(slot.item.currentArmor, damageDealt);
                            slot.item.currentArmor -= damage;
                            player.playerEquipment.slots[equipWithArmor[selectedEquipment]] = slot;
                            victimCombat.RpcOnReceivedDamaged(damage, damageType);
                            damageDealt -= damage;
                            return;
                        }
                    }

                    if (damageDealt > 0)
                    {
                        damageType = DamageType.Normal;
                        victim.health.current = Mathf.Max(victim.health.current - damageDealt, 0);
                    }

                }
                else
                {
                    if (victim.health.current < damageDealt) victim.health.current = 0;
                    else
                        victim.health.current -= damageDealt;
                }


                // call OnServerReceivedDamage event on the target
                // -> can be used for monsters to pull aggro
                // -> can be used by equipment to decrease durability etc.
                victimCombat.onServerReceivedDamage.Invoke(entity, damageDealt);

                // stun?
                if (UnityEngine.Random.value < stunChance)
                {
                    // dont allow a short stun to overwrite a long stun
                    // => if a player is hit with a 10s stun, immediately
                    //    followed by a 1s stun, we don't want it to end in 1s!
                    double newStunEndTime = NetworkTime.time + stunTime;
                    victim.stunTimeEnd = Math.Max(newStunEndTime, entity.stunTimeEnd);
                }
            }

            // call OnDamageDealtTo / OnKilledEnemy events
            onDamageDealtTo.Invoke(victim);
            if (victim.health.current == 0)
                onKilledEnemy.Invoke(victim);
        }

        // let's make sure to pull aggro in any case so that archers
        // are still attacked if they are outside of the aggro range
        victim.OnAggro(entity);

        // show effects on clients
        victimCombat.RpcOnReceivedDamaged(damageDealt, damageType);

        // reset last combat time for both
        entity.lastCombatTime = NetworkTime.time;
        victim.lastCombatTime = NetworkTime.time;
    }

    // no need to instantiate damage popups on the server
    // -> calculating the position on the client saves server computations and
    //    takes less bandwidth (4 instead of 12 byte)
    [Client]
    void ShowDamagePopup(float amount, DamageType damageType)
    {
        // spawn the damage popup (if any) and set the text
        if (damagePopupPrefab != null)
        {
            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = collider.bounds;
            Vector2 position = new Vector2(bounds.center.x, bounds.max.y);

            GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            TextMesh textMesh = popup.GetComponentInChildren<TextMesh>();
            if (damageType == DamageType.Normal)
            {
                textMesh.color = Color.white;
                textMesh.text = amount.ToString();
            }
            else if (damageType == DamageType.Block)
            {
                textMesh.color = Color.gray;
                textMesh.text = "<i>Block!</i>";
            }
            else if (damageType == DamageType.Crit)
            {
                textMesh.color = Color.red;
                textMesh.text = amount + " Crit!";
            }
            else if (damageType == DamageType.Miss)
            {
                textMesh.color = Color.blue;
                textMesh.text = amount + " Miss!";
            }
            else if (damageType == DamageType.Resist)
            {
                textMesh.color = Color.cyan;
                textMesh.text = amount + " Resist! " + amount.ToString();
            }
            else if (damageType == DamageType.Armor)
            {
                textMesh.color = Color.yellow;
                textMesh.text = "Armor - " + amount.ToString();
            }
        }
    }

    [ClientRpc]
    public void RpcOnReceivedDamaged(float amount, DamageType damageType)
    {
        // show popup above receiver's head in all observers via ClientRpc
        ShowDamagePopup(amount, damageType);

        // call OnClientReceivedDamage event
        onClientReceivedDamage.Invoke(amount, damageType);
    }
}
