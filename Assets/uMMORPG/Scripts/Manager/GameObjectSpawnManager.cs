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
    public GameObject fornitureAccessory;
    [Header("Map Markers")]
    public GameObject deathMarker;
    public GameObject pinMarker;
    public GameObject spawnpointMarker;
    [Header("Player Dummy")]
    public GameObject playerDummy;

    public void Start()
    {
        if (!singleton) singleton = this;
    }
}
