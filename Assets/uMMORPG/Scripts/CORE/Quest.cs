// The Quest struct only contains the dynamic quest properties, so that the
// static properties can be read from the scriptable object. The benefits are
// low bandwidth and easy Player database saving (saves always refer to the
// scriptable quest, so we can change that any time).
//
// Quests have to be structs in order to work with SyncLists.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public struct Missions
{
    // hashcode used to reference the real ScriptableQuest (can't link to data
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;

    // the progress field can be used by inheriting from ScriptableQuests
    // -> the field can be:
    //    * kill counters for 1 monster
    //    * kill counters for 4 monsters (split into 4 bytes, so 4 x 255 kills)
    //    * simple boolean checks (1/0)
    //    * checklists (by setting the 32 bits to 1/0)
    // -> could use long for 64 bits if needed later, or even multiple fields
    public int progress;

    public List<Kill> kills;
    public List<Kill> players;
    public List<Craft> craft;
    public List<Pick> pick;
    public List<BuildCreate> building;

    // a quest is complete after finishing it at the npc and getting rewards
    public bool completed;

    // constructors
    public Missions(GeneralQuest data)
    {
        hash = data.name.GetStableHashCode();
        progress = 0;
        completed = false;
        kills = new List<Kill>(data.kills);
        players = new List<Kill>(data.players);
        craft = new List<Craft>(data.craft);
        pick = new List<Pick>(data.pick);
        building = new List<BuildCreate>(data.building);
    }

    public Missions(Missions quest)
    {
        hash = quest.data.name.GetStableHashCode();
        progress = 0;
        completed = false;
        kills = quest.kills;
        players = quest.players;
        craft = quest.craft;
        pick = quest.pick;
        building = quest.building;
    }

    // wrappers for easier access
    public GeneralQuest data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableQuest.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!GeneralQuest.dict.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableQuest with hash=" + hash + ". Make sure that all ScriptableQuests are in the Resources folder so they are loaded properly.");
            return GeneralQuest.dict[hash];
        }
    }
    public string name => data.name;
    public int requiredLevel => data.requiredLevel;
    public string predecessor => data.predecessor != null ? data.predecessor.name : "";
    public long rewardGold => data.rewardGold;
    public long rewardCoins => data.rewardCoins;
    public long rewardExperience => data.rewardExperience;
    //public ScriptableItem rewardItem => data.rewardItem;

    // events
    public void OnKilled(Player player, int questIndex, Entity victim) { data.OnKilled(player, questIndex, victim.name); }
    public void OnLocation(Player player, int questIndex, Collider2D location) { data.OnLocation(player, questIndex, location); }
    public void OnCraft(Player player, int questIndex, string ObjectName, int amount) { data.OnCraft(player, questIndex, ObjectName, amount); }
    public void OnBuild(Player player, int questIndex, string ObjectName, int amount) { data.OnBuild(player, questIndex, ObjectName, amount); }
    public void OnPick(Player player, int questIndex, string ObjectName, int amount) { data.OnPick(player, questIndex, ObjectName, amount); }
    // completion
    public bool IsFulfilled(Player player, int questIndex) { return data.IsFulfilled(player, questIndex); }
    public void OnCompleted(Player player, int questIndex) { data.OnCompleted(player, questIndex); }

    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code. we can't do it in
    // ScriptableQuest because some variables can only be replaced here, hence we
    // would end up with some variables not replaced in the string when calling
    // Tooltip() from the data.
    // -> note: each tooltip can have any variables, or none if needed
    //public string ToolTip(Player player)
    //{
    //    // we use a StringBuilder so that addons can modify tooltips later too
    //    // ('string' itself can't be passed as a mutable object)
    //    // note: field0 tooltip part is done in the scriptable quest, because it
    //    //       might be a number, might be 'Yes'/'No', etc.
    //    StringBuilder tip = new StringBuilder(data.ToolTip(player, this));
    //    //tip.Replace("{STATUS}", IsFulfilled(player) ? "<i>Complete!</i>" : "");

    //    // addon system hooks
    //    Utils.InvokeMany(typeof(Quest), this, "ToolTip_", tip);

    //    return tip.ToString();
    //}
}
