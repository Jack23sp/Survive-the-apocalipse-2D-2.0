using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[Serializable]
public partial struct Boost
{
    public string boostType;
    public string timeStart;
    public string timeEnd;
    public double serverTimeBegin;
    public double serverTimeEnd;
    public float perc;
}

public partial struct BoostAttribute
{
    public double timer;
    public float perc;
}

public partial class Player
{
    public PlayerBoost playerBoost;
}

public partial class Database
{
    class boosts
    {
        public int myIndex { get; set; }
        public string character { get; set; }
        public string boostType { get; set; }
        public string timeStart { get; set; }
        public string timeEnd { get; set; }
        public double serverTimeBegin { get; set; }
        public double serverTimeEnd { get; set; }
        public float perc { get; set; }
    }

    public void SaveBoost(Player player)
    {
        if (!player.playerBoost) return;
        connection.Execute("DELETE FROM boosts WHERE character=?", player.name);
        for (int i = 0; i < player.playerBoost.boosts.Count; i++)
        {
            Boost slot = player.playerBoost.boosts[i];
            // note: .Insert causes a 'Constraint' exception. use Replace.
            connection.InsertOrReplace(new boosts
            {
                myIndex = i,
                character = player.name,
                boostType = slot.boostType,
                timeStart = slot.timeStart,
                timeEnd = slot.timeEnd,
                serverTimeBegin = slot.serverTimeBegin,
                serverTimeEnd = slot.serverTimeEnd,
                perc = slot.perc
            });
        }

    }

    public void LoadBoost(Player player)
    {
        PlayerBoost boost = player.GetComponent<PlayerBoost>();
        foreach (boosts row in connection.Query<boosts>("SELECT * FROM boosts WHERE character=?", player.name))
        {
            boost.boosts.Add(new Boost
            {
                boostType = row.boostType,
                timeStart = row.timeStart,
                timeEnd = row.timeEnd,
                serverTimeBegin = row.serverTimeBegin,
                serverTimeEnd = row.serverTimeEnd,
                perc = row.perc
            });
        }
    }
}

public class PlayerBoost : NetworkBehaviour
{
    public readonly SyncList<Boost> boosts = new SyncList<Boost>();
    private Player player;

    private TimeSpan difference;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        boosts.Callback += OnBoostChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        InvokeRepeating(nameof(CheckOldBoost), 1.0f, 1.0f);
        boosts.Callback += OnBoostChangedOnServer;
    }

    public void CheckOldBoost()
    {
        for (int i = 0; i < boosts.Count; i++)
        {
            int index = i;
            difference = DateTime.Parse(player.playerBoost.boosts[index].timeEnd) - DateTime.UtcNow;

            if (difference.TotalSeconds <= 0)
            {
                if (boosts[index].boostType == "Medician")
                {
                    player.health.SetTimerPercent(boosts[index].perc, false, 1);
                }
                if (boosts[index].boostType == "Athlet")
                {
                    player.mana.SetTimerPercent(boosts[index].perc, false, 1);
                }
                player.playerMove.CheckSpeed();
                boosts.RemoveAt(index);
            }
        }
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerBoost = this;
    }

    void OnBoostChanged(SyncList<Boost>.Operation op, int index, Boost oldSlot, Boost newSlot)
    {
        if (UIBoost.singleton && UIBoost.singleton.panel.activeInHierarchy)
        {
            UIBoost.singleton.UpdateBoost();
        }

        player.playerMove.CheckSpeed();
    }

    void OnBoostChangedOnServer(SyncList<Boost>.Operation op, int index, Boost oldSlot, Boost newSlot)
    {
        if (op == SyncList<Boost>.Operation.OP_ADD ||
            op == SyncList<Boost>.Operation.OP_INSERT ||
            op == SyncList<Boost>.Operation.OP_SET)
        {
            if (newSlot.boostType == "Medician")
            {
                player.health.SetTimerPercent(newSlot.perc, true, 1);
            }
            if (newSlot.boostType == "Athlet")
            {
                player.mana.SetTimerPercent(newSlot.perc, true, 1);
            }
            if (newSlot.boostType == "Sportive")
            {
                player.playerMove.CheckSpeed();
            }
        }

        player.GetComponent<PlayerMove>().CheckSpeed();
    }

    [Command]
    public void CmdAddBoost(string boostName)
    {
        if (player.itemMall.coins < LookAtBoostTemplate(boostName).coin) return;
        int present = -2;
        DateTime time = DateTime.UtcNow;
        BoostAttribute attribute = FindPercAndTimer(boostName);
        if (attribute.timer == 0) return;

        present = LookAtBoost(boostName);
        if (present == -1)
        {
            boosts.Add(new Boost
            {
                boostType = boostName,
                timeStart = time.ToString(),
                timeEnd = time.AddSeconds(attribute.timer).ToString(),
                serverTimeBegin = NetworkTime.time,
                serverTimeEnd = NetworkTime.time + attribute.timer,
                perc = attribute.perc
            });
        }
        else
        {
            Boost boost = boosts[present];
            if (DateTime.Parse(boost.timeEnd) <= time)
            {
                boost.timeStart = time.ToString();
                boost.timeEnd = time.AddSeconds(attribute.timer).ToString();
                boost.serverTimeBegin = NetworkTime.time;
                boost.serverTimeEnd = NetworkTime.time + attribute.timer;
                boost.perc += attribute.perc;
            }
            else
            {
                boost.timeEnd = time.AddSeconds(attribute.timer).ToString();
                boost.serverTimeEnd = NetworkTime.time + attribute.timer;
                boost.perc += attribute.perc;
            }
            boosts[present] = boost;
        }
        player.itemMall.coins -= LookAtBoostTemplate(boostName).coin;
    }

    public BoostAttribute FindPercAndTimer(string boostName)
    {
        BoostAttribute attribute = new BoostAttribute();
        ScriptableBoost boostTemplate = LookAtBoostTemplate(boostName);

        if (!boostTemplate) return attribute;

        if (boostName == "Sportive")
        {
            attribute.timer = boostTemplate.velocityTimer;
            attribute.perc = boostTemplate.velocityPerc;
        }
        else if (boostName == "Dexterity")
        {
            attribute.timer = boostTemplate.dexterityTimer;
            attribute.perc = boostTemplate.dexterityPerc;
        }
        else if (boostName == "Precision")
        {
            attribute.timer = boostTemplate.precisionTimer;
            attribute.perc = boostTemplate.precisionPerc;
        }
        else if (boostName == "Soldier")
        {
            attribute.timer = boostTemplate.soldierTimer;
            attribute.perc = boostTemplate.soldierPerc;
        }
        else if (boostName == "Experienced")
        {
            attribute.timer = boostTemplate.doubleEXP;
            attribute.perc = 0;
        }
        else if (boostName == "Rich")
        {
            attribute.timer = boostTemplate.doubleGold;
            attribute.perc = 0;
        }
        else if (boostName == "Miserly")
        {
            attribute.timer = boostTemplate.doubleLeaderPoints;
            attribute.perc = 0;
        }
        //else if (boostName == "Butcher")
        //{
        //    attribute.timer = boostTemplate.doubleDamageToMonster;
        //    attribute.perc = 0;
        //}
        else if (boostName == "Destroyer")
        {
            attribute.timer = boostTemplate.doubleDamageToBuilding;
            attribute.perc = 0;
        }
        else if (boostName == "Medician")
        {
            attribute.timer = boostTemplate.healthTimer;
            attribute.perc = boostTemplate.healthPerc;
        }
        else if (boostName == "Athlet")
        {
            attribute.timer = boostTemplate.staminaTimer;
            attribute.perc = boostTemplate.staminaPerc;
        }
        else if (boostName == "Sniper")
        {
            attribute.timer = boostTemplate.aimTimer;
            attribute.perc = boostTemplate.aimPrecision;
        }


        return attribute;
    }

    public int LookAtBoost(string boostName)
    {
        for (int i = 0; i < boosts.Count; i++)
        {
            int index = i;
            if (boosts[index].boostType == boostName)
            {
                return index;
            }
        }

        return -1;
    }

    public ScriptableBoost LookAtBoostTemplate(string boostName)
    {
        for (int i = 0; i < BoostManager.singleton.allBoosts.Count; i++)
        {
            int index = i;
            if (BoostManager.singleton.allBoosts[index].name == boostName)
            {
                return BoostManager.singleton.allBoosts[index];
            }
        }

        return null;
    }

    public float FindBoostPercent(string boostName)
    {
        for (int i = 0; i < boosts.Count; i++)
        {
            int index = i;
            if (boosts[index].boostType == boostName)
            {
                return boosts[index].perc;
            }
        }

        return 0.0f;
    }

    public Sprite LookAtBoostTemplateImage(string boostName)
    {
        for (int i = 0; i < BoostManager.singleton.allBoosts.Count; i++)
        {
            int index = i;
            if (BoostManager.singleton.allBoosts[index].name == boostName)
            {
                return BoostManager.singleton.allBoosts[index].image;
            }
        }

        return null;
    }

    public string LookAtBoostTemplateDescription(string boostName)
    {
        for (int i = 0; i < BoostManager.singleton.allBoosts.Count; i++)
        {
            int index = i;
            if (BoostManager.singleton.allBoosts[index].name == boostName)
            {
                return BoostManager.singleton.allBoosts[index].Description;
            }
        }

        return string.Empty;
    }
}
