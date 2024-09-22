using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;

public class ModularBuildingManager : MonoBehaviour
{
    public static ModularBuildingManager singleton;

    public GameObject skillbarObject;

    public GameObject spawnedBuilding;
    public GameObject spawnedWall;
    public GameObject spawnedAccesssory;
    public ScriptableBuilding scriptableBuilding;
    public ScriptableWall scriptableWall;
    public ScriptableDoor scriptableDoor;
    public ScriptableFence scriptableFence;
    public ScriptableGate scriptableGate;
    public ScriptableItem scriptableExternal;
    public ScriptableBuildingAccessory scriptableBuildingAccessory;
    public float sensibility = 0.1f;
    public int objectIndex = -1;
    public UIModularBuilding uIModularBuilding;
    public int inventoryIndex = -1;
    public bool isInventory;

    public List<ModularBuilding> combinedModulars = new List<ModularBuilding>();
    public List<BuildingAccessory> buildingAccessories = new List<BuildingAccessory>();
    public List<CraftAccessory> craftAccessories = new List<CraftAccessory>();
    public List<BathroomSink> bathroomSinks = new List<BathroomSink>();
    public List<KitchenSink> kitchenSinks = new List<KitchenSink>();
    public List<Billboard> billboards = new List<Billboard>();
    public List<Cabinet> cabinets = new List<Cabinet>();
    public List<Concrete> concretes = new List<Concrete>();
    public List<Flag> flags = new List<Flag>();
    public List<Fridge> fridges = new List<Fridge>();
    public List<Furnace> furnaces = new List<Furnace>();
    public List<Lamp> lamps = new List<Lamp>();
    public List<Upgrade> upgrades = new List<Upgrade>();
    public List<Warehouse> warehouses = new List<Warehouse>();
    public List<Library> libraries = new List<Library>();
    public List<WaterContainer> waterContainers = new List<WaterContainer>();
    public List<WeaponStorage> weaponStorages = new List<WeaponStorage>();
    public List<Fence> fences = new List<Fence>();
    public List<Gate> gates = new List<Gate>();
    public List<Aquarium> aquarium = new List<Aquarium>();
    public List<Tree> trees = new List<Tree>();
    public List<CuiltivableField> cultivableFields = new List<CuiltivableField>();
    public ModularBuilding[] modularBuildings;

    public bool activeBuildingModeDoor;
    public bool activeBuildingModeWall;
    public bool activeBasementPositioning;
    public bool activeFencePositioning;
    public bool activeGatePositioning;
    [HideInInspector] public int currentSelectedWall;
    [HideInInspector] public int selectedType;
    public ModularBuilding selectedModularBuilding;
    public PlacementBase prevPlacementBase;

    public GameObject selectedWall;

    public LayerMask basementLayerMask;
    public LayerMask treeLayerMask;
    public LayerMask externalLayerMask;
    public LayerMask grassLayerMask;
    public LayerMask spawnpointLayerMask;
    public LayerMask accessoryLayerToDestroyWithFloor;
    public LayerMask buildingPlacementLayerMask;
    public LayerMask skillBuildingRaycastCheckLayerMask;
    public LayerMask ambientSlashLayerMask;

    public bool isSpawnBuilding;


    [Header("---------------Sounds--------------------")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioSource source;
    [Header("---------------Color--------------------")]
    public Color closeColor = Color.red;
    public Color openColor = Color.green;
    [Header("---------------Accessory Shader--------------------")]
    public Material notBuildAccessoryMaterial;
    public Material buildAccessoryMaterial;
    public Material spawnedBuildAccessoryMaterial;
    public Material claimBuildingMaterial;
    public Material objectPresent;
    public Material objectNotPresent;
    public Material plantedCompleted;
    public Material plantedNotCompleted;
    [Header("---------------Aquifer--------------------")]
    public List<Aquifer> aquifers;

    [Header("---------------Player options--------------------")]
    public GameObject playerCanvas;

    private GameObject objectToMove;

    public PlayerTargetInvite playerTargetInvite;
    public PlayerPersonalOptions playerPersonalCanvas;

    public BuildingAccessory buildingAccessory;

    ResourceGathered resource;

    private double basementClick;
    [SerializeField] double basementClickThroubleShot = 0.5f;

    public int healthToAddToWalls = 100;

    public List<ScriptableBuildingAccessory> allScriptableAccessory = new List<ScriptableBuildingAccessory>();

    public string buildingAreaRestrainSpawnArea = "You are too close a player spawn area to build";
    public string buildingConstruction = "You cannot build because of some obstacles or out of perimeter of construction";

    private Transform roof, doorOptions, wallOptions, accessory, plantable;

    [Header("---------------UI to close on death--------------------")]
    public List<MonoBehaviour> UIToCloseOnDeath = new List<MonoBehaviour>();
    public List<MonoBehaviour> UIToCloseOnDeathNoBuilding = new List<MonoBehaviour>();


    void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void ClearAllCache()
    {
        if (spawnedAccesssory) Destroy(spawnedAccesssory);
        if (spawnedWall) Destroy(spawnedWall);
        if (spawnedBuilding) Destroy(spawnedBuilding);
    }


    public void ManageSound(bool doorSound)
    {
        if (source)
        {
            source.clip = doorSound == false ? closeSound : openSound;
            if (Player.localPlayer && Player.localPlayer.playerOptions.blockSound == false)
            {
                source.Play();
            }
        }
    }

    public void DoorManager(Transform doorOpt, string pin)
    {
        ModularDoor door = doorOpt.GetComponentInParent<WallManager>().modularDoor;

        if (door)
        {
            if (Vector3.Distance(door.transform.position, Player.localPlayer.transform.position) <= 2)
            {
                if (door.wallManager.up)
                {
                    if (door.wallManager.modularBuilding.upDoorOpen == true)
                    {
                        Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 1, pin);
                        return;
                    }
                }
                else if (door.wallManager.left)
                {
                    if (door.wallManager.modularBuilding.leftDoorOpen == true)
                    {
                        Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 2, pin);
                        return;
                    }
                }
                else if (door.wallManager.down)
                {
                    if (door.wallManager.modularBuilding.downDoorOpen == true)
                    {
                        Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 3, pin);
                        return;
                    }
                }
                else if (door.wallManager.right)
                {
                    if (door.wallManager.modularBuilding.rightDoorOpen == true)
                    {
                        Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 4, pin);
                        return;
                    }
                }
            }
            else
            {
                Player.localPlayer.playerNotification.SpawnNotification(ImageManager.singleton.refuse, "You are to far away to interact with this door!");
            }
        }

        if (door && pin != string.Empty && pin == door.modularBuilding.GetPin())
        {
            if (Vector3.Distance(door.transform.position, Player.localPlayer.transform.position) <= 2)
            {
                if (door.wallManager.up)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 1, pin);
                }
                else if (door.wallManager.left)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 2, pin);
                }
                else if (door.wallManager.down)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 3, pin);
                }
                else if (door.wallManager.right)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 4, pin);
                }
            }
            return;
        }

        if (!CanEnterHome(door.modularBuilding, Player.localPlayer))
        {
            if (AbilityManager.singleton.FindNetworkAbilityLevel("Thief", Player.localPlayer.name) <= door.modularBuilding.level)
            {
                BlurManager.singleton.Hide();
                UIPin.singleton.Open(Player.localPlayer, doorOpt.GetComponentInParent<WallManager>().modularBuilding.central.GetComponent<CentralManager>(), doorOpt);
                return;
            }
        }

        if (door)
        {
            if (Vector3.Distance(door.transform.position, Player.localPlayer.transform.position) <= 2)
            {
                if (door.wallManager.up)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 1, pin);
                }
                else if (door.wallManager.left)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 2, pin);
                }
                else if (door.wallManager.down)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 3, pin);
                }
                else if (door.wallManager.right)
                {
                    Player.localPlayer.playerModularBuilding.CmdManageDoor(door.modularBuilding.gameObject, Player.localPlayer.netIdentity, 4, pin);
                }
            }
        }
    }

    public void Update()
    {
        if (!Player.localPlayer || (Player.localPlayer && Player.localPlayer.playerAdditionalState.additionalState == "SLEEP")) return;

        if (Input.GetMouseButton(0) && !Utils.IsCursorOverUserInterface() && Input.touchCount <= 1)
        {
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(screenPos, Vector2.zero);

            if (spawnedAccesssory || spawnedBuilding)
            {
                if (Player.localPlayer && Player.localPlayer.playerNavMeshMovement2D.moveJoystick.input != Vector2.zero) return;
                for (int i = 0; i < hit.Length; i++)
                {
                    int index = i;

                    if (hit[index].collider.CompareTag("Roof"))
                    {
                        return;
                    }

                    if (hit[index].collider.CompareTag("Spawn"))
                    {
                        if (spawnedAccesssory) spawnedAccesssory.transform.position = hit[i].point;
                        if (spawnedBuilding) spawnedBuilding.transform.position = hit[i].point;
                        if (spawnedBuilding && spawnedBuilding.GetComponent<ModularBuilding>()) ResetModularBuilding();
                        CheckSpawn();
                    }
                }
            }
        }

        roof = null;
        doorOptions = null;
        wallOptions = null;

        if (Input.GetMouseButtonDown(0) && !Utils.IsCursorOverUserInterface() && Input.touchCount <= 1)
        {
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(screenPos, Vector2.zero);

            if (activeBuildingModeWall)
            {

                for (int i = 0; i < hit.Length; i++)
                {
                    int index = i;
                    if (selectedType == 0)
                    {
                        if (hit[index].collider.CompareTag("Roof"))
                        {
                            return;
                        }
                        if (hit[index].collider.CompareTag("PlacementWall"))
                        {
                            selectedModularBuilding = hit[index].collider.GetComponentInParent<ModularBuilding>();
                            if (hit[index].collider.GetComponent<PlacementWall>().up)
                            {
                                if (selectedModularBuilding.serverUpBasementDecoration == -1)
                                {
                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.upWall;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 1;
                                }
                            }
                            if (hit[index].collider.GetComponent<PlacementWall>().left)
                            {
                                if (selectedModularBuilding.serverLeftBasementDecoration == -1)
                                {

                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.leftWall;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 2;
                                }
                            }
                            if (hit[index].collider.GetComponent<PlacementWall>().down)
                            {
                                if (selectedModularBuilding.serverDownBasementDecoration == -1)
                                {

                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.downWall;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 3;
                                }
                            }
                            if (hit[index].collider.GetComponent<PlacementWall>().right)
                            {
                                if (selectedModularBuilding.serverRightBasementDecoration == -1)
                                {

                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.rightWall;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 4;
                                }
                            }
                            Player.localPlayer.playerModularBuilding.AbleFloorPositioningWalls(Player.localPlayer.playerModularBuilding.FindNearestFloorObjects());
                            return;
                        }
                    }
                    else if (selectedType == 1)
                    {
                        if (hit[index].collider.CompareTag("Roof"))
                        {
                            return;
                        }
                        //if (hit[index].collider.CompareTag("DoorOptions"))
                        //{
                        //    DoorManager(hit[index].collider.transform,"");
                        //    return;
                        //}

                        if (hit[index].collider.CompareTag("PlacementWall"))
                        {
                            selectedModularBuilding = hit[index].collider.GetComponentInParent<ModularBuilding>();
                            if (hit[index].collider.GetComponent<PlacementWall>().up)
                            {
                                if (selectedModularBuilding.serverUpBasementDecoration == -1)
                                {
                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.upDoor;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 1;
                                }
                            }
                            if (hit[index].collider.GetComponent<PlacementWall>().left)
                            {
                                if (selectedModularBuilding.serverLeftBasementDecoration == -1)
                                {

                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.leftDoor;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 2;
                                }
                            }
                            if (hit[index].collider.GetComponent<PlacementWall>().down)
                            {
                                if (selectedModularBuilding.serverDownBasementDecoration == -1)
                                {

                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.downDoor;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 3;
                                }
                            }
                            if (hit[index].collider.GetComponent<PlacementWall>().right)
                            {
                                if (selectedModularBuilding.serverRightBasementDecoration == -1)
                                {

                                    if (selectedWall)
                                    {
                                        selectedWall.SetActive(false);
                                        selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                                    }
                                    selectedWall = selectedModularBuilding.rightDoor;
                                    selectedWall.GetComponent<WallManager>().CheckWall();
                                    selectedWall.SetActive(true);
                                    currentSelectedWall = 4;
                                }
                            }
                            Player.localPlayer.playerModularBuilding.AbleFloorPositioningWalls(Player.localPlayer.playerModularBuilding.FindNearestFloorObjects());
                            return;
                        }
                    }
                }
            }
            else if (activeBasementPositioning)
            {
                for (int i = 0; i < hit.Length; i++)
                {
                    int index = i;

                    if (hit[index].collider.CompareTag("Roof"))
                    {
                        return;
                    }
                    if (hit[index].collider.CompareTag("FloorPositioning"))
                    {
                        if (spawnedBuilding == null)
                        {
                            spawnedBuilding = Instantiate(scriptableBuilding.buildingList[objectIndex].buildingObject, new Vector3(hit[index].transform.position.x, hit[index].transform.position.y, 0.0f), Quaternion.identity);
                            spawnedBuilding.GetComponent<ModularBuilding>().modularIndex = hit[index].transform.GetComponent<PlacementBase>().modularBuilding.modularIndex;
                            spawnedBuilding.GetComponent<ModularBuilding>().main = false;
                        }
                        else
                        {
                            Destroy(spawnedBuilding);
                            spawnedBuilding = Instantiate(scriptableBuilding.buildingList[objectIndex].buildingObject, new Vector3(hit[index].transform.position.x, hit[index].transform.position.y, 0.0f), Quaternion.identity);
                            spawnedBuilding.GetComponent<ModularBuilding>().main = false;
                            spawnedBuilding.GetComponent<ModularBuilding>().modularIndex = hit[index].transform.GetComponent<PlacementBase>().modularBuilding.modularIndex;
                        }

                        if (prevPlacementBase)
                        {
                            prevPlacementBase.Manage(true);
                            prevPlacementBase.colliderHits = new Collider2D[0];
                            prevPlacementBase = hit[index].collider.gameObject.GetComponent<PlacementBase>();
                        }
                        else
                        {
                            prevPlacementBase = hit[index].collider.gameObject.GetComponent<PlacementBase>();
                        }
                        return;
                    }
                }
            }

            roof = doorOptions = wallOptions = accessory = plantable = null;

            for (int e = 0; e < hit.Length; e++)
            {
                int index = e;

                if (hit[index].collider.CompareTag("Roof"))
                {
                    roof = hit[index].collider.transform;
                }
                if (hit[index].collider.CompareTag("DoorOptions"))
                {
                    doorOptions = hit[index].collider.transform;
                }
                if (hit[index].collider.CompareTag("WallOptions"))
                {
                    wallOptions = hit[index].collider.transform;
                }
                if (hit[index].collider.CompareTag("Accessory"))
                {
                    accessory = hit[index].collider.transform;
                }
                if (hit[index].collider.CompareTag("PlantSlot"))
                {
                    plantable = hit[index].collider.transform;
                }
            }


            for (int i = 0; i < hit.Length; i++)
            {
                int index = i;

                if (roof)
                {
                    return;
                }
                if (spawnedAccesssory || spawnedWall || spawnedBuilding) return;

                if (doorOptions)
                {

                    //if (CanEnterHome(hit[index].collider.GetComponentInParent<WallManager>().modularBuilding, Player.localPlayer))
                    DoorManager(doorOptions, "");
                    //else
                    //{
                    //    BlurManager.singleton.Hide();
                    //    UIPin.singleton.Open(Player.localPlayer, hit[index].collider.GetComponentInParent<WallManager>().modularBuilding.central.GetComponent<CentralManager>(), hit[index].collider.transform);
                    //}
                    return;
                }

                if(plantable)
                {
                    PlantableSlot slot = plantable.GetComponentInParent<PlantableSlot>();
                    if (slot.slotRenderer.color.a == 1.0f)
                    {
                        if(Player.localPlayer.playerHungry.objectToPlant != string.Empty)
                        {
                            Player.localPlayer.playerHungry.CmdPlant(slot.cultivableField.netIdentity, slot.indexPlant, Player.localPlayer.playerHungry.objectToPlant);
                            return;
                        }
                    }
                    
                    if (slot.cultivableField.cultivablePoints[slot.indexPlant].isCompleted)
                    {
                        Player.localPlayer.playerHungry.CmdPick(slot.cultivableField.netIdentity, slot.indexPlant);
                    }
                    
                    
                }
                if (wallOptions)
                {
                    WallOptions wallOption = wallOptions.GetComponent<WallOptions>();
                    if (wallOption)
                    {
                        BlurManager.singleton.Hide();
                        GameObject opt = Instantiate(GameObjectSpawnManager.singleton.confirmDeleteWall, GameObjectSpawnManager.singleton.canvas);
                        ConfirmDeleteWall deleteWall = opt.GetComponent<ConfirmDeleteWall>();
                        deleteWall.wallManager = wallOption.wallManager;
                        deleteWall.positioning = wallOption.positioning;
                        return;
                    }
                }

                if (hit[index].collider.CompareTag("Selector"))
                {
                    BuildingAccessory forniture = hit[index].collider.gameObject.GetComponentInParent<BuildingAccessory>();
                    if (forniture.netIdentity.netId == 0) return;

                    if(forniture.accessoriesInThisForniture.Count > 0)
                    {
                        GameObject g = Instantiate(GameObjectSpawnManager.singleton.fornitureAccessory, GameObjectSpawnManager.singleton.canvas);
                        g.GetComponent<UIAccessorySelector>().Open(forniture);
                        return;
                    }
                    SelectorClicked(forniture);
                    return;
                }
                if (hit[index].collider.CompareTag("Central"))
                {
                    BlurManager.singleton.Hide();
                    UICentralManager.singleton.Open(hit[index].collider.GetComponent<CentralManager>().modularBuilding, 1);
                    return;
                }
                if (hit[index].collider.CompareTag("Flower"))
                {
                    if (Vector2.Distance(Player.localPlayer.transform.GetChild(0).transform.position, hit[index].collider.transform.position) < 2)
                    {
                        Player.localPlayer.playerPick.CmdAddFlower(hit[index].collider.GetComponent<NetworkIdentity>());
                        return;
                    }
                }
                if (hit[index].collider.CompareTag("PlayerSelector"))
                {
                    if (Player.localPlayer && hit[index].collider.gameObject.transform.root.gameObject.name.Replace("(Clone)", "") != Player.localPlayer.name)
                    {
                        playerTargetInvite.gameObject.SetActive(true);
                        playerTargetInvite.target = hit[index].collider.GetComponentInParent<Player>();
                        playerTargetInvite.sender = Player.localPlayer;
                        playerCanvas.GetComponent<PlayerTargetInvite>().Open();
                    }
                    if (Player.localPlayer && hit[index].collider.gameObject.transform.root.gameObject.name.Replace("(Clone)", "") == Player.localPlayer.name)
                    {
                        playerPersonalCanvas.gameObject.SetActive(true);
                        playerPersonalCanvas.GetComponent<PlayerPersonalOptions>().Open();
                    }
                    return;
                }
                if (hit[index].collider.CompareTag("ResourceGathered"))
                {
                    resource = hit[index].collider.GetComponentInParent<ResourceGathered>();
                    if (resource && resource.slots.Count > 0)
                    {
                        UIResourceGathered.singleton.Open(resource);
                    }

                }

                if (hit[index].collider.CompareTag("Basement"))
                {
                    if (hit[index].collider.gameObject.GetComponent<NetworkIdentity>().isClient && CanDoOtherActionFloor(hit[index].collider.GetComponent<ModularBuilding>(), Player.localPlayer))
                    {
                        if (basementClick == 0)
                        {
                            basementClick = NetworkTime.time + basementClickThroubleShot;
                        }
                        else
                        {
                            if (NetworkTime.time < basementClick)
                            {
                                basementClick = 0;
                                BlurManager.singleton.Hide();
                                UICentralManager.singleton.Open(hit[index].collider.GetComponent<ModularBuilding>(), 0);
                            }
                            else
                            {
                                basementClick = 0;
                            }
                        }
                    }
                    return;
                }
            }

        }

        if (activeFencePositioning)
        {
            if (Player.localPlayer.state == "MOVING")
            {
                if (scriptableFence)
                {
                    GameObject fence = Player.localPlayer.playerModularBuilding.FindNearestFence();
                    if (spawnedBuilding != null && fence != null && fence != spawnedBuilding)
                    {
                        int fd = fence.GetComponent<FenceDirection>().direction;
                        GameObject instantiatedFence = Instantiate(scriptableFence.buildingList[fd].buildingObject, fence.transform.position, Quaternion.identity);
                        if (spawnedBuilding) Destroy(spawnedBuilding);
                        spawnedBuilding = instantiatedFence;
                    }
                }
            }
        }
        if (activeGatePositioning)
        {
            if (Player.localPlayer.state == "MOVING")
            {
                if (scriptableGate)
                {
                    GameObject gate = Player.localPlayer.playerModularBuilding.FindNearestGate();
                    if (spawnedBuilding != null && gate != null && gate != spawnedBuilding)
                    {

                        GameObject instantiatedFence = Instantiate(scriptableGate.buildingList[0].buildingObject, gate.transform.position, Quaternion.identity);
                        if (spawnedBuilding) Destroy(spawnedBuilding);
                        spawnedBuilding = instantiatedFence;
                    }
                }
            }
        }

    }

    public void SelectorClicked(BuildingAccessory forniture,Button closeButton = null)
    {
        switch (forniture.uiToOpen)
        {
            case -4:
                GameObject ins = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
                if (CanDoOtherActionForniture(forniture, Player.localPlayer))
                {
                    ins.GetComponent<UIBuildingAccessoryManager>().cleanButton.gameObject.SetActive(false);
                    ins.GetComponent<UIBuildingAccessoryManager>().levelUpButton.gameObject.SetActive(true);
                    ins.GetComponent<UIBuildingAccessoryManager>().claimButton.gameObject.SetActive(false);
                    ins.GetComponent<UIBuildingAccessoryManager>().Init(forniture.netIdentity, forniture.craftingAccessoryItem);
                }
                else
                {
                    ins.GetComponent<UIBuildingAccessoryManager>().cleanButton.gameObject.SetActive(false);
                    ins.GetComponent<UIBuildingAccessoryManager>().levelUpButton.gameObject.SetActive(false);
                    ins.GetComponent<UIBuildingAccessoryManager>().claimButton.gameObject.SetActive(true);
                    ins.GetComponent<UIBuildingAccessoryManager>().Init(forniture.netIdentity, forniture.craftingAccessoryItem);
                }
                break;
            case -3:               
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UICoffeeMachine.singleton.Open(buildingAccessory);
                if (closeButton) closeButton.onClick.Invoke();
                break;
            case -2:
                if (CanDoOtherActionForniture(forniture, Player.localPlayer))
                {
                    GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
                    g.GetComponent<UIBuildingAccessoryManager>().cleanButton.gameObject.SetActive(true);
                    g.GetComponent<UIBuildingAccessoryManager>().Init(forniture.netIdentity, forniture.craftingAccessoryItem);

                }
                break;
            case -1:
                if (CanDoOtherActionForniture(forniture, Player.localPlayer))
                {
                    GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
                    g.GetComponent<UIBuildingAccessoryManager>().cleanButton.gameObject.SetActive(false);
                    g.GetComponent<UIBuildingAccessoryManager>().Init(forniture.netIdentity, forniture.craftingAccessoryItem);
                }
                break;
            case 0:
                BlurManager.singleton.Hide();
                UIBathroomSink.singleton.aquifer = forniture.GetComponent<BathroomSink>().aquifer;
                buildingAccessory = forniture;
                Player.localPlayer.playerModularBuilding.CmdInteractwithThis(forniture.netIdentity);
                UIBathroomSink.singleton.Open(forniture.GetComponent<BathroomSink>());
                break;
            case 1:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                Player.localPlayer.playerModularBuilding.CmdInteractwithThis(forniture.netIdentity);
                UICraft.singleton.Open(forniture.GetComponent<CraftAccessory>());
                break;
            case 2:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                Player.localPlayer.playerModularBuilding.CmdInteractwithThis(forniture.netIdentity);
                UIKitchenSink.singleton.aquifer = forniture.GetComponent<KitchenSink>().aquifer;
                UIKitchenSink.singleton.Open(forniture.GetComponent<KitchenSink>());
                break;
            case 3:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UIRepair.singleton.Open(forniture.GetComponent<Upgrade>());
                break;
            case 4:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UIFridge.singleton.Open(forniture.GetComponent<Fridge>());
                break;
            case 5:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UICabinet.singleton.Open(forniture.GetComponent<Cabinet>());
                break;
            case 6:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UIWeaponStorage.singleton.Open(forniture.GetComponent<WeaponStorage>());
                break;
            case 7:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                Player.localPlayer.playerModularBuilding.CmdInteractwithThis(forniture.netIdentity);
                UIWarehouse.singleton.Open(forniture.GetComponent<Warehouse>());
                break;
            case 8:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UIBillboard.singleton.Open(forniture.GetComponent<Billboard>());
                break;
            case 9:
                if (CanDoOtherActionForniture(forniture, Player.localPlayer))
                {
                    BlurManager.singleton.Hide();
                    buildingAccessory = forniture;
                    UIFlag.singleton.Open(forniture.GetComponent<Flag>());
                }
                break;
            case 10:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UIFurnace.singleton.Open(forniture.GetComponent<Furnace>());
                break;
            case 11:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UIWaterContainer.singleton.Open(forniture.GetComponent<WaterContainer>());
                break;
            case 12:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UILibrary.singleton.Open(forniture.GetComponent<Library>());
                break;
            case 13:
                BlurManager.singleton.Hide();
                buildingAccessory = forniture;
                UIInteractableItemPanel.singleton.Open(forniture.GetComponent<BuildingAccessory>().craftingAccessoryItem, forniture.netIdentity);
                break;
            case 14:
                if (Player.localPlayer.playerHungry.objectToPlant == string.Empty)
                {
                    BlurManager.singleton.Hide();
                    buildingAccessory = forniture;
                    UICultivablefield.singleton.Open(forniture.GetComponent<CuiltivableField>());
                }
                break;

        }

    }
    public void AbleBuildingModeWall()
    {
        activeBuildingModeWall = true;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeFencePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = false;
        uIModularBuilding.up.interactable = false;
        uIModularBuilding.left.interactable = false;
        uIModularBuilding.down.interactable = false;
        uIModularBuilding.right.interactable = false;
        skillbarObject.SetActive(false);
        Player.localPlayer.playerModularBuilding.AbleFloorPositioningWalls(Player.localPlayer.playerModularBuilding.FindNearestFloorObjects());
    }

    public void AbleBasementPositioning()
    {
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = true;
        activeFencePositioning = false;
        activeGatePositioning = false;
        Player.localPlayer.playerModularBuilding.AbleFloorPositioningPoints(Player.localPlayer.playerModularBuilding.FindNearestFloorObjects());
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = false;
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void AbleFencePositioning()
    {
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = true;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = true;
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void AbleExternalPositioning()
    {
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = true;
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }


    public void AbleGatePositioning()
    {
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = true;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = false;
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void AbleExternalConcreteAccessory()
    {
        objectIndex = 0;
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = (scriptableExternal && ((ScriptableConcrete)scriptableExternal).buildingList.Count > 1);
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void AbleExternalLightAccessory()
    {
        objectIndex = 0;
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = (scriptableExternal && ((ScriptableLight)scriptableExternal).buildingList.Count > 1);
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }


    public void AbleExternalBillboardAccessory()
    {
        objectIndex = 0;
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = (scriptableExternal && ((ScriptableBillboard)scriptableExternal).buildingList.Count > 1);
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void AbleExternalFlagAccessory()
    {
        objectIndex = 0;
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = (scriptableExternal && ((ScriptableFlag)scriptableExternal).buildingList.Count > 1);
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void AbleExternalFurnaceAccessory()
    {
        objectIndex = 0;
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = (scriptableExternal && ((ScriptableFurnace)scriptableExternal).buildingList.Count > 1);
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void AbleExternalWaterContainerAccessory()
    {
        objectIndex = 0;
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = (scriptableExternal && ((ScriptableWaterContainer)scriptableExternal).buildingList.Count > 1);
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }


    public void AbleBasementAccessory()
    {
        objectIndex = 0;
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (ButtonManager.singleton) ButtonManager.singleton.closeButton.onClick.Invoke();
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(false);
        uIModularBuilding.panel.SetActive(true);
        uIModularBuilding.changePerspective.interactable = (scriptableBuildingAccessory && scriptableBuildingAccessory.buildingList.Count > 1);
        uIModularBuilding.up.interactable = true;
        uIModularBuilding.left.interactable = true;
        uIModularBuilding.down.interactable = true;
        uIModularBuilding.right.interactable = true;
        skillbarObject.SetActive(false);
    }

    public void CancelBuildingMode()
    {
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;
        if (UIPlayerBeltSkillBar.singleton) UIPlayerBeltSkillBar.singleton.gameObject.SetActive(true);
        if (spawnedBuilding) Destroy(spawnedBuilding);
        if (spawnedWall) Destroy(spawnedWall);
        if (spawnedAccesssory) Destroy(spawnedAccesssory);
        if (uIModularBuilding) uIModularBuilding.panel.SetActive(false);
        spawnedBuilding = null;
        objectIndex = 0;
        scriptableBuilding = null;
        scriptableWall = null;
        scriptableDoor = null;
        scriptableFence = null;
        scriptableGate = null;
        scriptableBuildingAccessory = null;
        scriptableExternal = null;
        if (isSpawnBuilding == false)
        {
            if (selectedModularBuilding && currentSelectedWall > -1)
            {
                switch (currentSelectedWall)
                {
                    case 1:
                        selectedModularBuilding.upBasementDecoration = -1;
                        break;
                    case 2:
                        selectedModularBuilding.leftBasementDecoration = -1;
                        break;
                    case 3:
                        selectedModularBuilding.downBasementDecoration = -1;
                        break;
                    case 4:
                        selectedModularBuilding.rightBasementDecoration = -1;
                        break;
                }
                selectedWall.GetComponent<WallManager>().CheckWallOnExit();
                selectedWall.gameObject.SetActive(false);
            }
        }
        isSpawnBuilding = false;
        selectedWall = null;
        currentSelectedWall = -1;
        //isInventory = false;
        Player.localPlayer.playerModularBuilding.DisbleFloorPositioningPoints(Player.localPlayer.playerModularBuilding.FindNearestFloorObjects());
        Player.localPlayer.playerModularBuilding.DisableFloorPositioningWallsOnCancel(Player.localPlayer.playerModularBuilding.FindNearestFloorObjects());
        selectedModularBuilding = null;
        inventoryIndex = -1;
        foreach (PlacementBase placementBase in FindObjectsOfType<PlacementBase>())
        {
            placementBase.Manage(false);
        }
        foreach (PlacementWall placementWall in FindObjectsOfType<PlacementWall>())
        {
            placementWall.Manage(false);
        }
        if (prevPlacementBase)
        {
            prevPlacementBase.Manage(false);
            prevPlacementBase.colliderHits = new Collider2D[0];
        }
        objectToMove = null;
        skillbarObject.SetActive(true);
    }

    public void ResetModularBuilding()
    {
        if (spawnedBuilding != null && spawnedBuilding.GetComponent<ModularBuilding>())
        {
            spawnedBuilding.GetComponent<ModularBuilding>().modularIndex = -1;
        }
    }

    public void CheckSpawn()
    {
        if (spawnedAccesssory)
        {
            spawnedAccesssory.GetComponent<BuildingAccessory>().CheckPossibleSpawn();
        }
        else if (spawnedBuilding)
        {
            ModularBuilding modularBuilding = spawnedBuilding.GetComponent<ModularBuilding>();
            if (modularBuilding != null)
            {
                if (!modularBuilding.basementTrigger.Check()) return;
            }
        }
    }

    public void SpawnBuilding()
    {
        activeBuildingModeWall = false;
        activeBuildingModeDoor = false;
        activeBasementPositioning = false;
        activeFencePositioning = false;
        activeGatePositioning = false;

        if (spawnedBuilding)
        {
            if (spawnedBuilding.GetComponent<Fence>() && spawnedBuilding.GetComponent<Fence>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<Fence>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<Fence>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnFence(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<Fence>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }
            else if (spawnedBuilding.GetComponent<Concrete>() && spawnedBuilding.GetComponent<Concrete>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<Concrete>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<Concrete>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnExternal(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<Concrete>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }
            else if (spawnedBuilding.GetComponent<Billboard>() && spawnedBuilding.GetComponent<Billboard>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<Billboard>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<Billboard>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnBillboard(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<Billboard>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }
            else if (spawnedBuilding.GetComponent<Flag>() && spawnedBuilding.GetComponent<Flag>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<Flag>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<Flag>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnFlag(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<Flag>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }
            else if (spawnedBuilding.GetComponent<Furnace>() && spawnedBuilding.GetComponent<Furnace>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<Furnace>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<Furnace>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnFurnace(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<Furnace>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }
            else if (spawnedBuilding.GetComponent<WaterContainer>() && spawnedBuilding.GetComponent<WaterContainer>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<WaterContainer>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<WaterContainer>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnWaterContainer(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<WaterContainer>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }
            else if (spawnedBuilding.GetComponent<Gate>() && spawnedBuilding.GetComponent<Gate>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<Gate>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<Gate>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnGate(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<Gate>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }
            else if (spawnedBuilding.GetComponent<Lamp>() && spawnedBuilding.GetComponent<Lamp>().avoidHits.Count == 0)
            {
                spawnedBuilding.GetComponent<Lamp>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<Lamp>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnLamp(inventoryIndex, isInventory, spawnedBuilding.transform.position, spawnedBuilding.GetComponent<Lamp>().oldPositioning, Player.localPlayer.guild.guild.name, Player.localPlayer.name, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            }

            else if (spawnedBuilding.GetComponent<ModularBuilding>() && spawnedBuilding.GetComponent<ModularBuilding>().basementTrigger.Check())
            {
                spawnedBuilding.GetComponent<ModularBuilding>().group = Player.localPlayer.guild.guild.name == string.Empty ? string.Empty : Player.localPlayer.guild.guild.name;
                spawnedBuilding.GetComponent<ModularBuilding>().owner = Player.localPlayer.name;
                Player.localPlayer.playerModularBuilding.CmdSpawnBuilding(inventoryIndex, isInventory, spawnedBuilding.transform.position, 0, Player.localPlayer.guild.guild.name, Player.localPlayer.name, spawnedBuilding.GetComponent<ModularBuilding>().main, spawnedBuilding.GetComponent<ModularBuilding>().modularIndex);
            }
            else
            {
                Player.localPlayer.playerNotification.SpawnNotification(ImageManager.singleton.refuse, buildingConstruction);
                //CancelBuildingMode();
                return;
            }
        }
        if (selectedModularBuilding)
        {
            if (scriptableBuilding || scriptableDoor || scriptableWall)
            {
                string itemName = string.Empty;
                if (scriptableBuilding)
                {
                    itemName = scriptableBuilding.name;
                }
                else if (scriptableDoor)
                {
                    itemName = scriptableDoor.name;
                }
                else if (scriptableWall)
                {
                    itemName = scriptableWall.name;
                }
                Player.localPlayer.playerModularBuilding.CmdAddWall(selectedModularBuilding.identity, currentSelectedWall, selectedType, inventoryIndex, isInventory, itemName);
            }
            isSpawnBuilding = true;
        }
        if (spawnedAccesssory)
        {
            if (spawnedAccesssory.GetComponent<BuildingAccessory>().CheckPossibleSpawn())
                Player.localPlayer.playerModularBuilding.CmdCreateAccessory(inventoryIndex, isInventory, scriptableBuildingAccessory.name, spawnedAccesssory.GetComponent<BuildingAccessory>().oldPositioning, spawnedAccesssory.transform.position, Player.localPlayer.playerModularBuilding.fakeBuildingID);
            else
                Player.localPlayer.playerNotification.SpawnNotification(ImageManager.singleton.refuse, buildingConstruction);
        }

            Player.localPlayer.playerModularBuilding.CmdDeleteAccessory();


        if (spawnedBuilding || spawnedAccesssory || spawnedWall)
        {
            spawnedWall = null;
            CancelBuildingMode();
            if (scriptableBuilding && scriptableBuilding.continuePlacing)
                UIModularBuildingSelector.singleton.Use(scriptableBuilding, isInventory);
            if (scriptableDoor && scriptableDoor.continuePlacing)
                UIModularBuildingSelector.singleton.Use(scriptableDoor, isInventory);
            if (scriptableWall && scriptableWall.continuePlacing)
                UIModularBuildingSelector.singleton.Use(scriptableWall, isInventory);
            if (scriptableExternal && scriptableExternal.continuePlacing)
                UIModularBuildingSelector.singleton.Use(scriptableExternal, isInventory);
            if (scriptableFence && scriptableFence.continuePlacing)
                UIModularBuildingSelector.singleton.Use(scriptableFence, isInventory);
            if (scriptableGate && scriptableGate.continuePlacing)
                UIModularBuildingSelector.singleton.Use(scriptableGate, isInventory);
            if (scriptableBuildingAccessory && scriptableBuildingAccessory.continuePlacing)
                UIModularBuildingSelector.singleton.Use(scriptableBuildingAccessory, isInventory);
        }
        else
        {
            spawnedWall = null;
            CancelBuildingMode();
        }
    }


    public void Up()
    {
        objectToMove = spawnedBuilding != null ? spawnedBuilding : spawnedAccesssory;
        Vector3 pos = objectToMove.transform.position;
        pos.y += sensibility;

        objectToMove.transform.position = pos;
        ResetModularBuilding();
        CheckSpawn();
    }
    public void Left()
    {
        objectToMove = spawnedBuilding != null ? spawnedBuilding : spawnedAccesssory;

        Vector3 pos = objectToMove.transform.position;
        pos.x -= sensibility;

        objectToMove.transform.position = pos;
        ResetModularBuilding();
        CheckSpawn();
    }
    public void Down()
    {
        objectToMove = spawnedBuilding != null ? spawnedBuilding : spawnedAccesssory;

        Vector3 pos = objectToMove.transform.position;
        pos.y -= sensibility;

        objectToMove.transform.position = pos;
        ResetModularBuilding();
        CheckSpawn();
    }
    public void Right()
    {
        objectToMove = spawnedBuilding != null ? spawnedBuilding : spawnedAccesssory;

        Vector3 pos = objectToMove.transform.position;
        pos.x += sensibility;

        objectToMove.transform.position = pos;
        ResetModularBuilding();
        CheckSpawn();
    }
    public void ChangePerspective()
    {
        if (scriptableBuilding)
        {
            objectIndex++;
            if (objectIndex > scriptableBuilding.buildingList.Count - 1) objectIndex = 0;
            GameObject cacheSpawnedBuilding = spawnedBuilding;
            spawnedBuilding = Instantiate(scriptableBuilding.buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            Destroy(cacheSpawnedBuilding);
        }
        else if (scriptableBuildingAccessory)
        {
            objectIndex++;
            if (objectIndex > scriptableBuildingAccessory.buildingList.Count - 1) objectIndex = 0;
            GameObject cacheSpawnedBuilding = spawnedAccesssory;
            spawnedAccesssory = Instantiate(scriptableBuildingAccessory.buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            CheckSpawn();
            Destroy(cacheSpawnedBuilding);
        }
        else if (scriptableFence)
        {
            objectIndex++;
            if (objectIndex > scriptableFence.buildingList.Count - 1) objectIndex = 0;
            GameObject cacheSpawnedBuilding = spawnedBuilding;
            spawnedBuilding = Instantiate(scriptableFence.buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            CheckSpawn();
            Destroy(cacheSpawnedBuilding);
        }
        else if (scriptableExternal)
        {
            objectIndex++;
            GameObject cacheSpawnedBuilding = spawnedBuilding;
            if (scriptableExternal is ScriptableConcrete)
            {
                if (objectIndex > ((ScriptableConcrete)scriptableExternal).buildingList.Count - 1) objectIndex = 0;
                spawnedBuilding = Instantiate(((ScriptableConcrete)scriptableExternal).buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            }
            else if (scriptableExternal is ScriptableFence)
            {
                if (objectIndex > ((ScriptableFence)scriptableExternal).buildingList.Count - 1) objectIndex = 0;
                spawnedBuilding = Instantiate(((ScriptableFence)scriptableExternal).buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            }
            else if (scriptableExternal is ScriptableFlag)
            {
                if (objectIndex > ((ScriptableFlag)scriptableExternal).buildingList.Count - 1) objectIndex = 0;
                spawnedBuilding = Instantiate(((ScriptableFlag)scriptableExternal).buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            }
            else if (scriptableExternal is ScriptableFurnace)
            {
                if (objectIndex > ((ScriptableFurnace)scriptableExternal).buildingList.Count - 1) objectIndex = 0;
                spawnedBuilding = Instantiate(((ScriptableFurnace)scriptableExternal).buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            }
            else if (scriptableExternal is ScriptableGate)
            {
                if (objectIndex > ((ScriptableGate)scriptableExternal).buildingList.Count - 1) objectIndex = 0;
                spawnedBuilding = Instantiate(((ScriptableGate)scriptableExternal).buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            }
            else if (scriptableExternal is ScriptableLight)
            {
                if (objectIndex > ((ScriptableLight)scriptableExternal).buildingList.Count - 1) objectIndex = 0;
                spawnedBuilding = Instantiate(((ScriptableLight)scriptableExternal).buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            }
            else if (scriptableExternal is ScriptableBillboard)
            {
                if (objectIndex > ((ScriptableBillboard)scriptableExternal).buildingList.Count - 1) objectIndex = 0;
                spawnedBuilding = Instantiate(((ScriptableLight)scriptableExternal).buildingList[objectIndex].buildingObject, cacheSpawnedBuilding.transform.position, Quaternion.identity);
            }

            CheckSpawn();
            Destroy(cacheSpawnedBuilding);
        }
    }

    public int CanSetPIN(ModularBuilding building, string playerName)
    {
        Player player = Player.onlinePlayers[playerName];
        if (!player) return 0;

        //Premium
        if (string.IsNullOrEmpty(building.group) && string.IsNullOrEmpty(building.owner))
        {
            return 0;
        }

        // building with guild
        if (!string.IsNullOrEmpty(building.group))
        {
            // if un guild check
            if (player.guild.InGuild())
            {
                // if building of my guild
                if (building.group == player.guild.guild.name)
                {
                    for (int i = 0; i < player.guild.guild.members.Length; i++)
                    {
                        if (player.guild.guild.members[i].name == player.name)
                        {
                            if (player.guild.guild.members[i].rank == GuildRank.Master ||
                                player.guild.guild.members[i].rank == GuildRank.Vice)
                                return 2;
                        }
                    }
                }
                return 0;
            }
            else
            {
                if (AbilityManager.singleton.FindNetworkAbilityLevel("Thief", player.name) >= building.level)
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return 2;
            }
            else
            {
                if (AbilityManager.singleton.FindNetworkAbilityLevel("Thief", player.name) >= building.level)
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
        }

        return 0;

    }


    public bool CanDoOtherActionFloor(ModularBuilding building, Player player)
    {
        if (!player) return false;

        //Premium
        if (string.IsNullOrEmpty(building.group) && string.IsNullOrEmpty(building.owner))
        {
            return false;
        }

        // building with guild
        if (!string.IsNullOrEmpty(building.group))
        {

            // if un guild check
            if (player.guild.InGuild())
            {
                // if building of my guild
                if (building.group == player.guild.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.group))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsOverlapPercentageAboveThreshold(Collider2D staticCollider, CapsuleCollider2D enteringCollider, float overlapThreshold = 0.7f)
    {
        if (staticCollider == null || enteringCollider == null)
        {
            Debug.LogError("I collider non possono essere nulli.");
            return false;
        }

        if (!(staticCollider is BoxCollider2D) || !(enteringCollider is CapsuleCollider2D))
        {
            Debug.LogError("Il collider statico deve essere un BoxCollider2D e quello entrante deve essere un CapsuleCollider2D.");
            return false;
        }

        BoxCollider2D staticBoxCollider = (BoxCollider2D)staticCollider;
        CapsuleCollider2D enteringCapsuleCollider = (CapsuleCollider2D)enteringCollider;

        // Calcola i punti estremi del CapsuleCollider2D
        Vector2 capsuleTop = (Vector2)enteringCapsuleCollider.transform.position + enteringCapsuleCollider.offset +
                             Vector2.up * enteringCapsuleCollider.size.y / 2f;

        Vector2 capsuleBottom = (Vector2)enteringCapsuleCollider.transform.position + enteringCapsuleCollider.offset -
                                Vector2.up * enteringCapsuleCollider.size.y / 2f;

        float capsuleRadius = enteringCapsuleCollider.size.x / 2f;

        // Conta il numero di punti estremi del CapsuleCollider2D contenuti nel BoxCollider2D
        int pointsInside = 0;

        if (staticBoxCollider.OverlapPoint(capsuleTop + Vector2.left * capsuleRadius)) pointsInside++;
        if (staticBoxCollider.OverlapPoint(capsuleTop + Vector2.right * capsuleRadius)) pointsInside++;
        if (staticBoxCollider.OverlapPoint(capsuleBottom + Vector2.left * capsuleRadius)) pointsInside++;
        if (staticBoxCollider.OverlapPoint(capsuleBottom + Vector2.right * capsuleRadius)) pointsInside++;

        // Calcola la percentuale di overlap rispetto al numero totale di punti
        float overlapPercentage = (float)pointsInside / 4f;

        // Verifica se la percentuale di overlap è almeno il 70%
        return overlapPercentage >= overlapThreshold;
    }

    public bool CanDoOtherActionFloorWithouPlayer(ModularBuilding building, string owner, string group)
    {

        //Premium
        if (string.IsNullOrEmpty(building.group) && string.IsNullOrEmpty(building.owner))
        {
            return false;
        }

        // building with guild
        if (!string.IsNullOrEmpty(building.group))
        {

            // if un guild check
            if (group != "")
            {
                // if building of my guild
                if (building.group == group)
                {
                    return true;
                }
            }
        }
        else
        {
            if (building.owner == owner)
            {
                return true;
            }
        }

        return false;
    }

    public int CanDoOtherActionFloorInt(ModularBuilding building, Player player)
    {
        if (!player) return -1;

        //Premium
        if (string.IsNullOrEmpty(building.group) && string.IsNullOrEmpty(building.owner))
        {
            return -2;
        }

        // building with guild
        if (!string.IsNullOrEmpty(building.group))
        {

            // if un guild check
            if (player.guild.InGuild())
            {
                // if building of my guild
                if (building.group == player.guild.guild.name)
                {
                    return 0;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.group))
                    {
                        return 1;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return 2;
            }
        }

        return -1;
    }

    public bool CanEnterHome(ModularBuilding building, Player player)
    {
        if (!player) return false;

        //Premium
        if (string.IsNullOrEmpty(building.group) && string.IsNullOrEmpty(building.owner))
        {
            return false;
        }

        // building with guild
        if (!string.IsNullOrEmpty(building.group))
        {

            // if un guild check
            if (player.guild.InGuild())
            {
                // if building of my guild
                if (building.group == player.guild.guild.name)
                {
                    return true;
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanDoOtherActionForniture(BuildingAccessory building, Player player)
    {
        if (!player) return false;

        //Premium
        if (string.IsNullOrEmpty(building.group) && string.IsNullOrEmpty(building.owner))
        {
            return false;
        }

        // building with guild
        if (!string.IsNullOrEmpty(building.group))
        {

            // if un guild check
            if (player.guild.InGuild())
            {
                // if building of my guild
                if (building.group == player.guild.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.group))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }

        return false;
    }

}
