using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdvancedPeopleSystem;

public class CharacterCreationSlot : MonoBehaviour
{
    public int index;
    public Button button;
    public int gender;
    public int type;
    public Color skinColor;

    public void Start()
    {
        button.onClick.SetListener(() =>
        {
            if (gender == 0)
            {
                if (type == 0)
                {
                    UICharacterCreationCustom.singleton.characterCustomization.SetElementByIndex(CharacterElementType.Hair, index);
                    UICharacterCreationCustom.singleton.maleHairType = index;
                    UICharacterCreationCustom.singleton.HairBeardColorManagement();
                }
                if (type == 1)
                {
                    UICharacterCreationCustom.singleton.characterCustomization.SetElementByIndex(CharacterElementType.Beard, index);
                    UICharacterCreationCustom.singleton.maleBeardtype = index;
                    //UICharacterCreationCustom.singleton.HairBeardColorManagement();
                }
            }
            else
            {
                if (type == 0)
                {
                    UICharacterCreationCustom.singleton.characterCustomization.SetElementByIndex(CharacterElementType.Hair, index);
                    UICharacterCreationCustom.singleton.femaleHairType = index;
                    UICharacterCreationCustom.singleton.HairBeardColorManagement();
                }
            }

            if (type == 2)
            {
                if (gender == 0)
                {
                    UICharacterCreationCustom.singleton.characterCustomization.SetBodyColor(BodyColorPart.Skin, skinColor);
                    UICharacterCreationCustom.singleton.maleSkinColor = skinColor;
                }
                else
                {
                    UICharacterCreationCustom.singleton.characterCustomization.SetBodyColor(BodyColorPart.Skin, skinColor);
                    UICharacterCreationCustom.singleton.femaleSkinColor = skinColor;
                }
            }
        });
    }

    public void SetImageAndCallback(int pIndex, Sprite pImg, int pGender, int pType)
    {
        button.image.sprite = pImg;
        gender = pGender;
        type = pType;
        index = pIndex;
    }

    public void SetSkinColor(int pIndex, Color color, int pType, int pGender)
    {
        index = pIndex;
        skinColor = color;
        type = pType;
        button.image.color = skinColor;
        button.image.sprite = null;
        gender = pGender;
    }


}
