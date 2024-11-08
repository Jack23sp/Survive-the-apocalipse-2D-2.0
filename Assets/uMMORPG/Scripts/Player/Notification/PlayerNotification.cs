using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public partial class Player
{
    [HideInInspector] public PlayerNotification playerNotification;
}

public class PlayerNotification : NetworkBehaviour
{
    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
        player.playerNotification = this;
    }

    public void SpawnNotification(Sprite spriteImage, string message)
    {
        GameObject g = Instantiate(NotificationManager.singleton.notificationToSpawn, NotificationManager.singleton.contentToSpawn);
        NotificationSlot slot = g.GetComponent<NotificationSlot>();
        slot.contentText.text = message;
        slot.notificationImage.sprite = spriteImage;
        slot.notificationImage.preserveAspect = true;
    }

    [TargetRpc]
    public void TargetSpawnNotification(string message)
    {
        if (!player.isLocalPlayer) return;
        SpawnNotification(ImageManager.singleton.refuse, message);
    }

    [TargetRpc]
    public void TargetSpawnNotificationGeneral(string itemName, string message)
    {
        if (!player.isLocalPlayer) return;
        if (ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out ScriptableItem itemData))
        {
            SpawnNotification(itemData.image, message);
        }
    }


    [TargetRpc]
    public void TargetSpawnBookNotification(string bookTitle, string message)
    {
        if (!player.isLocalPlayer) return;
        if (ScriptableBook.dict.TryGetValue(bookTitle.GetStableHashCode(), out ScriptableBook itemData))
        {
            SpawnNotification(itemData.image, message);           
        }
    }

    [TargetRpc]
    public void TargetSpawnDumbbellNotification(string dumbbell, string message)
    {
        if (!player.isLocalPlayer) return;
        if (ScriptableDumbbell.dict.TryGetValue(dumbbell.GetStableHashCode(), out ScriptableDumbbell itemData))
        {
            SpawnNotification(itemData.image, message);
        }
    }

    [TargetRpc]
    public void TargetSpawnNotificationFullBodyExercise(string message)
    {
        if (!player.isLocalPlayer) return;
        SpawnNotification(ImageManager.singleton.upgrade, message);
    }

    [TargetRpc]
    public void TargetSpawnNotificationExperince(int amount)
    {
        if (!player.isLocalPlayer) return;
        float expereience = ResourceManager.singleton.AddExperienceAmount(player, amount);
        player.combat.ShowDamagePopup(expereience, DamageType.Exp);
    }
}
