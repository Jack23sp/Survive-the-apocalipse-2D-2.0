using UnityEngine;

public class SpriteScaler : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float targetPixelPerUnit = 100f;

    public void Adjust()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        targetPixelPerUnit = UnityEngine.Random.Range(70, 1000);

        // Calcola la scala necessaria in base al pixel per unit della sprite
        float currentPixelPerUnit = spriteRenderer.sprite.pixelsPerUnit;
        float scaleRatio = targetPixelPerUnit / currentPixelPerUnit;
        Vector3 newScale = transform.localScale * scaleRatio;

        // Imposta la nuova scala
        spriteRenderer.transform.localScale = newScale;
    }
}
