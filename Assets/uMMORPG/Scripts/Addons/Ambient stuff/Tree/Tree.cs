using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tree : BuildingAccessory
{
    public ItemDropChance dropChance;
    //public SpriteRenderer snowLayer;
    public SpriteRenderer cracksImage;

    public List<SpriteRenderer> treeSprites;

    public DamagableObject damagableObject;

    public new void Start()
    {
        base.Start();
        damagableObject.tree = this;
    }

    public void ManageVisibility(bool condition)
    {
        for(int i = 0; i < treeSprites.Count; i++)
        {
            treeSprites[i].enabled = condition;
        }
    }
}
