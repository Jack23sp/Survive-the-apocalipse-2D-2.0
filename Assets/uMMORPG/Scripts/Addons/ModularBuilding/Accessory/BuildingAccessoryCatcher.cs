using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;



public class BuildingAccessoryCatcher : MonoBehaviour
{
    public BuildingAccessory buildingAccessory;

    public BoxCollider2D thisCollider;

    public void OnTriggerStay2D(Collider2D collision)
    { 
        if (buildingAccessory.isClient)
        {
            if (collision.CompareTag("Accessory"))
            {
                if (collision.GetComponent<BuildingAccessory>().netIdentity.isClient)
                {
                    if (!buildingAccessory.accessoriesInThisForniture.Contains(collision.GetComponent<BuildingAccessory>()))
                        buildingAccessory.accessoriesInThisForniture.Add(collision.GetComponent<BuildingAccessory>());
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (buildingAccessory.isClient)
        {
            if (collision.CompareTag("Accessory"))
            {
                if (collision.GetComponent<BuildingAccessory>().netIdentity.isClient)
                {
                    if (buildingAccessory.accessoriesInThisForniture.Contains(collision.GetComponent<BuildingAccessory>()))
                        buildingAccessory.accessoriesInThisForniture.Remove(collision.GetComponent<BuildingAccessory>());
                }
            }
        }
    }
}
