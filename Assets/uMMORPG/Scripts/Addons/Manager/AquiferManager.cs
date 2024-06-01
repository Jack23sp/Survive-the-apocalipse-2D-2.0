using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AquiferManager : MonoBehaviour
{
    public static AquiferManager singleton;

    public List<Aquifer> aquifers = new List<Aquifer>();

    void Awake()
    {
        if (!singleton) singleton = this;
    }
}
