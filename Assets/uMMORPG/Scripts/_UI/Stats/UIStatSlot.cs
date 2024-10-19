using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIStatSlot : MonoBehaviour
{
    //public list type
    public bool hungry;
    public bool thirsty;
    public bool torch;
    public bool bag;
    public bool coin;
    public bool gold;

    public Image image;
    public int intAmount;
    public TextMeshProUGUI amount;
    public Button button;

    public void SpawnRestAmount(int amountToType)
    {
        GameObject g = Instantiate(UIFrontStats.singleton.toSpawn, GameObjectSpawnManager.singleton.canvas);
        g.transform.position = amount.transform.position;
        TextMeshProUGUI t = g.GetComponent<TextMeshProUGUI>();
        t.color = amountToType > 0 ? Color.green : Color.red;
        t.text = amountToType.ToString();
    }
}
