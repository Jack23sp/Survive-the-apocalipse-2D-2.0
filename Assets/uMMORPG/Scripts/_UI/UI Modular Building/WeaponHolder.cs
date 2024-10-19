using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public static WeaponHolder singleton;

    public List<WeaponObject> weaponPresentation = new List<WeaponObject>();
    List<int> containInWeapon = new List<int>();

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    public void EnableRightWeapon(string weaponName)
    {
        for(int i = 0; i < weaponPresentation.Count; i++)
        {
            int index_i = i;
            weaponPresentation[index_i].gameObject.SetActive(weaponPresentation[index_i].gameObject.name == weaponName);
        }
    }
}
