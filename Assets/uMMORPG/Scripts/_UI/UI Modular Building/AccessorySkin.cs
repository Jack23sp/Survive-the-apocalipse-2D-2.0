using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccessorySkin : MonoBehaviour
{
    public List<Shadow> shadows = new List<Shadow>();
    public Button button;

    public void ManageShadow(bool condition)
    {
        foreach (Shadow shadow in shadows)
        {
            shadow.enabled = condition;
        }
    }
}
