using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeManager : MonoBehaviour
{
    public static BadgeManager singleton;
    public List<Sprite> background = new List<Sprite>();
    public List<Sprite> foreground = new List<Sprite>();

    void Start()
    {
        if (!singleton) singleton = this;        
    }

}
