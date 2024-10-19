using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCreationManager : MonoBehaviour
{
    public static CharacterCreationManager singleton;

    public EquipmentItem maleUp;
    public EquipmentItem maleDown;
    public EquipmentItem maleShoes;

    public EquipmentItem femaleUp;
    public EquipmentItem femaleDown;
    public EquipmentItem femaleShoes;


    void Start()
    {
        if (!singleton) singleton = this;
    }
}
