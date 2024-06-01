using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionSlot : MonoBehaviour
{
    public Toggle toogle;
    public Button button;
    public TextMeshProUGUI onText;

    public void EnableOnObject(bool isActive)
    {
        toogle.isOn = isActive;
    }
}
