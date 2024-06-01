using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeaponPiece
{
    public ScriptableItem item;
    public MeshRenderer renderer;
    public bool isPreset;
}

public class WeaponObject : MonoBehaviour
{
    public List<WeaponPiece> weaponPieces = new List<WeaponPiece>();


    public void Manage(int accessoryType, string accessoryName, int skin)
    {
        for (int i = 0; i < weaponPieces.Count; i++)
        {
            int index_i = i;
            if (weaponPieces[index_i].item.name == accessoryName && Convert.ToInt32(weaponPieces[index_i].item.accessoriesType) == accessoryType)
            {
                weaponPieces[index_i].renderer.gameObject.SetActive(true);
                weaponPieces[index_i].renderer.material = SkinManager.singleton.weaponAccessoryMaterials[skin];
            }
            else if (weaponPieces[index_i].item.name != accessoryName && Convert.ToInt32(weaponPieces[index_i].item.accessoriesType) == accessoryType)
            {
                weaponPieces[index_i].renderer.gameObject.SetActive(false);
                weaponPieces[index_i].renderer.material = SkinManager.singleton.weaponAccessoryMaterials[0];
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < weaponPieces.Count; i++)
        {
            int index_i = i;
            if (weaponPieces[index_i].isPreset)
            {
                weaponPieces[index_i].renderer.gameObject.SetActive(true);
                weaponPieces[index_i].renderer.material = SkinManager.singleton.weaponAccessoryMaterials[0];
            }
            else
            {
                weaponPieces[index_i].renderer.gameObject.SetActive(false);
                weaponPieces[index_i].renderer.material = SkinManager.singleton.weaponAccessoryMaterials[0];
            }
        }

    }
}
