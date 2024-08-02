using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TargetAttackChecker
{
    public string target;
    public ScriptableItem[] allowedWeapon;
}

public class AttackManager : MonoBehaviour
{
    public static AttackManager singleton;
    public List<TargetAttackChecker> checkers = new List<TargetAttackChecker>();
    public Sprite attackImage;
    public Sprite notAttackImage;
    public bool overrideControls;
    public bool isPC;

    void Start()
    {
        if (!singleton) singleton = this;
        isPC = !Application.isMobilePlatform;
    }

    public void CalculateDevice()
    {
        isPC = !Application.isMobilePlatform;
    }

    public bool CheckAttack(string weaponName, string targetName)
    {
        for(int i = 0; i < checkers.Count; i++)
        {
            if (checkers[i].target == targetName)
            {
                for(int e = 0;e < checkers[i].allowedWeapon.Length; e++)
                {
                    if (checkers[i].allowedWeapon[e].name == weaponName) return true;
                }
            }
        }

        return false;
    }
}
