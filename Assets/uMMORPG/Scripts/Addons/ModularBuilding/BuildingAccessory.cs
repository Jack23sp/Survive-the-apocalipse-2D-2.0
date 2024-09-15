using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public partial class Database
{
    class building_base
    {
        public int ind { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string group { get; set; }
        public float health { get; set; }
        public int positioning { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public string nameRename { get; set; }
        public int level { get; set; }
    }



    public void SaveBuildingAccessory()
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM building_base");

        connection.Execute("DELETE FROM billboard");

        connection.Execute("DELETE FROM warehouse_item");
        connection.Execute("DELETE FROM warehouse_item_accessories");

        connection.Execute("DELETE FROM cabinet_item");
        connection.Execute("DELETE FROM cabinet_item_accessories");

        connection.Execute("DELETE FROM fridge_item");
        connection.Execute("DELETE FROM fridge_item_accessories");

        connection.Execute("DELETE FROM watercontainer");

        connection.Execute("DELETE FROM weaponstorage_item");
        connection.Execute("DELETE FROM weaponstorage_item_accessories");

        connection.Execute("DELETE FROM furnace_status");
        connection.Execute("DELETE FROM furnace_elements");
        connection.Execute("DELETE FROM furnace_results");
        connection.Execute("DELETE FROM furnace_wood");
        connection.Execute("DELETE FROM aquarium");
        connection.Execute("DELETE FROM tree");

        SaveBuildingBasement();

        for (int i = 0; i < ModularBuildingManager.singleton.buildingAccessories.Count; i++)
        {
            int index = i;
            connection.InsertOrReplace(new building_base
            {
                ind = i,
                name = ModularBuildingManager.singleton.buildingAccessories[index].craftingAccessoryItem.name,
                owner = ModularBuildingManager.singleton.buildingAccessories[index].owner,
                group = ModularBuildingManager.singleton.buildingAccessories[index].group,
                health = ModularBuildingManager.singleton.buildingAccessories[index].health,
                positioning = ModularBuildingManager.singleton.buildingAccessories[index].oldPositioning,
                posX = ModularBuildingManager.singleton.buildingAccessories[index].transform.position.x,
                posY = ModularBuildingManager.singleton.buildingAccessories[index].transform.position.y,
                nameRename = ModularBuildingManager.singleton.buildingAccessories[index].newName,
                level = (ModularBuildingManager.singleton.buildingAccessories[index] is Fence) ?
                        ((Fence)ModularBuildingManager.singleton.buildingAccessories[index]).level :
                        (ModularBuildingManager.singleton.buildingAccessories[index] is Gate) ?
                        ((Gate)ModularBuildingManager.singleton.buildingAccessories[index]).level : 1

            });

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Billboard)
            {
                SaveBillboard(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Warehouse)
            {
                SaveWarehouse(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Cabinet)
            {
                SaveCabinet(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Flag)
            {
                SaveFlag(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Fridge)
            {
                SaveFridge(index);
            }


            if (ModularBuildingManager.singleton.buildingAccessories[index] is CraftAccessory)
            {
                SaveCraftAccessory(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is WaterContainer)
            {
                SaveWaterContainer(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is WeaponStorage)
            {
                SaveWeaponStorage(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Furnace)
            {
                SaveFurnace(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Library)
            {
                SaveLibrary(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Aquarium)
            {
                SaveAquarium(index);
            }

            if (ModularBuildingManager.singleton.buildingAccessories[index] is Tree)
            {
                SaveTree(index);
            }
        }
        connection.Commit();
    }

    public void LoadBuildingAccessory()
    {
        GameObject g = null;
        BuildingAccessory acc = null;

        LoadBuildingBasement();

        foreach (building_base row in connection.Query<building_base>("SELECT * FROM building_base"))
        {
            if (ScriptableBuildingAccessory.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableBuildingAccessory itemData))
            {
                g = Instantiate(itemData.buildingList[row.positioning].buildingObject, new Vector2(row.posX, row.posY), Quaternion.identity);
                acc = g.GetComponent<BuildingAccessory>();
                acc.owner = row.owner;
                acc.group = row.group;
                acc.health = row.health;
                acc.oldPositioning = row.positioning;
                acc.newName = row.nameRename;
                if(g.GetComponent<Fence>())
                    g.GetComponent<Fence>().level = row.level;
                if (g.GetComponent<Gate>())
                    g.GetComponent<Gate>().level = row.level;

            }

            if (acc is Billboard)
            {
                LoadBillboard(row.ind, ((Billboard)acc));
            }

            if (acc is Warehouse)
            {
                LoadWarehouse(row.ind, ((Warehouse)acc));
            }

            if (acc is Cabinet)
            {
                LoadCabinet(row.ind, ((Cabinet)acc));
            }

            if (acc is Flag)
            {
                LoadFlag(row.ind, ((Flag)acc));
            }

            if (acc is Fridge)
            {
                LoadFridge(row.ind, ((Fridge)acc));
            }

            if (acc is CraftAccessory)
            {
                LoadCraftAccessory(row.ind, ((CraftAccessory)acc));
            }

            if (acc is WaterContainer)
            {
                LoadWaterContainer(row.ind, ((WaterContainer)acc));
            }

            if (acc is WeaponStorage)
            {
                LoadWeaponStorage(row.ind, ((WeaponStorage)acc));
            }

            if (acc is Furnace)
            {
                LoadFurnace(row.ind, ((Furnace)acc));
            }

            if (acc is Library)
            {
                LoadLibrary(row.ind, ((Library)acc));
            }

            if (acc is Aquarium)
            {
                LoadAquarium(row.ind, ((Aquarium)acc));
            }

            if (acc is Tree)
            {
                LoadTree(row.ind, ((Tree)acc));
            }

            NetworkServer.Spawn(g);
        }
    }
}

public class BuildingAccessory : NetworkBehaviour
{
    [SyncVar(hook = nameof(ManageName))]
    public string newName;
    [SyncVar(hook = nameof(ManageAdmin))]
    public string group;
    [SyncVar(hook = nameof(ManageAdmin))]
    public string owner;
    [SyncVar(hook = nameof(ManageHealth))]
    public float health = 100;
    public int maxHealth = 100;

    public BoxCollider2D collider;
    public SpriteRenderer renderer;

    public NavMeshObstacle2DCustom navMeshObstacle2D;

    public int oldPositioning = 0;
    public bool isExternal = false;
    public LayerMask necessaryForPositioning;
    public LayerMask otherAccessory;
    public LayerMask needToAvoid;
    public bool needToCheckOtherAccessory;
    public List<Collider2D> basementColliderHits;
    public List<Collider2D> colliderHits;
    public List<Collider2D> avoidHits;
    public List<SpriteRenderer> cracksImages;
    public List<SortByDepth> sortByDepths;

    public int uiToOpen = -1;

    public ScriptableBuildingAccessory craftingAccessoryItem;

    public GameObject oldBuilding;
    [HideInInspector] public Collider2D zoneCollider;
    public Collider2D[] grassUnder;

    public List<SpriteRenderer> spritesList;
    public List<BuildingAccessory> accessoriesInThisForniture;

    public void Start()
    {
        if (isServer || isClient)
        {
            if (navMeshObstacle2D) navMeshObstacle2D.enabled = true;
            renderer.material = ModularBuildingManager.singleton.spawnedBuildAccessoryMaterial;
            if (!ModularBuildingManager.singleton.buildingAccessories.Contains(this) && craftingAccessoryItem && (owner != string.Empty || group != string.Empty)) ModularBuildingManager.singleton.buildingAccessories.Add(this);
        }
        GetComponent<DamagableObject>().buildingAccessory = this;
    }

    public void ManageName(string oldValue, string newValue)
    { 
        if(this is Warehouse)
        {
            GetComponent<Warehouse>().ManageName(oldValue, newValue);
        }
    }

    public void ManageAdmin(string oldValue, string newValue)
    {
        renderer.material = ModularBuildingManager.singleton.claimBuildingMaterial;
        Invoke(nameof(RestoreMaterial), 1.0f);
    }

    public void RestoreMaterial()
    {
        renderer.material = ModularBuildingManager.singleton.spawnedBuildAccessoryMaterial;
    }

    public void OnDestroy()
    {
        if(isServer)
        {
            SpawnManager.singleton.RemoveFromZone(zoneCollider, this.gameObject);
        }

        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.buildingAccessories.Contains(this)) ModularBuildingManager.singleton.buildingAccessories.Remove(this);
        }

        if (isClient || (!isClient && !isServer))
        {
            for (int i = 0; i < grassUnder.Length; i++)
            {
                grassUnder[i].GetComponent<Grass>().isOverlayed = false;
                grassUnder[i].GetComponent<Grass>().Manage(true);
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        collider.isTrigger = false;
        for(int i = 0; i < sortByDepths.Count; i++)
        {
            sortByDepths[i].enabled = true;
            sortByDepths[i].SetOrder();
        }
    }

    public void ManageHealth(float oldHealth, float newHealth)
    {
        Rock rock = GetComponent<Rock>();
        Tree tree = GetComponent<Tree>();
        ScaleAnimation anim = GetComponent<ScaleAnimation>();

        if (anim)
        {
            //anim.targetScale = anim.fixedScale;
            anim.StartAnimation();
        }

        if (rock)
        {
            if (rock.cracksImage == null) return;
            Color c = rock.cracksImage.color;
            c.a = 1.0f - (float)health / (float)(maxHealth);
            rock.cracksImage.color = c;
        }
        if (tree)
        {
            if (tree.cracksImage == null) return;
            Color c = tree.cracksImage.color;
            c.a = 1.0f - (float)health / (float)(maxHealth);
            tree.cracksImage.color = c;
        }
        else
        {
            if (cracksImages.Count == 0) return;
            for (int i = 0; i < cracksImages.Count; i++)
            {
                Color c = cracksImages[i].color;
                c.a = 1.0f - (float)health / (float)(maxHealth);
                cracksImages[i].color = c;
            }
        }

        if (UIBuildingAccessoryManager.singleton)
        {
            UIBuildingAccessoryManager.singleton.ManageUI(3);
        }
    }


    public bool CheckPossibleSpawn()
    {
        if (isClient || isServer) return true;

        if (ModularBuildingManager.singleton.spawnedAccesssory)
        {
            if (this.gameObject != ModularBuildingManager.singleton.spawnedAccesssory)
            {
                renderer.material = ModularBuildingManager.singleton.spawnedBuildAccessoryMaterial;
                return true;
            }
        }
        basementColliderHits = Physics2D.OverlapBoxAll(collider.bounds.center, new Vector2(collider.size.x, collider.size.y), 0, necessaryForPositioning).ToList();
        if (!isExternal)
        {
            if (avoidHits.Count > 0) return false;

            if (basementColliderHits.Count == 1)
            {

                if (needToCheckOtherAccessory)
                {
                    //colliderHits = Physics2D.OverlapBoxAll(collider.bounds.center, new Vector2(collider.size.x, collider.size.y), 0, otherAccessory);
                    if (colliderHits.Count > 0)
                    {
                        renderer.material = ModularBuildingManager.singleton.notBuildAccessoryMaterial;
                        return false;
                    }
                    else
                    {
                        if (Utilities.IsInside((BoxCollider2D)basementColliderHits[0], collider))
                        {
                            renderer.material = ModularBuildingManager.singleton.spawnedBuildAccessoryMaterial;
                            return true;
                        }
                        else
                        {
                            renderer.material = ModularBuildingManager.singleton.notBuildAccessoryMaterial;
                            return false;
                        }
                    }

                }
                if (Utilities.IsInside((BoxCollider2D)basementColliderHits[0], collider))
                {
                    renderer.material = ModularBuildingManager.singleton.spawnedBuildAccessoryMaterial;
                    return true;
                }
                else
                {
                    renderer.material = ModularBuildingManager.singleton.notBuildAccessoryMaterial;
                    return false;
                }
                return false;
            }
            else
            {

                renderer.material = ModularBuildingManager.singleton.notBuildAccessoryMaterial;
                return false;
            }
        }
        else
        {
            if (avoidHits.Count > 0)
            {
                renderer.material = ModularBuildingManager.singleton.notBuildAccessoryMaterial;
                return false;
            }

            renderer.material = ModularBuildingManager.singleton.spawnedBuildAccessoryMaterial;
            return true;
        }
    }
}
