using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeeFriendSlot : MonoBehaviour
{
    public TextMeshProUGUI guildName;
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
}
