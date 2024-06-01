using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerInformation : MonoBehaviour
{
    public static UIPlayerInformation singleton;
    public GameObject panel;
    public Image healthSlider;
    public TextMeshProUGUI healthStatus;
    public Image manaSlider;
    public TextMeshProUGUI manaStatus;
    public Image armorSlider;
    public TextMeshProUGUI armorStatus;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerLevel;
    public Image experienceSlider;

    [HideInInspector] public Player player;

    public void Start()
    {
        if (!singleton) singleton = this;
        Invoke(nameof(SearchPlayer), 0.1f);
    }

    public void SearchPlayer()
    {
        player = Player.localPlayer;
        if (!player) 
            Invoke(nameof(SearchPlayer), 0.1f);
        else
            Open();
    }

    public void Open()
    {
        player = Player.localPlayer;
        if (!player) return;
        panel.SetActive(true);

        experienceSlider.fillAmount = player.experience.Percent();
        armorSlider.fillAmount = player.playerArmor.Percent();
        armorStatus.text = "Armor : " + player.playerArmor.GetCurrentArmor() + " / " + player.playerArmor.GetMaxArmor();

        healthSlider.fillAmount = player.health.Percent();
        healthStatus.text = "Health : " + player.health.current + " / " + player.health.max;

        manaSlider.fillAmount = player.mana.Percent();
        manaStatus.text = "Stamina : " + player.mana.current + " / " + player.mana.max;
        playerName.text = player.name;

        playerLevel.text = player.level.current.ToString();
    }
}