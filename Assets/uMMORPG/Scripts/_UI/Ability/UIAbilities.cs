using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIAbilities : MonoBehaviour
{
    public static UIAbilities singleton;
    private Player player;
    public Transform content;
    public GameObject gameObjectToSpawn;
    private int selectedAbilities = -1;
    public TextMeshProUGUI description;
    public Button UpgradeButton;
    private ScriptableAbility abilityTemplate;
    private string textToSet;

    public void Start()
    {
        if (!singleton) singleton = this;
        Spawn();
    }

    public void Spawn()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        UpgradeButton.onClick.RemoveAllListeners();
        UpgradeButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(11);
            player.playerAbility.CmdIncreaseAbility(selectedAbilities);
            SetAbilitiesDescription();
        });

        UIUtils.BalancePrefabs(gameObjectToSpawn, player.playerAbility.networkAbilities.Count, content);
        for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
        {
            int index = i;
            AbilitySlot slot = content.GetChild(index).GetComponent<AbilitySlot>();
            slot.statName.text = player.playerAbility.networkAbilities[index].name;
            slot.image.sprite = player.playerAbility.abilities[index].image;
            slot.statAmount.text = player.playerAbility.networkAbilities[index].level + " / " + player.playerAbility.networkAbilities[index].maxLevel;
            slot.button.gameObject.SetActive(true);
            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                selectedAbilities = index;
                SetAbilitiesDescription();
            });

        }
    }

    public void Reset()
    {
        description.text = string.Empty;
        UpgradeButton.gameObject.SetActive(false);
        selectedAbilities = -1;
        content.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void RefreshAbilities(bool setDescription)
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
        {
            int index = i;
            AbilitySlot slot = content.GetChild(index).GetComponent<AbilitySlot>();
            slot.statName.text = player.playerAbility.networkAbilities[index].name;
            slot.image.sprite = player.playerAbility.abilities[index].image;
            slot.statAmount.text = player.playerAbility.networkAbilities[index].level + " / " + player.playerAbility.networkAbilities[index].maxLevel;
            slot.button.gameObject.SetActive(true);
        }

        if(setDescription)
        {
            SetAbilitiesDescription();
        }
    }

    public void SetAbilitiesDescription()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        abilityTemplate = AbilityManager.singleton.FindAbility(player.playerAbility.networkAbilities[selectedAbilities].name);
        textToSet = string.Empty;
        if (selectedAbilities > -1)
        {
            textToSet = abilityTemplate.Description + "\n\n" + "Current ability level is : " + player.playerAbility.networkAbilities[selectedAbilities].level + "\n";
            textToSet = textToSet.Replace("{}", ((player.playerAbility.networkAbilities[selectedAbilities].level * abilityTemplate.bonus) + "%"));
            if (player.playerAbility.CanUpgradeAbilities(selectedAbilities))
            {
                textToSet += "\n\nTo upgrade to the next level you need : " + abilityTemplate.baseValue * (player.playerAbility.networkAbilities[selectedAbilities].level + 1) + " gold!";
            }
            else if (player.gold < abilityTemplate.baseValue * (player.playerAbility.networkAbilities[selectedAbilities].level + 1))
            {
                textToSet += "\n\nNot enough gold to upgrade this ability : " + "<b>" + abilityTemplate.baseValue * (player.playerAbility.networkAbilities[selectedAbilities].level + 1) + "</b> required!";
            }
            else if ((int)player.playerAbility.networkAbilities[selectedAbilities].level >= abilityTemplate.maxLevel)
            {
                textToSet += "\n\nCongratulations, you reach the maximum level for this ability!";
            }
            textToSet = textToSet.Replace("{BONUS}", abilityTemplate.bonus.ToString());
        }
        description.text = textToSet;
        UpgradeButton.gameObject.SetActive(selectedAbilities > -1 && player.playerAbility.CanUpgradeAbilities(selectedAbilities));
    }
}
