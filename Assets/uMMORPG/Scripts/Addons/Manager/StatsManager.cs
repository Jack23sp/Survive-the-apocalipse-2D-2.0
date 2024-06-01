using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class StatsManager : MonoBehaviour
{
    public static StatsManager singleton;
    public float openSize;
    public float closeSize;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void ManageStatSlot (UIStatsSlot statsSlot)
    {
        if (!Player.localPlayer) return;
        if(statsSlot.hungry)
        {
            statsSlot.image.sprite = ImageManager.singleton.hungryImage;
            statsSlot.statsName.text = "Hungry";
            statsSlot.description.text = "Describe how much are you need food!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.playerHungry.current.ToString();
            statsSlot.max.text = "100";
            statsSlot.slider.fillAmount = Player.localPlayer.playerHungry.current == 0 ? 0 : Player.localPlayer.playerHungry.current / 100;
        }
        else if (statsSlot.thirsty)
        {
            statsSlot.image.sprite = ImageManager.singleton.thirstyImage;
            statsSlot.statsName.text = "Thirsty";
            statsSlot.description.text = "Describe how much are you need drink!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.playerThirsty.current.ToString();
            statsSlot.max.text = "100";
            statsSlot.slider.fillAmount = Player.localPlayer.playerThirsty.current == 0 ? 0 : Player.localPlayer.playerThirsty.current / 100;
        }
        else if (statsSlot.armor)
        {
            statsSlot.image.sprite = ImageManager.singleton.armorImage;
            statsSlot.statsName.text = "Armor";
            statsSlot.description.text = "Your actual cover by armor!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.playerArmor.current.ToString();
            statsSlot.max.text = Player.localPlayer.playerArmor.GetMaxArmor().ToString();
            statsSlot.slider.fillAmount = Player.localPlayer.playerArmor.current == 0 ? 0 : Player.localPlayer.playerArmor.current / Player.localPlayer.playerArmor.GetMaxArmor();
        }
        else if (statsSlot.health)
        {
            statsSlot.image.sprite = ImageManager.singleton.healthImage;
            statsSlot.statsName.text = "Health";
            statsSlot.description.text = "Your actual condition!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.health.current.ToString();
            statsSlot.max.text = Player.localPlayer.health.max.ToString();
            statsSlot.slider.fillAmount = Player.localPlayer.health.current == 0 ? 0 : Player.localPlayer.health.current / Player.localPlayer.health.max;
        }
        else if (statsSlot.adrenaline)
        {
            statsSlot.image.sprite = ImageManager.singleton.adrenalineImage;
            statsSlot.statsName.text = "Adrenaline";
            statsSlot.description.text = "Your remaining stamina!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.mana.current.ToString();
            statsSlot.max.text = Player.localPlayer.mana.max.ToString();
            statsSlot.slider.fillAmount = Player.localPlayer.mana.current == 0 ? 0 : Player.localPlayer.mana.current / Player.localPlayer.mana.max;
        }
        else if (statsSlot.damage)
        {
            statsSlot.image.sprite = ImageManager.singleton.damageImage;
            statsSlot.statsName.text = "Damage";
            statsSlot.description.text = "The damage that you can do to other people";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = CalculateDamage().ToString();
            statsSlot.max.text = "";
            statsSlot.slider.fillAmount = 0;
        }
        else if (statsSlot.evasion)
        {
            statsSlot.image.sprite = ImageManager.singleton.evasionImage;
            statsSlot.statsName.text = "Dexterity";
            statsSlot.description.text = "Your actual possibilites of avoid melee attack!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.playerMiss._current.ToString();
            statsSlot.max.text = "100";
            statsSlot.slider.fillAmount = Player.localPlayer.playerMiss._current == 0 ? 0 : Player.localPlayer.playerMiss._current / 100;
        }
        else if (statsSlot.soldier)
        {
            statsSlot.image.sprite = ImageManager.singleton.soldierImage;
            statsSlot.statsName.text = "Soldier";
            statsSlot.description.text = "Your actual possibilities of resist attack!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = AbilityManager.singleton.FindNetworkAbilityLevel("Soldier", Player.localPlayer.name).ToString();
            statsSlot.max.text = "50";
            statsSlot.slider.fillAmount = AbilityManager.singleton.FindNetworkAbilityLevel("Soldier", Player.localPlayer.name) / 50;
        }
        else if (statsSlot.precision)
        {
            statsSlot.image.sprite = ImageManager.singleton.accuracyImage;
            statsSlot.statsName.text = "Precision";
            statsSlot.description.text = "Your actual possibilities of override dexterity ability of player!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.playerAccuracy._accuracy.ToString();
            statsSlot.max.text = "100";
            statsSlot.slider.fillAmount = Player.localPlayer.playerAccuracy._accuracy == 0 ? 0 : Player.localPlayer.playerAccuracy._accuracy / 100;
        }
        else if (statsSlot.speed)
        {
            statsSlot.image.sprite = ImageManager.singleton.speedImage;
            statsSlot.statsName.text = "Speed";
            statsSlot.description.text = "Your actual speed, can be increase doing level up or using boosts!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.GetSpeed().ToString();
            statsSlot.max.text = "";
            statsSlot.slider.fillAmount = 0;
        }
        else if (statsSlot.weight)
        {
            statsSlot.image.sprite = ImageManager.singleton.weightImage;
            statsSlot.statsName.text = "Weight";
            statsSlot.description.text = "Your actual weight,carrying to much weight slow you down, equip a backpack!";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.playerWeight.current.ToString();
            statsSlot.max.text = Player.localPlayer.playerWeight.max.ToString();
            statsSlot.slider.fillAmount = Player.localPlayer.playerWeight.current == 0 ? 0 : Player.localPlayer.playerWeight.current / Player.localPlayer.playerWeight.max;
        }
        else if (statsSlot.aimPrecision)
        {
            statsSlot.image.sprite = ImageManager.singleton.aimImage;
            statsSlot.statsName.text = "Aim Precision";
            statsSlot.description.text = "Your actual ability of aiming with fire weapon! (" + Player.localPlayer.playerShootPrecision.currentAimPrecision + ")";
            statsSlot.button.gameObject.SetActive(false);
            statsSlot.min.text = Player.localPlayer.playerShootPrecision.Calculate().ToString();
            statsSlot.max.text = "100";
            statsSlot.slider.fillAmount = Player.localPlayer.playerShootPrecision.currentAimPrecision  / 100;
        }
        else if (statsSlot.partner)
        {
            statsSlot.image.sprite = ImageManager.singleton.partnerImage;
            statsSlot.statsName.text = "Partner";
            statsSlot.description.text = Player.localPlayer.playerPartner.partnerName == String.Empty ? "You have no partner,make a bond with someone and protect it! If your partner will be in dangerous you stats will be increase at the moment!" : "You actual partner is : " + Player.localPlayer.playerPartner.partnerName + " when your partner is near you and has low health you acquire bonus stats!";
            statsSlot.min.text = "";
            statsSlot.max.text = "";
            statsSlot.slider.fillAmount = 0;
            statsSlot.button.gameObject.SetActive(Player.localPlayer.playerPartner._partner != null);
            statsSlot.button.onClick.RemoveAllListeners();
            statsSlot.button.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                Player.localPlayer.playerPartner.CmdRemovePartner();
            });
        }
    }

    public int FindIndexOfPartner()
    {
        if(UIStats.singleton)
        {
            for(int i = 0; i < UIStats.singleton.content.childCount; i++)
            {
                int index = i;
                UIStatsSlot slot = UIStats.singleton.content.GetChild(i).GetComponent<UIStatsSlot>();
                if(slot.partner)
                {
                    return index;
                }
            }
        }
        return -1;
    }


    public float CalculateDamage()
    {
        if (Player.localPlayer.playerEquipment.slots[0].amount == 0)
        {
            return 0f;
        }
        else
        {
            if(((WeaponItem)Player.localPlayer.playerEquipment.slots[0].item.data).requiredAmmo != null)
            {
                float d = ((MunitionSkill)((WeaponItem)Player.localPlayer.playerEquipment.slots[0].item.data).requiredSkill).damage.baseValue;
                return d;
            }
            else
            {
                float d = ((SlashDamagePlayerSkill)((WeaponItem)Player.localPlayer.playerEquipment.slots[0].item.data).requiredSkill).damage.baseValue;
                return d;
            }
        }
    }
}
