using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementWall : MonoBehaviour
{
    [HideInInspector] public Collider2D collider;
    [HideInInspector] public SpriteRenderer renderer;
    public ModularBuilding modularBuilding;

    public bool up, left, down, right;

    void Awake()
    {
        collider = GetComponent<Collider2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    public void Manage(bool condition)
    {
        if (condition)
        {
            collider.enabled = condition;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 120.0f);
        }
        else
        {
            collider.enabled = condition;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.0f);
        }
    }
}
