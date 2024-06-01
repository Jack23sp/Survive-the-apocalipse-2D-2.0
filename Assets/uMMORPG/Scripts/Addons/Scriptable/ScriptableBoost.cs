using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "uMMORPG Item/Boost", order = 999)]
public partial class ScriptableBoost : ScriptableObject
{

    public double velocityTimer;
    public float velocityPerc;

    public double dexterityTimer;
    public float dexterityPerc;

    public double soldierTimer;
    public float soldierPerc;

    public double precisionTimer;
    public float precisionPerc;

    public double doubleEXP;

    public double doubleGold;

    public double doubleLeaderPoints;

    public double doubleDamageToMonster;
    public double doubleDamageToPlayer;
    public double doubleDamageToBuilding;

    public double healthTimer;
    public float healthPerc;

    public double staminaTimer;
    public float staminaPerc;

    public double aimTimer;
    public float aimPrecision;


    public long coin;
    public long gold;

    [TextArea(5,5)]
    public string Description;

    public Sprite image;


    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableBoost> cache;
    public static Dictionary<int, ScriptableBoost> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableBoost[] items = Resources.LoadAll<ScriptableBoost>("");

                // check for duplicates, then add to cache
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableAbility with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }

    //public string GetDescription()
    //{
    //    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
    //    {
    //        string description = string.Empty;

    //        description += "                        " + "<b>" + name + "</b>" + "\n";
    //        description += "Velocita' : \n                            " + (velocityTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(velocityTimer) + " ( + " + velocityPerc + " %)" + "</color></b>\n" : GeneralManager.singleton.ConvertToTimer(velocityTimer) + " ( + " + velocityPerc + " %) \n");
    //        description += "Accuratezza : \n                            " + (accuracyTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(accuracyTimer) + " ( + " + accuracyPerc + " %)" + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(accuracyTimer) + " ( + " + accuracyPerc + " %) \n");
    //        description += "Evasione : \n                            " + (missTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(missTimer) + " ( + " + missPerc + " %)" + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(missTimer) + " ( + " + missPerc + " %) \n");
    //        description += "Premium zone : \n                            " + (hiddenIslandTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(hiddenIslandTimer) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(hiddenIslandTimer) + "\n");
    //        description += "Doppia esperienza : \n                            " + (doubleEXP > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleEXP) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleEXP) + "\n");
    //        description += "Doppio oro : \n                            " + (doubleGold > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleGold) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleGold) + "\n");
    //        description += "Doppi punti : \n                            " + (doubleLeaderPoints > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleLeaderPoints) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleLeaderPoints) + "\n");
    //        description += "Doppio danno agli zombie : \n                            " + (doubleDamageToMonster > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleDamageToMonster) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleDamageToMonster) + "\n");
    //        description += "Doppio danno ai giocatori : \n                            " + (doubleDamageToPlayer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleDamageToPlayer) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleDamageToPlayer) + "\n");

    //        return description;
    //    }
    //    else
    //    {
    //        description += "                        " + "<b>" + name + "</b>" + "\n";
    //        description += "Velocity : \n                            " + (velocityTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(velocityTimer) + " ( + " + velocityPerc + " %)" + "</color></b>\n" : GeneralManager.singleton.ConvertToTimer(velocityTimer) + " ( + " + velocityPerc + " %) \n");
    //        description += "Accuracy : \n                            " + (accuracyTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(accuracyTimer) + " ( + " + accuracyPerc + " %)" + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(accuracyTimer) + " ( + " + accuracyPerc + " %) \n");
    //        description += "Miss : \n                            " + (missTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(missTimer) + " ( + " + missPerc + " %)" + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(missTimer) + " ( + " + missPerc + " %) \n");
    //        description += "Premium zone : \n                            " + (hiddenIslandTimer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(hiddenIslandTimer) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(hiddenIslandTimer) + "\n");
    //        description += "Double exp. : \n                            " + (doubleEXP > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleEXP) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleEXP) + "\n");
    //        description += "Double gold : \n                            " + (doubleGold > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleGold) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleGold) + "\n");
    //        description += "Double points : \n                            " + (doubleLeaderPoints > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleLeaderPoints) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleLeaderPoints) + "\n");
    //        description += "Double damage to zombie : \n                            " + (doubleDamageToMonster > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleDamageToMonster) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleDamageToMonster) + "\n");
    //        description += "Double damage to player : \n                            " + (doubleDamageToPlayer > 0 ? "<b><color=green>" + GeneralManager.singleton.ConvertToTimer(doubleDamageToPlayer) + "</color></b> \n" : GeneralManager.singleton.ConvertToTimer(doubleDamageToPlayer) + "\n");


    //        return description;
    //    }

    //    return string.Empty;
    //}
    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {

    }
}