using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public partial class Database
{
    class quests
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string questName { get; set; }
        public string type { get; set; }
        public string objectName { get; set; }
        public int request { get; set; }
        public int amount { get; set; }

    }

    public void SaveQuests(Player player)
    {
        PlayerQuests quests = player.GetComponent<PlayerQuests>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM quests WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.

        for (int i = 0; i < quests.MissionToAccomplish.Count; i++)
        {
            for (int e = 0; e < quests.MissionToAccomplish[i].kills.Count; e++)
            {
                connection.InsertOrReplace(new quests
                {
                    characterName = player.name,
                    questName = quests.MissionToAccomplish[i].data.name,
                    type = "Kills",
                    objectName = quests.MissionToAccomplish[i].kills[e].name,
                    request = quests.MissionToAccomplish[i].kills[e].amountRequest,
                    amount = quests.MissionToAccomplish[i].kills[e].actual
                });
            }

            for (int a = 0; a < quests.MissionToAccomplish[i].pick.Count; a++)
            {
                connection.InsertOrReplace(new quests
                {
                    characterName = player.name,
                    questName = quests.MissionToAccomplish[i].data.name,
                    type = "Pick",
                    objectName = quests.MissionToAccomplish[i].pick[a].item,
                    request = quests.MissionToAccomplish[i].pick[a].amountRequest,
                    amount = quests.MissionToAccomplish[i].pick[a].actual
                });
            }

            for (int u = 0; u < quests.MissionToAccomplish[i].building.Count; u++)
            {
                connection.InsertOrReplace(new quests
                {
                    characterName = player.name,
                    questName = quests.MissionToAccomplish[i].data.name,
                    type = "Building",
                    objectName = quests.MissionToAccomplish[i].building[u].item,
                    request = quests.MissionToAccomplish[i].building[u].amountRequest,
                    amount = quests.MissionToAccomplish[i].building[u].actual
                });
            }

            for (int o = 0; o < quests.MissionToAccomplish[i].craft.Count; o++)
            {
                connection.InsertOrReplace(new quests
                {
                    characterName = player.name,
                    questName = quests.MissionToAccomplish[i].data.name,
                    type = "Crafting",
                    objectName = quests.MissionToAccomplish[i].craft[o].item,
                    request = quests.MissionToAccomplish[i].craft[o].amountRequest,
                    amount = quests.MissionToAccomplish[i].craft[o].actual
                });
            }

            for (int x = 0; x < quests.MissionToAccomplish[i].players.Count; x++)
            {
                connection.InsertOrReplace(new quests
                {
                    characterName = player.name,
                    questName = quests.MissionToAccomplish[i].data.name,
                    type = "Players",
                    objectName = quests.MissionToAccomplish[i].players[x].name,
                    request = quests.MissionToAccomplish[i].players[x].amountRequest,
                    amount = quests.MissionToAccomplish[i].players[x].actual
                });
            }
        }
    }

    public void LoadQuests(Player player)
    {
        PlayerQuests quests = player.GetComponent<PlayerQuests>();
        Missions q;
        foreach (quests rowM in connection.Query<quests>("SELECT DISTINCT questName FROM quests WHERE characterName=?", player.name))
        {
            if (GeneralQuest.dict.TryGetValue(rowM.questName.GetStableHashCode(), out GeneralQuest itemData))
            {
                q = new Missions(itemData);

                foreach (quests row in connection.Query<quests>("SELECT * FROM quests WHERE characterName=? AND questName=?", player.name, rowM.questName))
                {
                    for (int i = 0; i < q.kills.Count; i++)
                    {
                        if (q.kills[i].name == row.objectName)
                        {
                            Kill k = q.kills[i];
                            k.actual = row.amount;
                            k.amountRequest = row.request;
                            q.kills[i] = k;
                        }
                    }
                    for (int i = 0; i < q.pick.Count; i++)
                    {
                        if (q.pick[i].item == row.objectName)
                        {
                            Pick p = q.pick[i];
                            p.actual = row.amount;
                            p.amountRequest = row.request;
                            q.pick[i] = p;
                        }
                    }
                    for (int i = 0; i < q.building.Count; i++)
                    {
                        if (q.building[i].item == row.objectName)
                        {
                            BuildCreate c = q.building[i];
                            c.actual = row.amount;
                            c.amountRequest = row.request;
                            q.building[i] = c;
                        }
                    }
                    for (int i = 0; i < q.craft.Count; i++)
                    {
                        if (q.craft[i].item == row.objectName)
                        {
                            Craft cr = q.craft[i];
                            cr.actual = row.amount;
                            cr.amountRequest = row.request;
                            q.craft[i] = cr;
                        }
                    }
                    if (row.objectName == "Players")
                    {
                        Kill pl = q.players[0];
                        pl.actual = row.amount;
                        pl.amountRequest = row.request;
                        q.players[0] = pl;
                    }
                }
                quests.MissionToAccomplish.Add(q);
            }
        }
        for (int s = 0; s < quests.MissionToAccomplish.Count; s++)
        {
            quests.TargerRpcSyncQuest(s, quests.MissionToAccomplish[s]);
        }
        quests.TargetRpcSyncUI();
    }
}

[System.Serializable]
public partial struct DetailOfQuest
{
    public string objectName;
    public int amount;

    public DetailOfQuest(string objectname, int amountToAdd)
    {
        objectName = objectname;
        amount = amountToAdd;
    }
}

public partial class Player
{
    public List<string> playerNames = new List<string>();
}

[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerQuests : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;

    [Header("Quests")] // contains active and completed quests (=all)
    public int activeQuestLimit = 10;
    public List<GeneralQuest> firstQuest = new List<GeneralQuest>();
    public List<Missions> MissionToAccomplish = new List<Missions>();
    public List<int> toSync = new List<int>();


    [Command]
    public void CmdClaimQuest(int index)
    {
        MissionToAccomplish[index].OnCompleted(player, index);
    }

    [TargetRpc]
    public void TargetRpcSyncUI()
    {
        if (UIQuests.singleton)
        {
            if (UIQuests.singleton.questIndex > -1)
                UIQuests.singleton.SpawnQuestDetail(UIQuests.singleton.questIndex);
            else
                UIQuests.singleton.Open();
        }
    }

    public void Kill(DetailOfQuest newQuestObject)
    {
        for (int i = 0; i < MissionToAccomplish.Count; i++)
        {
            int index = i;
            Missions quest = MissionToAccomplish[index];
            for (int e = 0; e < quest.data.kills.Count; e++)
            {
                bool present = false;
                if (quest.data.kills[e].name.ToLower().Contains(newQuestObject.objectName.ToLower()))
                {
                    for (int y = 0; y < quest.kills.Count; y++)
                    {
                        if (quest.kills[y].name.ToLower().Contains(newQuestObject.objectName.ToLower()))
                        {
                            quest.kills[y] = new Kill(newQuestObject.objectName, quest.kills[y].amountRequest, (quest.kills[y].actual + newQuestObject.amount));
                            MissionToAccomplish[index] = new Missions(quest);
                            if (!toSync.Contains(index)) toSync.Add(index);
                            present = true;
                        }
                    }
                    if (!present)
                    {
                        quest.kills.Add(new Kill(quest.data.kills[e].name, quest.data.kills[e].amountRequest, newQuestObject.amount));
                        MissionToAccomplish[index] = new Missions(quest);
                        if (!toSync.Contains(index)) toSync.Add(index);
                    }
                }
            }
            //MissionToAccomplish[index] = quest;
        }

        for(int s = 0; s < toSync.Count(); s++)
        {
            TargerRpcSyncQuest(toSync[s], MissionToAccomplish[toSync[s]]);
        }
        toSync.Clear();
    }

    public void Build(DetailOfQuest newQuestObject)
    {
        for (int i = 0; i < MissionToAccomplish.Count; i++)
        {
            int index = i;
            Missions quest = MissionToAccomplish[index];
            for (int e = 0; e < quest.data.building.Count; e++)
            {
                bool present = false;
                if (quest.data.building[e].item.ToLower().Contains(newQuestObject.objectName.ToLower()))
                {
                    for (int y = 0; y < quest.building.Count; y++)
                    {
                        if (quest.building[y].item.ToLower().Contains(newQuestObject.objectName.ToLower()))
                        {
                            quest.building[y] = new BuildCreate(newQuestObject.objectName, quest.building[y].amountRequest, (quest.building[y].actual + newQuestObject.amount));
                            MissionToAccomplish[index] = new Missions(quest);
                            if (!toSync.Contains(index)) toSync.Add(index);
                            present = true;
                        }
                    }
                    if (!present)
                    {
                        quest.building.Add(new BuildCreate(quest.data.building[e].item, quest.data.building[e].amountRequest, newQuestObject.amount));
                        MissionToAccomplish[index] = new Missions(quest);
                        if (!toSync.Contains(index)) toSync.Add(index);
                    }
                }
            }
            //MissionToAccomplish[index] = quest;
        }
        for (int s = 0; s < toSync.Count(); s++)
        {
            TargerRpcSyncQuest(toSync[s], MissionToAccomplish[toSync[s]]);
        }
        TargetRpcSyncUI();
        toSync.Clear();
    }

    public void Craft(DetailOfQuest newQuestObject)
    {
        for (int i = 0; i < MissionToAccomplish.Count; i++)
        {
            int index = i;
            Missions quest = MissionToAccomplish[index];
            for (int e = 0; e < quest.data.craft.Count; e++)
            {
                bool present = false;
                if (quest.data.craft[e].item.ToLower().Contains(newQuestObject.objectName.ToLower()))
                {
                    for (int y = 0; y < quest.craft.Count; y++)
                    {
                        if (quest.craft[y].item.ToLower().Contains(newQuestObject.objectName.ToLower()))
                        {
                            quest.craft[y] = new Craft(newQuestObject.objectName, quest.craft[y].amountRequest, (quest.craft[y].actual + newQuestObject.amount));
                            MissionToAccomplish[index] = new Missions(quest);
                            if (!toSync.Contains(index)) toSync.Add(index);
                            present = true;
                        }
                    }
                    if (!present)
                    {
                        quest.craft.Add(new Craft(quest.data.craft[e].item, quest.data.craft[e].amountRequest, newQuestObject.amount));
                        MissionToAccomplish[index] = new Missions(quest);
                        if (!toSync.Contains(index)) toSync.Add(index);
                    }
                }
            }
            //MissionToAccomplish[index] = quest;
        }
        for (int s = 0; s < toSync.Count(); s++)
        {
            TargerRpcSyncQuest(toSync[s], MissionToAccomplish[toSync[s]]);
        }
        TargetRpcSyncUI();
        toSync.Clear();
    }

    public void Pick(DetailOfQuest newQuestObject)
    {
        for (int i = 0; i < MissionToAccomplish.Count; i++)
        {
            int index = i;
            Missions quest = MissionToAccomplish[index];
            for (int e = 0; e < quest.data.pick.Count; e++)
            {
                bool present = false;
                if (quest.data.pick[e].item.ToLower().Contains(newQuestObject.objectName.ToLower()))
                {
                    for (int y = 0; y < quest.pick.Count; y++)
                    {
                        if (quest.pick[y].item.ToLower().Contains(newQuestObject.objectName.ToLower()))
                        {
                            quest.pick[y] = new Pick(newQuestObject.objectName, quest.pick[y].amountRequest, (quest.pick[y].actual + newQuestObject.amount));
                            MissionToAccomplish[index] = new Missions(quest);
                            if (!toSync.Contains(index)) toSync.Add(index);
                            present = true;
                        }
                    }
                    if (!present)
                    {
                        quest.pick.Add(new Pick(quest.data.pick[e].item, quest.data.pick[e].amountRequest, newQuestObject.amount));
                        MissionToAccomplish[index] = new Missions(quest);
                        if (!toSync.Contains(index)) toSync.Add(index);
                    }
                }
            }
            //MissionToAccomplish[index] = quest;
        }
        for (int s = 0; s < toSync.Count(); s++)
        {
            TargerRpcSyncQuest(toSync[s], MissionToAccomplish[toSync[s]]);
        }
        TargetRpcSyncUI();
        toSync.Clear();

    }

    public void SyncKillOnServer(DetailOfQuest newQuestObject)
    {
        Kill(newQuestObject);
    }

    public void SyncCraftOnServer(DetailOfQuest newQuestObject)
    {
        Craft(newQuestObject);
    }

    public void SyncBuildOnServer(DetailOfQuest newQuestObject)
    {
        Build(newQuestObject);
    }

    public void SyncPickOnServer(DetailOfQuest newQuestObject)
    {
        Pick(newQuestObject);
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        if (MissionToAccomplish.Count == 0)
        {
            for (int i = 0; i < firstQuest.Count; i++)
            {
                int index = i;
                MissionToAccomplish.Add(new Missions(firstQuest[index]));
                if (!toSync.Contains(index)) toSync.Add(index);
            }
            for (int s = 0; s < toSync.Count(); s++)
            {
                TargerRpcSyncQuest(toSync[s], MissionToAccomplish[toSync[s]]);
            }
            TargetRpcSyncUI();
            toSync.Clear();
        }
        else
        {
            for (int s = 0; s < MissionToAccomplish.Count(); s++)
            {
                TargerRpcSyncQuest(s, MissionToAccomplish[s]);
            }
            TargetRpcSyncUI();
            toSync.Clear();
        }
    }

    [TargetRpc]
    public void TargerRpcSyncQuest(int missionIndex, Missions mission)
    {
        if (missionIndex <= player.quests.MissionToAccomplish.Count - 1) player.quests.MissionToAccomplish[missionIndex] = mission;
        else
            player.quests.MissionToAccomplish.Add(mission);
    }

    // combat //////////////////////////////////////////////////////////////////
    [Server]
    public void OnKilledEnemy(Entity victim)
    {
        // call OnKilled in all active (not completed) quests
        for (int i = 0; i < MissionToAccomplish.Count; ++i)
            if (!MissionToAccomplish[i].completed)
                MissionToAccomplish[i].OnKilled(player, i, victim);
    }

    // ontrigger ///////////////////////////////////////////////////////////////
    [ServerCallback]
    void OnTriggerEnter2D(Collider2D col)
    {
        // quest location? then call OnLocation in active (not completed) quests
        // (we use .CompareTag to avoid .tag allocations)
        if (col.CompareTag("QuestLocation"))
        {
            for (int i = 0; i < MissionToAccomplish.Count; ++i)
                if (!MissionToAccomplish[i].completed)
                    MissionToAccomplish[i].OnLocation(player, i, col);
        }
    }
}
