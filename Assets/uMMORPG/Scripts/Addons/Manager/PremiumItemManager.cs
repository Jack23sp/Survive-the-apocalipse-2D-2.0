using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PremiumItemManager : MonoBehaviour
{
    public static PremiumItemManager singleton;
    public ScriptableItem Instantresurrect;
    public ScriptableItem instantResurrectOtherPlayer;
    public FoodItem petFood;

    void Start()
    {
        if (!singleton) singleton = this;    
    }

}
