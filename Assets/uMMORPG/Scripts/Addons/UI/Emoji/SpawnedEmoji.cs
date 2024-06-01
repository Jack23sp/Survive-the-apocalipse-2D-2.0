using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnedEmoji : NetworkBehaviour
{
    [SyncVar(hook = (nameof(ManageChange)))] public string playerName;

    public void ManageChange(string oldValue, string newValue)
    {
        if(newValue != string.Empty)
        {
            this.transform.SetParent(Player.onlinePlayers[newValue].transform);
            GetComponent<RectTransform>().localPosition = new Vector3(0, 4, 0);
        }
    }
}
