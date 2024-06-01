using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeaponAccessories
{
    public WeaponItem mainWeapon;
    public List<WeaponItem> weaponItemAccessories;
}

public class SkinManager : MonoBehaviour
{
    public static SkinManager singleton;
    public List<Sprite> skins = new List<Sprite>();
    public List<Material> weaponAccessoryMaterials = new List<Material>();
    public int costToBuySkin;
    public bool gold;

    public List<WeaponItem> commonItems;
    public List<WeaponAccessories> WeaponAccessories;

    void Start()
    {
        if (!singleton) singleton = this;    
    }
}
