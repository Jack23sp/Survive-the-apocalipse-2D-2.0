// simple script to always set y position to order in layer for visiblity
using UnityEngine;
using Mirror;

public class SortByDepth : MonoBehaviour
{
#pragma warning disable CS0109 // member does not hide accessible member
    new public SpriteRenderer renderer;
#pragma warning restore CS0109 // member does not hide accessible member
    private Canvas canvas;

    // precision is useful for cases where two players stand at
    //   y=0 and y=0.1, which would both be sortingOrder=0 otherwise
    public float precision = 100;

    // offset in case it's needed (e.g. for mounts that should be behind the
    // player, even if the player is above it in .y)
    public float offset = 0;
    public NetworkIdentity identity;
    public bool onlyOnStart;

    private void Awake()
    {
        if(!renderer) renderer = GetComponent<SpriteRenderer>();
        if(!canvas) canvas = GetComponent<Canvas>();
        SetOrder();
        if(onlyOnStart) { this.enabled = false; }
    }

    void Update()
    {
        if (renderer.enabled || canvas)
        {
            SetOrder();
        }
    }

    public void SetOrder()
    {
        if(canvas) canvas.sortingOrder = -Mathf.RoundToInt((canvas.transform.parent.transform.position.y + offset) * precision);
        if (renderer) renderer.sortingOrder = -Mathf.RoundToInt((transform.position.y + offset) * precision);
        if (canvas) Destroy(this, 3.0f);
    }
}
