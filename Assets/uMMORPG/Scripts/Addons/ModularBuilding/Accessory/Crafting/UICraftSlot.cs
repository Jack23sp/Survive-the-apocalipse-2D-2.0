using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICraftSlot : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public GameObject itemAmountOverlay;
    public TextMeshProUGUI itemAmount;
    public Transform content;
    public TextMeshProUGUI gold;
    public Button goldButton;
    public TextMeshProUGUI coin;
    public Button coinButton;
    public Button claimButton;
    public TextMeshProUGUI timer;
    public RectTransform rectTransform;
    public Button panelButton;
    public GameObject scrollView;
}
