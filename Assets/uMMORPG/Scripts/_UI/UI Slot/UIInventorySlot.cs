﻿// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour
{
    public UIShowToolTip tooltip;
    public Button button;
    public UIDragAndDropable dragAndDropable;
    public Image image;
    public Image cooldownCircle;
    public GameObject amountOverlay;
    public Text amountText;
    public RegisterItemToSeeDetails registerItem;
    public Image durabilitySlider;
    public Image unsanitySlider;
    public Outline outline;
}
