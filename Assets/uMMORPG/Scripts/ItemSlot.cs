// Inventories need a slot type to hold Item + Amount. This is better than
// storing .amount in 'Item' because then we can use Item.Equals properly
// any workarounds to ignore the .amount.
//
// Note: always check .amount > 0 before accessing .item.
//       set .amount=0 to clear it.
using System;
using System.Text;
using UnityEngine;

[Serializable]
public partial struct ItemSlot
{
    public Item item;
    public int amount;

    // constructors
    public ItemSlot(Item item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    // helper functions to increase/decrease amount more easily
    // -> returns the amount that we were able to increase/decrease by
    public int DecreaseAmount(int reduceBy)
    {
        // as many as possible
        int limit = Mathf.Clamp(reduceBy, 0, amount);
        amount -= limit;
        return limit;
    }

    public int IncreaseAmount(int increaseBy)
    {
        // as many as possible
        int limit = Mathf.Clamp(increaseBy, 0, item.maxStack - amount);
        amount += limit;
        return limit;
    }

    // tooltip
    public string ToolTip()
    {
        if (amount == 0) return "";

        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(item.ToolTip());
        if (item.data is FoodItem)
        {
            tip.Replace("{FOODTOADD}", ((FoodItem)item.data).foodToAdd.ToString());
            tip.Replace("{DRINKTOADD}", ((FoodItem)item.data).waterToAdd.ToString());
            tip.Replace("{MAXUNSANITY}", ((FoodItem)item.data).maxUnsanity.ToString());
            tip.Replace("{MAXBLOOD}", ((FoodItem)item.data).maxBlood.ToString());
            tip.Replace("{PETCURE}", ((FoodItem)item.data).petHealthToAdd.ToString());
        }
        if (item.data is ScriptableItem)
        {
            tip.Replace("{MAXUNSANITY}", ((ScriptableItem)item.data).maxUnsanity.ToString());
        }
        if (item.data is EquipmentItem)
        {
            tip.Replace("{MAXBATTERYSTATUS}", ((EquipmentItem)item.data).battery.Get(item.batteryLevel).ToString());
            tip.Replace("{ADDITIONALSLOT}", ((EquipmentItem)item.data).additionalSlot.Get(item.bagLevel).ToString());
            tip.Replace("{CARRYWEIGHT}", ((EquipmentItem)item.data).maxWeight.Get(item.bagLevel).ToString());
            tip.Replace("{MAXDURABILITY}", ((EquipmentItem)item.data).maxDurability.Get(item.durabilityLevel).ToString());

            if (((EquipmentItem)item.data).armor.baseValue > 0)
            {
                tip.Replace("{MAXARMOR}", ((EquipmentItem)item.data).armor.Get(item.armorLevel).ToString());
                tip.Replace("{CURRENTARMOR}", item.currentArmor.ToString());
            }
            else
            {
                tip.Replace("{MAXARMOR}", "");
                tip.Replace("{CURRENTARMOR}", "");
            }
        }
        if (item.data is WaterBottleItem)
        {
            tip.Replace("{MAXWATER}", ((WaterBottleItem)item.data).maxWater.ToString());

        }
        if (item.data is PotionItem)
        {
            tip.Replace("{USAGEMANA}", ((PotionItem)item.data).usageMana.ToString());
            tip.Replace("{USAGEHEALTH}", ((PotionItem)item.data).usageHealth.ToString());
        }
        if (item.data is WeaponItem)
        {
            tip.Replace("{MAXMUNITION}", ((WeaponItem)item.data).maxMunition.ToString());
            if(((WeaponItem)item.data).weaponPrecision > 0)
            {
                tip.Replace("{PRECISION}", ((WeaponItem)item.data).weaponPrecision.ToString());
            }
        }

        if (item.data is ScriptableBook)
        {
            tip.Replace("{ABILITYNAME}", ((ScriptableBook)item.data).abilityType.name.ToString());
            tip.Replace("{AMOUNTINCREASE}", ((ScriptableBook)item.data).amountToIncreaseAbilityFromBook.ToString());
            tip.Replace("{TIMER}", ((ScriptableBook)item.data).timerIncreasePerPointAbility.ToString());
        }

        tip.Replace("{CURRENTBLOOD}", item.currentBlood.ToString());
        tip.Replace("{CURRENTWATER}", item.waterContainer.ToString());
        tip.Replace("{CURRENTBATTERYSTATUS}", item.torchCurrentBattery.ToString());
        tip.Replace("{CURRENTUNSANITY}", item.currentUnsanity.ToString());
        tip.Replace("{CURRENTMUNITON}", item.bulletsRemaining.ToString());
        tip.Replace("{CURRENTFUEL}", item.gasContainer.ToString());
        tip.Replace("{CURRENTDURABILITY}", item.currentDurability.ToString());
        return tip.ToString();
    }
}
