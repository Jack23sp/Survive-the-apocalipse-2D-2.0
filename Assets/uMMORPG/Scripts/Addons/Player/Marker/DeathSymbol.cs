using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class DeathSymbol : MonoBehaviour
{
    [HideInInspector] public List<NetworkIdentity> symbols = new List<NetworkIdentity>();

    void Start()
    {
        Invoke(nameof(Check), 1.0f);
    }

    void Check()
    {
        int countNonNull = symbols.Count(n => n != null);
        if (countNonNull == 0)
            Destroy(this.gameObject);
        else
        {
            Invoke(nameof(Check), 1.0f);
        }
    }
}
