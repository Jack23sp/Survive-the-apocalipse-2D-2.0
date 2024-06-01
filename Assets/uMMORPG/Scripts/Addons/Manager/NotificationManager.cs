using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager singleton;
    public Transform contentToSpawn;
    public GameObject notificationToSpawn;

    void Start()
    {
        if (!singleton) singleton = this;   
    }

    public void ShowNotification(NetworkIdentity identity, string itemName, int skinIndex, string description)
    {
        if (ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out ScriptableItem itemData))
        {
            PlayerNotification notification = identity.GetComponent<PlayerNotification>();
            notification.SpawnNotification(itemData.skinImages.Count > 0 ? itemData.skinImages[skinIndex] : itemData.image, description);
        }
    }
}
