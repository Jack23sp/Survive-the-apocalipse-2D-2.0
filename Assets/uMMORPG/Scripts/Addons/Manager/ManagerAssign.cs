using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ManagerAssign : MonoBehaviour
{
    public static ManagerAssign singleton;
    public Light2D temperatureManagerLight;
    public RainController temperatureManagerRain;
    public SnowController temperatureManagerSnow;
    public AudioSource temperatureManagerAudioSource;

    public GameObject modularBuildingPanelSkillbar;
    public UIModularBuilding UIModularBuilding;
    public AudioSource modularBuildingAudioSource;
    public GameObject playerCanvas;
    public PlayerTargetInvite playerTarget;

    public BoxCollider2D mapCollider;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void Assign(GameObject gm)
    {
        if (gm.GetComponent<TemperatureManager>())
        {
            TemperatureManager temperatureManager = gm.GetComponent<TemperatureManager>();
            if (temperatureManager != null)
            {
                temperatureManager.light2D = temperatureManagerLight;
                temperatureManager.rainObject = temperatureManagerRain;
                temperatureManager.snowObject = temperatureManagerSnow;
                temperatureManager.audioSource = temperatureManagerAudioSource;
            }
        }
        if (gm.GetComponent<ModularBuildingManager>())
        {
            ModularBuildingManager modularBuildingManager = gm.GetComponent<ModularBuildingManager>();
            if (modularBuildingManager != null)
            {
                modularBuildingManager.skillbarObject = modularBuildingPanelSkillbar;
                modularBuildingManager.uIModularBuilding = UIModularBuilding;
                modularBuildingManager.source = modularBuildingAudioSource;
                modularBuildingManager.playerCanvas = playerCanvas;
                modularBuildingManager.playerTargetInvite = playerTarget;
            }
        }
        if (gm.GetComponent<ResourceSpawnManager>())
        {
            ResourceSpawnManager resourceSpawnManager = gm.GetComponent<ResourceSpawnManager>();
            if (resourceSpawnManager != null)
            {
                //resourceSpawnManager.spawnArea = mapCollider;
            }
        }
        if (gm.GetComponent<MonsterSpawnManager>())
        {
            MonsterSpawnManager monsterSpawnManager = gm.GetComponent<MonsterSpawnManager>();
            if (monsterSpawnManager != null)
            {
                monsterSpawnManager.spawnArea = mapCollider;
            }
        }
        if (gm.GetComponent<BarrellSpawnManager>())
        {
            BarrellSpawnManager barellSpawnManager = gm.GetComponent<BarrellSpawnManager>();
            if (barellSpawnManager != null)
            {
                barellSpawnManager.spawnArea = mapCollider;
            }
        }
    }
}
