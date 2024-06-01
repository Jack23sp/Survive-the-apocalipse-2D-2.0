using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    public Button slotButton;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI price;
    public Image itemImage;
    public string onlineShopCode;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI coinText;
    public Image coin;
    public Image gold;
    public int amount = 1;
}
