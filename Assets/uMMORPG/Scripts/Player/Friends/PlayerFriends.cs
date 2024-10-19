using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerFriends playerFriends;
}

public partial class Database
{

    class friends
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string friendName { get; set; }
    }

    class friendsRequest
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string friendName { get; set; }
    }


    public void Connect_Friends()
    {
        connection.CreateTable<friends>();
        connection.CreateTable<friendsRequest>();
    }

    public void SaveFriends(Player player)
    {
        PlayerFriends friends = player.GetComponent<PlayerFriends>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friends WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        foreach (string s in friends.friends)
        {
            connection.InsertOrReplace(new friends
            {
                friendName = s,
                characterName = player.name
            });
        }

    }
    public void SaveSingleFriend(Player playerInviter, Player Invite)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.InsertOrReplace(new friends
        {
            friendName = playerInviter.name,
            characterName = Invite.name
        });
    }

    public void SaveFriendsRequest(Player player)
    {
        PlayerFriends friends = player.GetComponent<PlayerFriends>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friendsRequest WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        foreach (string s in friends.request)
        {
            connection.InsertOrReplace(new friendsRequest
            {
                friendName = s,
                characterName = player.name
            });
        }
    }

    public void LoadFriends(Player player)
    {
        PlayerFriends friends = player.GetComponent<PlayerFriends>();

        // then load valid items and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (friends r in connection.Query<friends>("SELECT * FROM friends WHERE characterName=?", player.name))
        {
            friends.friends.Add(r.friendName);
        }
    }

    public void LoadFriendsRequest(Player player)
    {
        PlayerFriends friends = player.GetComponent<PlayerFriends>();

        // then load valid items and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (friends r in connection.Query<friends>("SELECT * FROM friendsRequest WHERE characterName=?", player.name))
        {
            friends.request.Add(r.friendName);
        }
    }

    public void RemoveFriend(string player, string friendName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friends WHERE friendName=? AND characterName=?", friendName, player);
    }

    public void RemoveFriendRequest(string player, string friendName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friendsRequest WHERE friendName=? AND characterName=?", friendName, player);
    }
    public GameObject CharacterFriendLoad(string characterName, GameObject prefab)
    {
        characters row = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? AND deleted=0", characterName);
        if (row != null)
        {
            // instantiate based on the class name
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();

                player.name = row.name;
                player.account = row.account;
                player.className = row.classname;
                Vector2 position = new Vector2(row.x, row.y);
                player.level.current = Mathf.Min(row.level, player.level.max); // limit to max level in case we changed it
                player.strength.value = row.strength;
                player.intelligence.value = row.intelligence;
                player.experience.current = row.experience;
                ((PlayerSkills)player.skills).skillExperience = row.skillExperience;
                player.gold = row.gold;
                player.itemMall.coins = row.coins;

                LoadEquipment((PlayerEquipment)player.equipment);
                LoadGuildOnDemand(player.guild);
                LoadCustomFriendStat(player);

                player.health.current = (int)row.health;
                player.mana.current = (int)row.mana;


                return go;
            }
            else Debug.LogError("no prefab found for class: " + row.classname);
        }
        return null;
    }

    public void LoadCustomFriendStat(Player player)
    {
        LoadAbilities(player);
        LoadBoost(player);
        LoadPartner(player);
        LoadGuilAlly(player);
        LoadCharacterCreation(player);
    }

}

public class PlayerFriends : NetworkBehaviour
{
    private Player player;
    public SyncList<string> request = new SyncList<string>();
    public SyncList<string> friends = new SyncList<string>();

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerFriends = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }

    public void AddFriend(NetworkIdentity identity)
    {
        Player sender = identity.GetComponent<Player>();
        if (sender && sender is Player &&
           !player.playerOptions.blockFriend &&
           sender.playerFriends.request.Count < FriendsManager.singleton.maxFriendRequest &&
           player.playerFriends.friends.Count < FriendsManager.singleton.maxFriends &&
           sender.playerFriends.friends.Count < FriendsManager.singleton.maxFriends)
        {
            if (!player.playerFriends.request.Contains(name) &&
                !player.playerFriends.friends.Contains(name))
            {
                if (!player.playerFriends.request.Contains(sender.name))
                    player.playerFriends.request.Add(sender.name);
            }
        }
    }

    [Command]
    public void CmdLoadFriendStat(string friendName, int sexType)
    {
        GameObject friendDummy = Database.singleton.CharacterFriendLoad(friendName, sexType == 0 ? GameObjectSpawnManager.singleton.male : GameObjectSpawnManager.singleton.female);
        TargetRpcReturnFriend(friendDummy);
    }

    [TargetRpc]
    public void TargetRpcReturnFriend(GameObject returnFriend)
    {
        //if (UIFriends.singleton)
        //    UIFriends.singleton.selectedFriend = returnFriend.GetComponent<Player>();
    }

    [TargetRpc]
    public void TargetRpcFriendNotification(NetworkConnection connection, string message)
    {
        Player pl = connection.identity.GetComponent<Player>();
        pl.playerNotification.SpawnNotification(ImageManager.singleton.friend, message);
    }

    [Command]
    public void CmdAcceptFriends(string friendName)
    {
        AcceptFriends(friendName);
    }

    public void AcceptFriends(string friendName)
    {
        Player sender = null;
        Player.onlinePlayers.TryGetValue(friendName, out sender);

        if (sender)
        {
            if (friends.Count < FriendsManager.singleton.maxFriends &&
                sender.playerFriends.friends.Count < FriendsManager.singleton.maxFriends)
            {
                if (!friends.Contains(friendName))
                {
                    if (!sender.playerFriends.friends.Contains(player.name)) sender.playerFriends.friends.Add(player.name);
                    if (!friends.Contains(sender.name)) friends.Add(sender.name);
                    if (request.Contains(friendName)) request.Remove(friendName);
                    if (sender.playerFriends.request.Contains(player.name)) sender.playerFriends.request.Remove(player.name);
                }
            }
        }

    }

    [Command]
    public void CmdRemoveFriends(string friendName)
    {
        player.playerFriends.friends.Remove(friendName);

        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(friendName, out onlinePlayer))
        {
            onlinePlayer.playerFriends.friends.Remove(player.name);
        }
        else
        {
            Database.singleton.RemoveFriend(onlinePlayer.name, player.name);
        }
    }

    [Command]
    public void CmdRemoveRequestFriends(string friendName)
    {
        if (player.playerFriends.request.Contains(friendName)) player.playerFriends.request.Remove(friendName);
    }

    public void RemoveRequestFriends(string friendName)
    {
        if (player.playerFriends.request.Contains(friendName)) player.playerFriends.request.Remove(friendName);
    }
}
