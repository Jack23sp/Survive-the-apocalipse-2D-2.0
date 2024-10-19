using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorManager : MonoBehaviour
{
    public static FloorManager singleton;
    public List<Sprite> floorSprites = new List<Sprite>();

    void Start()
    {
        if(!singleton) singleton = this;
    }
}
