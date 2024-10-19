using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField] GameObject spriteObject;
    public bool isOverlayed;

    public void Manage(bool condition)
    {
        spriteObject.SetActive(condition);
    }
}
