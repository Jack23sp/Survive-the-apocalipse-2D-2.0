using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDrop
{
    public float level;
    public List<ItemDropChance> itemDropChance;
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager singleton;

    public List<ItemDrop> rockItemDrops = new List<ItemDrop>();
    public ScriptableItem woodItem;
    public CurvedMovement objectDrop;
    public List<Sprite> bloodSprites = new List<Sprite>();
    public BloodParticles bloodDrop;

    void Start()
    {
        if (!singleton) singleton = this;    
    }

    public ScriptableItem GetRockRewards(Player player)
    {
        float ab = AbilityManager.singleton.FindNetworkAbilityLevel("Miner", player.name);
        int abilityLevel = -1;
        for(int i = 0; i < rockItemDrops.Count; i++)
        {
            if(ab <= rockItemDrops[i].level && abilityLevel == -1)
            {
                abilityLevel = i;
            }
        }

        for(int e = UnityEngine.Random.Range(0, rockItemDrops[abilityLevel].itemDropChance.Count -1); e < rockItemDrops[abilityLevel].itemDropChance.Count; e++)
        {
            if( UnityEngine.Random.Range(0,1) <= rockItemDrops[abilityLevel].itemDropChance[e].probability)
            {
                return rockItemDrops[abilityLevel].itemDropChance[e].item;
            }
        }
        return GetRockRewards(player);
    }

    public int GetTreeRewards(Player player)
    {
        float ab = AbilityManager.singleton.FindNetworkAbilityLevel("Woodcutter", player.name);
        return (ab / 10) < 1.0f ? 1 : UnityEngine.Random.Range(1,Convert.ToInt32(ab/10));   
    }

    public void SpawnDropBlood(int amountToDrop, Transform entity, float hei)
    {
        for (int i = 0; i < amountToDrop; i++)
        {
            GameObject g = Instantiate(bloodDrop.gameObject, entity.transform.position, Quaternion.identity);
            BloodParticles bloodParticles = g.GetComponent<BloodParticles>();
            bloodParticles.startEntity = entity;
            bloodParticles.height = hei;
            bloodParticles.SpawnAtPosition(UnityEngine.Random.Range(0, bloodSprites.Count));
        }
    }

    public float AddExperienceAmount(Player player, int amount)
    {
        return (float)(player.experience.max / 100) * amount;
    }

}
