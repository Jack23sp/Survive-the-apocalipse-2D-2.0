using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeaponPlaceholder
{
    public Vector3 pos;
    public Vector3 rot;
}

public class WeaponIK : MonoBehaviour
{
    public GameObject weaponObject;
    public WeaponPlaceholder idle;
    public WeaponPlaceholder run;
    public WeaponPlaceholder sneak;
    public WeaponPlaceholder walk;
    public WeaponPlaceholder aim;
    public WeaponPlaceholder shoot;
}
