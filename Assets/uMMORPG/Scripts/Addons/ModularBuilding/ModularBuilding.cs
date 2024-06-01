using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public partial class Database
{
    class building_basement
    {
        public int ind { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public int up { get; set; }
        public int down { get; set; }
        public int left { get; set; }
        public int right { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public int upDoorOpen { get; set; }
        public int downDoorOpen { get; set; }
        public int leftDoorOpen { get; set; }
        public int rightDoorOpen { get; set; }
        public int serverUpBasementDecoration { get; set; }
        public int serverDownBasementDecoration { get; set; }
        public int serverLeftBasementDecoration { get; set; }
        public int serverRightBasementDecoration { get; set; }
        public string group { get; set; }
        public string owner { get; set; }
        public int modularIndex { get; set; }
        public int main { get; set; }
        public int floorTexture { get; set; }
        public float wallHealthDx { get; set; }
        public float wallHealthSx { get; set; }
        public float wallHealthUp { get; set; }
        public float wallHealthDown { get; set; }

        public float doorHealthDx { get; set; }
        public float doorHealthSx { get; set; }
        public float doorHealthUp { get; set; }
        public float doorHealthDown { get; set; }
        public string pin { get; set; }
    }

    public void SaveBuildingBasement()
    {
        connection.Execute("DELETE FROM building_basement");

        for (int i = 0; i < ModularBuildingManager.singleton.combinedModulars.Count; i++)
        {
            int index = i;
            connection.InsertOrReplace(new building_basement
            {
                ind = i,
                name = ModularBuildingManager.singleton.combinedModulars[index].name.Replace("(Clone)", ""),
                level = ModularBuildingManager.singleton.combinedModulars[index].level,
                up = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].up),
                down = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].down),
                left = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].left),
                right = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].right),
                posX = ModularBuildingManager.singleton.combinedModulars[index].transform.position.x,
                posY = ModularBuildingManager.singleton.combinedModulars[index].transform.position.y,
                upDoorOpen = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].upDoorOpen),
                downDoorOpen = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].downDoorOpen),
                leftDoorOpen = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].leftDoorOpen),
                rightDoorOpen = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].rightDoorOpen),
                serverUpBasementDecoration = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].serverUpBasementDecoration),
                serverDownBasementDecoration = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].serverDownBasementDecoration),
                serverLeftBasementDecoration = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].serverLeftBasementDecoration),
                serverRightBasementDecoration = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].serverRightBasementDecoration),
                group = ModularBuildingManager.singleton.combinedModulars[index].group != "" ? ModularBuildingManager.singleton.combinedModulars[index].group : "",
                owner = ModularBuildingManager.singleton.combinedModulars[index].owner != "" ? ModularBuildingManager.singleton.combinedModulars[index].owner : "",
                modularIndex = ModularBuildingManager.singleton.combinedModulars[index].modularIndex,
                main = Convert.ToInt32(ModularBuildingManager.singleton.combinedModulars[index].main),
                floorTexture = ModularBuildingManager.singleton.combinedModulars[index].floorTexture,
                wallHealthDx = ModularBuildingManager.singleton.combinedModulars[index].wallHealthDx,
                wallHealthSx = ModularBuildingManager.singleton.combinedModulars[index].wallHealthSx,
                wallHealthUp = ModularBuildingManager.singleton.combinedModulars[index].wallHealthUp,
                wallHealthDown = ModularBuildingManager.singleton.combinedModulars[index].wallHealthDown,
                doorHealthDx = ModularBuildingManager.singleton.combinedModulars[index].doorHealthDx,
                doorHealthSx = ModularBuildingManager.singleton.combinedModulars[index].doorHealthSx,
                doorHealthUp = ModularBuildingManager.singleton.combinedModulars[index].doorHealthUp,
                doorHealthDown = ModularBuildingManager.singleton.combinedModulars[index].doorHealthDown,
                pin = ModularBuildingManager.singleton.combinedModulars[index].GetPin()
            });
        }
    }

    public void LoadBuildingBasement()
    {
        GameObject g = null;
        ModularBuilding acc = null;
        ModularBuildingIndexAssignManager modularBuildingManager = GameObject.Find("Manager_").GetComponent<ModularBuildingIndexAssignManager>();
        foreach (building_basement row in connection.Query<building_basement>("SELECT * FROM building_basement"))
        {
            if (ScriptableBuilding.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableBuilding itemData))
            {
                g = Instantiate(itemData.buildingList[0].buildingObject, new Vector2(row.posX, row.posY), Quaternion.identity);
                acc = g.GetComponent<ModularBuilding>();
                acc.name = row.name;
                acc.level = row.level;
                acc.up = Convert.ToBoolean(row.up);
                acc.down = Convert.ToBoolean(row.down);
                acc.left = Convert.ToBoolean(row.left);
                acc.right = Convert.ToBoolean(row.right);
                acc.upDoorOpen = Convert.ToBoolean(row.upDoorOpen);
                acc.downDoorOpen = Convert.ToBoolean(row.downDoorOpen);
                acc.leftDoorOpen = Convert.ToBoolean(row.leftDoorOpen);
                acc.rightDoorOpen = Convert.ToBoolean(row.rightDoorOpen);
                acc.serverUpBasementDecoration = row.serverUpBasementDecoration;
                acc.serverDownBasementDecoration = row.serverDownBasementDecoration;
                acc.serverLeftBasementDecoration = row.serverLeftBasementDecoration;
                acc.serverRightBasementDecoration = row.serverRightBasementDecoration;
                acc.group = row.group == null ? "" : row.group;
                acc.owner = row.owner == null ? "" : row.owner;
                acc.modularIndex = row.modularIndex;
                acc.main = Convert.ToBoolean(row.main);
                acc.floorTexture = row.floorTexture;
                acc.wallHealthDx = row.wallHealthDx;
                acc.wallHealthSx = row.wallHealthSx;
                acc.wallHealthUp = row.wallHealthUp;
                acc.wallHealthDown = row.wallHealthDown;
                acc.doorHealthDx = row.doorHealthDx;
                acc.doorHealthSx = row.doorHealthSx;
                acc.doorHealthUp = row.doorHealthUp;
                acc.doorHealthDown = row.doorHealthDown;
                acc.SetPin(row.pin);

                if (acc.modularIndex > modularBuildingManager.incrementalModularBuildingIndex)
                {
                    modularBuildingManager.incrementalModularBuildingIndex = acc.modularIndex;
                }

                NetworkServer.Spawn(g);
            }

        }
    }
}

[System.Serializable]
public class ModularBuilding : NetworkBehaviour
{
    [SyncVar(hook = nameof(ManageLevel))]
    public int level = 1;
    [SyncVar]
    public bool up;
    [SyncVar]
    public bool down;
    [SyncVar]
    public bool left;
    [SyncVar]
    public bool right;

    [SyncVar(hook = nameof(NetworkDoorUpManage))]
    public bool upDoorOpen;
    [SyncVar(hook = nameof(NetworkDoorDownManage))]
    public bool downDoorOpen;
    [SyncVar(hook = nameof(NetworkDoorLeftManage))]
    public bool leftDoorOpen;
    [SyncVar(hook = nameof(NetworkDoorRightManage))]
    public bool rightDoorOpen;

    [SyncVar(hook = nameof(NetworkManageWallUp))]
    public int serverUpBasementDecoration;
    [SyncVar(hook = nameof(NetworkManageWallDown))]
    public int serverDownBasementDecoration;
    [SyncVar(hook = nameof(NetworkManageWallLeft))]
    public int serverLeftBasementDecoration;
    [SyncVar(hook = nameof(NetworkManageWallRight))]
    public int serverRightBasementDecoration;

    public int upBasementDecoration, downBasementDecoration, leftBasementDecoration, rightBasementDecoration;

    [SyncVar(hook = nameof(ManageAdminChange))]
    public string group = string.Empty;
    [SyncVar(hook = nameof(ManageAdminChange))]
    public string owner = string.Empty;
    [SyncVar]
    public int modularIndex = -1;
    [SyncVar(hook = nameof(ManageCentral))]
    public bool main = false;
    [SyncVar(hook = nameof(ManageFloor))]
    public int floorTexture = 0;

    [SyncVar(hook = nameof(ManageWallDx))]
    public float wallHealthDx = 500;
    [SyncVar(hook = nameof(ManageWallSx))]
    public float wallHealthSx = 500;
    [SyncVar(hook = nameof(ManageWallUp))]
    public float wallHealthUp = 500;
    [SyncVar(hook = nameof(ManageWallDown))]
    public float wallHealthDown = 500;

    [SyncVar(hook = nameof(ManageDoorDx))]
    public float doorHealthDx = 500;
    [SyncVar(hook = nameof(ManageDoorSx))]
    public float doorHealthSx = 500;
    [SyncVar(hook = nameof(ManageDoorUp))]
    public float doorHealthUp = 500;
    [SyncVar(hook = nameof(ManageDoorDown))]
    public float doorHealthDown = 500;

    public float defaultValue = 500;

    [SyncVar(hook = nameof(ManagePin))]
    private string pin = "0000";

    public float maxWallHealth => defaultValue + ModularBuildingManager.singleton.healthToAddToWalls * level;

    public Collider2D[] grassUnder;

    public SpriteRenderer floorRenderer;

    [SyncVar] public Aquifer aquifer;
    //public Aquifer aquifer;

    public NetworkIdentity identity;

    public PlacementBase upBasementPositiioning;
    public PlacementBase downBasementPositiioning;
    public PlacementBase leftBasementPositiioning;
    public PlacementBase rightBasementPositiioning;

    public PlacementWall upRenderer;
    public PlacementWall downRenderer;
    public PlacementWall leftRenderer;
    public PlacementWall rightRenderer;

    public GameObject upDoor, upWall, leftDoor, leftWall, downDoor, downWall, rightDoor, rightWall;

    public List<GameObject> wallActive;

    public Collider2D thisCollider;

    public GameObject roof;
    public BasementTrigger basementTrigger;

    private List<NavMeshObstacle2DCustom> riep = new List<NavMeshObstacle2DCustom>();
    private List<Aquifer> aquiferOrdered = new List<Aquifer>();
    private List<ModularBuilding> modularBuildingOrdered = new List<ModularBuilding>();

    private WallManager cachedWallManager;

    public GameObject central;
    public BuildingMarker buildingMarker;

    public ScriptableBuilding building;

    public WallManager doorSx;
    public WallManager doorDx;
    public WallManager doorUp;
    public WallManager doorDown;
    public WallManager wallSx;
    public WallManager wallDx;
    public WallManager wallUp;
    public WallManager wallDown;

    public ModularBuilding leftBuilding;
    public ModularBuilding upBuilding;
    public ModularBuilding rightBuilding;
    public ModularBuilding downBuilding;

    public List<SortByDepth> sortByDepths = new List<SortByDepth>();

    public void AddHealth(int health)
    {
        wallHealthDx = wallHealthDx + health > maxWallHealth ? maxWallHealth : wallHealthDx + health;
        wallHealthSx = wallHealthSx + health > maxWallHealth ? maxWallHealth : wallHealthSx + health;
        wallHealthUp = wallHealthUp + health > maxWallHealth ? maxWallHealth : wallHealthUp + health;
        wallHealthDown = wallHealthDown + health > maxWallHealth ? maxWallHealth : wallHealthDown + health;

        doorHealthDx = doorHealthDx + health > maxWallHealth ? maxWallHealth : doorHealthDx + health;
        doorHealthSx = doorHealthSx + health > maxWallHealth ? maxWallHealth : doorHealthSx + health;
        doorHealthUp = doorHealthUp + health > maxWallHealth ? maxWallHealth : doorHealthUp + health;
        doorHealthDown = doorHealthDown + health > maxWallHealth ? maxWallHealth : doorHealthDown + health;
    }

    public void RefreshWallOptions()
    {
        doorSx.ManageVisibility();
        doorDx.ManageVisibility();
        doorUp.ManageVisibility();
        doorDown.ManageVisibility();
        wallSx.ManageVisibility();
        wallDx.ManageVisibility();
        wallUp.ManageVisibility();
        wallDown.ManageVisibility();
    }

    public void ManagePin(string oldValue, string newValue)
    {
        if(UICentralManager.singleton && UICentralManager.singleton.panel.activeInHierarchy && UICentralManager.singleton.modularBuilding == this && UICentralManager.singleton.mode == 1)
        {
            UICentralManager.singleton.Open(this, UICentralManager.singleton.mode);
        }

        ManageAdminChange(oldValue, newValue);
    }

    public void ManageFloor(int oldValue, int newValue)
    {
        floorRenderer.sprite = FloorManager.singleton.floorSprites[newValue];
    }

    public void Start()
    {
        GetComponent<DamagableObject>().modularBuilding = this;
        basementTrigger.roof.SetActive(false);
        if (isServer) Invoke(nameof(ManageAquifer), 1.0f);
        if (isServer) Invoke(nameof(ManagePin), 1.0f);
        if (isServer || isClient) Invoke(nameof(Register), 1.0f);
        if (!isServer && !isClient) ModularBuildingManager.singleton.ClearGrass(this.gameObject);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ManageCentral(main, main);
        if (!main) pin = string.Empty;
        Register();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Register();
        ManageCentral(main,main);
        ModularBuildingManager.singleton.ClearGrass(this.gameObject);

        NetworkDoorUpManage(upDoorOpen, upDoorOpen);
        NetworkDoorDownManage(downDoorOpen,downDoorOpen);
        NetworkDoorLeftManage(leftDoorOpen,leftDoorOpen);
        NetworkDoorRightManage(rightDoorOpen,rightDoorOpen);
        NetworkManageWallUp(serverUpBasementDecoration, serverUpBasementDecoration);
        NetworkManageWallDown(serverDownBasementDecoration, serverDownBasementDecoration);
        NetworkManageWallLeft(serverLeftBasementDecoration, serverLeftBasementDecoration);
        NetworkManageWallRight(serverRightBasementDecoration, serverRightBasementDecoration);

        ManageWallDx(wallHealthDx, wallHealthDx);
        ManageWallSx(wallHealthSx, wallHealthSx);
        ManageWallUp(wallHealthUp, wallHealthUp);
        ManageWallDown(wallHealthDown, wallHealthDown);
        ManageDoorDx(doorHealthDx, doorHealthDx);
        ManageDoorSx(doorHealthSx, doorHealthSx);
        ManageDoorUp(doorHealthUp, doorHealthUp);
        ManageDoorDown(doorHealthDown, doorHealthDown);
    }

    public void ManageCracks()
    {
        ManageWallDx(1, 1);

        ManageWallSx(1, 1);

        ManageWallUp(1, 1);

        ManageWallDown(1, 1);


        ManageDoorDx(1, 1);

        ManageDoorSx(1, 1);

        ManageDoorUp(1, 1);

        ManageDoorDown(1, 1);
    }

    

    #region "PIN"

    public void SetPin(string pin)
    {
        this.pin = pin;
    }

    public string GetPin()
    {
        return pin;
    }

    public void ManagePin()
    {
        if (!main
            && pin == string.Empty && modularIndex != -1)
        {
            pin = FindPinFromMain(modularIndex);
        }

        if (pin != string.Empty)
            CancelInvoke(nameof(ManagePin));
        else
        {
            Invoke(nameof(ManagePin), 1.0f);
        }
    }

    public string SearchPinOnMain()
    {
        return FindPinFromMain(modularIndex);
    }

    public string FindPinFromMain(int index)
    {
        foreach(ModularBuilding modular in ModularBuildingManager.singleton.combinedModulars)
        {
            if(modular.main && modular.modularIndex == index)
            {
                return modular.pin;
            }
        }
        return "";
    }

    #endregion

    #region "Acquifer"
    public void SearchAquiferOnMain()
    {
        aquifer = FindNearestAquiferFromMain(modularIndex).aquifer;
    }

    public void SearchAquifer()
    {
        aquifer = FindNearestAquifer();
    }

    public Aquifer FindNearestAquifer()
    {
        List<Aquifer> aquifer = AquiferManager.singleton.aquifers;
        aquiferOrdered = aquifer.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (aquiferOrdered.Count > 0)
            return aquiferOrdered[0];
        else return null;
    }

    public ModularBuilding FindNearestAquiferFromMain(int index)
    {
        List<ModularBuilding> modularBuildings = ModularBuildingManager.singleton.combinedModulars;
        modularBuildingOrdered = modularBuildings.Select(x => x).Where(x => (x.main && x.modularIndex == index)).ToList();
        modularBuildingOrdered = modularBuildings.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();
        if (modularBuildingOrdered.Count > 0)
            return modularBuildingOrdered[0];
        else return null;
    }

    public void ManageAquifer()
    {
        if (!aquifer)
        {
            if (main)
                SearchAquifer();
            else
                SearchAquiferOnMain();

            if (aquifer)
                CancelInvoke(nameof(ManageAquifer));
            else
            {
                Invoke(nameof(ManageAquifer), 1.0f);
            }
        }
    }
    #endregion


    public void ManageCentral(bool oldValue, bool newValue)
    {
        central.SetActive(newValue);
        central.GetComponent<NavMeshObstacle2DCustom>().enabled = newValue;
    }

    public int IncreaseIndex()
    {
        return ModularBuildingIndexAssignManager.singleton.AskNewBaseIndex();
    }

    public void ManageAdminChange(string oldValue, string newValue)
    {
        if (serverUpBasementDecoration == 1) upDoor.GetComponent<WallManager>().ManageVisibility();
        if (serverLeftBasementDecoration == 1) leftDoor.GetComponent<WallManager>().ManageVisibility();
        if (serverDownBasementDecoration == 1) downDoor.GetComponent<WallManager>().ManageVisibility();
        if (serverRightBasementDecoration == 1) rightDoor.GetComponent<WallManager>().ManageVisibility();
        buildingMarker.Check();
    }

    public bool CheckRoof()
    {
        return ((serverUpBasementDecoration > -1 || ( upBuilding && upBuilding.serverDownBasementDecoration > -1)) &&
                (serverLeftBasementDecoration > -1 || (leftBuilding && leftBuilding.serverRightBasementDecoration > -1)) &&
                (serverDownBasementDecoration > -1 || (downBuilding && downBuilding.serverUpBasementDecoration > -1)) &&
                (serverRightBasementDecoration > -1 || (rightBuilding && rightBuilding.serverLeftBasementDecoration > -1)));
    }
    public void Register()
    {
        if (identity.netId > 0)
        {
            if (ModularBuildingManager.singleton)
            {
                if (!ModularBuildingManager.singleton.combinedModulars.Contains(this))
                {
                    ModularBuildingManager.singleton.combinedModulars.Add(this);
                    CancelInvoke(nameof(Register));
                }
            }
            else
            {
                Invoke(nameof(Register), 1.0f);
            }
        }
        else
        {
            Invoke(nameof(Register), 1.0f);
        }
    }

    public void OnDestroy()
    {
        if (identity.netId > 0)
        {
            if (ModularBuildingManager.singleton.combinedModulars.Contains(this)) ModularBuildingManager.singleton.combinedModulars.Remove(this);
        }
        if (isClient || (!isClient && !isServer))
        {
            for (int i = 0; i < grassUnder.Length; i++)
            {
                grassUnder[i].GetComponent<Grass>().isOverlayed = false;
                grassUnder[i].GetComponent<Grass>().Manage(true);
            }
        }

        if(isClient || isServer)
        {
            Collider2D[] coll = Physics2D.OverlapBoxAll(thisCollider.bounds.center, thisCollider.bounds.size, 0, ModularBuildingManager.singleton.buildingPlacementLayerMask);
            for (int e = 0; e < coll.Length; e++)
            {
                if (coll[e].GetComponent<PlacementBase>())
                    coll[e].GetComponent<PlacementBase>().Manage(false);
            }

        }
    }


    public bool CheckComplete()
    {
        if (upBasementDecoration > -1 && leftBasementDecoration > -1 && downBasementDecoration > -1 && rightBasementDecoration > -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckCompleteAroundBase()
    {
        return up && left && down && right;
    }

    public void NetworkDoorUpManage(bool oldValue, bool newValue)
    {
        cachedWallManager = upDoor.GetComponent<WallManager>();
        cachedWallManager.ManageOpening(newValue, true);
        if (cachedWallManager.modularBuilding.serverUpBasementDecoration == 1)
            upDoor.GetComponent<WallManager>().openStatus.color = newValue == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
        ManageAdminChange(string.Empty, string.Empty);
    }

    public void NetworkDoorLeftManage(bool oldValue, bool newValue)
    {
        cachedWallManager = leftDoor.GetComponent<WallManager>();
        cachedWallManager.ManageOpening(newValue, true);
        if (cachedWallManager.modularBuilding.serverLeftBasementDecoration == 1)
            leftDoor.GetComponent<WallManager>().openStatus.color = newValue == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
        ManageAdminChange(string.Empty, string.Empty);
    }

    public void NetworkDoorDownManage(bool oldValue, bool newValue)
    {
        cachedWallManager = downDoor.GetComponent<WallManager>();
        cachedWallManager.ManageOpening(newValue, true);
        if (cachedWallManager.modularBuilding.serverDownBasementDecoration == 1)
            downDoor.GetComponent<WallManager>().openStatus.color = newValue == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
        ManageAdminChange(string.Empty, string.Empty);
    }

    public void NetworkDoorRightManage(bool oldValue, bool newValue)
    {
        cachedWallManager = rightDoor.GetComponent<WallManager>();
        cachedWallManager.ManageOpening(newValue, true);
        if (cachedWallManager.modularBuilding.serverRightBasementDecoration == 1)
            rightDoor.GetComponent<WallManager>().openStatus.color = newValue == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
        ManageAdminChange(string.Empty, string.Empty);
    }

    public void NetworkManageWallUp(int oldValue, int newValue)
    {
        riep.Clear();
        if (newValue == -1)
        {
            if (oldValue == 1)
                upDoor.GetComponent<WallManager>().CheckWallOnExit();
            else if (oldValue == 0)
                upWall.GetComponent<WallManager>().CheckWallOnExit();
        }
        upWall.SetActive(newValue == 0);
        upDoor.SetActive(newValue == 1);
        upBasementDecoration = newValue;
        if (newValue == 1) upDoor.GetComponent<WallManager>().openStatus.color = ModularBuildingManager.singleton.closeColor;
        if (newValue == -1)
        {
            riep = (upWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(upDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }
        }
        else
        {
            riep = (upWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(upDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }

            riep = newValue == 0 ? upWall.GetComponent<WallManager>().navMeshObstacle2Ds : upDoor.GetComponent<WallManager>().navMeshObstacle2Ds;
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = true;
            }
        }
    }
    public void NetworkManageWallLeft(int oldValue, int newValue)
    {
        riep.Clear();
        if (newValue == -1)
        {
            if (oldValue == 1)
                leftDoor.GetComponent<WallManager>().CheckWallOnExit();
            else if (oldValue == 0)
                leftWall.GetComponent<WallManager>().CheckWallOnExit();
        }
        leftWall.SetActive(newValue == 0);
        leftDoor.SetActive(newValue == 1);
        leftBasementDecoration = newValue;
        if (newValue == 1) leftDoor.GetComponent<WallManager>().openStatus.color = ModularBuildingManager.singleton.closeColor;
        if (newValue == -1)
        {
            riep = (leftWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(leftDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }
        }
        else
        {
            riep = (leftWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(leftDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }

            riep = newValue == 0 ? leftWall.GetComponent<WallManager>().navMeshObstacle2Ds : leftDoor.GetComponent<WallManager>().navMeshObstacle2Ds;
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = true;
            }
        }
    }
    public void NetworkManageWallDown(int oldValue, int newValue)
    {
        riep.Clear();
        if (newValue == -1)
        {
            if (oldValue == 1)
                downDoor.GetComponent<WallManager>().CheckWallOnExit();
            else if (oldValue == 0)
                downWall.GetComponent<WallManager>().CheckWallOnExit();
        }
        downWall.SetActive(newValue == 0);
        downDoor.SetActive(newValue == 1);
        downBasementDecoration = newValue;
        if (newValue == 1) downDoor.GetComponent<WallManager>().openStatus.color = ModularBuildingManager.singleton.closeColor;
        if (newValue == -1)
        {
            riep = (downWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(downDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }
        }
        else
        {
            riep = (downWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(downDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }

            riep = newValue == 0 ? downWall.GetComponent<WallManager>().navMeshObstacle2Ds : downDoor.GetComponent<WallManager>().navMeshObstacle2Ds;
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = true;
            }
        }
    }
    public void NetworkManageWallRight(int oldValue, int newValue)
    {
        riep.Clear();
        if (newValue == -1)
        {
            if (oldValue == 1)
                rightDoor.GetComponent<WallManager>().CheckWallOnExit();
            else if (oldValue == 0)
                rightWall.GetComponent<WallManager>().CheckWallOnExit();
        }
        rightWall.SetActive(newValue == 0);
        rightDoor.SetActive(newValue == 1);
        rightBasementDecoration = newValue;
        if (newValue == 1) rightDoor.GetComponent<WallManager>().openStatus.color = ModularBuildingManager.singleton.closeColor;
        if (newValue == -1)
        {
            riep = (rightWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(rightDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }
        }
        else
        {
            riep = (rightWall.GetComponent<WallManager>().navMeshObstacle2Ds.Union(rightDoor.GetComponent<WallManager>().navMeshObstacle2Ds)).ToList();
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = false;
            }

            riep = newValue == 0 ? rightWall.GetComponent<WallManager>().navMeshObstacle2Ds : rightDoor.GetComponent<WallManager>().navMeshObstacle2Ds;
            foreach (NavMeshObstacle2DCustom nav in riep)
            {
                nav.enabled = true;
            }
        }
    }



    private void ManageLevel(int oldValue, int newValue)
    {

    }


    private void ManageWallDx(float oldValue, float newValue)
    {
        if (wallDx.cracksImage == null) return;
        Color c = wallDx.cracksImage.color;
        c.a = 1.0f - ((float)wallHealthDx / (float)(maxWallHealth));
        wallDx.cracksImage.color = c;
    }

    private void ManageWallSx(float oldValue, float newValue)
    {
        if (wallSx.cracksImage == null) return;
        Color c = wallSx.cracksImage.color;
        c.a = 1.0f - ((float)wallHealthSx / (float)(maxWallHealth));
        wallSx.cracksImage.color = c;
    }

    private void ManageWallUp(float oldValue, float newValue)
    {
        if (wallUp.cracksImage == null) return;
        Color c = wallUp.cracksImage.color;
        c.a = 1.0f - ((float)wallHealthUp / (float)(maxWallHealth));
        wallUp.cracksImage.color = c;
    }

    private void ManageWallDown(float oldValue, float newValue)
    {
        if (wallDown.cracksImage == null) return;
        Color c = wallDown.cracksImage.color;
        c.a = 1.0f - ((float)wallHealthDown / (float)(maxWallHealth));
        wallDown.cracksImage.color = c;
    }


    private void ManageDoorDx(float oldValue, float newValue)
    {
        if (doorDx.cracksImage == null) return;
        Color c = doorDx.cracksImage.color;
        c.a = 1.0f - ((float)doorHealthDx / (float)(maxWallHealth));
        doorDx.cracksImage.color = c;
    }

    private void ManageDoorSx(float oldValue, float newValue)
    {
        if (doorSx.cracksImage == null) return;
        Color c = doorSx.cracksImage.color;
        c.a = 1.0f - ((float)doorHealthSx / (float)(maxWallHealth));
        doorSx.cracksImage.color = c;
    }

    private void ManageDoorUp(float oldValue, float newValue)
    {
        if (doorUp.cracksImage == null) return;
        Color c = doorUp.cracksImage.color;
        c.a = 1.0f - ((float)doorHealthUp / (float)(maxWallHealth));
        doorUp.cracksImage.color = c;
    }

    private void ManageDoorDown(float oldValue, float newValue)
    {
        if (doorDown.cracksImage == null) return;
        Color c = doorDown.cracksImage.color;
        c.a = 1.0f - ((float)doorHealthDown / (float)(maxWallHealth));
        doorDown.cracksImage.color = c;
    }
}
