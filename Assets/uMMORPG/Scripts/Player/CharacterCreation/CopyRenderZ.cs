using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRenderZ : MonoBehaviour
{
    public Canvas canvas;
    public SpriteRenderer spriteRenderer;

    public void Update()
    {
        canvas.sortingOrder = spriteRenderer.sortingOrder;
    }
}
