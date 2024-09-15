using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantableSlot : MonoBehaviour
{
    public CuiltivableField cultivableField;
    public int indexPlant;
    public GameObject slot;
    public SpriteRenderer slotRenderer;
    public Transform slotTransform;

    public GameObject planted;
    public SpriteRenderer plantedRenderer;
    public Transform plantedTransform;

    public void Start()
    {
        if(!cultivableField)
        {
            cultivableField = transform.root.GetComponent<CuiltivableField>();
        }
    }
}
