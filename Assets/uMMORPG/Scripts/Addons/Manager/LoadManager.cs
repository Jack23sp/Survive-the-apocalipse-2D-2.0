using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    public static LoadManager singleton;
    public UIShop UIShop;
    public UIFlag UIFlag;
    public BadgeCustom BadgeCustom;
    void Start()
    {
        if (!singleton) singleton = this;
        Invoke(nameof(SpawnItemMall), 1.0f);
        Invoke(nameof(SpawnFlag), 5.0f);
        Invoke(nameof(SpawnBadge), 8.0f);
    }

    public void SpawnItemMall()
    {
        if (!UIShop) return;
        UIShop.Spawn();
    }

    public void SpawnFlag()
    {
        if (!UIFlag) return;
        UIFlag.Spawn();
    }

    public void SpawnBadge()
    {
        if (!BadgeCustom) return;
        BadgeCustom.Spawn();
    }

}
