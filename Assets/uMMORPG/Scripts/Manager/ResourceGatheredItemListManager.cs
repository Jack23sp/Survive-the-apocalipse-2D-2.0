using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceGatheredList
{
    public string label;
    public List<ResourceItem> obligatory = new List<ResourceItem>();
    public List<ResourceItem> book = new List<ResourceItem>();
    public List<ResourceItem> toSpawn = new List<ResourceItem>();
    public List<ResourceItem> randomToSpawn = new List<ResourceItem>();
}

public class ResourceGatheredItemListManager : MonoBehaviour
{
    public static ResourceGatheredItemListManager singleton;
    public List<ResourceGatheredList> listGatheredResource = new List<ResourceGatheredList>();

    void Start()
    {
        if(!singleton) singleton = this;
    }
}
