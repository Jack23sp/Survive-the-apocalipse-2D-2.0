using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

public partial class Player
{
    [HideInInspector] public PlayerItemDrop playerItemDrop;

    [Command]
    public void CmdAddItemDrop(NetworkIdentity identity)
    {
        if (!identity.isServer && !identity.isClient) return;
        CurvedMovement curv = identity.GetComponent<CurvedMovement>();
        if (curv)
        {
            if (state == "MOVING")
            {
                if (curv.gold > 0)
                {
                    gold += curv.gold;
                    quests.SyncPickOnServer(new DetailOfQuest("Gold", Convert.ToInt32(curv.gold)));
                    NetworkServer.Destroy(curv.gameObject);
                }
                else
                {
                    if (inventory.CanAddItem(curv.itemToDrop, curv.amountItem))
                    {
                        inventory.AddItem(curv.itemToDrop, curv.amountItem);
                        quests.SyncPickOnServer(new DetailOfQuest(curv.itemToDrop.name, curv.amountItem));
                        
                        if(curv.itemToDrop.name == "Wood")
                        {
                            playerPoints.woodPick++;
                        }
                        if (curv.itemToDrop.name == "Stone")
                        {
                            playerPoints.stonePick++;
                        }
                        NetworkServer.Destroy(curv.gameObject);
                    }
                }
            }
        }
    }
}

public class PlayerItemDrop : NetworkBehaviour
{
    [HideInInspector] public Player player;

    void Awake()
    {
        player = GetComponent<Player>();
        player.playerItemDrop = this;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ItemDrop"))
        {   
            if(collision.GetComponent<NetworkIdentity>().isClient || collision.GetComponent<NetworkIdentity>().isServer)
                player.CmdAddItemDrop(collision.GetComponent<NetworkIdentity>());
        }
    }

    public void CalculateItemToDrop()
    {
        float abLevel = AbilityManager.singleton.FindNetworkAbilityLevel("Conservative", player.name);
        int toDrop = 5;
        if(abLevel / 10 > 0 ) {
            toDrop = toDrop - Convert.ToInt32(abLevel / 10); 
        }
        SpawnItemOnDeath(toDrop);
    }


    [Command]
    public void CmdSpawnDropSpecificItem(int index, bool inventory, int amount)
    {
        ItemSlot slot = inventory ? player.inventory.slots[index] : player.playerBelt.belt[index];
        if(slot.amount > 0 && slot.amount >= amount)
        {
            GameObject g = Instantiate(ResourceManager.singleton.objectDrop.gameObject, player.transform.position, Quaternion.identity);
            g.GetComponent<CurvedMovement>().startEntity = player.transform;
            g.GetComponent<CurvedMovement>().SpawnAtPosition(slot.item, amount, -1, 0);

            slot.DecreaseAmount(amount);
            if(inventory)
            {
                player.inventory.slots[index] = slot;
            }
            else
            {
                player.playerBelt.belt[index] = slot;
            }
        }
    }

    public void SpawnItemOnDeath(int amountItem)
    {
        SpawnItemDrop(player.netIdentity, amountItem);
    }

    public void SpawnItemDrop(NetworkIdentity identity,int amountItem)
    {
        Player player = identity.GetComponent<Player>();

        for (int i = 0; i < amountItem; i++)
        {
            int itmIndex = Drop();
            if (itmIndex == -1) return;
            int amount = UnityEngine.Random.Range(1, player.inventory.slots[itmIndex].amount);
            GameObject g = Instantiate(ResourceManager.singleton.objectDrop.gameObject, player.transform.position, Quaternion.identity);
            g.GetComponent<CurvedMovement>().startEntity = player.transform;
            g.GetComponent<CurvedMovement>().SpawnAtPosition(player.inventory.slots[itmIndex].item, amount, itmIndex, amount);
        }
    }

    [Command]
    public void CmdItemDrop( GameObject gm)
    {
        NetworkServer.Spawn(gm);
    }

    public int Drop()
    {
        List<int> presentItems = PresentItem();
        if (presentItems.Count == 0) return -1;
        int range = UnityEngine.Random.Range(0, presentItems.Count);
        return presentItems[range];

    }

    public List<int> PresentItem()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            if (player.inventory.slots[i].amount > 0)
                list.Add(i);
        }
        return list;
    }
}
