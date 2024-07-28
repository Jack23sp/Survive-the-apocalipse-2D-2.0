using UnityEngine;
using Mirror;

public class SpawnedObject : NetworkBehaviour
{
    [SyncVar]
    public int index;
    [SyncVar]
    public  NetworkIdentity parent;
    public IrregularColliderSpawner spawner;
    [SyncVar(hook = nameof(Overlaychanged))]
    public bool hasOverlay;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D collider;
    public DamagableObject damagableObject;
    public ScriptableItem reward;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Initialize(index, parent);
        damagableObject.ambient = this;
    }

    public void Initialize(int newIndex, NetworkIdentity parentSpawner)
    {
        index = newIndex;
        parent = parentSpawner;
        spawner = parentSpawner.gameObject.GetComponent<IrregularColliderSpawner>();
        AmbientDecoration dec = new AmbientDecoration
        {
            overlay = hasOverlay, obj = this.gameObject, position = transform.position, index = newIndex
        };
        if (!spawner.spawnedObjects.Contains(dec)) spawner.spawnedObjects.Add(dec);
        Overlaychanged(false, false);
    }

    // Metodo per richiedere interazione con l'oggetto principale
    [Command]
    public void CmdRequestInteraction()
    {
        spawner.InteractWithChild(this);
    }

    // Metodo per gestire il click del mouse
    public void OnMouseDown()
    {
        if (isClient)
        {
            CmdRequestInteraction();
        }
    }

    [ClientRpc]
    public void RpcSyncState(int newIndex)
    {
        index = newIndex;
    }

    public void Overlaychanged (bool oldValue, bool newValue)
    {
        spriteRenderer.enabled = !newValue;
    }
}
