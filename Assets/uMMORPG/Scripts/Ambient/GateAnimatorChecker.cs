using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateAnimatorChecker : MonoBehaviour
{
    public Gate gate;

    public List<Player> players = new List<Player>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gate.isServer)
        {
            if (collision.CompareTag("Player"))
            {
                if (ModularBuildingManager.singleton.CanDoOtherActionForniture(gate.GetComponent<BuildingAccessory>(), collision.GetComponent<Player>()))
                {
                    if (!players.Contains(collision.GetComponent<Player>()))
                        players.Add(collision.GetComponent<Player>());
                    gate.playerInside = players.Count;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (gate.isServer)
        {
            if (collision.CompareTag("Player"))
            {
                //if (ModularBuildingManager.singleton.CanDoOtherActionForniture(gate.GetComponent<BuildingAccessory>(), collision.GetComponent<Player>()))
                //{
                    if (players.Contains(collision.GetComponent<Player>()))
                        players.Remove(collision.GetComponent<Player>());
                    gate.playerInside = players.Count;
                //}
            }
        }
    }
}
