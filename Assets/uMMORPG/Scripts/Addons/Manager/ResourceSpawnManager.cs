using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UIElements;

public class ResourceSpawnManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> spawnableObjects;
    [SerializeField] private int maxSpawnedObjects = 10;
    [SerializeField] private int maxSpawnGroups = 5;
    [SerializeField] private LayerMask invalidSpawnLayers;
    public PolygonCollider2D spawnArea;
    [SerializeField] private int maxSpawn = 100;
    [SerializeField] private int alreadySpawned;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // spawn objects at random intervals
        StartCoroutine(SpawnObjects());
    }

    private IEnumerator<WaitForSeconds> SpawnObjects()
    {
        while (spawnArea)
        {
            // generate a random number of groups to spawn
            int numGroups = Random.Range(1, maxSpawnGroups + 1);

            for (int i = 0; i < numGroups; i++)
            {
                // generate a random spawn point within the spawn area
                //Vector2 spawnPoint = GetRandomValidSpawnPoint();

                // generate a random number of objects to spawn
                int numObjects = Random.Range(1, maxSpawnedObjects + 1);

                // spawn objects in a random radius around the spawn point
                for (int j = 0; j < numObjects; j++)
                {
                    if (alreadySpawned < maxSpawn)
                    {
                        //Vector2 spawnPosition = spawnPoint + Random.insideUnitCircle * 5f;

                        //// check if spawn position is valid
                        //Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, 1f, invalidSpawnLayers);
                        //if (colliders.Length == 0)
                        //{
                        //    // spawn a random object from the list
                        //    GameObject spawnObject = spawnableObjects[Random.Range(0, spawnableObjects.Count)];
                        //    GameObject newObj = Instantiate(spawnObject, spawnPosition, Quaternion.identity);
                        //    //if (NavMesh2D.SamplePosition(spawnPosition, out NavMeshHit2D _, 0.1f, NavMesh2D.AllAreas))
                        //    //{
                        //    NetworkServer.Spawn(newObj);
                        //    alreadySpawned++;
                        //    //}
                        //}

                        // Ottieni il rettangolo delimitatore del collider generico
                        Rect colliderBounds = GetColliderBounds(spawnArea);

                        // Genera una posizione casuale all'interno del rettangolo delimitatore
                        Vector2 randomPosition = GetRandomPositionInsideBounds(colliderBounds);

                        // Verifica se la posizione casuale è contenuta all'interno del collider generico
                        if (IsPointInsideCollider(randomPosition, spawnArea))
                        {
                            Collider2D[] colliders = Physics2D.OverlapCircleAll(randomPosition, 1f, invalidSpawnLayers);
                            if (colliders.Length == 0)
                            {
                                // spawn a random object from the list
                                GameObject spawnObject = spawnableObjects[Random.Range(0, spawnableObjects.Count)];
                                GameObject newObj = Instantiate(spawnObject, randomPosition, Quaternion.identity);

                                NetworkServer.Spawn(newObj);
                                alreadySpawned++;
                            }
                        } 
                    }
                }
            }

            // wait for some time before spawning again
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    //private bool IsColliderInsidePolygon(Collider2D genericCollider, PolygonCollider2D polygonCollider)
    //{
    //    // Crea un ContactFilter2D per filtrare solo i collider PolygonCollider2D
    //    ContactFilter2D filter = new ContactFilter2D();
    //    filter.SetLayerMask(Physics2D.GetLayerCollisionMask(genericCollider.gameObject.layer)); // Usa il layer del collider generico

    //    // Crea un array per memorizzare i collider sovrapposti
    //    Collider2D[] results = new Collider2D[1];

    //    // Esegue il check di sovrapposizione del collider generico
    //    int numOverlaps = Physics2D.OverlapCollider(genericCollider, filter, results);

    //    // Controlla se il collider generico è contenuto completamente nel Polygon Collider 2D
    //    if (numOverlaps > 0 && results[0] == polygonCollider)
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    private Rect GetColliderBounds(Collider2D collider)
    {
        // Ottieni il rettangolo di delimitazione del collider
        Bounds bounds = collider.bounds;
        return new Rect(bounds.min, bounds.size);
    }

    private Vector2 GetRandomPositionInsideBounds(Rect bounds)
    {
        // Genera una posizione casuale all'interno del rettangolo delimitatore
        float randomX = Random.Range(bounds.xMin, bounds.xMax);
        float randomY = Random.Range(bounds.yMin, bounds.yMax);
        return new Vector2(randomX, randomY);
    }

    private bool IsPointInsideCollider(Vector2 point, Collider2D collider)
    {
        // Controlla se il punto specificato è contenuto nel collider
        return collider.OverlapPoint(point);
    }

    private Vector2 GetRandomValidSpawnPoint()
    {
        // get a random point within the spawn area
        Vector2 spawnPoint = new Vector2(
            Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
            Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y)
        );

        // check if spawn point is valid
        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, invalidSpawnLayers);
        while (colliders.Length > 0)
        {
            spawnPoint = new Vector2(
                Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y)
            );
            colliders = Physics2D.OverlapCircleAll(spawnPoint, 1f, invalidSpawnLayers);
        }

        // return the position of the valid spawn point
        return spawnPoint;
    }
}
