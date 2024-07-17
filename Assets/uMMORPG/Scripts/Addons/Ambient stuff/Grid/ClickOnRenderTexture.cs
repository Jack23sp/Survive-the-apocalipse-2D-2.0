using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickOnRenderTexture : MonoBehaviour
{
    public GameObject prefabToInstantiate; // Il prefab dell'oggetto da istanziare
    public RectTransform rectTransform;
    public BoxCollider2D boxCollider;
    public Transform anchor;
    public Vector3 size = new Vector3(2937.828f,1351.968f);

    void Update()
    {
        // Verifica se è stato fatto un click sinistro del mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Posizione del click rispetto al RectTransform
            Vector2 clickPosition = Input.mousePosition;

            // Calcolo delle coordinate locali rispetto al centro esatto del RectTransform
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, clickPosition, null, out localPoint);
            
            // Calcolo delle percentuali rispetto al centro del RectTransform
            float percentX = (localPoint.x - (- rectTransform.rect.width /2)) / ((rectTransform.rect.width / 2) - (- rectTransform.rect.width / 2)) * 100f;

            float percentY = (localPoint.y - (-rectTransform.rect.height / 2)) / ((rectTransform.rect.height / 2) - (-rectTransform.rect.height / 2)) * 100f;
            
            Debug.Log("Percentuale su X: " + percentX.ToString("0.00") + "%");
            Debug.Log("Percentuale su Y: " + percentY.ToString("0.00") + "%");

            float offsetX = Mathf.Abs(percentX * (size.x / 100));
            float offsetY = Mathf.Abs(percentY * (size.y / 100));

            Vector3 position = anchor.position + new Vector3(offsetX, offsetY, 0f);

            // Creazione o posizionamento dell'oggetto
            Instantiate(prefabToInstantiate, position, Quaternion.identity);
        }
    }
}
