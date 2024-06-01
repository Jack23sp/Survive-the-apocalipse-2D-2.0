using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerAbility playerAbility;
}

[Serializable]
public partial struct Ability
{
    public string name;
    public float level;
    public int maxLevel;
    public int baseValue;

    public Ability(string Name, float Level, int MaxLevel, int BaseValue)
    {
        name = Name;
        level = Level;
        maxLevel = MaxLevel;
        baseValue = BaseValue;
    }
}

public partial class Database
{
    class ability
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string abilityName { get; set; }
        public float level { get; set; }
        public int maxLevel { get; set; }
        public int baseValue { get; set; }
    }

    public void Connect_Ability()
    {
        connection.CreateTable<ability>();
    }

    public void SaveAbilities(Player player)
    {
        connection.Execute("DELETE FROM ability WHERE characterName=?", player.name);
        PlayerAbility ability = player.GetComponent<PlayerAbility>();
        if (ability.networkAbilities.Count > 0)
        {
            for (int i = 0; i < ability.networkAbilities.Count; i++)
            {
                Ability slot = ability.networkAbilities[i];
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new ability
                {
                    characterName = player.name,
                    abilityName = ability.networkAbilities[i].name,
                    level = ability.networkAbilities[i].level,
                    maxLevel = ability.networkAbilities[i].maxLevel,
                    baseValue = ability.networkAbilities[i].baseValue
                });
            }
        }
    }

    public void LoadAbilities(Player player)
    {
        PlayerAbility abilities = player.GetComponent<PlayerAbility>();

        foreach (ability row in connection.Query<ability>("SELECT * FROM ability WHERE characterName=?", player.name))
        {
            Ability ability = new Ability();
            ability.name = row.abilityName;
            ability.level = row.level;
            ability.maxLevel = row.maxLevel;
            ability.baseValue = row.baseValue;
            abilities.networkAbilities.Add(ability);
        }

    }

}


public class SyncListAbility : SyncList<Ability> { }

public class PlayerAbility : NetworkBehaviour
{
    private Player player;
    [HideInInspector] public List<ScriptableAbility> abilities = new List<ScriptableAbility>();
    public SyncListAbility networkAbilities = new SyncListAbility();
    private BoxCollider2D checkCollider;
    private List<Collider2D> basement = new List<Collider2D>();



    void Assign()
    {
        player = GetComponent<Player>();
        player.playerAbility = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        if (networkAbilities.Count == 0)
        {
            abilities = AbilityManager.singleton.abilityList;
            foreach (ScriptableAbility ab in abilities)
            {
                networkAbilities.Add(new Ability(ab.name, 0, ab.maxLevel, ab.baseValue));
            }
        }
        networkAbilities.Callback += OnAbilitiesChangedOnServer;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        abilities = AbilityManager.singleton.abilityList;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        networkAbilities.Callback += OnAbilitiesChanged;
    }

    void OnAbilitiesChanged(SyncListAbility.Operation op, int index, Ability oldSlot, Ability newSlot)
    {
        if (UIAbilities.singleton) UIAbilities.singleton.RefreshAbilities(true);

        if (newSlot.name == "Thief" && player.isLocalPlayer)
        {
            checkCollider = (BoxCollider2D)player.transform.GetComponentInChildren<PlayerGrassDetector>().thisCollider;
            basement = Physics2D.OverlapBoxAll(checkCollider.bounds.center, checkCollider.bounds.size, 0, ModularBuildingManager.singleton.basementLayerMask).ToList();

            for (int i = 0; i < basement.Count; i++)
            {
                if (basement[i].GetComponent<ModularBuilding>())
                    basement[i].GetComponent<ModularBuilding>().RefreshWallOptions();
            }
        }
    }
    void OnAbilitiesChangedOnServer(SyncListAbility.Operation op, int index, Ability oldSlot, Ability newSlot)
    {
        Assign();
        if (op == SyncList<Ability>.Operation.OP_ADD ||
           op == SyncList<Ability>.Operation.OP_INSERT ||
           op == SyncList<Ability>.Operation.OP_SET)
        {
            if (newSlot.name == "Medician")
            {
                player.health.SetTimerPercent(newSlot.level, true, 0);
            }
            if (newSlot.name == "Sportive")
            {
                player.mana.SetTimerPercent(newSlot.level, true, 0);
            }
            if (newSlot.name == "Explorer")
            {
                player.inventory.slots.Add(new ItemSlot());
                player.inventory.abilitySize++;
            }
        }
        player.GetComponent<PlayerMove>().CheckSpeed();
    }

    public void ManageAbility(Ability newSlot)
    {
        Assign();
        if (newSlot.name == "Medician")
        {
            player.health.SetTimerPercent(newSlot.level, true, 0);
        }
        if (newSlot.name == "Sportive")
        {
            player.mana.SetTimerPercent(newSlot.level, true, 0);
        }
        if (newSlot.name == "Explorer")
        {
            player.inventory.slots.Add(new ItemSlot());
            player.inventory.abilitySize++;
        }
        player.GetComponent<PlayerMove>().CheckSpeed();
    }

    [Command]
    public void CmdIncreaseAbility(int index)
    {
        if (CanUpgradeAbilities(index))
        {
            Ability netAbility = networkAbilities[index];
            netAbility.level++;
            if (netAbility.level > netAbility.maxLevel) netAbility.level = netAbility.maxLevel;
            networkAbilities[index] = netAbility;
            player.gold -= networkAbilities[index].baseValue * networkAbilities[index].level <= 0 ? networkAbilities[index].baseValue : Convert.ToInt32(networkAbilities[index].baseValue * networkAbilities[index].level);
        }
    }

    public bool CanUpgradeAbilities(int index)
    {
        return networkAbilities[index].level < networkAbilities[index].maxLevel && player.gold >= AbilityManager.singleton.FindAbility(player.playerAbility.networkAbilities[index].name).baseValue * (networkAbilities[index].level <= 0 ? 1 : Convert.ToInt32(networkAbilities[index].level));
    }

}
