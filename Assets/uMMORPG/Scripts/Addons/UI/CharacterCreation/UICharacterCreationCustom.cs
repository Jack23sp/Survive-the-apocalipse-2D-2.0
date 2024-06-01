using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AdvancedPeopleSystem;
using Mirror;

public class UICharacterCreationCustom : MonoBehaviour
{
    public static UICharacterCreationCustom singleton;
    public NetworkManagerMMO manager; // singleton is null until update
    public UICharacterSelection UICharacterSelection;

    public GameObject panel;

    //public GameObject beardObject;
    public GameObject thinObject;
    public GameObject muscleObject;
    public GameObject breastObject;

    public GameObject slot;

    public Transform content;
    public List<Color> skinColor = new List<Color>();

    // hair and underwear
    public GameObject colorSelector;
    public UIColorPicker colorPicker;

    //public Transform hairContent;
    public Button hairButton;

    public Button eyesButton;
    public Button underwearButton;
    public Button beardButton;
    public Button skinButton;

    public Button maleButton;
    public Button femaleButton;
    public List<Button> listButton = new List<Button>();

    public Button cancel;
    public Button create;

    public Slider height;
    public Slider weight;
    public Slider thin;
    public Slider muscle;
    public Slider breast;

    public float maleHeight;
    public float maleWeight;
    public float maleThin;
    public float maleMuscle;
    public float maleBreast;

    public float femaleHeight;
    public float femaleWeight;
    public float femaleThin;
    public float femaleMuscle;
    public float femaleBreast;


    public string maleEyesColor;
    public string femaleEyesColor;
    public Color maleSkinColor;
    public Color femaleSkinColor;
    public string maleUnderwearColor;
    public string femaleUnderwearColor;
    public string maleHairColor;
    public string femaleHairColor;
    public int maleBeardtype;
    public int maleHairType;
    public int femaleHairType;

    public float Height;
    public float Weight;
    public float Thin;
    public float Muscle;
    public float Breast;
    public string EyesColor;
    public string SkinColor;
    public string UnderwearColor;
    public string HairColor;
    public int BeardType;
    public int HairType;

    public TMP_InputField nameInput;

    public int gender;

    public CharacterCustomization characterCustomization;

    public List<Sprite> maleHairPreset = new List<Sprite>();
    public List<Sprite> femaleHairPreset = new List<Sprite>();

    public List<Sprite> beardPreset = new List<Sprite>();

    public int whoOpenColorPicker = -1;
    public int up, down, shoes;


    void Start()
    {
        if (!singleton) singleton = this;

        ManageHair();
        ManageChoiceButton(null);

        maleButton.onClick.SetListener(() =>
        {
            gender = 0;
            colorSelector.SetActive(false);
            SetBodyBasedOnGender();
            ManageGenderButton(maleButton);
        });

        maleButton.onClick.Invoke();

        femaleButton.onClick.SetListener(() =>
        {
            gender = 1;
            colorSelector.SetActive(false);
            SetBodyBasedOnGender();
            ManageGenderButton(femaleButton);
        });

        hairButton.onClick.SetListener(() =>
        {
            colorSelector.SetActive(false);
            ManageHair();
            ManageChoiceButton(hairButton);
        });

        beardButton.onClick.SetListener(() =>
        {
            colorSelector.SetActive(false);
            ManageBeard();
            ManageChoiceButton(beardButton);
        });

        skinButton.onClick.SetListener(() =>
        {
            colorSelector.SetActive(false);
            ManageSkin();
            ManageChoiceButton(skinButton);
        });

        eyesButton.onClick.SetListener(() =>
        {
            UIUtils.BalancePrefabs(slot, 0, content);
            ManageChoiceButton(eyesButton);
            if (gender == 0)
                ManageColorPickerOpening(2);
            else
                ManageColorPickerOpening(3);
        });

        underwearButton.onClick.SetListener(() =>
        {
            colorSelector.SetActive(false);
            UIUtils.BalancePrefabs(slot, 0, content);
            ManageChoiceButton(underwearButton);
            if (gender == 0)
                ManageColorPickerOpening(4);
            else
                ManageColorPickerOpening(5);
        });


        height.onValueChanged.AddListener(delegate { ChangeBodySize(0); });
        weight.onValueChanged.AddListener(delegate { ChangeBodySize(1); });
        thin.onValueChanged.AddListener(delegate { ChangeBodySize(2); });
        muscle.onValueChanged.AddListener(delegate { ChangeBodySize(3); });
        breast.onValueChanged.AddListener(delegate { ChangeBodySize(4); });

        nameInput.onValueChanged.AddListener(delegate { CheckName(); });

        create.onClick.SetListener(() =>
        {
            AssignPlayerCharacteristicsToCreate();

            CharacterCreateMsg message = new CharacterCreateMsg
            {
                name = nameInput.text,
                classIndex = 0,
                gender = gender,
                heightValue = Height,
                fatValue = Weight,
                thinValue = Thin,
                muscleValue = Muscle,
                breastValue = Breast,
                eyesColor = EyesColor,
                skinColor = SkinColor,
                underwearColor = UnderwearColor,
                hairColor = HairColor,
                bearIndex = BeardType,
                hairIndex = HairType,
                accessory = -1,
                hats = -1,
                upper = up,
                down = down,
                shoes = shoes,
                bag = -1
            };
            NetworkClient.Send(message);
            ResetParameters();
            cancel.onClick.Invoke();
        });

        // cancel
        cancel.onClick.SetListener(() =>
        {
            nameInput.text = "";
            ResetParameters();
            ManageVisibility(false);
            colorSelector.SetActive(false);
            UICharacterSelection.panel.SetActive(true);

        });
    }

    public void HairBeardColorManagement()
    {
        if (gender == 0)
            ManageColorPickerOpening(0);
        else
            ManageColorPickerOpening(1);
    }

    public void ManageGenderButton(Button btn)
    {
        maleButton.image.enabled = btn == maleButton ? true : false;
        femaleButton.image.enabled = btn == femaleButton ? true : false;
    }

    public void ManageChoiceButton(Button btn)
    {
        foreach(Button choice in listButton)
        {
            choice.image.enabled = btn != null && btn == choice;
        }
    }

    public void ClearSlotColor()
    {
        for (int i = 0; i < content.childCount -1; i++)
        {
            int index = i;
            content.GetChild(index).gameObject.GetComponent<CharacterCreationSlot>().GetComponentInChildren<Image>().color = Color.white;
        }
    }

    public void ManageHair()
    {
        if (gender == 0)
        {
            UIUtils.BalancePrefabs(slot, maleHairPreset.Count, content);
            for (int i = 0; i < maleHairPreset.Count; i++)
            {
                int index = i;
                content.GetChild(index).gameObject.GetComponent<CharacterCreationSlot>().SetImageAndCallback(index - 1, maleHairPreset[index], gender, 0);
            }
            //beardObject.SetActive(true);
            breastObject.SetActive(false);
            muscleObject.SetActive(true);
            thinObject.SetActive(true);
        }
        else
        {
            UIUtils.BalancePrefabs(slot, femaleHairPreset.Count, content);
            for (int i = 0; i < femaleHairPreset.Count; i++)
            {
                int index = i;
                content.GetChild(index).gameObject.GetComponent<CharacterCreationSlot>().SetImageAndCallback(index - 1, femaleHairPreset[index], gender, 0);
            }

            //beardObject.SetActive(false);
            breastObject.SetActive(true);
            muscleObject.SetActive(false);
            thinObject.SetActive(false);
        }
        ClearSlotColor();
    }

    public void ManageSkin()
    {
        UIUtils.BalancePrefabs(slot, skinColor.Count, content);
        for (int i = 0; i < skinColor.Count; i++)
        {
            int index = i;
            content.GetChild(index).gameObject.GetComponent<CharacterCreationSlot>().SetSkinColor(index, skinColor[index], 2, gender);
        }
    }

    public void ManageBeard()
    {
        if (gender == 0)
        {
            beardButton.gameObject.SetActive(true);
            UIUtils.BalancePrefabs(slot, beardPreset.Count, content);
            for (int i = 0; i < beardPreset.Count; i++)
            {
                int index = i;
                content.GetChild(index).gameObject.GetComponent<CharacterCreationSlot>().SetImageAndCallback(index - 1, beardPreset[index], gender, 1);
            }
        }
        else
        {
            UIUtils.BalancePrefabs(slot, 0, content);
            beardButton.gameObject.SetActive(false);
        }
        ClearSlotColor();
    }

    public void ManageAttributes(int gender)
    {
        if(gender == 0)
        {
            characterCustomization.SwitchCharacterSettings(gender);
            height.value = maleHeight;
            weight.value = maleWeight;
            thin.value = maleThin;
            muscle.value = maleMuscle;
            breast.value = maleBreast;

            SetBodySize();
            ClearSlotColor();
            up = SearchIndexOnclothes(CharacterCreationManager.singleton.maleUp);
            down = SearchIndexOnclothes(CharacterCreationManager.singleton.maleDown);
            shoes = SearchIndexOnclothes(CharacterCreationManager.singleton.maleShoes);

            characterCustomization.SetElementByIndex(CharacterElementType.Shirt, up);
            characterCustomization.SetElementByIndex(CharacterElementType.Pants, down);
            characterCustomization.SetElementByIndex(CharacterElementType.Shoes, shoes);
        }
        else
        {
            characterCustomization.SwitchCharacterSettings(gender);
            height.value = femaleHeight;
            weight.value = femaleWeight;
            thin.value = femaleThin;
            muscle.value = femaleMuscle;
            breast.value = femaleBreast;

            SetBodySize();
            ClearSlotColor();

            up = SearchIndexOnclothes(CharacterCreationManager.singleton.femaleUp);
            down = SearchIndexOnclothes(CharacterCreationManager.singleton.femaleDown);
            shoes = SearchIndexOnclothes(CharacterCreationManager.singleton.femaleShoes);

            characterCustomization.SetElementByIndex(CharacterElementType.Shirt, up);
            characterCustomization.SetElementByIndex(CharacterElementType.Pants, down);
            characterCustomization.SetElementByIndex(CharacterElementType.Shoes, shoes);
        }
    }

    public void CheckName()
    {
        create.interactable = manager.IsAllowedCharacterName(nameInput.text);
    }



    public void ManageVisibility(bool vis)
    {
        panel.SetActive(vis);
    }

    public void AssignPlayerCharacteristicsToCreate()
    {
        if (gender == 0)
        {
            Height = maleHeight;
            Weight = maleWeight;
            Thin = maleThin;
            Muscle = maleMuscle;
            Breast = maleBreast;
            EyesColor = maleEyesColor;
            SkinColor = ColorUtility.ToHtmlStringRGBA(maleSkinColor);
            UnderwearColor = maleUnderwearColor;
            HairColor = maleHairColor;
            BeardType = maleBeardtype;
            HairType = maleHairType;
        }
        else if (gender == 1)
        {
            Height = femaleHeight;
            Weight = femaleWeight;
            Thin = femaleThin;
            Muscle = femaleMuscle;
            Breast = femaleBreast;
            EyesColor = femaleEyesColor;
            SkinColor = ColorUtility.ToHtmlStringRGBA(femaleSkinColor);
            UnderwearColor = femaleUnderwearColor;
            HairColor = femaleHairColor;
            BeardType = -1;
            HairType = femaleHairType;
        }
    }

    public void ResetParameters()
    {
        maleHeight = 0;
        maleWeight = 0;
        maleThin = 0;
        maleMuscle = 0;
        maleBreast = 0;
        maleEyesColor = "";
        //maleSkinColor = Color.clear;
        maleUnderwearColor = "";
        maleHairColor = "";
        maleBeardtype = -1;
        maleHairType = 0;

        femaleHeight = 0;
        femaleWeight = 0;
        femaleThin = 0;
        femaleMuscle = 0;
        femaleBreast = 0;
        femaleEyesColor = "";
        //femaleSkinColor = Color.clear;
        femaleUnderwearColor = "";
        femaleHairColor = "";
        femaleHairType = 0;

        Height = 0;
        Weight = 0;
        Thin = 0;
        Muscle = 0;
        Breast = 0;
        EyesColor = "";
        SkinColor = "";
        UnderwearColor = "";
        HairColor = "";
        BeardType = -1;
        HairType = 0;
    }

    public void SetBodySize()
    {
        ChangeBodySize(0);
        ChangeBodySize(1);
        ChangeBodySize(2);
        ChangeBodySize(3);
        ChangeBodySize(4);
    }

    public void ChangeBodySize(int index)
    {
        if (index == 0)
        {
            characterCustomization.SetHeight(height.value);
        }
        if (index == 1)
        {
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, weight.value);
        }
        if (index == 2)
        {
            if (gender == 0)
                characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, thin.value);
        }
        if (index == 3)
        {
            if (gender == 0)
                characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, muscle.value);
        }
        if (index == 4)
        {
            if (gender == 1)
                characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, breast.value);
        }

        if (gender == 0)
        {
            maleHeight = height.value;
            maleWeight = weight.value;
            maleThin = thin.value;
            maleMuscle = muscle.value;
            maleBreast = breast.value;

            characterCustomization.SetBodyColor(BodyColorPart.Skin, maleSkinColor);
            characterCustomization.SetElementByIndex(CharacterElementType.Hair, maleHairType);
            characterCustomization.SetElementByIndex(CharacterElementType.Beard, maleBeardtype);

            Color col;
            ColorUtility.TryParseHtmlString("#" + maleHairColor, out col);
            characterCustomization.SetBodyColor(BodyColorPart.Hair, col);
            ColorUtility.TryParseHtmlString("#" + maleEyesColor, out col);
            characterCustomization.SetBodyColor(BodyColorPart.Eye, col);
            ColorUtility.TryParseHtmlString("#" + maleUnderwearColor, out col);
            characterCustomization.SetBodyColor(BodyColorPart.Underpants, col);
        }
        else if (gender == 1)
        {
            femaleHeight = height.value;
            femaleWeight = weight.value;
            femaleThin = thin.value;
            femaleMuscle = muscle.value;
            femaleBreast = breast.value;

            characterCustomization.SetBodyColor(BodyColorPart.Skin, femaleSkinColor);
            characterCustomization.SetElementByIndex(CharacterElementType.Hair, femaleHairType);

            Color col;
            ColorUtility.TryParseHtmlString("#" + femaleHairColor, out col);
            characterCustomization.SetBodyColor(BodyColorPart.Hair, col);
            ColorUtility.TryParseHtmlString("#" + femaleEyesColor, out col);
            characterCustomization.SetBodyColor(BodyColorPart.Eye, col);
            ColorUtility.TryParseHtmlString("#" + femaleUnderwearColor, out col);
            characterCustomization.SetBodyColor(BodyColorPart.Underpants, col);
        }
    }

    public void Update()
    {
        if (whoOpenColorPicker > -1)
        {
            switch (whoOpenColorPicker)
            {
                case (0):
                    maleHairColor = ColorUtility.ToHtmlStringRGBA(colorPicker.pickedColor);
                    characterCustomization.SetBodyColor(BodyColorPart.Hair, colorPicker.pickedColor);
                    break;
                case (1):
                    femaleHairColor = ColorUtility.ToHtmlStringRGBA(colorPicker.pickedColor);
                    characterCustomization.SetBodyColor(BodyColorPart.Hair, colorPicker.pickedColor);
                    break;
                case (2):
                    maleEyesColor = ColorUtility.ToHtmlStringRGBA(colorPicker.pickedColor);
                    characterCustomization.SetBodyColor(BodyColorPart.Eye, colorPicker.pickedColor);
                    break;
                case (3):
                    femaleEyesColor = ColorUtility.ToHtmlStringRGBA(colorPicker.pickedColor);
                    characterCustomization.SetBodyColor(BodyColorPart.Eye, colorPicker.pickedColor);
                    break;
                case (4):
                    maleUnderwearColor = ColorUtility.ToHtmlStringRGBA(colorPicker.pickedColor);
                    characterCustomization.SetBodyColor(BodyColorPart.Underpants, colorPicker.pickedColor);
                    break;
                case (5):
                    femaleUnderwearColor = ColorUtility.ToHtmlStringRGBA(colorPicker.pickedColor);
                    characterCustomization.SetBodyColor(BodyColorPart.Underpants, colorPicker.pickedColor);
                    break;
            }
        }
    }

    public void ManageColorPickerOpening(int colorIndex)
    {
        var whoOpenColorPickerTemp = colorSelector.activeInHierarchy == true ? colorIndex : -1;
        if (colorSelector.activeInHierarchy && whoOpenColorPicker == whoOpenColorPickerTemp)
        {
            // Reset and close
            colorPicker.Reset();
            colorSelector.SetActive(false);
        }
        else if (colorSelector.activeInHierarchy && whoOpenColorPicker != whoOpenColorPickerTemp)
        {
            //Reset
            colorPicker.Reset();
        }
        else if (!colorSelector.activeInHierarchy)
        {
            colorSelector.SetActive(!colorSelector.activeSelf);
        }

        whoOpenColorPicker = colorSelector.activeInHierarchy == true ? colorIndex : -1;
    }

    public void SetBodyBasedOnGender()
    {
        whoOpenColorPicker = -1;
        colorSelector.SetActive(false);

        ManageHair();
        ManageBeard();
        ManageAttributes(gender);
    }

    public int SearchIndexOnclothes(EquipmentItem equipmentItem)
    {
        if (equipmentItem.indexShirt != -1) return equipmentItem.indexShirt;
        if (equipmentItem.indexPants != -1) return equipmentItem.indexPants;
        if (equipmentItem.indexShoes != -1) return equipmentItem.indexShoes;

        return -1;
    }

}
