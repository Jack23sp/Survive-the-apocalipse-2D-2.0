using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIFillSlot : MonoBehaviour
{
    public int inventoryIndex;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI min;
    public TextMeshProUGUI max;
    public TextMeshProUGUI actual;
    public Image slider;
    public Button fillButton;
}
