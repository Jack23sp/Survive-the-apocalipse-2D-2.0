using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageManager : MonoBehaviour
{
    public static ImageManager singleton;

    public Sprite hungryImage;
    public Sprite thirstyImage;
    public Sprite armorImage;
    public Sprite healthImage;
    public Sprite adrenalineImage;
    public Sprite damageImage;
    public Sprite defenseImage;
    public Sprite accuracyImage;
    public Sprite evasionImage;
    public Sprite critImage;
    public Sprite blockImage;
    public Sprite speedImage;
    public Sprite weightImage;
    public Sprite poisonedImage;
    public Sprite bloodImage;
    public Sprite partnerImage;
    public Sprite torchImage;
    public Sprite gold;
    public Sprite coin;
    public Sprite refuse;
    public Sprite friend;
    public Sprite message;
    public Sprite soldierImage;
    public Sprite aimImage;

    public Sprite waterImage;
    public Sprite upgrade;
    [Header("Warehouse")]
    public List<Sprite> warehouseOpen = new List<Sprite>();
    public List<Sprite> warehouseClosed = new List<Sprite>();
    void Start()
    {
        if (!singleton) singleton = this;    
    }
}
