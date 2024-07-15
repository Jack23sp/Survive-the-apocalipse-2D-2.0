using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSpawnManager : MonoBehaviour
{
    public static GameObjectSpawnManager singleton;
    [Header("Canvas")]
    public Transform canvas;
    public GameObject male;
    public GameObject female;
    [Header("Teleport")]
    public GameObject spawnedteleportInviter;
    public GameObject teleportInviter;
    public GameObject spawnedTeleport;
    public GameObject teleportInviteSlot;
    [Header("Selected Item")]
    public GameObject selectedItem;
    public GameObject spawnedSelectedItem;
    [Header("Confirm delete wall")]
    public GameObject confirmDeleteWall;
    [Header("Manager building accessory")]
    public GameObject confirmManagerAccessory;
    [Header("Death Marker")]
    public GameObject deathMarker;

    public void Start()
    {
        if (!singleton) singleton = this;
    }
}
