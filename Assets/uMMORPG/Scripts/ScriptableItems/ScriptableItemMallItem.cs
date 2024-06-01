using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Item/Item mall item", order=999)]
public partial class ScriptableItemMallItem : ScriptableObject
{
    public Sprite image;
    public string category;
    public List<ScriptableItem> allItem = new List<ScriptableItem>();
    public int baseIncrease;
    public int coin;
    public int gold;

    static Dictionary<int, ScriptableItemMallItem> cache;
    public static Dictionary<int, ScriptableItemMallItem> All
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableItemMallItem[] items = Resources.LoadAll<ScriptableItemMallItem>("");

                // check for duplicates, then add to cache
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableItems with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }
}
