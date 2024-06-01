using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RegistrablePrefabManager : NetworkBehaviour
{
    [SyncVar] public long lastPosition = 1;
    public static RegistrablePrefabManager singleton;
    public List<GameObject> toRegisterOnNetworkManager;
    public NetworkManagerMMO manager;

    void Awake()
    {
        if (!singleton) singleton = this;
        manager = FindObjectOfType<NetworkManagerMMO>();
    }

    void Start()
    {
        for (int i = 0; i < toRegisterOnNetworkManager.Count; i++)
        {
            int index = i;
            if (!manager.spawnPrefabs.Contains(toRegisterOnNetworkManager[index])) manager.spawnPrefabs.Add(toRegisterOnNetworkManager[index]);
        }
    }

    public GameObject FindObjectInSpawnablePrefab(string objectName)
    {
        for (int i = 0; i < manager.spawnPrefabs.Count; i++)
        {
            int index = i;
            if (manager.spawnPrefabs[index].name.Contains(objectName))
            {
                return manager.spawnPrefabs[index];
            }
        }
        return null;
    }
}
