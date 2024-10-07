using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Rock : BuildingAccessory
{
    public ItemDropChance dropChance;
    public SpriteRenderer snowLayer;
    public SpriteRenderer cracksImage;
    //public DamagableObject damagableObject;

    public new void Start()
    {
        base.Start();
        damagableObject.rock = this;
    }
}
