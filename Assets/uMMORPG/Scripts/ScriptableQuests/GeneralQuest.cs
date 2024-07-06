// a simple gather quest example
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public partial struct Kill
{
    public string name;
    public int amountRequest;
    public int actual;

    public Kill(string itm, int amount, int act)
    {
        name = itm;
        amountRequest = amount;
        actual = act;
    }
}

[System.Serializable]
public partial struct Craft
{
    public string item;
    public int amountRequest;
    public int actual;

    public Craft(string itm, int amount, int act)
    {
        item = itm;
        amountRequest = amount;
        actual = act;
    }
}

[System.Serializable]
public partial struct BuildCreate
{
    public string item;
    public int amountRequest;
    public int actual;

    public BuildCreate(string itm, int amount, int act)
    {
        item = itm;
        amountRequest = amount;
        actual = act;
    }
}

[System.Serializable]
public partial struct Pick
{
    public string item;
    public int amountRequest;
    public int actual;

    public Pick(string itm, int amount, int act)
    {
        item = itm;
        amountRequest = amount;
        actual = act;
    }
}

[System.Serializable]
public partial struct Rewards
{
    public string item;
    public int amount;

    public Rewards(string itm, int am)
    {
        item = itm;
        amount = am;
    }
}

[CreateAssetMenu(menuName = "uMMORPG Quest/General Quest", order = 999)]
public class GeneralQuest : ScriptableQuest
{
    [Header("Fulfillment")]

    public List<Kill> kills = new List<Kill>();
    public List<Kill> players = new List<Kill>();
    public List<Craft> craft = new List<Craft>();
    public List<Pick> pick = new List<Pick>();
    public List<BuildCreate> building = new List<BuildCreate>();


    // fulfillment /////////////////////////////////////////////////////////////
    public override bool IsFulfilled(Player player, int index)
    {
        Missions quest = player.quests.MissionToAccomplish[index];
        for (int i = 0; i < quest.kills.Count; i++)
        {
            if (quest.kills[i].actual < quest.kills[i].amountRequest) return false;
        }

        for (int i = 0; i < quest.players.Count; i++)
        {
            if (quest.players[i].actual < quest.players[i].amountRequest) return false;
        }

        for (int i = 0; i < quest.craft.Count; i++)
        {
            if (quest.craft[i].actual < quest.craft[i].amountRequest) return false;
        }

        for (int i = 0; i < quest.pick.Count; i++)
        {
            if (quest.pick[i].actual < quest.pick[i].amountRequest) return false;
        }

        for (int i = 0; i < quest.building.Count; i++)
        {
            if (quest.building[i].actual < quest.building[i].amountRequest) return false;
        }

        return true;
    }


    public override void OnCompleted(Player player, int index)
    {
        Missions quest = player.quests.MissionToAccomplish[index];


        if (!IsFulfilled(player, index)) return;

        // remove gathered items from player's inventory
        for (int i = 0; i < quest.data.rewards.Count; i++)
        {
            if (ScriptableItem.All.TryGetValue(quest.data.rewards[i].item.GetStableHashCode(), out ScriptableItem itemData))
            {
                if (!player.inventory.CanAdd(new Item(itemData), quest.data.rewards[i].amount))
                    return;
            }
            //if (!player.inventory.CanAdd(new Item(quest.data.rewards[i].item), quest.data.rewards[i].amount))
            //    return;
        }

        for (int i = 0; i < quest.data.rewards.Count; i++)
        {
            if (ScriptableItem.All.TryGetValue(quest.data.rewards[i].item.GetStableHashCode(), out ScriptableItem itemData))
            {
                if (!player.inventory.Add(new Item(itemData), quest.data.rewards[i].amount))
                    return;
            }
            //player.inventory.Add(new Item(quest.data.rewards[i].item), quest.data.rewards[i].amount);
        }

        player.gold += quest.data.rewardGold;
        player.itemMall.coins += quest.data.rewardCoins;
        player.experience.current += quest.data.rewardExperience;
        if (player.petControl.activePet != null)
            player.petControl.activePet.experience.current += quest.data.rewardExperience;

        if (quest.data.successive != null)
        {
            if (GeneralQuest.dict.TryGetValue(quest.data.successive.GetStableHashCode(), out GeneralQuest itemData))
            {
                Missions q = new Missions(itemData);
                player.quests.MissionToAccomplish.Add(q);
                player.quests.MissionToAccomplish.Remove(quest);
            }
            
        }
        else
        {
            player.quests.MissionToAccomplish.Remove(quest);
        }
    }

    public override void OnKilled(Player player, int questIndex, string victim)
    {
        // not done yet, and same name as prefab? (hence same monster?)
        Missions quest = player.quests.MissionToAccomplish[questIndex];

        for (int i = 0; i < quest.kills.Count; i++)
        {
            if (quest.kills[i].name.Contains(victim))
            {
                Kill kill = quest.kills[i];
                kill.actual++;
                quest.kills[i] = kill;
            }
        }
    }

    public override void OnCraft(Player player, int questIndex, string objectName, int amount)
    {
        Missions quest = player.quests.MissionToAccomplish[questIndex];

        for (int i = 0; i < quest.craft.Count; i++)
        {
            if (quest.craft[i].item.Contains(objectName))
            {
                Craft craft = quest.craft[i];
                craft.actual += amount;
                quest.craft[i] = craft;
            }
        }
    }

    public override void OnBuild(Player player, int questIndex, string objectName, int amount)
    {
        Missions quest = player.quests.MissionToAccomplish[questIndex];

        for (int i = 0; i < quest.building.Count; i++)
        {
            if (quest.building[i].item.Contains(objectName))
            {
                BuildCreate build = quest.building[i];
                build.actual += amount;
                quest.building[i] = build;
            }
        }
    }

    public override void OnPick(Player player, int questIndex, string objectName, int amount)
    {
        Missions quest = player.quests.MissionToAccomplish[questIndex];

        for (int i = 0; i < quest.pick.Count; i++)
        {
            if (quest.pick[i].item.Contains(objectName))
            {
                Pick pick = quest.pick[i];
                pick.actual += amount;
                quest.pick[i] = pick;
            }
        }
    }


    public override void OnLocation(Player player, int questIndex, Collider2D location)
    {
        // the location counts if it has exactly the same name as the quest.
        // simple and stupid.
        if (location.name == name)
        {
            Missions quest = player.quests.MissionToAccomplish[questIndex];
            quest.progress = 1;
            player.quests.MissionToAccomplish[questIndex] = quest;
        }
    }


    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip(Player player, Missions quest)
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(base.ToolTip(player, quest));
        //tip.Replace("{GATHERAMOUNT}", gatherAmount.ToString());
        //if (gatherItem != null)
        //{
        //    int gathered = player.inventory.Count(new Item(gatherItem));
        //    tip.Replace("{GATHERITEM}", gatherItem.name);
        //    tip.Replace("{GATHERED}", Mathf.Min(gathered, gatherAmount).ToString());
        //}
        return tip.ToString();
    }

    static Dictionary<int, GeneralQuest> cache;
    public static Dictionary<int, GeneralQuest> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableQuests in resources
                GeneralQuest[] quests = Resources.LoadAll<GeneralQuest>("");

                // check for duplicates, then add to cache
                List<string> duplicates = quests.ToList().FindDuplicates(quest => quest.name);
                if (duplicates.Count == 0)
                {
                    cache = quests.ToDictionary(quest => quest.name.GetStableHashCode(), quest => quest);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableQuests with the name " + duplicate + ". If you are using subfolders like 'Warrior/BeginnerQuest' and 'Archer/BeginnerQuest', then rename them to 'Warrior/(Warrior)BeginnerQuest' and 'Archer/(Archer)BeginnerQuest' instead.");
                }
            }
            return cache;
        }
    }

}
