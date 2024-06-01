using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStatsSlot : MonoBehaviour
{
    public RectTransform rectTransform;
    public Button panelButton;
    public Image image;
    public TextMeshProUGUI statsName;
    public TextMeshProUGUI description;
    public Image slider;
    public TextMeshProUGUI min;
    public TextMeshProUGUI max;
    public Button button;
    public TextMeshProUGUI buttonText;
    public bool hungry;
    public bool thirsty;
    public bool armor;
    public bool health;
    public bool adrenaline;
    public bool damage;
    public bool evasion;
    public bool soldier;
    public bool precision;
    public bool speed;
    public bool weight;
    public bool aimPrecision;
    //public bool poisoned;
    //public bool blood;
    public bool partner;
}
