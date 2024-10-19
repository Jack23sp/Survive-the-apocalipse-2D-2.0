using UnityEngine;
using Mirror;
using System.Collections.Generic;

[System.Serializable]
public struct AmbientDecoration
{
    public int index;
    public Vector2 position;
    public GameObject obj;
    public bool overlay;
}

public class IrregularColliderSpawner : NetworkBehaviour
{
    public GameObject prefabToSpawn; // Prefab degli oggetti da spawnare
    public int gridResolution = 10; // Risoluzione della griglia per il riempimento
    public LayerMask layerMask; // Layer da controllare per le sovrapposizioni
    public int numberOfPoints = 10; // Numero di punti per il collider irregolare
    public float radius = 5.0f; // Raggio del cerchio per i punti casuali
    public float irregularity = 0.5f; // Grado di irregolarità dei bordi
    public int maxObjects = 100; // Numero massimo di oggetti da creare

    private PolygonCollider2D _polygonCollider;
    private Bounds _bounds;

    public List<AmbientDecoration> spawnedObjects = new List<AmbientDecoration>();
    public NetworkIdentity identity;

    void Awake()
    {
        // Configura il PolygonCollider2D con una forma irregolare
        _polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        _polygonCollider.isTrigger = true;
        _polygonCollider.points = GenerateRoundedPolygon(numberOfPoints, radius, irregularity);
    }

    void Start()
    {
        // Assicurati che questo script sia eseguito solo sul server
        if (!isServer) return;

        _bounds = _polygonCollider.bounds;

        FillAreaWithPrefabs();
    }

    private Vector2[] GenerateRoundedPolygon(int pointsCount, float radius, float irregularity)
    {
        Vector2[] points = new Vector2[pointsCount];
        float angleStep = 2 * Mathf.PI / pointsCount;

        for (int i = 0; i < pointsCount; i++)
        {
            float angle = i * angleStep;
            float distance = radius * (1 + Random.Range(-irregularity, irregularity));
            float x = Mathf.Cos(angle) * distance;
            float y = Mathf.Sin(angle) * distance;
            points[i] = new Vector2(x, y);
        }

        // Ordina i punti in senso orario per formare un poligono valido
        System.Array.Sort(points, (a, b) => Mathf.Atan2(a.y, a.x).CompareTo(Mathf.Atan2(b.y, b.x)));

        return points;
    }

    [Server]
    void FillAreaWithPrefabs()
    {
        int objectsCreated = 0;
        int attempts = 0;
        while (objectsCreated < maxObjects && attempts < maxObjects * 10)
        {
            float x = Random.Range(_bounds.min.x, _bounds.max.x);
            float y = Random.Range(_bounds.min.y, _bounds.max.y);
            Vector2 point = new Vector2(x, y);

            if (IsPointInPolygon(out point))
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(point, 0.1f, layerMask);
                if (colliders.Length == 0)
                {
                    GameObject obj = Instantiate(prefabToSpawn, point, Quaternion.identity);
                    var spawnedObj = obj.GetComponent<SpawnedObject>();
                    spawnedObj.index = objectsCreated;
                    spawnedObj.parent = identity;
                    NetworkServer.Spawn(obj);
                    objectsCreated++;
                    //Debug.Log($"Created object at {point}");
                }
                else
                {
                    //Debug.Log($"OverlapCircleAll found {colliders.Length} colliders at {point}");
                }
            }
            else
            {
                //Debug.Log($"Point {point} is not in the polygon.");
            }

            attempts++;
        }

        //Debug.Log($"Created {objectsCreated} objects out of {maxObjects} attempts.");
    }

    private bool IsPointInPolygon(out Vector2 result)
    {
        result = Vector2.zero;
        if (_polygonCollider == null)
        {
            //Debug.LogError("PolygonCollider2D non assegnato.");
            return false;
        }

        // Ottieni i bounds del PolygonCollider2D
        Bounds bounds = _polygonCollider.bounds;
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;

        for (int i = 0; i < 20; i++) // Tenta per maxAttempts volte di trovare un punto randomico
        {
            float randomX = Random.Range(min.x, max.x);
            float randomY = Random.Range(min.y, max.y);
            Vector2 randomPoint = new Vector2(randomX, randomY);

            if (_polygonCollider.OverlapPoint(randomPoint))
            {
                result = randomPoint;
                return true;
            }
        }

        // Se non troviamo un punto valido dopo maxAttempts tentativi, ritorniamo false
        return false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Sincronizza lo stato degli oggetti spawnati per i nuovi client
        foreach (var obj in spawnedObjects)
        {
            if (obj.obj != null)
            {
                obj.obj.GetComponent<SpawnedObject>().RpcSyncState(obj.obj.GetComponent<SpawnedObject>().index);
            }
        }
    }
}
