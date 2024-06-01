using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RegisterItemToSeeDetails : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool delete, use, equip;
    public bool skillSlot,inventorySlot,equipmentSlot, warehouseSlot, fridgeSlot, librarySlot;

    public int index;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (index > -1)
        {
            UISelectedItem.singleton.use = use;
            UISelectedItem.singleton.delete = delete;
            UISelectedItem.singleton.equip = equip;
            UISelectedItem.singleton.skillSlot = skillSlot;
            UISelectedItem.singleton.inventorySlot = inventorySlot;
            UISelectedItem.singleton.equipmentSlot = equipmentSlot;
            UISelectedItem.singleton.warehouseSlot = warehouseSlot;
            UISelectedItem.singleton.librarySlot = librarySlot;
            UISelectedItem.singleton.fridgeSlot = fridgeSlot;
            UISelectedItem.singleton.index = index;
            UISelectedItem.singleton.CallInvokeToCheck();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

}
