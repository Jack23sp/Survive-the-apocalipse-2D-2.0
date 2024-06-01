using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager singleton;

    public List<ScriptableAbility> abilityList = new List<ScriptableAbility>();
    public ScriptableAbility allianceAbility;
    public ScriptableAbility accuracyAbility;
    public ScriptableAbility missAbility;
    //public ScriptableAbility plantAbility;
    public ScriptableAbility rockAbility;
    public ScriptableAbility treeAbility;
    public ScriptableAbility conservativeAbility;
    public ScriptableAbility spawnpointAbility;

    public float increaseAbilityOnAction = 0.05f;


    void Start()
    {
        if (!singleton) singleton = this;
    }

    public int FindNetworkAbility(string abilityName, string playerName)
    {
        Player player;
        int index = -1;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {

            for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
            {
                if (player.playerAbility.networkAbilities[i].name == abilityName)
                {
                    return i;
                }
            }
        }

        return index;
    }

    public float FindNetworkAbilityLevel(string abilityName, string playerName)
    {
        Player player;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
            {
                int index = i;
                if (player.playerAbility.networkAbilities[index].name == abilityName)
                {
                    return player.playerAbility.networkAbilities[index].level;
                }
            }
        }
        return 0;
    }

    public int FindNetworkAbilityMaxLevel(string abilityName, string playerName)
    {
        Player player;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
            {
                if (player.playerAbility.networkAbilities[i].name == abilityName)
                {
                    return player.playerAbility.networkAbilities[i].maxLevel;
                }
            }
        }
        return -1;
    }

    public ScriptableAbility FindAbility(string abilityName)
    {

        for (int i = 0; i < abilityList.Count; i++)
        {
            if (abilityList[i].name == abilityName)
            {
                return abilityList[i];
            }
        }
        return null;
    }
}
