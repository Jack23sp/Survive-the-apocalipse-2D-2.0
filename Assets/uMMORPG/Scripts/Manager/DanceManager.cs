using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceManager : MonoBehaviour
{
    public static DanceManager singleton;
    public List<ScriptableDance> listCompleteOfDance = new List<ScriptableDance>();
    public RuntimeAnimatorController defaultAnimatorController;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public int FindNetworkDance(string danceName, string playerName)
    {
        Player player;
        int index = -1;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerDance.networkDance.Count; i++)
            {
                if (player.playerDance.networkDance[i] == danceName)
                {
                    return i;
                }
            }
        }

        return index;
    }

}
