using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BloodParticles : NetworkBehaviour
{
    public Transform startEntity;
    public SpriteRenderer spriteRenderer;

    public Vector3 startPoint; // Il punto di partenza
    public Vector3 endPoint; // Il punto di destinazione
    public float height = 5.0f; // L'altezza della curva

    private float progress = 0.0f; // La posizione corrente del movimento
    public SpriteScaler spriteScaler;

    public void SpawnAtPosition(int spriteIndex)
    {
        Vector2 circle2D = UnityEngine.Random.insideUnitCircle * 1.0f;
        Vector3 position = startEntity.transform.position + new Vector3(circle2D.x, circle2D.y, 0);
        startPoint = startEntity.transform.position;
        endPoint = position;
        spriteRenderer.sprite = ResourceManager.singleton.bloodSprites[spriteIndex];
        //spriteScaler.Adjust();
        spriteRenderer.transform.rotation = new Quaternion(spriteRenderer.transform.rotation.x, spriteRenderer.transform.rotation.y, Random.Range(0.0f, 360.0f),1);
        NetworkServer.Spawn(this.gameObject);
    }

    private void Update()
    {
        if (isServer)
        {
            progress += Time.deltaTime; // Aggiorna la posizione corrente in base al tempo trascorso

            if (progress > 1.0f) // Se l'oggetto ha raggiunto la destinazione
            {
                progress = 1.0f; // Fissa la posizione corrente al punto di destinazione
            }
            else
            {
                // Calcola la posizione corrente utilizzando Lerp e una curva di Bezier
                Vector3 currentPos = BezierCurve(startPoint, endPoint, height, progress);

                // Muovi l'oggetto alla nuova posizione
                transform.position = currentPos;
            }
        }
    }

    // Funzione per calcolare una curva di Bezier
    private Vector3 BezierCurve(Vector3 start, Vector3 end, float height, float progress)
    {
        // Calcola i punti intermedi utilizzando la formula della curva di Bezier
        Vector3 mid1 = Vector3.Lerp(start, end, progress);
        Vector3 mid2 = Vector3.Lerp(start, end, progress + 0.1f);
        mid2 += Vector3.up * height;

        spriteRenderer.transform.localScale = new Vector3(progress, progress, progress);

        // Calcola la posizione corrente utilizzando Lerp tra i punti intermedi
        return Vector3.Lerp(Vector3.Lerp(mid1, mid2, progress), Vector3.Lerp(mid2, end, progress), progress);
    }

    public void OnDestroy()
    {
        NetworkServer.Destroy(this.gameObject);
    }

}
