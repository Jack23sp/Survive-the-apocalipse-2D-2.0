using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public class TreeChecker : MonoBehaviour
{
    public WallManager wallManager;

    public BoxCollider2D collider;

    public List<BoxCollider2D> inside = new List<BoxCollider2D>();

    private void OnDisable()
    {
        if(!wallManager.identity.isServer)
            RecoverAllResources();
    }

    public void CheckResources(int condition, GameObject go)
    {
            if (condition == 0)
                NetworkServer.Destroy(go.transform.root.gameObject);
            else if(condition == 1)
                go.transform.root.gameObject.SetActive(false);
            else
                go.transform.root.gameObject.SetActive(true);
    }

    public void RecoverAllResources()
    {
        for(int i = 0; i < inside.Count; i++)
        {
            if (inside[i] != null)
                inside[i].transform.root.gameObject.SetActive(true);
            inside.RemoveAt(i);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Rock") || collision.CompareTag("Tree"))
        {
            if (wallManager.identity.isClient)
            {
                if (wallManager.modularBuilding.upBasementDecoration != -1)
                {
                    if (inside.Contains(collision.gameObject.GetComponent<BoxCollider2D>())) inside.Remove(collision.gameObject.GetComponent<BoxCollider2D>());
                    if (wallManager.up)
                    {
                        CheckResources(1, collision.gameObject);
                    }
                }
                if (wallManager.modularBuilding.downBasementDecoration != -1)
                {
                    if (inside.Contains(collision.gameObject.GetComponent<BoxCollider2D>())) inside.Remove(collision.gameObject.GetComponent<BoxCollider2D>());
                    if (wallManager.down)
                    {
                        CheckResources(1, collision.gameObject);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Rock") || collision.CompareTag("Tree"))
        {
            if (wallManager.identity.isClient)
            {
                if (!inside.Contains(collision.gameObject.GetComponent<BoxCollider2D>())) inside.Add(collision.gameObject.GetComponent<BoxCollider2D>());

                if (wallManager.up)
                    CheckResources(1, collision.gameObject);
                else if (wallManager.down)
                    CheckResources(1, collision.gameObject);
            }
            else
            {
                if (wallManager.up)
                    CheckResources(0, collision.gameObject);
                else if (wallManager.down)
                    CheckResources(0, collision.gameObject);
            }
        }
    }
}
