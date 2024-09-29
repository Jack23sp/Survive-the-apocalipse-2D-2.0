using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShowerTrigger : MonoBehaviour
{
    public BuildingAccessory accessory;
    public ParticleSystem waterParticleSystem;
    public List<Player> playerInside = new List<Player>();

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(accessory.isServer)
        {
            if(collision.CompareTag("Player"))
            {
                Player pl = null;
                pl = collision.GetComponent<Player>();
                if (pl)
                {
                    if (!playerInside.Contains(pl))
                    {
                        playerInside.Add(pl);
                        pl.health.InvokeStartClean();
                    }
                }
            }

            if(playerInside.Count > 0)
            {
                waterParticleSystem.gameObject.SetActive(true);
                waterParticleSystem.Play();
            }
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (accessory.isServer)
        {
            if (collision.CompareTag("Player"))
            {
                Player pl = null;
                pl = collision.GetComponent<Player>();
                if (pl)
                {
                    if (playerInside.Contains(pl))
                    {
                        playerInside.Remove(pl);
                        pl.health.InvokeStopClean();
                    }
                }
            }

            if (playerInside.Count <= 0)
            {
                waterParticleSystem.gameObject.SetActive(false);
                waterParticleSystem.Play();
            }
        }
    }
}
