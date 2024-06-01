using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponObjectRenderer : MonoBehaviour
{
    public List<MeshRenderer> weaponObjects;
    public List<SkinnedMeshRenderer> skinnedWeaponObjects;

    public void SetLayer(string layerName)
    {
        for(int i = 0; i < weaponObjects.Count; i++)
        {
            weaponObjects[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }
        for (int i = 0; i < skinnedWeaponObjects.Count; i++)
        {
            skinnedWeaponObjects[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
}
