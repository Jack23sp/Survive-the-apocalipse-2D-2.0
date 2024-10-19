using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Aquifer : NetworkBehaviour
{
    [SyncVar(hook = (nameof(ManageAquiferWater)))]
    public int actualWater;

    public int maxWater = 500;

    public void ManageAquiferWater(int oldValue, int newValue)
    {
        if (UIKitchenSink.singleton)
        {
            if (UIKitchenSink.singleton.aquifer == this)
            {
                UIKitchenSink.singleton.ChangeWater();
                UIKitchenSink.singleton.Open(UIKitchenSink.singleton.sink);
            }
        }
        if (UIBathroomSink.singleton)
        {
            if (UIBathroomSink.singleton.aquifer == this)
            {
                UIBathroomSink.singleton.ChangeWater();
                UIBathroomSink.singleton.Open(UIBathroomSink.singleton.sink);
            }
        }

    }
}
