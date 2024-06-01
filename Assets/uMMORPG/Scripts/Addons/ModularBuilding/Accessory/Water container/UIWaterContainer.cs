using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

//public partial class Experience
//{
//    public void ManageExperience(long oldValue, long newValue)
//    {
//        if (UIWaterContainer.singleton)
//        {
//            UIWaterContainer.singleton.SetExperienceValue();
//        }
//        if (UIWaterContainer.singleton)
//        {
//            UIWaterContainer.singleton.SetExperienceValue();
//        }
//    }
//}

//public partial class Energy
//{
//    public void ManageEnergy(int oldValue, int newValue)
//    {
//        if (UIWaterContainer.singleton)
//        {
//            UIWaterContainer.singleton.SetManaValue();
//            UIWaterContainer.singleton.SetHealthValue();
//        }
//        if (UIWaterContainer.singleton)
//        {
//            UIWaterContainer.singleton.SetManaValue();
//            UIWaterContainer.singleton.SetHealthValue();
//        }
//    }
//}

public class UIWaterContainer : MonoBehaviour
{
    public static UIWaterContainer singleton;
    public GameObject panel;
    public WaterContainer waterCont;

    public Button closeButton;
    public Button manageButton;

    public Button drinkButton;
    public Button fillButton;
    public GameObject drinkImage;
    public GameObject fillImage;
    public GameObject panelDrink;
    public GameObject panelFill;

    public TextMeshProUGUI minValue;
    public TextMeshProUGUI maxValue;
    public Slider slider;
    public TextMeshProUGUI sliderValue;

    public Button actionButton;

    public Transform content;
    public GameObject drinkObjectToSpawn;

    public RawImage image;
    public TextMeshProUGUI level;
    public UIDrinkSlot armor;
    public UIDrinkSlot health;
    public UIDrinkSlot mana;
    public UIDrinkSlot experience;
    public UIDrinkSlot thirsty;
    public UIDrinkSlot hungry;
    public UIDrinkSlot miss;
    public UIDrinkSlot accuracy;
    public UIDrinkSlot weight;

    public TextMeshProUGUI water;

    public bool drink;
    public List<int> waterContainerList = new List<int>();

    void Start()
    {
        if (!singleton) singleton = this;


    }
    public void Update()
    {
        if (panel.activeInHierarchy)
        {
            image.texture = ((PlayerEquipment)Player.localPlayer.equipment).avatarCamera.targetTexture;
        }
    }
    public void SpawnBottleObject()
    {
        UIUtils.BalancePrefabs(drinkObjectToSpawn, waterContainerList.Count, content);
        for(int i = 0; i < waterContainerList.Count; i++)
        {
            int index = i;
            UIFillSlot fillSlot = content.GetChild(index).GetComponent<UIFillSlot>();
            fillSlot.actual.text = Player.localPlayer.inventory.slots[waterContainerList[index]].item.waterContainer.ToString();
            fillSlot.fillButton.interactable = Player.localPlayer.inventory.slots[waterContainerList[index]].item.waterContainer < ((WaterBottleItem)Player.localPlayer.inventory.slots[waterContainerList[index]].item.data).maxWater;
            fillSlot.min.text = "0";
            fillSlot.max.text = ((WaterBottleItem)Player.localPlayer.inventory.slots[waterContainerList[index]].item.data).maxWater.ToString();
            fillSlot.itemImage.sprite = ((WaterBottleItem)Player.localPlayer.inventory.slots[waterContainerList[index]].item.data).image;
            fillSlot.itemName.text = ((WaterBottleItem)Player.localPlayer.inventory.slots[waterContainerList[index]].item.data).name;
            fillSlot.fillButton.onClick.RemoveAllListeners();
            fillSlot.fillButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton)
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(6);
                Player.localPlayer.playerThirsty.CmdFillBottleFromContainer(waterContainerList[index], waterCont.netIdentity);
            });
        }
        ChangeWater();
    }

    public void ChangeWater()
    {
        if (waterCont) water.text = "Water : \n " + waterCont.water.ToString() + " l.";
    }

    public void SearchWaterBottle()
    {
        if(Player.localPlayer)
        {
            waterContainerList.Clear();
            for(int i= 0; i < Player.localPlayer.inventory.slots.Count; i++)
            {
                int index = i;
                if (Player.localPlayer.inventory.slots[index].amount > 0)
                {
                    if (Player.localPlayer.inventory.slots[index].item.data is WaterBottleItem)
                    {
                        if (Player.localPlayer.inventory.slots[index].item.CanAddWater())
                        {
                            if (!waterContainerList.Contains(index)) waterContainerList.Add(index);
                        }
                    }
                }
            }
            SpawnBottleObject();
        }
    }

    public void ManageSlider()
    {
        if (slider.value > 0) actionButton.interactable = true;
        else actionButton.interactable = false;
        sliderValue.text = slider.value.ToString();
        if ((slider.value > (100.0f - (float)Player.localPlayer.playerThirsty.current)) || (slider.value > (waterCont.water))) maxValue.color = Color.red;
        else maxValue.color = Color.white;
    }

    public void Open(WaterContainer waterContainer)
    {
        if (Player.localPlayer)
        {
            waterCont = waterContainer;

            drinkImage.SetActive(true);
            fillImage.SetActive(false);
            content.gameObject.SetActive(false);
            panelDrink.SetActive(true);
            slider.gameObject.SetActive(true);
            minValue.gameObject.SetActive(true);
            maxValue.gameObject.SetActive(true);
            actionButton.gameObject.SetActive(true);
            panel.SetActive(true);
            waterContainerList.Clear();

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton)
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
                slider.value = 0.0f;
                panel.SetActive(false);
                closeButton.image.raycastTarget = false;
                closeButton.image.enabled = false;
                drinkImage.SetActive(true);
                fillImage.SetActive(false);
                content.gameObject.SetActive(false);
                panelDrink.SetActive(true);
                slider.gameObject.SetActive(true);
                minValue.gameObject.SetActive(true);
                maxValue.gameObject.SetActive(true);
                actionButton.gameObject.SetActive(true);
                waterContainerList.Clear();
                BlurManager.singleton.Show();

            });

            drinkButton.onClick.RemoveAllListeners();
            drinkButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton)
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(5);
                drinkImage.SetActive(true);
                fillImage.SetActive(false);
                content.gameObject.SetActive(false);
                panelDrink.SetActive(true);
                panelFill.SetActive(false);
                slider.gameObject.SetActive(true);
                minValue.gameObject.SetActive(true);
                maxValue.gameObject.SetActive(true);
                actionButton.gameObject.SetActive(true);
                Open(waterCont);
                waterContainerList.Clear();
            });

            fillButton.onClick.RemoveAllListeners();
            fillButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton)
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(6);
                drinkImage.SetActive(false);
                fillImage.SetActive(true);
                content.gameObject.SetActive(true);
                panelDrink.SetActive(false);
                panelFill.SetActive(true);
                slider.gameObject.SetActive(false);
                minValue.gameObject.SetActive(false);
                maxValue.gameObject.SetActive(false);
                actionButton.gameObject.SetActive(false);
                SearchWaterBottle();
            });

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton)
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                if (slider.value > 0 && waterCont)
                {
                    Player.localPlayer.playerThirsty.CmdAddWaterFromContainer(Convert.ToInt32(slider.value), waterCont.netIdentity);
                }
            });

            manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(waterCont, Player.localPlayer));
            manageButton.onClick.RemoveAllListeners();
            manageButton.onClick.AddListener(() =>
            {
                GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
                g.GetComponent<UIBuildingAccessoryManager>().Init(waterCont.netIdentity, waterCont.craftingAccessoryItem, closeButton);
            });

            //drinkButton.onClick.Invoke();

            closeButton.image.raycastTarget = true;
            closeButton.image.enabled = true;
            Refresh();

            SetLevel();
            SetArmorValue();
            SetHealthValue();
            SetManaValue();
            SetThirstyValue();
            SetHungryValue();
            SetMissValue();
            SetAccuracyValue();
            SetWeightValue();
            SetExperienceValue();
            ((PlayerEquipment)Player.localPlayer.equipment).avatarCamera.enabled = true;
            ChangeWater();
        }
    }

    public void Refresh()
    {
        slider.minValue = 0.0f;
        slider.maxValue = (100.0f - (float)Player.localPlayer.playerThirsty.current);
        slider.value = 0.0f;
        actionButton.interactable = false;
        //panel.SetActive(true);
        closeButton.image.raycastTarget = true;
        minValue.text = "0";
        maxValue.text = slider.maxValue.ToString();
        sliderValue.text = "0";
        ChangeWater();
    }

    public void SetLevel()
    {
        level.text = Player.localPlayer.level.current.ToString();
    }

    public void SetExperienceValue()
    {
        if (Player.localPlayer)
        {
            experience.title.text = "Experience";
            experience.min.text = Player.localPlayer.experience.current.ToString();
            experience.max.text = Player.localPlayer.experience.max.ToString();
            experience.slider.fillAmount = (Player.localPlayer.experience.current != 0 && Player.localPlayer.experience.max != 0) ? (float)Player.localPlayer.experience.current / (float)Player.localPlayer.experience.max : 0;
        }
    }

    public void SetArmorValue()
    {
        if (Player.localPlayer)
        {

            armor.title.text = "Armor";
            armor.min.text = Player.localPlayer.playerArmor.GetCurrentArmor().ToString();
            armor.max.text = Player.localPlayer.playerArmor.GetMaxArmor().ToString();
            armor.slider.fillAmount = (Player.localPlayer.playerArmor.GetCurrentArmor() != 0 && Player.localPlayer.playerArmor.GetMaxArmor() != 0) ? (float)Player.localPlayer.playerArmor.GetCurrentArmor() / (float)Player.localPlayer.playerArmor.GetMaxArmor() : 0;
        }
    }

    public void SetHealthValue()
    {
        if (Player.localPlayer)
        {

            health.title.text = "Health";
            health.min.text = Player.localPlayer.health.current.ToString();
            health.max.text = Player.localPlayer.health.max.ToString();
            health.slider.fillAmount = (Player.localPlayer.health.current != 0 && Player.localPlayer.health.max != 0) ? (float)Player.localPlayer.health.current / (float)Player.localPlayer.health.max : 0;
        }
    }

    public void SetManaValue()
    {
        if (Player.localPlayer)
        {

            mana.title.text = "Mana";
            mana.min.text = Player.localPlayer.mana.current.ToString();
            mana.max.text = Player.localPlayer.mana.max.ToString();
            mana.slider.fillAmount = (Player.localPlayer.mana.current != 0 && Player.localPlayer.mana.max != 0) ? (float)Player.localPlayer.mana.current / (float)Player.localPlayer.mana.max : 0;
        }
    }

    public void SetThirstyValue()
    {
        if (Player.localPlayer)
        {

            thirsty.title.text = "Thirsty";
            thirsty.min.text = Player.localPlayer.playerThirsty.current.ToString();
            thirsty.max.text = Player.localPlayer.playerThirsty.max.ToString();
            thirsty.slider.fillAmount = (Player.localPlayer.playerThirsty.current != 0 && Player.localPlayer.playerThirsty.max != 0) ? (float)Player.localPlayer.playerThirsty.current / (float)Player.localPlayer.playerThirsty.max : 0;
        }
    }

    public void SetHungryValue()
    {
        if (Player.localPlayer)
        {

            hungry.title.text = "Hungry";
            hungry.min.text = Player.localPlayer.playerHungry.current.ToString();
            hungry.max.text = Player.localPlayer.playerHungry.max.ToString();
            hungry.slider.fillAmount = (Player.localPlayer.playerHungry.current != 0 && Player.localPlayer.playerHungry.max != 0) ? (float)Player.localPlayer.playerHungry.current / (float)Player.localPlayer.playerHungry.max : 0;
        }
    }

    public void SetMissValue()
    {
        if (Player.localPlayer)
        {

            miss.title.text = "Miss";
            miss.min.text = Player.localPlayer.playerMiss._current.ToString();
            miss.max.text = "100";
            miss.slider.fillAmount = (Player.localPlayer.playerMiss._current != 0 && 100 != 0) ? (float)Player.localPlayer.playerMiss._current / (float)100 : 0;
        }
    }

    public void SetAccuracyValue()
    {
        if (Player.localPlayer)
        {

            accuracy.title.text = "Accuracy";
            accuracy.min.text = Player.localPlayer.playerAccuracy._accuracy.ToString();
            accuracy.max.text = "100";
            accuracy.slider.fillAmount = (Player.localPlayer.playerAccuracy._accuracy != 0 && 100 != 0) ? (float)Player.localPlayer.playerAccuracy._accuracy / (float)100 : 0;
        }
    }

    public void SetWeightValue()
    {
        if (Player.localPlayer)
        {

            weight.title.text = "Weight";
            weight.min.text = Player.localPlayer.playerWeight.current.ToString();
            weight.max.text = Player.localPlayer.playerWeight.max.ToString();
            weight.slider.fillAmount = (Player.localPlayer.playerWeight.current != 0 && Player.localPlayer.playerWeight.max != 0) ? (float)Player.localPlayer.playerWeight.current / (float)Player.localPlayer.playerWeight.max : 0;
        }
    }
}
