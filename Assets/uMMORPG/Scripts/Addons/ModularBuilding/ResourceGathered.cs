using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mono.Cecil;
using JetBrains.Annotations;

public partial class Player
{
    [Command]
    public void CmdAddGatheredResorce(int index, NetworkIdentity identity)
    {
        ResourceGathered resource = identity.gameObject.GetComponent<ResourceGathered>();
        if (inventory.CanAddItem(resource.slots[index].item, resource.slots[index].amount))
        {
            switch (resource.buildingType)
            {
                case "Scrap":
                    quests.barrels.Add(new QuestObject(resource.slots[index].item.name, resource.slots[index].amount));
                    playerPoints.barrellsPick++;
                    break;
                case "Clothes":
                    quests.boxes.Add(new QuestObject(resource.slots[index].item.name, resource.slots[index].amount));
                    playerPoints.boxesPick++;
                    break;
                case "Food":
                    quests.boxes.Add(new QuestObject(resource.slots[index].item.name, resource.slots[index].amount));
                    playerPoints.boxesPick++;
                    break;
            }
            inventory.AddItem(resource.slots[index].item, resource.slots[index].amount);
            resource.RemoveItem(index);

        }
    }
}

[System.Serializable]
public struct ResourceItem
{
    public ScriptableItem item;
    public int minAmount;
    public int maxAmount;

    public ResourceItem (ScriptableItem item,  int min, int max)
    {
        this.item = item;
        this.minAmount = min;
        this.maxAmount = max;
    }
}

public class ResourceGathered : NetworkBehaviour
{
    public SyncList<ItemSlot> slots = new SyncList<ItemSlot>();
    public int maxItem = 5;
    
    int rand = -1;
    public SpriteRenderer spriteRenderer;
    public int managerIndex;

    public string buildingType;


    void Start()
    {
        if (isServer)
        {
            SpawnItem();
            //InvokeRepeating(nameof(SpawnItem), 10, 1800);
        }
    }

    public override void OnStartServer()
    {
        slots.Callback += OnServerChangeCurrentSlot;
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        spriteRenderer.material = slots.Count > 0 ? ModularBuildingManager.singleton.objectPresent : ModularBuildingManager.singleton.objectNotPresent;
        slots.Callback += OnChangeCurrentSlot;
    }
    void OnChangeCurrentSlot(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        spriteRenderer.material = slots.Count > 0 ? ModularBuildingManager.singleton.objectPresent : ModularBuildingManager.singleton.objectNotPresent;
        if (UIResourceGathered.singleton && UIResourceGathered.singleton.panel.activeInHierarchy && UIResourceGathered.singleton.resource.netId == netIdentity.netId)
        {
            if (slots.Count == 0) UIResourceGathered.singleton.closeButton.onClick.Invoke();
            else
            {
                UIResourceGathered.singleton.Open(this);
            }
        }
    }

    void OnServerChangeCurrentSlot(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if(op == SyncList<ItemSlot>.Operation.OP_REMOVEAT)
        {
            if(slots.Count == 0)
            {
                NetworkServer.Destroy(this.gameObject);
            }
        }
    }

    public void SpawnItem()
    {   
        for(int i = 0; i < ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].obligatory.Count; i++ )
        {
            int index = i;
            AddItemObligatory(index);
        }

        if (ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].randomToSpawn.Count > 0)
        {
            int random = Random.Range(1, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].randomToSpawn.Count);
            for (int i = 0; i < random; i++)
            {
                int index = i;
                int chooseRandom = Random.Range(1, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].randomToSpawn.Count);
                AddItemRandom(chooseRandom);
            }
        }
        else
        {
            if (Random.Range(0, 2) == 1) return;
            for (int i = slots.Count; i < maxItem + ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].obligatory.Count; i++)
            {
                AddItem();
            }
        }

        if (ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].book.Count > 0)
        {
            int random = Random.Range(0, 10);
            if (random > 7)
            {
                AddItemBook();
            }
        }
    }

    public void AddItem()
    {
        rand = Random.Range(0, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].toSpawn.Count);
        slots.Add(new ItemSlot(new Item(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].toSpawn[rand].item), Random.Range(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].toSpawn[rand].minAmount, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].toSpawn[rand].maxAmount +1)));
    }

    public void AddItemBook()
    {
        rand = Random.Range(0, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].book.Count);
        slots.Add(new ItemSlot(new Item(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].book[rand].item), Random.Range(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].book[rand].minAmount, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].book[rand].maxAmount + 1)));
    }

    public void AddItemRandom(int index)
    {
        slots.Add(new ItemSlot(new Item(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].randomToSpawn[index].item), Random.Range(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].randomToSpawn[index].minAmount, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].randomToSpawn[index].maxAmount + 1)));
    }

    public void AddItemObligatory(int ind)
    {
        for(int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item.data.name == ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].obligatory[ind].item.name)
                return;
        }
        slots.Add(new ItemSlot(new Item(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].obligatory[ind].item), Random.Range(ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].obligatory[ind].minAmount, ResourceGatheredItemListManager.singleton.listGatheredResource[managerIndex].obligatory[ind].maxAmount + 1)));
    }


    public void RemoveItem(int index)
    {
        slots.RemoveAt(index);
    }
}
