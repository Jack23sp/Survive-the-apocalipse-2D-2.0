using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial struct Item
{
    public bool CanAddWater()
    {
        return data is WaterBottleItem &&             
            waterContainer <=
            ((WaterBottleItem)data).maxWater &&
            waterContainer > 0;
    }

    public bool CanAddHoney()
    {
        return data is WaterBottleItem &&
            honeyContainer <=
            ((WaterBottleItem)data).maxWater &&
            honeyContainer > 0;
    }

    public bool CanAddGas()
    {
        return data is WaterBottleItem &&
            (waterContainer == 0 &&
            gasContainer == 0 &&
            honeyContainer == 0) ||
            (waterContainer == 0 &&
            honeyContainer == 0 &&
            gasContainer <
            ((WaterBottleItem)data).maxWater);
    }
}

[CreateAssetMenu(menuName = "uMMORPG Item/Water Bottle", order = 999)]

public partial class WaterBottleItem : UsableItem
{
    public int maxWater = 100;

}
