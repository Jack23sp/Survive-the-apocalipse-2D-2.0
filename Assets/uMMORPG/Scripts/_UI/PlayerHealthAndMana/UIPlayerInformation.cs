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
    public Image tiredSlider;

    [HideInInspector] public Player player;

    public Color goodColor;
    public int goodLimit = 70;
    public Color mediumColor;
    public int mediumLimit = 30;
    public Color poorColor;

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

    public void Tired()
    {
        if (!Player.localPlayer.playerTired) Player.localPlayer.GetComponent<PlayerTired>().Assign();
        tiredSlider.fillAmount = (float)Player.localPlayer.playerTired.tired / Player.localPlayer.playerTired.maxTiredness;
        if (Player.localPlayer.playerTired.tired >= goodLimit)
            tiredSlider.color = goodColor;
        else if (Player.localPlayer.playerTired.tired >= mediumLimit)
            tiredSlider.color = mediumColor;
        else
            tiredSlider.color = poorColor;
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