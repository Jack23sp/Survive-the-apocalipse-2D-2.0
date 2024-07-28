using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[System.Serializable]
public struct EntityInZone
{
    public Collider2D collider;
    public int playerInside;
    public List<ZoneEntity> trees;
    public List<ZoneEntity> rocks;
    public List<ZoneEntity> zombie;
    public List<ZoneEntity> notAggresiveAnimal;
    public List<ZoneEntity> aggresiveAnimal;
    public List<ZoneEntity> flower;
    public List<ZoneEntity> mushroom;
    public List<IrregularColliderSpawner> grass;
    public List<IrregularColliderSpawner> grains;
    public List<Player> players;

    public EntityInZone (Collider2D coll)
    {
        collider = coll;
        playerInside = 0;
        trees = new List<ZoneEntity>();
        rocks = new List<ZoneEntity>();
        zombie = new List<ZoneEntity>();
        notAggresiveAnimal = new List<ZoneEntity>();
        aggresiveAnimal = new List<ZoneEntity>();
        flower = new List<ZoneEntity>();
        mushroom = new List<ZoneEntity>();
        grass = new List<IrregularColliderSpawner>();
        grains = new List<IrregularColliderSpawner>();
        players = new List<Player>();
    }
}

[System.Serializable]
public struct ZoneEntity
{
    public Vector2 pos;
    public string typeName;
    public float health;
    public GameObject actual;

    public ZoneEntity(Vector2 posit, string type, float heal, GameObject act)
    {
        pos = posit;
        typeName = type;
        health = heal;
        actual = act;
    }
}

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager singleton;
    public List<Collider2D> colliders;
    [SerializeField] private LayerMask invalidSpawnLayers;
    public LayerMask invalidGrassLayers;
    
    private GameObject inst;
    private ZoneEntity zoneEntity;
    private EntityInZone entityInZone;

    public int maxRocks;
    public int maxTree;
    public int maxZombie;
    public int maxNotAggressiveAnimal;
    public int maxAggressiveAnimal;
    public int maxFlower;
    public int maxMushroom;
    public int maxGrass;
    public int maxGrain;

    public List<EntityInZone> zones = new List<EntityInZone>();

    public List<GameObject> rock;
    public List<GameObject> tree;
    public List<GameObject> zombie;
    public List<GameObject> notAggressiveAnimal;
    public List<GameObject> aggressiveAnimal;
    public List<GameObject> flower;
    public List<GameObject> mushroom;
    public GameObject grass;
    public GameObject grain;
    public GameObject childGrass;

    public float timeCheck;

    private Vector2 pos;
    private GameObject insta;
    private GameObject instChildGrass;

    public void Start()
    {
        if (!singleton) singleton = this;

        foreach (var collider in colliders)
        {
            collider.gameObject.AddComponent<GenericColliderEvent>();
            zones.Add(new EntityInZone(collider));
        }
    }

    public void SpawnObject(Collider2D zoneCollider, Collider2D playerCollider)
    {
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].collider == zoneCollider)
            {
                Player pl = null;
                entityInZone = zones[i];
                entityInZone.playerInside++;
                if (playerCollider.GetComponent<Player>() && !entityInZone.players.Contains(playerCollider.GetComponent<Player>()))
                {
                    pl = playerCollider.GetComponent<Player>();
                    entityInZone.players.Add(playerCollider.GetComponent<Player>());
                }
                zones[i] = entityInZone;

                #region Tree
                for(int e = 0; e < zones[i].trees.Count; e++)
                {
                    if (zones[i].playerInside == 0) return;
                    if (zones[i].trees[e].actual == null)
                    {
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(zones[i].trees[e].pos, 2f, invalidSpawnLayers);
                        if (colliders.Length == 0)
                        {
                            // spawn a random object from the list
                            for (int j = 0; j < tree.Count; j++)
                            {
                                if (tree[j].name == zones[i].trees[e].typeName)
                                {
                                    inst = Instantiate(tree[j], zones[i].trees[e].pos, Quaternion.identity);
                                    inst.GetComponent<Tree>().health = zones[i].trees[e].health;
                                    zoneEntity = zones[i].trees[e];
                                    zoneEntity.actual = inst;
                                    zones[i].trees[e] = zoneEntity;
                                    NetworkServer.Spawn(inst);
                                }
                            }                          
                        }
                    }
                }

                for(int x = zones[i].trees.Count; x < maxTree; x++)
                {
                    if (zones[i].playerInside == 0) return;
                    Vector2 spawnPoint = GetRandomValidSpawnPoint(zoneCollider, invalidSpawnLayers);

                    if (spawnPoint == Vector2.one) continue;
                    // check if spawn position is valid
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, invalidSpawnLayers);
                    if (colliders.Length == 0)
                    {
                        // spawn a random object from the list
                        inst = Instantiate(tree[UnityEngine.Random.Range(0, tree.Count)], spawnPoint, Quaternion.identity);
                        NetworkServer.Spawn(inst);
                        zones[i].trees.Add(new ZoneEntity()
                        {
                            pos = spawnPoint,
                            typeName = inst.name.Replace("(Clone)",""),
                            health = inst.GetComponent<BuildingAccessory>().health,
                            actual = inst
                        });
                    }
                }
                #endregion

                #region Rock
                for (int e = 0; e < zones[i].rocks.Count; e++)
                {
                    if (zones[i].playerInside == 0) return;
                    if (zones[i].rocks[e].actual == null)
                    {
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(zones[i].rocks[e].pos, 1f, invalidSpawnLayers);
                        if (colliders.Length == 0)
                        {
                            // spawn a random object from the list
                            for (int j = 0; j < rock.Count; j++)
                            {
                                if (rock[j].name == zones[i].rocks[e].typeName)
                                {
                                    inst = Instantiate(rock[j], zones[i].rocks[e].pos, Quaternion.identity);
                                    inst.GetComponent<Rock>().health = zones[i].rocks[e].health;
                                    zoneEntity = zones[i].rocks[e];
                                    zoneEntity.actual = inst;
                                    zones[i].rocks[e] = zoneEntity;
                                    NetworkServer.Spawn(inst);
                                }
                            }
                        }
                    }
                }

                for (int x = zones[i].rocks.Count; x < maxRocks; x++)
                {
                    if (zones[i].playerInside == 0) return;
                    Vector2 spawnPoint = GetRandomValidSpawnPoint(zoneCollider, invalidSpawnLayers);

                    if (spawnPoint == Vector2.one) continue;
                    // check if spawn position is valid
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 2f, invalidSpawnLayers);
                    if (colliders.Length == 0)
                    {
                        // spawn a random object from the list
                        inst = Instantiate(rock[UnityEngine.Random.Range(0, rock.Count)], spawnPoint, Quaternion.identity);
                        NetworkServer.Spawn(inst);
                        zones[i].rocks.Add(new ZoneEntity()
                        {
                            pos = spawnPoint,
                            typeName = inst.name.Replace("(Clone)", ""),
                            health = inst.GetComponent<BuildingAccessory>().health,
                            actual = inst
                        });
                    }
                }
                #endregion

                #region Zombie
                for (int e = 0; e < zones[i].zombie.Count; e++)
                {
                    if (zones[i].playerInside == 0) return;
                    if (zones[i].zombie[e].actual == null)
                    {
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(zones[i].zombie[e].pos, 1f, invalidSpawnLayers);
                        if (colliders.Length == 0)
                        {
                            // spawn a random object from the list
                            for (int j = 0; j < zombie.Count; j++)
                            {
                                if (zombie[j].name == zones[i].zombie[e].typeName)
                                {
                                    if (NavMesh2D.SamplePosition(zones[i].zombie[e].pos, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas))
                                    {
                                        inst = Instantiate(zombie[j], zones[i].zombie[e].pos, Quaternion.identity);
                                        inst.GetComponent<Monster>().health.current = Convert.ToInt32(zones[i].zombie[e].health);
                                        zoneEntity = zones[i].zombie[e];
                                        zoneEntity.actual = inst;
                                        zones[i].zombie[e] = zoneEntity;
                                        NetworkServer.Spawn(inst);
                                    }
                                }
                            }
                        }
                    }
                }

                for (int x = zones[i].zombie.Count; x < maxZombie; x++)
                {
                    if (zones[i].playerInside == 0) return;
                    Vector2 spawnPoint = GetRandomValidSpawnPoint(zoneCollider, invalidSpawnLayers);

                    if (spawnPoint == Vector2.one) continue;
                    // check if spawn position is valid
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 2f, invalidSpawnLayers);
                    if (colliders.Length == 0)
                    {
                        if (NavMesh2D.SamplePosition(spawnPoint, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas))
                        {
                            inst = Instantiate(zombie[UnityEngine.Random.Range(0, zombie.Count)], spawnPoint, Quaternion.identity);
                            NetworkServer.Spawn(inst);
                            zones[i].zombie.Add(new ZoneEntity()
                            {
                                pos = spawnPoint,
                                typeName = inst.name.Replace("(Clone)", ""),
                                health = Convert.ToInt32(inst.GetComponent<Monster>().health),
                                actual = inst
                            });
                        }
                    }
                }
                #endregion

                #region NotAggressiveAnimal
                for (int e = 0; e < zones[i].notAggresiveAnimal.Count; e++)
                {
                    if (zones[i].playerInside == 0) return;
                    if (zones[i].notAggresiveAnimal[e].actual == null)
                    {
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(zones[i].notAggresiveAnimal[e].pos, 1f, invalidSpawnLayers);
                        if (colliders.Length == 0)
                        {
                            // spawn a random object from the list
                            for (int j = 0; j < notAggressiveAnimal.Count; j++)
                            {
                                if (notAggressiveAnimal[j].name == zones[i].notAggresiveAnimal[e].typeName)
                                {
                                    if (NavMesh2D.SamplePosition(zones[i].notAggresiveAnimal[e].pos, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas))
                                    {
                                        inst = Instantiate(notAggressiveAnimal[j], zones[i].notAggresiveAnimal[e].pos, Quaternion.identity);
                                        inst.GetComponent<Monster>().health.current = Convert.ToInt32(zones[i].notAggresiveAnimal[e].health);
                                        zoneEntity = zones[i].notAggresiveAnimal[e];
                                        zoneEntity.actual = inst;
                                        zones[i].notAggresiveAnimal[e] = zoneEntity;
                                        NetworkServer.Spawn(inst);
                                    }
                                }
                            }
                        }
                    }
                }

                for (int x = zones[i].notAggresiveAnimal.Count; x < maxNotAggressiveAnimal; x++)
                {
                    if (zones[i].playerInside == 0) return;
                    Vector2 spawnPoint = GetRandomValidSpawnPoint(zoneCollider, invalidSpawnLayers);
                    
                    if (spawnPoint == Vector2.one) continue;
                    // check if spawn position is valid
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, invalidSpawnLayers);
                    if (colliders.Length == 0)
                    {
                        if (NavMesh2D.SamplePosition(spawnPoint, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas))
                        {
                            inst = Instantiate(notAggressiveAnimal[UnityEngine.Random.Range(0, notAggressiveAnimal.Count)], spawnPoint, Quaternion.identity);
                            NetworkServer.Spawn(inst);
                            zones[i].notAggresiveAnimal.Add(new ZoneEntity()
                            {
                                pos = spawnPoint,
                                typeName = inst.name.Replace("(Clone)", ""),
                                health = Convert.ToInt32(inst.GetComponent<Monster>().health),
                                actual = inst
                            });
                        }
                    }
                }
                #endregion

                #region AggressiveAnimal
                for (int e = 0; e < zones[i].aggresiveAnimal.Count; e++)
                {
                    if (zones[i].playerInside == 0) return;
                    if (zones[i].aggresiveAnimal[e].actual == null)
                    {
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(zones[i].aggresiveAnimal[e].pos, 1f, invalidSpawnLayers);
                        if (colliders.Length == 0)
                        {
                            // spawn a random object from the list
                            for (int j = 0; j < aggressiveAnimal.Count; j++)
                            {
                                if (notAggressiveAnimal[j].name == zones[i].aggresiveAnimal[e].typeName)
                                {
                                    if (NavMesh2D.SamplePosition(zones[i].aggresiveAnimal[e].pos, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas))
                                    {
                                        inst = Instantiate(aggressiveAnimal[j], zones[i].aggresiveAnimal[e].pos, Quaternion.identity);
                                        inst.GetComponent<Monster>().health.current = Convert.ToInt32(zones[i].aggresiveAnimal[e].health);
                                        zoneEntity = zones[i].aggresiveAnimal[e];
                                        zoneEntity.actual = inst;
                                        zones[i].aggresiveAnimal[e] = zoneEntity;
                                        NetworkServer.Spawn(inst);
                                    }
                                }
                            }
                        }
                    }
                }

                for (int x = zones[i].aggresiveAnimal.Count; x < maxAggressiveAnimal; x++)
                {
                    if (zones[i].playerInside == 0) return;
                    Vector2 spawnPoint = GetRandomValidSpawnPoint(zoneCollider, invalidSpawnLayers);
                    
                    if (spawnPoint == Vector2.one) continue;
                    // check if spawn position is valid
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, invalidSpawnLayers);
                    if (colliders.Length == 0)
                    {
                        if (NavMesh2D.SamplePosition(spawnPoint, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas))
                        {
                            inst = Instantiate(aggressiveAnimal[UnityEngine.Random.Range(0, aggressiveAnimal.Count)], spawnPoint, Quaternion.identity);
                            NetworkServer.Spawn(inst);
                            zones[i].aggresiveAnimal.Add(new ZoneEntity()
                            {
                                pos = spawnPoint,
                                typeName = inst.name.Replace("(Clone)", ""),
                                health = Convert.ToInt32(inst.GetComponent<Monster>().health),
                                actual = inst
                            });
                        }
                    }
                }
                #endregion

                #region Flower
                for (int e = 0; e < zones[i].flower.Count; e++)
                {
                    if (zones[i].playerInside == 0) return;
                    if (zones[i].flower[e].actual == null)
                    {
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(zones[i].flower[e].pos, 1f, invalidSpawnLayers);
                        if (colliders.Length == 0)
                        {
                            // spawn a random object from the list
                            for (int j = 0; j < flower.Count; j++)
                            {
                                if (flower[j].name == zones[i].flower[e].typeName)
                                {
                                    inst = Instantiate(flower[j], zones[i].flower[e].pos, Quaternion.identity);
                                    //inst.GetComponent<Rock>().health = zones[i].flower[e].health;
                                    zoneEntity = zones[i].flower[e];
                                    zoneEntity.actual = inst;
                                    zones[i].flower[e] = zoneEntity;
                                    NetworkServer.Spawn(inst);
                                }
                            }
                        }
                    }
                }

                while (zones[i].flower.Count < maxFlower)
                {
                    if (zones[i].playerInside == 0) return;
                    Vector2 spawnPoint = GetRandomValidSpawnPoint(zoneCollider, invalidSpawnLayers);

                    // check if spawn position is valid
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, invalidSpawnLayers);
                    if (colliders.Length == 0)
                    {
                        // spawn a random object from the list
                        inst = Instantiate(flower[UnityEngine.Random.Range(0, flower.Count)], spawnPoint, Quaternion.identity);
                        NetworkServer.Spawn(inst);
                        zones[i].flower.Add(new ZoneEntity()
                        {
                            pos = spawnPoint,
                            typeName = inst.name.Replace("(Clone)", ""),
                            health = 0,
                            actual = inst
                        });
                    }
                }
                #endregion

                #region Mushroom
                for (int e = 0; e < zones[i].mushroom.Count; e++)
                {
                    if (zones[i].playerInside == 0) return;
                    if (zones[i].mushroom[e].actual == null)
                    {
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(zones[i].mushroom[e].pos, 1f, invalidSpawnLayers);
                        if (colliders.Length == 0)
                        {
                            // spawn a random object from the list
                            for (int j = 0; j < mushroom.Count; j++)
                            {
                                if (mushroom[j].name == zones[i].mushroom[e].typeName)
                                {
                                    inst = Instantiate(mushroom[j], zones[i].mushroom[e].pos, Quaternion.identity);
                                    //inst.GetComponent<Rock>().health = zones[i].flower[e].health;
                                    zoneEntity = zones[i].mushroom[e];
                                    zoneEntity.actual = inst;
                                    zones[i].mushroom[e] = zoneEntity;
                                    NetworkServer.Spawn(inst);
                                }
                            }
                        }
                    }
                }

                while (zones[i].mushroom.Count < maxMushroom)
                {
                    if (zones[i].playerInside == 0) return;
                    Vector2 spawnPoint = GetRandomValidSpawnPoint(zoneCollider, invalidSpawnLayers);

                    // check if spawn position is valid
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, invalidSpawnLayers);
                    if (colliders.Length == 0)
                    {
                        // spawn a random object from the list
                        inst = Instantiate(mushroom[UnityEngine.Random.Range(0, mushroom.Count)], spawnPoint, Quaternion.identity);
                        NetworkServer.Spawn(inst);
                        zones[i].mushroom.Add(new ZoneEntity()
                        {
                            pos = spawnPoint,
                            typeName = inst.name.Replace("(Clone)", ""),
                            health = 0,
                            actual = inst
                        });
                    }
                }
                #endregion

                #region Grass

                if (zones[i].grass.Count == 0)
                {
                    if (zones[i].playerInside == 0) return;

                    for (int u = 0; u < maxGrass; u++)
                    {
                        inst = Instantiate(grass, Utilities.RandomPointInRectangle(new Vector2(zones[i].collider.bounds.min.x, zones[i].collider.bounds.min.y), ((BoxCollider2D)zones[i].collider).size, 15), Quaternion.identity);
                        NetworkServer.Spawn(inst);
                        if (isServer && !isClient) inst.SetActive(false);
                        zones[i].grass.Add(inst.GetComponent<IrregularColliderSpawner>());
                    }
                }
                else
                {
                    if (zones[i].playerInside == 0) return;

                    for (int u = 0; u < zones[i].grass.Count; u++)
                    {
                        NetworkIdentity id = null;
                        id = zones[i].grass[u].gameObject.GetComponent<NetworkIdentity>();
                        for (int e = 0; e < zones[i].grass[u].spawnedObjects.Count; e++)
                        {
                            if (zones[i].grass[u].spawnedObjects[e].obj == null && !zones[i].grass[u].spawnedObjects[e].overlay)
                            {
                                GameObject g = Instantiate(zones[i].grass[u].prefabToSpawn, zones[i].grass[u].spawnedObjects[e].position, Quaternion.identity);
                                var spawnedObj = g.GetComponent<SpawnedObject>();
                                spawnedObj.index = zones[i].grass[u].spawnedObjects[e].index;
                                spawnedObj.parent = id;
                                NetworkServer.Spawn(g);

                            }
                        }
                    }

                    //if (isServer && !isClient)
                    //{
                    //    zones[i].grass[0].actual.SetActive(false);
                    //}
                    //pl.playerCallback.TargetManageGrass(zones[i].grass[0].actual.GetComponent<NetworkIdentity>(), true);
                }
                #endregion

                #region Grain

                if (zones[i].grains.Count == 0)
                {
                    if (zones[i].playerInside == 0) return;

                    for (int u = 0; u < maxGrain; u++)
                    {
                        inst = Instantiate(grain, Utilities.RandomPointInRectangle(new Vector2(zones[i].collider.bounds.min.x, zones[i].collider.bounds.min.y), ((BoxCollider2D)zones[i].collider).size,15), Quaternion.identity);
                        NetworkServer.Spawn(inst);
                        if (isServer && !isClient) inst.SetActive(false);
                            zones[i].grains.Add(inst.GetComponent<IrregularColliderSpawner>());
                    }
                }
                else
                {
                    if (zones[i].playerInside == 0) return;

                    for (int u = 0; u < zones[i].grains.Count; u++)
                    {
                        NetworkIdentity id = null;
                        id = zones[i].grains[u].gameObject.GetComponent<NetworkIdentity>();
                        for(int e = 0; e < zones[i].grains[u].spawnedObjects.Count; e++)
                        {
                            if (zones[i].grains[u].spawnedObjects[e].obj == null && !zones[i].grains[u].spawnedObjects[e].overlay)
                            {
                                GameObject g = Instantiate(zones[i].grains[u].prefabToSpawn, zones[i].grains[u].spawnedObjects[e].position, Quaternion.identity);
                                var spawnedObj = g.GetComponent<SpawnedObject>();
                                spawnedObj.index = zones[i].grains[u].spawnedObjects[e].index;
                                spawnedObj.parent = id;
                                NetworkServer.Spawn(g);

                            }
                        }
                    }

                    //if (isServer && !isClient)
                    //{
                    //    zones[i].grass[0].actual.SetActive(false);
                    //}
                    //pl.playerCallback.TargetManageGrass(zones[i].grass[0].actual.GetComponent<NetworkIdentity>(), true);
                }
                #endregion

            }
        }
    }

    public Vector3 CalculateGrassPosition(Collider2D coll, LayerMask layerMask)
    {
        pos = GetRandomValidSpawnPoint(coll,layerMask);
        return new Vector3(pos.x,pos.y,0.0f);
    }

    public void DespawnObject(Collider2D zoneCollider, Collider2D playerCollider)
    {
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].collider == zoneCollider)
            {
                Player pl = null;
                entityInZone = zones[i];
                entityInZone.playerInside--;
                if (playerCollider.GetComponent<Player>() && entityInZone.players.Contains(playerCollider.GetComponent<Player>()))
                {
                    pl = playerCollider.GetComponent<Player>();
                    entityInZone.players.Remove(playerCollider.GetComponent<Player>());
                }
                zones[i] = entityInZone;

                for (int e = 0; e < zones[i].trees.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].trees[e].actual != null)
                    {
                        zoneEntity = zones[i].trees[e];
                        zoneEntity.health = zoneEntity.actual.GetComponent<BuildingAccessory>().health;
                        if (NetworkServer.active)
                        {
                            GameObject g = zoneEntity.actual;
                            NetworkServer.Destroy(g.gameObject);
                        }
                        zoneEntity.actual = null;
                        zones[i].trees[e] = zoneEntity;
                    }
                }

                for (int e = 0; e < zones[i].rocks.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].rocks[e].actual != null)
                    {
                        zoneEntity = zones[i].rocks[e];
                        zoneEntity.health = zoneEntity.actual.GetComponent<BuildingAccessory>().health;
                        if (NetworkServer.active)
                        {
                            GameObject g = zoneEntity.actual;
                            NetworkServer.Destroy(g.gameObject);
                        }
                        zoneEntity.actual = null;
                        zones[i].rocks[e] = zoneEntity;
                    }
                }

                for (int e = 0; e < zones[i].zombie.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].zombie[e].actual != null)
                    {
                        if(zones[i].zombie[e].actual.GetComponent<Monster>())
                            if (zones[i].zombie[e].actual.GetComponent<Monster>().target != null) continue;
                        zoneEntity = zones[i].zombie[e];
                        zoneEntity.health = zoneEntity.actual.GetComponent<Monster>().health.current;
                        if (NetworkServer.active)
                        {
                            GameObject g = zoneEntity.actual;
                            NetworkServer.Destroy(g.gameObject);
                        }
                        zoneEntity.actual = null;
                        zones[i].zombie[e] = zoneEntity;
                    }
                }

                for (int e = 0; e < zones[i].notAggresiveAnimal.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].notAggresiveAnimal[e].actual.GetComponent<Monster>()) 
                        if (zones[i].notAggresiveAnimal[e].actual.GetComponent<Monster>().target != null) continue;
                    if (zones[i].notAggresiveAnimal[e].actual != null)
                    {
                        zoneEntity = zones[i].notAggresiveAnimal[e];
                        zoneEntity.health = zoneEntity.actual.GetComponent<Monster>().health.current;
                        if (NetworkServer.active)
                        {
                            GameObject g = zoneEntity.actual;
                            NetworkServer.Destroy(g.gameObject);
                        }
                        zoneEntity.actual = null;
                        zones[i].notAggresiveAnimal[e] = zoneEntity;
                    }
                }

                for (int e = 0; e < zones[i].aggresiveAnimal.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].aggresiveAnimal[e].actual.GetComponent<Monster>())
                        if (zones[i].aggresiveAnimal[e].actual.GetComponent<Monster>().target != null) continue;
                    if (zones[i].aggresiveAnimal[e].actual != null)
                    {
                        zoneEntity = zones[i].aggresiveAnimal[e];
                        zoneEntity.health = zoneEntity.actual.GetComponent<Monster>().health.current;
                        if (NetworkServer.active)
                        {
                            GameObject g = zoneEntity.actual;
                            NetworkServer.Destroy(g.gameObject);
                        }
                        zoneEntity.actual = null;
                        zones[i].aggresiveAnimal[e] = zoneEntity;
                    }
                }

                for (int e = 0; e < zones[i].flower.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].flower[e].actual != null)
                    {
                        zoneEntity = zones[i].flower[e];
                        zoneEntity.health = 0;
                        if (NetworkServer.active)
                        {
                            GameObject g = zoneEntity.actual;
                            NetworkServer.Destroy(g.gameObject);
                        }
                        zoneEntity.actual = null;
                        zones[i].flower[e] = zoneEntity;
                    }
                }

                for (int e = 0; e < zones[i].grains.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].grains[e].spawnedObjects.Count > 0)
                    {
                        for(int j = 0; j < zones[i].grains[e].spawnedObjects.Count; j++)
                        {
                            if (zones[i].grains[e].spawnedObjects[j].obj != null)
                            {
                                if (NetworkServer.active)
                                {
                                    NetworkServer.Destroy(zones[i].grains[e].spawnedObjects[j].obj);
                                }
                            }
                        }
                    }
                }

                for (int e = 0; e < zones[i].grass.Count; e++)
                {
                    if (zones[i].playerInside > 0) return;
                    if (zones[i].grass[e].spawnedObjects.Count > 0)
                    {
                        for (int j = 0; j < zones[i].grass[e].spawnedObjects.Count; j++)
                        {
                            if (zones[i].grass[e].spawnedObjects[j].obj != null)
                            {
                                if (NetworkServer.active)
                                {
                                    NetworkServer.Destroy(zones[i].grass[e].spawnedObjects[j].obj);
                                }
                            }
                        }
                    }
                }


                //if (zones[i].playerInside > 0) return;
                //if (zones[i].grass.Count > 0)
                //{
                //    pl.playerCallback.TargetManageGrass(zones[i].grass[0].actual.GetComponent<NetworkIdentity>(), false);
                //    zones[i].grass[0].actual.SetActive(false);
                //}
            }
        }
    }

    private Vector2 GetRandomValidSpawnPoint(Collider2D coll, LayerMask layerMask)
    {
        int maxTry = 5;
        int current = 0;

        // get a random point within the spawn area
        Vector2 spawnPoint = new Vector2(
            UnityEngine.Random.Range(coll.bounds.min.x, coll.bounds.max.x),
            UnityEngine.Random.Range(coll.bounds.min.y, coll.bounds.max.y)
        );

        // check if spawn point is valid
        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, layerMask);
        while (colliders.Length > 0 && current < maxTry)
        {
            current++;
            spawnPoint = new Vector2(
                UnityEngine.Random.Range(coll.bounds.min.x, coll.bounds.max.x),
                UnityEngine.Random.Range(coll.bounds.min.y, coll.bounds.max.y)
            );
            colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, layerMask);
        }

        if (colliders.Length > 0) return Vector2.one;

        // return the position of the valid spawn point
        return spawnPoint;
    }


    public void RemoveFromZone(Collider2D zone, GameObject entity)
    {
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].collider == zone)
            {
                for(int e = 0; e < zones[i].trees.Count; e++)
                {
                    if (zones[i].trees[e].actual != null && zones[i].trees[e].actual == entity)
                    {
                        zones[i].trees.RemoveAt(e);
                    }
                }

                for (int e = 0; e < zones[i].rocks.Count; e++)
                {
                    if (zones[i].rocks[e].actual != null && zones[i].rocks[e].actual == entity)
                    {
                        zones[i].rocks.RemoveAt(e);
                    }
                }

                for (int e = 0; e < zones[i].zombie.Count; e++)
                {
                    if (zones[i].zombie[e].actual != null && zones[i].zombie[e].actual == entity)
                    {
                        zones[i].zombie.RemoveAt(e);
                    }
                }

                for (int e = 0; e < zones[i].notAggresiveAnimal.Count; e++)
                {
                    if (zones[i].notAggresiveAnimal[e].actual != null && zones[i].notAggresiveAnimal[e].actual == entity)
                    {
                        zones[i].notAggresiveAnimal.RemoveAt(e);
                    }
                }

                for (int e = 0; e < zones[i].aggresiveAnimal.Count; e++)
                {
                    if (zones[i].aggresiveAnimal[e].actual != null && zones[i].aggresiveAnimal[e].actual == entity)
                    {
                        zones[i].aggresiveAnimal.RemoveAt(e);
                    }
                }


                for (int e = 0; e < zones[i].flower.Count; e++)
                {
                    if (zones[i].flower[e].actual != null && zones[i].flower[e].actual == entity)
                    {
                        zones[i].flower.RemoveAt(e);
                    }
                }
            }
        }
    }
}

public class GenericColliderEvent : MonoBehaviour
{
    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (!NetworkServer.active) return;
    //    // Chiamata all'evento personalizzato quando un collider entra in collisione con questo collider
    //    SpawnManager.singleton.OnTriggerEnter2D(GetComponent<Collider2D>(), other.GetComponentInParent<Player>());
    //}

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (!NetworkServer.active) return;
    //    // Chiamata all'evento personalizzato quando un collider entra in collisione con questo collider
    //    SpawnManager.singleton.OnTriggerExit2D(GetComponent<Collider2D>(), other.GetComponentInParent<Player>());
    //}
}
