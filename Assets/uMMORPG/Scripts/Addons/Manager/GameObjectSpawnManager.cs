using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSpawnManager : MonoBehaviour
{
    public static GameObjectSpawnManager singleton;
    [Header("Canvas")]
    public Transform canvas;
    public GameObject makeMarriage;
    public GameObject breakMarriage;
    public GameObject emojiToSpawn;
    public GameObject male;
    public GameObject female;
    [Header("Teleport")]
    public GameObject spawnedteleportInviter;
    public GameObject teleportInviter;
    public GameObject spawnedTeleport;
    public GameObject teleportInviteSlot;
    public GameObject rockObjectText;
    public GameObject treeObjectText;
    [Header("Selected Item")]
    public GameObject selectedItem;
    public GameObject spawnedSelectedItem;
    [Header("Confirm delete wall")]
    public GameObject confirmDeleteWall;
    [Header("Central manager")]
    public GameObject centralManager;
    [Header("Manager building accessory")]
    public GameObject confirmManagerAccessory;

    public void Start()
    {
        if (!singleton) singleton = this;
    }
}
