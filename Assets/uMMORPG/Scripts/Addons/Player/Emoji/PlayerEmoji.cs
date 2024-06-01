using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerEmoji playerEmoji;
}

public partial class Database
{
    class emoji
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string emojiName { get; set; }
    }

    public void Connect_Emoji()
    {
        connection.CreateTable<emoji>();
    }

    public void SaveEmoji(Player player)
    {
        PlayerEmoji playerEmoji = player.GetComponent<PlayerEmoji>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM emoji WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < playerEmoji.networkEmoji.Count; i++)
        {
            int index = i;
            connection.InsertOrReplace(new emoji
            {
                characterName = player.name,
                emojiName = player.playerEmoji.networkEmoji[index]
            }); ;
        }
    }
    public void LoadEmoji(Player player)
    {
        PlayerEmoji emoji = player.GetComponent<PlayerEmoji>();

        foreach (emoji row in connection.Query<emoji>("SELECT * FROM emoji WHERE characterName=?", player.name))
        {
            emoji.networkEmoji.Add(row.emojiName);
        }

    }
}

public class PlayerEmoji : NetworkBehaviour
{
    private Player player;
    public SyncList<string> networkEmoji = new SyncList<string>();

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerEmoji = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        networkEmoji.Callback += OnEmojiListUpdated;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (UIEmoji.singleton) UIEmoji.singleton.RefreshEmoji();
    }

    void OnEmojiListUpdated(SyncList<string>.Operation op, int index, string oldItem, string newItem)
    {
        if (player.isLocalPlayer)
        {
            if (UIEmoji.singleton) UIEmoji.singleton.RefreshEmoji();
        }
    }

    [Command]
    public void CmdSpawnEmoji(string emojiName, string playerName)
    {
        if (EmojiManager.singleton.FindNetworkEmoji(emojiName, playerName) >= 0)
        {
            for (int i = 0; i < EmojiManager.singleton.listCompleteOfEmoji.Count; i++)
            {
                int index = i;
                if (EmojiManager.singleton.listCompleteOfEmoji[index].name.Contains(emojiName))
                {
                    if (RegistrablePrefabManager.singleton.FindObjectInSpawnablePrefab(emojiName) != null)
                    {
                        GameObject g = Instantiate(RegistrablePrefabManager.singleton.FindObjectInSpawnablePrefab(emojiName));
                        g.GetComponent<SpawnedEmoji>().playerName = playerName;
                        NetworkServer.Spawn(g);
                    }
                }
            }
        }
    }


    [Command]
    public void CmdAddEmoji(string emojiName, int currencyType)
    {
        ScriptableEmoji emoji = null;
        for (int i = 0; i < EmojiManager.singleton.listCompleteOfEmoji.Count; i++)
        {
            if (EmojiManager.singleton.listCompleteOfEmoji[i].name == emojiName)
            {
                emoji = EmojiManager.singleton.listCompleteOfEmoji[i];
            }
        }
        if (currencyType == 0)
        {
            if (player.itemMall.coins >= emoji.coinToBuy)
            {
                player.playerEmoji.networkEmoji.Add(emojiName);
                player.itemMall.coins -= emoji.coinToBuy;
            }
        }
        if (currencyType == 1)
        {
            if (player.gold >= emoji.goldToBuy)
            {
                player.playerEmoji.networkEmoji.Add(emojiName);
                player.gold -= emoji.goldToBuy;
            }
        }
    }

    public int FindCoinEmojiByName(string emojiName)
    {
        for (int i = 0; i < EmojiManager.singleton.listCompleteOfEmoji.Count; i++)
        {
            if (EmojiManager.singleton.listCompleteOfEmoji[i].name == emojiName)
            {
                return EmojiManager.singleton.listCompleteOfEmoji[i].coinToBuy;
            }
        }

        return 100000;
    }
}
