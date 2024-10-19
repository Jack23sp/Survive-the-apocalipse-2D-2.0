using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerArmor playerArmor;
}

public partial struct Item
{
    public int currentArmor;
    public int armorLevel;
}

public class PlayerArmor : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = (nameof(ManageCurrentArmor)))]
    public int current;
    [SyncVar(hook = (nameof(ManageMaxArmor)))]
    public int max;

    public void ManageCurrentArmor(int oldValue, int maxValue)
    {
        if (oldValue == maxValue) return;
        if (UIKitchenSink.singleton) UIKitchenSink.singleton.SetArmorValue();
        if (UIBathroomSink.singleton) UIBathroomSink.singleton.SetArmorValue();
        if (UIWaterContainer.singleton) UIWaterContainer.singleton.SetArmorValue();
        GetCurrentArmor();
        GetMaxArmor();
        if (UIPlayerInformation.singleton) UIPlayerInformation.singleton.Open();
        //if (UIHealthMana.singleton) UIHealthMana.singleton.armorSlider.value = ArmorPercent();
        //if (UIHealthMana.singleton) UIHealthMana.singleton.armorStatus.text = currentArmor + " / " + maxArmor;

    }

    public void ManageMaxArmor(int oldValue, int maxValue)
    {
        if (oldValue == maxValue) return;
        if (UIKitchenSink.singleton) UIKitchenSink.singleton.SetArmorValue();
        if (UIBathroomSink.singleton) UIBathroomSink.singleton.SetArmorValue();
        if (UIWaterContainer.singleton) UIWaterContainer.singleton.SetArmorValue();
        GetCurrentArmor();
        GetMaxArmor();
        if (UIPlayerInformation.singleton) UIPlayerInformation.singleton.Open();
    }

    public float Percent()
    {
        return (current != 0 && max != 0) ? (float)current / (float)max : 0;
    }

    public int GetCurrentArmor()
    {
            int equipmentBonus = 0;
            foreach (ItemSlot slot in player.equipment.slots)
                if (slot.amount > 0)
                    equipmentBonus += slot.item.currentArmor;

            if (isServer && current != equipmentBonus) current = equipmentBonus;
            return equipmentBonus;
    }
    public int GetMaxArmor()
    {
            int equipmentBonus = 0;
            foreach (ItemSlot slot in player.equipment.slots)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).armor.Get(slot.item.armorLevel);

            if (isServer && max != equipmentBonus) max = equipmentBonus;
            return equipmentBonus;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        GetCurrentArmor();
        GetMaxArmor();
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerArmor = this;
    }

    public float ArmorPercent()
    {
        return (GetCurrentArmor() != 0 && GetMaxArmor() != 0) ? (float)GetCurrentArmor() / (float)GetMaxArmor() : 0;
    }



}
