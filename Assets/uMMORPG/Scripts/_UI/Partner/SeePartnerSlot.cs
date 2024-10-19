using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeePartnerSlot : MonoBehaviour
{
    public TextMeshProUGUI guildName;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI friendName;
    public TextMeshProUGUI level;
    public TextMeshProUGUI health;
    public TextMeshProUGUI stamina;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI armor;
    public TextMeshProUGUI dexterity;
    public TextMeshProUGUI partner;
    public GameObject guildSlot;
    public GroupSlot personalGroupSlot;
    public AbilitySlot abilitySlot;
    public Transform abilitiesContent;
    public Transform groupContent;
    public Button closeButton;
    public GameObject myGroup;
    public GameObject partnerPanel;
    public GameObject partnerNoPanel;
    public Button leftArrow;
    public Button rightArrow;
}
