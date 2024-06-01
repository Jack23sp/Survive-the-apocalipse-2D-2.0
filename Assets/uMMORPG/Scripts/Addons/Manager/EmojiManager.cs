using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiManager : MonoBehaviour
{
    public static EmojiManager singleton;
    public List<ScriptableEmoji> listCompleteOfEmoji = new List<ScriptableEmoji>();

    void Start()
    {
        if (!singleton) singleton = this;    
    }

    public int FindNetworkEmoji(string emojiName, string playerName)
    {
        Player player;
        int index = -1;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerEmoji.networkEmoji.Count; i++)
            {
                if (player.playerEmoji.networkEmoji[i] == emojiName)
                {
                    return i;
                }
            }
        }

        return index;
    }

}
