using System.Collections;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    public GameObject sprite1; // Primo sprite
    public GameObject sprite2; // Secondo sprite
    public GameObject sprite3; // Terzo sprite
    public float moveDistance = 5f; // Distanza di spostamento verso l'alto
    public float moveSpeed = 2f; // Velocità di movimento
    public float waitTime = 1f; // Tempo di attesa prima di ripetere l'animazione

    private Vector3 startPos1, startPos2, startPos3;

    void OnEnable()
    {
        // Imposta le posizioni di partenza delle tre sprite
        startPos1 = sprite1.transform.position;
        startPos2 = sprite2.transform.position;
        startPos3 = sprite3.transform.position;

        // Avvia l'animazione
        StartCoroutine(AnimateSprites());
    }

    private void OnDisable()
    {
        StopCoroutine(AnimateSprites());
        sprite1.transform.position = startPos1;
        sprite2.transform.position = startPos2;
        sprite3.transform.position = startPos3;
    }

    IEnumerator AnimateSprites()
    {
        while (true)
        {
            // Muovi il primo sprite
            yield return StartCoroutine(MoveSprite(sprite1, startPos1, moveDistance));

            // Muovi il secondo sprite quando il primo è a metà
            yield return StartCoroutine(MoveSprite(sprite2, startPos2, moveDistance));

            // Muovi il terzo sprite quando il secondo è a metà
            yield return StartCoroutine(MoveSprite(sprite3, startPos3, moveDistance));

            // Attendi che tutti tornino alla posizione iniziale
            yield return StartCoroutine(ReturnSpritesToStart());

            // Attendi il tempo specificato prima di ripetere l'animazione
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator MoveSprite(GameObject sprite, Vector3 startPos, float distance)
    {
        float currentHeight = 0f;
        Vector3 targetPos = startPos + Vector3.up * distance;

        // Muovi lo sprite verso l'alto
        while (currentHeight < distance)
        {
            float step = moveSpeed * Time.deltaTime;
            sprite.transform.position = Vector3.MoveTowards(sprite.transform.position, targetPos, step);
            currentHeight += step;

            // Attendi fino al prossimo frame
            yield return null;
        }
    }

    IEnumerator ReturnSpritesToStart()
    {
        // Riporta tutte le sprite alla posizione iniziale contemporaneamente
        while (sprite1.transform.position != startPos1 ||
               sprite2.transform.position != startPos2 ||
               sprite3.transform.position != startPos3)
        {
            float step = moveSpeed * Time.deltaTime;
            sprite1.transform.position = Vector3.MoveTowards(sprite1.transform.position, startPos1, step);
            sprite2.transform.position = Vector3.MoveTowards(sprite2.transform.position, startPos2, step);
            sprite3.transform.position = Vector3.MoveTowards(sprite3.transform.position, startPos3, step);

            yield return null;
        }
    }
}
