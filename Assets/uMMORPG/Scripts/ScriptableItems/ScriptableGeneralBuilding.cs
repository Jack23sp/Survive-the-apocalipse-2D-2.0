using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Building
{
    public UsableItem mainObject;
    public List<BuildingChild> usableItems;
}

[System.Serializable]
public class BuildingChild
{
    public UsableItem item;
    public List<UsableItem> children;
}

[CreateAssetMenu(menuName = "uMMORPG Item/General building", order = 999)]
public class ScriptableGeneralBuilding : ScriptableObject
{
    public Building mainBuilding;
}
