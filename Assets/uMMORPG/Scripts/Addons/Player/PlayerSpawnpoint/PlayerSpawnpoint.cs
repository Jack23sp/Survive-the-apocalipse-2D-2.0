using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerSpawnpoint playerSpawnpoint;
}


[Serializable]
public partial struct Spawnpoint
{
    public string name;
    public float spawnPositionx;
    public float spawnPositiony;
    public bool prefered;
    public string color;

    public Spawnpoint(string Name, float SpawnPositionX, float SpawnPositionY, bool Prefered, string Color)
    {
        name = Name;
        spawnPositionx = SpawnPositionX;
        spawnPositiony = SpawnPositionY;
        prefered = Prefered;
        color = Color;
    }
}

public class SyncListSpawnPoint : SyncList<Spawnpoint> { }

public partial class Database
{
    class spawnpoint
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string spawnpointName { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public int prefered { get; set; }
        public string color { get; set; }
    }

    class Pin
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
    }

    public void Connect_Spawnpoint()
    {
        connection.CreateTable<spawnpoint>();
    }

    public void SaveSpawnpoint(Player player)
    {
        PlayerSpawnpoint spawnpoint = player.GetComponent<PlayerSpawnpoint>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM spawnpoint WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < spawnpoint.spawnpoint.Count; i++)
        {
            connection.InsertOrReplace(new spawnpoint
            {
                characterName = player.name,
                spawnpointName = spawnpoint.spawnpoint[i].name,
                posX = spawnpoint.spawnpoint[i].spawnPositionx,
                posY = spawnpoint.spawnpoint[i].spawnPositiony,
                prefered = Convert.ToInt32(spawnpoint.spawnpoint[i].prefered),
                color = spawnpoint.spawnpoint[i].color
            });
        }

    }

    public void SavePin(Player player)
    {
        PlayerSpawnpoint spawnpoint = player.GetComponent<PlayerSpawnpoint>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM Pin WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < spawnpoint.pins.Count; i++)
        {
            connection.InsertOrReplace(new Pin
            {
                characterName = player.name,
                posX = spawnpoint.pins[i].x,
                posY = spawnpoint.pins[i].y
            });
        }

    }

    public void LoadSpawnpoint(Player player)
    {
        PlayerSpawnpoint spawnpoint = player.GetComponent<PlayerSpawnpoint>();

        foreach (spawnpoint row in connection.Query<spawnpoint>("SELECT * FROM spawnpoint WHERE characterName=?", player.name))
        {
            Spawnpoint sp = new Spawnpoint(row.spawnpointName, row.posX, row.posY, Convert.ToBoolean(row.prefered), row.color);
            spawnpoint.spawnpoint.Add(sp);
        }
    }

    public void LoadPin(Player player)
    {
        PlayerSpawnpoint spawnpoint = player.GetComponent<PlayerSpawnpoint>();

        foreach (Pin row in connection.Query<Pin>("SELECT * FROM Pin WHERE characterName=?", player.name))
        {
            float x = row.posX;
            float y = row.posY;
            float z = 0.0f;

            Vector3 newObject = new Vector3(x, y, z);
            spawnpoint.pins.Add(newObject);
        }
    }
}


public class PlayerSpawnpoint : NetworkBehaviour
{
    private Player player;
    public SyncListSpawnPoint spawnpoint = new SyncListSpawnPoint();
    [HideInInspector] public ScriptableAbility ability;
    [SerializeField] private GameObject controlSpawnpointPrefab;
    private GameObject cachedSpawnpointObject;
    List<string> spawnPointToRemove = new List<string>();
    Transform[] orderedTransform = new Transform[0];
    public List<Vector3> pins = new List<Vector3>();
    public List<GameObject> spawnpointObjects = new List<GameObject>();
    public bool pin, pointofSpawn, path;


    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerSpawnpoint = this;
        ability = AbilityManager.singleton.spawnpointAbility;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        ControlSpawnpoint(true);
        OrderSpawnpoint();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();

    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        TargetPin(pins.ToArray());
        spawnpoint.Callback += SpawnInRealWorld;
        SpawnSpawnpointInRealWorld();
    }

    void SpawnInRealWorld(SyncList<Spawnpoint>.Operation op, int index, Spawnpoint oldValue, Spawnpoint newValue)
    {
        SpawnSpawnpointInRealWorld();
    }

    public void SpawnSpawnpointInRealWorld()
    {
        DestroyConcreteSpawnpoint();
        CreateConcreteSpawnpoint();
    }

    public void DestroyConcreteSpawnpoint()
    {
        for (int i = 0; i < spawnpointObjects.Count; i++)
        {
            Destroy(spawnpointObjects[i]);
        }
        spawnpointObjects.Clear();
    }

    public void CreateConcreteSpawnpoint()
    {
        for (int i = 0; i < spawnpoint.Count; i++)
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.spawnpointMarker, new Vector2(spawnpoint[i].spawnPositionx, spawnpoint[i].spawnPositiony), Quaternion.identity);
            Color newColor;

            if (ColorUtility.TryParseHtmlString(spawnpoint[i].color, out newColor))
            {
                g.GetComponent<SpriteRenderer>().color = newColor;
            }
            spawnpointObjects.Add(g);
        }
    }

    public void OrderSpawnpoint()
    {
        if (player.isServer)
        {
            for (int i = 0; i < spawnpoint.Count; i++)
            {
                int index = i;
                if (spawnpoint[index].prefered)
                {
                    Spawnpoint sp = spawnpoint[FindFirstNotPreferedSpawnpoint()];
                    spawnpoint[FindFirstNotPreferedSpawnpoint()] = spawnpoint[index];
                    spawnpoint[index] = sp;
                }
            }
        }
    }

    public int FindFirstNotPreferedSpawnpoint()
    {
        for (int e = 0; e < spawnpoint.Count; e++)
        {
            int index = e;
            if (!spawnpoint[index].prefered)
            {
                return index;
            }
        }
        return 0;
    }

    public void AdditionRevive()
    {
        if(player.playerPoisoned) player.playerPoisoned.current = 0;
        player.playerHungry.current = player.playerHungry.max;
        player.playerThirsty.current = player.playerThirsty.max;
        player.mana.current = player.mana.max;
        for (int i = 0; i < player.equipment.slots.Count; i++)
        {
            int index = i;
            if (player.equipment.slots[index].amount > 0)
            {
                ItemSlot slot = player.equipment.slots[index];
                slot.item.currentArmor = ((EquipmentItem)slot.item.data).armor.Get(slot.item.armorLevel);
                player.equipment.slots[index] = slot;
            }
        }
        player.playerMove.tired = player.playerMove.maxTiredness;
        player.playerAdditionalState.additionalState = string.Empty;
        player.health.cleaningState = 0;
    }

    [Command]
    public void CmdSpawnpointRevive(float healthPercentage)
    {
        if (player.health.current <= 0 && player.inventory.CountItem(new Item(PremiumItemManager.singleton.Instantresurrect)) > 0)
        {
            player.health.current = player.health.max;
            player.mana.current = player.mana.max;
            SetState("IDLE");
            player.inventory.RemoveItem(new Item(PremiumItemManager.singleton.Instantresurrect), 1);
            AdditionRevive();
            player.playerNames.Clear();
        }
    }

    [Command]
    public void CmdSendResurrectAction(NetworkIdentity identity)
    {
        SetResurrectAction(identity);
    }

    public void SetResurrectAction(NetworkIdentity identity)
    {
        if (player.health.current > 0) return;
        Player sender = identity.GetComponent<Player>();
        if(sender && player.friendResurrect.friendActResurrect == null)
        {
            player.friendResurrect.friendActResurrect = sender.netIdentity;
        }
    }

    [Command]
    public void CmdAcceptResurrect(NetworkIdentity identity)
    {
        Player sender = identity.GetComponent<Player>();
        if (sender && sender.inventory.Count(new Item(PremiumItemManager.singleton.instantResurrectOtherPlayer)) > 0)
        {
            sender.inventory.Remove(new Item(PremiumItemManager.singleton.instantResurrectOtherPlayer), 1);
            player.health.current = player.health.max;
            player.mana.current = player.mana.max;
            player.friendResurrect.friendActResurrect = null;
            SetState("IDLE");
            AdditionRevive();
        }
    }

    [Command]
    public void CmdDeclineResurrect(NetworkIdentity identity, bool message)
    {
        DeclineInvite(identity, message);
    }


    public void DeclineInvite(NetworkIdentity identity, bool message)
    {
        Player other = identity.GetComponent<Player>();
        player.friendResurrect.friendActResurrect = null;
        if (!other) return;
        if(message) TargetRpcDeclineResurrect(other.connectionToClient, player.name + " has refuse your revive!");
    }

    [TargetRpc]
    public void TargetRpcDeclineResurrect(NetworkConnection connection, string message)
    {
        Player pl = connection.identity.GetComponent<Player>();
        if(pl && pl.isLocalPlayer)
            pl.playerNotification.SpawnNotification(ImageManager.singleton.refuse, message);
    }

    [Command]
    public void CmdSpawnSomewhere()
    {
        player.playerItemDrop.CalculateItemToDrop();        
        Transform start = NetworkManagerMMO.GetNearestStartPosition(transform.position);
        player.Revive(1.0f);
        AdditionRevive();
        SetState("IDLE");
        player.movement.Warp(start.position);
        player.playerNames.Clear();
    }

    public void SetState(string state)
    {
        player._state = state;
    }

    [Command]
    public void CmdSetSpawnpoint(string name, float x, float y, bool prefered, string playerName)
    {
        int spawnpointAbility = Convert.ToInt32(AbilityManager.singleton.FindNetworkAbilityLevel(player.playerSpawnpoint.ability.name, playerName) / 10);
        int possibleSpawnpoint = spawnpointAbility - player.playerSpawnpoint.spawnpoint.Count;

        if (possibleSpawnpoint > 0)
        {
            Spawnpoint sp = new Spawnpoint(name, x, y, prefered, Utilities.GenerateRandomColorHex());
            player.playerSpawnpoint.spawnpoint.Add(sp);
        }
        player.playerSpawnpoint.OrderSpawnpoint();
    }

    [Command]
    public void CmdDeleteSpawnpoint(string spawnpointName)
    {
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            if (player.playerSpawnpoint.spawnpoint[index].name == spawnpointName)
            {
                player.playerSpawnpoint.spawnpoint.RemoveAt(index);
            }
        }
        player.playerSpawnpoint.OrderSpawnpoint();
    }

    public void DeleteSpawnpoint(string spawnpointName)
    {
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            if (player.playerSpawnpoint.spawnpoint[index].name == spawnpointName)
            {
                player.playerSpawnpoint.spawnpoint.RemoveAt(index);
            }
        }
        player.playerSpawnpoint.OrderSpawnpoint();
    }

    [Command]
    public void CmdSetPrefered(string spawnpoint)
    {
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            if (player.playerSpawnpoint.spawnpoint[index].name == spawnpoint)
            {
                Spawnpoint s = player.playerSpawnpoint.spawnpoint[index];
                s.prefered = !s.prefered;
                player.playerSpawnpoint.spawnpoint[index] = s;
            }
        }
        player.playerSpawnpoint.OrderSpawnpoint();
    }
    public void SetPrefered(string spawnpoint)
    {
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            if (player.playerSpawnpoint.spawnpoint[index].name == spawnpoint)
            {
                Spawnpoint s = player.playerSpawnpoint.spawnpoint[index];
                s.prefered = !s.prefered;
                player.playerSpawnpoint.spawnpoint[index] = s;
            }
        }
        player.playerSpawnpoint.OrderSpawnpoint();
    }


    [Command]
    public void CmdControlSpawnpoint(bool atStartUp)
    {
        ControlSpawnpoint(atStartUp);
    }

    public void OrderTransformSpawnPosition(List<Transform> toOrderTransform)
    {
        orderedTransform = toOrderTransform.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToArray();
    }


    public void ControlSpawnpoint(bool atStartUp)
    {
        cachedSpawnpointObject = Instantiate(controlSpawnpointPrefab, player.transform.position, Quaternion.identity);
        if(atStartUp) NetworkManagerMMO.singleton.PopulateList();
        bool result = false;
        
        // check if there are someone spawnpoint now in enemy base to delete
        // and cache it
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            cachedSpawnpointObject.transform.position = new Vector3(player.playerSpawnpoint.spawnpoint[index].spawnPositionx, player.playerSpawnpoint.spawnpoint[index].spawnPositiony, 0.0f);
            result = cachedSpawnpointObject.GetComponent<ControlSpawnpoint>().Check(player);
            if (!result)
                spawnPointToRemove.Add(player.playerSpawnpoint.spawnpoint[index].name);
        }

        // check if player is in a enemy base
        cachedSpawnpointObject.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 0.0f);
        result = cachedSpawnpointObject.GetComponent<ControlSpawnpoint>().Check(player);

        // if in a enemy base look for the nearest point to spawn my player
        if (atStartUp)
        {
            if (!result) 
            {
                OrderTransformSpawnPosition(NetworkManagerMMO.singleton.startPositionsList);
                for (int i = 0; i < orderedTransform.Length; i++)
                {
                    cachedSpawnpointObject.transform.position = new Vector3(orderedTransform[i].position.x, orderedTransform[i].position.y, 0.0f);
                    result = cachedSpawnpointObject.GetComponent<ControlSpawnpoint>().Check(player);
                    if (result)
                    {
                        player.movement.Warp(new Vector3(orderedTransform[i].position.x, orderedTransform[i].position.y));
                        player.GetComponent<PlayerNotification>().TargetSpawnNotification("You has been moved because spawned in a enemy base");
                    }
                }
            }
        }

        // delete the spawnpoint that now are inside enemy base
        for (int i = 0; i < spawnPointToRemove.Count; i++)
        {
            DeleteSpawnpoint(spawnPointToRemove[i]);
        }

        if (spawnPointToRemove.Count > 0)
            player.GetComponent<PlayerNotification>().TargetSpawnNotification("Some of your spawnpoints have been deleted because they are now in an enemy base");

        player.playerSpawnpoint.OrderSpawnpoint();
        Destroy(cachedSpawnpointObject);

    }


    [Command]
    public void CmdSpawnAtPoint(float x, float y)
    {
        if (player.health.current == 0)
        {
            player.playerItemDrop.CalculateItemToDrop();
            player.movement.Warp(new Vector2(x, y));
            player.Revive(1.0f);
            SetState("IDLE");
            AdditionRevive();
            player.playerNames.Clear();
        }
    }

    [Command]
    public void CmdSyncToServerPin(Vector3[] pinsSync)
    {
        if (pinsSync.Length > 5) return;
        pins.Clear();
        pins = pinsSync.ToList();
    }

    [TargetRpc]
    public void TargetPin(Vector3[] pinsSync)
    {
        if (pinsSync.Length > 5) return;
        pins.Clear();
        pins = pinsSync.ToList();
    }
}
