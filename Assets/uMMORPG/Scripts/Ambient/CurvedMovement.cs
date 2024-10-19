using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class CurvedMovement : NetworkBehaviour
{
    public Transform startEntity;
    public Vector3 startPoint; // Il punto di partenza
    public Vector3 endPoint; // Il punto di destinazione
    public float height = 5.0f; // L'altezza della curva

    private float progress = 0.0f; // La posizione corrente del movimento

    [SyncVar] public Item itemToDrop;
    [SyncVar] public int amountItem;
    [SyncVar] public long gold;

    //public BoxCollider2D collider;
    //public Collider2D[] colliderHits = new Collider2D[0];
    //public LayerMask layerToCheck;

    public Image spriteRenderer;
    public TextMeshProUGUI itemName;

    public void SpawnAtPosition(Item itm, int amount, int inventoryIndex, int itemAmountToRemove, long goldToAdd = 0)
    {
        if (startEntity == null) SpawnAtPosition(itm, amount, inventoryIndex, itemAmountToRemove);
        else
        {
            gold = goldToAdd;
            Vector2 circle2D = UnityEngine.Random.insideUnitCircle * 4.0f;
            Vector3 position = startEntity.transform.position + new Vector3(circle2D.x, circle2D.y, 0);

            spriteRenderer.sprite = goldToAdd > 0 ? ImageManager.singleton.gold : itm.data.skinImages.Count > 0 ? itm.data.skinImages[itm.skin] : itm.data.image;
            spriteRenderer.preserveAspect = true;
            itemName.text = goldToAdd > 0 ? "GOLD" + "(" + goldToAdd + ")" : itm.data.name + " (" + amount + ")";
            amountItem = amount;
            //colliderHits = new Collider2D[0];

            //colliderHits = Physics2D.OverlapBoxAll(position, new Vector2(collider.size.x, collider.size.y), 0.0f, layerToCheck);

            //if (colliderHits.Length == 0)
            //{
            startPoint = startEntity.transform.position;
            endPoint = position;
            itemToDrop = itm;
            if (inventoryIndex > -1)
            {
                if (startEntity.GetComponent<Player>())
                {
                    ItemSlot slot = startEntity.GetComponent<Player>().inventory.slots[inventoryIndex];
                    slot.DecreaseAmount(itemAmountToRemove);
                    startEntity.GetComponent<Player>().inventory.slots[inventoryIndex] = slot;
                    NetworkServer.Spawn(this.gameObject);
                }
                if (startEntity.GetComponent<Monster>())
                {
                    ItemSlot slot = startEntity.GetComponent<Monster>().inventory.slots[inventoryIndex];
                    slot.DecreaseAmount(itemAmountToRemove);
                    startEntity.GetComponent<Monster>().inventory.slots[inventoryIndex] = slot;
                    NetworkServer.Spawn(this.gameObject);
                }
            }
            else
            {
                NetworkServer.Spawn(this.gameObject);
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        spriteRenderer.sprite = gold > 0 ? ImageManager.singleton.gold : itemToDrop.data.skinImages.Count > 0 ? itemToDrop.data.skinImages[itemToDrop.skin] : itemToDrop.data.image;
        itemName.text = gold > 0 ? "GOLD" + "(" + gold + ")" : itemToDrop.data.name + " (" + amountItem + ")";
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

        // Calcola la posizione corrente utilizzando Lerp tra i punti intermedi
        return Vector3.Lerp(Vector3.Lerp(mid1, mid2, progress), Vector3.Lerp(mid2, end, progress), progress);
    }
}
