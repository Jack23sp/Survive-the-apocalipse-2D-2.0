﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "uMMORPG Item/Emoji", order = 999)]
public partial class ScriptableEmoji : ScriptableObject
{
    public int coinToBuy;
    public int goldToBuy;

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableEmoji> cache;
    public static Dictionary<int, ScriptableEmoji> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableEmoji[] items = Resources.LoadAll<ScriptableEmoji>("");

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

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {

    }
}